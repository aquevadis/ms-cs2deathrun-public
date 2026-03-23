using DeathrunManager.Shared.Enums;
using DeathrunManager.Shared.Objects;

namespace DeathrunManager.Shared.Managers;

public interface IGameplayManager : IManager
{
    delegate void GameMasterPickedDelegate(IDeathrunPlayer gameMaster);
    delegate void PlayerSpawnPostDelegate(IDeathrunPlayer deathrunPlayer);
    delegate void RoundStartDelegate();
    delegate void RoundEndDelegate();

    /// <summary>
    /// Fired when a game master is picked at the start of a deathrun round.
    /// </summary>
    event GameMasterPickedDelegate? GameMasterPicked;

    /// <summary>
    /// Fired after a player spawns and their spawn setup (class assignment, weapon equip) has completed.
    /// </summary>
    event PlayerSpawnPostDelegate? PlayerSpawned;

    /// <summary>
    /// Fired after the deathrun round has started.
    /// </summary>
    event RoundStartDelegate? RoundStarted;

    /// <summary>
    /// Fired after the deathrun round has ended.
    /// </summary>
    event RoundEndDelegate? RoundEnded;

    /// <summary>
    /// Retrieves the current state of the deathrun round.
    /// </summary>
    /// <returns>
    /// A value of type <see cref="DRoundState"/> indicating the current round state.
    /// </returns>
    DRoundState GetRoundState();

    /// <summary>
    /// Retrieves the deathrun player currently designated as the game master.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IDeathrunPlayer"/> representing the game master if one is assigned; otherwise, null.
    /// </returns>
    IDeathrunPlayer? GetGameMaster();
}