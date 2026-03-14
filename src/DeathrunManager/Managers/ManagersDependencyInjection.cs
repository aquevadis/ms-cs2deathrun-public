using DeathrunManager.Extensions;
using DeathrunManager.Interfaces.Managers.LivesSystemManager;
using DeathrunManager.Interfaces.Managers.Native;
using DeathrunManager.Managers.Native.ClientListener;
using DeathrunManager.Managers.Native.Event;
using DeathrunManager.Managers.Native.GameListener;
using DeathrunManager.Shared.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace DeathrunManager.Managers;

internal static class ManagersDependencyInjection
{
    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        //Native Managers
        services.AddSingleton<IManager, IClientListenerManager, ClientListenerManager>();
        services.AddSingleton<IManager, IEventManager, EventManager>();
        services.AddSingleton<IManager, IGameListenerManager, GameListenerManager>();
        
        //DeathrunManager Managers
        services.AddSingleton<IManager, ILivesSystemManager, LivesSystemManager.LivesSystemManager>();
        services.AddSingleton<IManager, IPlayersManager, PlayersManager.PlayersManager>();
        services.AddSingleton<IManager, IGameplayManager, GameplayManager.GameplayManager>();
        
        services.AddSingleton<IManager, IDeathrunManagers, DeathrunManagers>();

        return services;
    }
}
