using DeathrunManager.Shared.Managers;

namespace DeathrunManager.Managers;

public class DeathrunManagers(
    IPlayersManager playersManager,
    IGameplayManager gameplayManager
    ) : IManager, IDeathrunManagers
{
    public IPlayersManager PlayersManager => playersManager;
    public IGameplayManager GameplayManager => gameplayManager;
    
    public bool Init() => true;
}