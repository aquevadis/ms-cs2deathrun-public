namespace DeathrunManager.Shared.Managers;

public interface IDeathrunManagers
{
    public IPlayersManager PlayersManager { get; }
    public IGameplayManager GameplayManager { get; }
}