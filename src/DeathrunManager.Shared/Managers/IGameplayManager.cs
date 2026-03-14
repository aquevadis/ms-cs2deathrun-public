using DeathrunManager.Shared.Enums;
using DeathrunManager.Shared.Objects;

namespace DeathrunManager.Shared.Managers;

public interface IGameplayManager : IManager
{
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