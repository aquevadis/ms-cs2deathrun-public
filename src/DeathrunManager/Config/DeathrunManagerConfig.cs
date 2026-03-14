using System.IO;
using System.Text.Json;
using DeathrunManager.Shared.Config;
using DeathrunManager.Shared.Data;
using Microsoft.Extensions.Logging;

namespace DeathrunManager.Config;

public class DeathrunManagerConfig : IDeathrunManagerConfig
{
    
#pragma warning disable CA2211
    public static DeathrunManagerConfigStructure Config = null!;
#pragma warning restore CA2211
    
    public IDeathrunManagerConfigStructure GetConfig => Config;
    
    #region Config

    public static DeathrunManagerConfigStructure LoadDeathrunManagerConfig()
    {
        if (!Directory.Exists(DeathrunManager.ModulePath + "/configs")) 
            Directory.CreateDirectory(DeathrunManager.ModulePath + "/configs");
        
        var configPath = Path.Combine(DeathrunManager.ModulePath, "configs/deathrun.json");
        if (!File.Exists(configPath)) CreateDeathrunManagerConfig(configPath);

        var config = JsonSerializer.Deserialize<DeathrunManagerConfigStructure>(File.ReadAllText(configPath))!;
        
        DeathrunManager.Logger.LogInformation("[DeathrunManager] {colorMessage}", "Load DeathrunManager Config!");
        
        return Config = config;
    }

    private static DeathrunManagerConfigStructure CreateDeathrunManagerConfig(string configPath)
    {
        var config = new DeathrunManagerConfigStructure() {};
            
        File.WriteAllText(configPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        return config;
    }
    
    //reload config
    public static void ReloadConfig() => LoadDeathrunManagerConfig();
    
    #endregion
}

public class DeathrunManagerConfigStructure : IDeathrunManagerConfigStructure
{
    public bool GiveWeaponToCTs { get; init; } = true;
    public bool RemoveBuyZones { get; init; } = true;
    public bool RemoveMoneyFromGameAndHud { get; init; } = true;
    public bool SetRoundTimeOneHour { get; init; } = true;
    public bool EnableClippingThroughTeamMates { get; init; } = true;
    public bool EnableAutoBunnyHopping { get; init; } = true;
    
    public bool EnableKillCommandForCTs { get; init; } = true;
    public bool EnableKillCommandForTs { get; init; } = false;
    
    public string Prefix { get; init; } = "{GREEN}[Deathrun]{DEFAULT}";
}