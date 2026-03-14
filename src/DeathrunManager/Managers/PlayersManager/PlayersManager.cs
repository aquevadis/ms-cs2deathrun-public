using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using DeathrunManager.Config;
using DeathrunManager.Shared.Managers;
using DeathrunManager.Shared.Objects;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;

namespace DeathrunManager.Managers.PlayersManager;

internal class PlayersManager(
    ILogger<PlayersManager> logger,
    IModSharp modSharp,
    IClientManager clientManager,
    DeathrunManagerConfigStructure deathrunManagerConfig) : IPlayersManager, IClientListener
{
    public static PlayersManager Instance = null!;
    
    public readonly ConcurrentDictionary<ulong, DeathrunPlayer> DeathrunPlayers = new();
    
    #region IModule
    
    public bool Init()
    {
        Instance = this;
        
        logger.LogInformation("[Deathrun][Managers] {colorMessage}", "Load Players Manager");
        
        modSharp.InstallGameFrameHook(null, OnGameFramePost);
        clientManager.InstallClientListener(this);
        
        clientManager.InstallCommandCallback("kill", OnClientKillCommand);
        
        return true;
    }

    public static void OnPostInit() { }

    public void Shutdown()
    {
        ClearDeathrunPlayers();
        
        modSharp.RemoveGameFrameHook(null, OnGameFramePost);
        clientManager.RemoveClientListener(this);
        
        clientManager.RemoveCommandCallback("kill", OnClientKillCommand);
        
        logger.LogInformation("[Deathrun][Managers] {colorMessage}", "Unload Players Manager");
    }

    #endregion

    #region Hooks

    private readonly List<IDeathrunPlayer> _deathrunPlayersBuffer = new(64);
    
    private void OnGameFramePost(bool simulating, bool bFirstTick, bool bLastTick)
    {
        GetAllValidDeathrunPlayersZAlloc(_deathrunPlayersBuffer);

        foreach (var iDeathrunPlayer in _deathrunPlayersBuffer)
        {
            //skip bots here
            //if (iDeathrunPlayer.Client.SteamId == 0) continue;
            
            if (iDeathrunPlayer is not DeathrunPlayer {} deathrunPlayer) return;
            
            //call the player think method
            deathrunPlayer.PlayerThink();
        }
    }

    #endregion
    
    #region Listeners
    
    public void OnClientConnected(IGameClient client)
    {
        if (client?.IsValid is not true) return;
        
        //skip if we couldn't add the client to the deathrun players dictionary
        DeathrunPlayer? deathrunPlayer;
        if (DeathrunPlayers.TryAdd(client.SteamId != 0 ? client.SteamId : client.Slot,
                                 deathrunPlayer = new DeathrunPlayer(client)) is not true)
                                                                                   return;
        
        if (deathrunPlayer?.LivesSystem is null) return;
  
        //skip getting data from the database if we've set the SaveLivesToDatabase to false
        if (LivesSystemManager.LivesSystemManager.LivesSystemConfig?.SaveLivesToDatabase is not true)
        {
            deathrunPlayer.LivesSystem?.SetLivesNum(LivesSystemManager.LivesSystemManager.LivesSystemConfig?.StartLivesNum ?? 1);
        }
        else
        {
            Task.Run(async () =>
            {
                ulong steamId64 = deathrunPlayer.Client.SteamId;
                var livesNumFromDb = await GetSavedLives(steamId64);
                
                deathrunPlayer.LivesSystem?.SetLivesNum(livesNumFromDb);
            });
        }
    }
    
    public void OnClientPutInServer(IGameClient client)
    {
        if (client?.IsValid is not true) return;

        DeathrunPlayers.TryAdd(client.SteamId != 0 ? client.SteamId : client.Slot, new DeathrunPlayer(client));
    }
    
    public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
    {
        if (client?.IsValid is not true) return;

        if (DeathrunPlayers.TryRemove(client.SteamId != 0 ? client.SteamId : client.Slot, out var removedDeathrunPlayer) is true)
        {
            //remove thinking functions
            removedDeathrunPlayer.StopPlayerThink();
            
            //skip bots here
            if (client.SteamId == 0) return;
            
            //check if the lives system is enabled and we are saving the lives to the database
            if (LivesSystemManager.LivesSystemManager.LivesSystemConfig?.EnableLivesSystem is true
                && LivesSystemManager.LivesSystemManager.LivesSystemConfig.SaveLivesToDatabase is true)
            {
                if (removedDeathrunPlayer.LivesSystem is null) return;
                
                ulong steamId64 = removedDeathrunPlayer.Client.SteamId;
                var livesNum = removedDeathrunPlayer.LivesSystem.GetLivesNum;
                
                Task.Run(() => SaveLivesToDatabase(steamId64, livesNum));
            }
        }
    }
    
    #endregion

    #region Commands
    
    private ECommandAction OnClientKillCommand(IGameClient client, StringCommand command)
    {
        var deathrunPlayer = GetDeathrunPlayer(client);
        if (deathrunPlayer is null) return ECommandAction.Stopped;

        if (deathrunPlayer.IsValidAndAlive is not true) return ECommandAction.Stopped;
        
        if (deathrunManagerConfig.EnableKillCommandForCTs is true
            && deathrunPlayer.Controller?.Team is CStrikeTeam.CT)
        {
            deathrunPlayer.PlayerPawn?.Slay();
        }
        
        if (deathrunManagerConfig.EnableKillCommandForTs is true
            && deathrunPlayer.Controller?.Team is CStrikeTeam.TE)
        {
            deathrunPlayer.PlayerPawn?.Slay();
        }
        
        return ECommandAction.Stopped;
    }
    
    #endregion
    
    #region DeathrunPlayer Async

    private static async Task SaveLivesToDatabase(ulong steamId64, int newLivesNum)
    {
        try
        {
            await using var connection = new MySqlConnection(LivesSystemManager.LivesSystemManager.ConnectionString);
            await connection.OpenAsync();
            
            var insertUpdateLivesQuery 
                = $@" INSERT INTO `{(LivesSystemManager.LivesSystemManager.LivesSystemConfig?.TableName ?? "deathrun_players")}` 
                      ( steamid64, `lives` )  
                      VALUES 
                      ( @SteamId64, @NewLives ) 
                      ON DUPLICATE KEY UPDATE 
                                       `lives`  = '{newLivesNum}'
                    ";
    
            await connection.ExecuteAsync(insertUpdateLivesQuery,
                new {
                            SteamId64        = steamId64, 
                            NewLives         = newLivesNum
                          });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
    }
    
    private static async Task<int> GetSavedLives(ulong steamId64)
    {
        try
        {
            await using var connection = new MySqlConnection(LivesSystemManager.LivesSystemManager.ConnectionString);
            await connection.OpenAsync();
    
            //fast check if the player has saved lives data
            var hasSavedLivesData = await HasSavedLivesData(steamId64);
            if (hasSavedLivesData is not true) return 0;
            
            //take the lives num from the database
            var livesNum = await connection.QueryFirstOrDefaultAsync<int>
            ($@"SELECT
                       `lives`
                    FROM `{(LivesSystemManager.LivesSystemManager.LivesSystemConfig?.TableName ?? "deathrun_players")}`
                    WHERE steamid64 = @SteamId64
                 ",
                new { SteamId64 = steamId64 }
            
            );
            
            return livesNum;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return 0;
    }

    private static async Task<bool> HasSavedLivesData(ulong steamId64)
    {
        try
        {
            await using var connection = new MySqlConnection(LivesSystemManager.LivesSystemManager.ConnectionString);
            await connection.OpenAsync();
    
            var hasSavedLivesData 
                = await connection.QueryFirstOrDefaultAsync<bool>
                                    ($@"SELECT EXISTS(SELECT 1 FROM `{(LivesSystemManager.LivesSystemManager.LivesSystemConfig?.TableName ?? "deathrun_players")}`
                                            WHERE steamid64 = @SteamId64 LIMIT 1)
                                         ",
                                        new { SteamId64 = steamId64 }
                                    
                                    );
            
            return hasSavedLivesData;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return false;
    }

    #endregion
    
    #region DeathrunPlayer
    
    public IDeathrunPlayer? GetDeathrunPlayer(IGameClient client)
    {
        if (client?.IsValid is not true) return null;
        
        var deathrunPlayer = DeathrunPlayers.GetValueOrDefault(client.SteamId != 0 ? client.SteamId : client.Slot);
        return deathrunPlayer?.IsValid is not true ? null : deathrunPlayer;
    }
    
    public IDeathrunPlayer? GetDeathrunPlayer(ulong steamId64)
    {
        var client = clientManager.GetGameClient(steamId64);
        return client?.IsValid is not true ? null : GetDeathrunPlayer(client);
    }

    public IDeathrunPlayer? GetDeathrunPlayer(PlayerSlot slot)
    {
        var client = clientManager.GetGameClient(slot);
        return client?.IsValid is not true ? null : GetDeathrunPlayer(client);
    }
    
    #endregion
    
    #region DeathrunPlayers
    
    private void ClearDeathrunPlayers() => DeathrunPlayers.Clear();
    
    public IReadOnlyCollection<IDeathrunPlayer> GetAllValidDeathrunPlayers()
    {
        var deathrunPlayers = new List<IDeathrunPlayer>(DeathrunPlayers.Count);

        foreach (var deathrunPlayer in DeathrunPlayers.Values)
            if (deathrunPlayer.IsValid)
                deathrunPlayers.Add(deathrunPlayer);
        
        return deathrunPlayers;
    }
    
    public int ValidDeathrunPlayersNum => GetAllValidDeathrunPlayers().Count;

    public IReadOnlyCollection<IDeathrunPlayer> GetAllAliveDeathrunPlayers()
    {
        var deathrunPlayers = new List<IDeathrunPlayer>(DeathrunPlayers.Count);

        foreach (var deathrunPlayer in DeathrunPlayers.Values)
            if (deathrunPlayer.IsValidAndAlive)
                deathrunPlayers.Add(deathrunPlayer);
        
        return deathrunPlayers;
    }
   
    public IReadOnlyCollection<IDeathrunPlayer> GetAllDeadDeathrunPlayers()
    {
        var deathrunPlayers = new List<IDeathrunPlayer>(DeathrunPlayers.Count);

        foreach (var deathrunPlayer in DeathrunPlayers.Values)
            if (deathrunPlayer.IsValidAndAlive is not true)
                deathrunPlayers.Add(deathrunPlayer);
        
        return deathrunPlayers;
    }
    
    #endregion
    
    #region DeathrunPlayers(Zero allocations)
    
    public void GetAllValidDeathrunPlayersZAlloc(List<IDeathrunPlayer> buffer)
    {
        buffer.Clear();

        foreach (var deathrunPlayer in DeathrunPlayers.Values)
            if (deathrunPlayer.IsValid) 
                buffer.Add(deathrunPlayer);
    }

    public void GetAllAliveDeathrunPlayersZAlloc(List<IDeathrunPlayer> buffer)
    {
        buffer.Clear();

        foreach (var deathrunPlayer in DeathrunPlayers.Values)
            if (deathrunPlayer.IsValidAndAlive) 
                buffer.Add(deathrunPlayer);
    }
    
    #endregion
    
    int IClientListener.ListenerVersion => IClientListener.ApiVersion;
    int IClientListener.ListenerPriority => 8;
}




