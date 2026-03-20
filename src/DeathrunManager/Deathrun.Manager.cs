using System;
using System.IO;
using DeathrunManager.Config;
using DeathrunManager.Managers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using DeathrunManager.Shared;
using DeathrunManager.Shared.Config;
using DeathrunManager.Shared.Interfaces;
using DeathrunManager.Shared.Managers;
using Sharp.Shared;
using Sharp.Shared.Abstractions;

namespace DeathrunManager;

public class DeathrunManager : IModSharpModule, IDeathrunManager
{
    public string DisplayName         => $"[Deathrun] Manager - Last Build Time: {Bridge.FileTime}";
    public string DisplayAuthor       => "AquaVadis";
    
    private readonly ServiceProvider  _serviceProvider;
    private static ISharedSystem      _sharedSystem = null!;
    public static ISharedSystem SharedSystem => _sharedSystem;
    
#pragma warning disable CA2211
    public static string ModulePath                 = "";
    public static ILogger<DeathrunManager> Logger   = null!;
    public static InterfaceBridge Bridge            = null!;
#pragma warning restore CA2211
    
    public DeathrunManager(ISharedSystem sharedSystem,
        string                   dllPath,
        string                   sharpPath,
        Version                  version,
        IConfiguration           coreConfiguration,
        bool                     hotReload)
    {
        ModulePath = dllPath;
        Bridge = new InterfaceBridge(dllPath, sharpPath, version, sharedSystem);
        Logger = sharedSystem.GetLoggerFactory().CreateLogger<DeathrunManager>();
        _sharedSystem = sharedSystem;
        
        var configuration = new ConfigurationBuilder()
                                .AddJsonFile(Path.Combine(dllPath, "base.json"), true, false)
                                .Build();
        
        var services = new ServiceCollection();

        services.AddSingleton(Bridge);
        services.AddSingleton(Bridge.ClientManager);
        services.AddSingleton(Bridge.EventManager);
        services.AddSingleton(Bridge.EntityManager);
        services.AddSingleton(Bridge.HookManager);
        services.AddSingleton(Bridge.ModSharp);
        services.AddSingleton(Bridge.ConVarManager);
        services.AddSingleton(Bridge.LoggerFactory);
        services.AddSingleton(sharedSystem);
        services.AddSingleton<IConfiguration>(configuration);
        services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
        
        //load base config and add to DI
        var deathrunManagerConfig = DeathrunManagerConfig.LoadDeathrunManagerConfig();
        services.AddSingleton(deathrunManagerConfig);
        
        services.AddManagers();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    #region IModule
    
    public bool Init()
    {
        Logger.LogInformation("[DeathrunManager] {colorMessage}", "Load DeathrunManager!");
        
        //load managers
        CallInit<IManager>();
        return true;
    }

    public void PostInit() 
    {
        //expose shared interface
        Bridge.SharpModuleManager.RegisterSharpModuleInterface<IDeathrunManager>(this, IDeathrunManager.Identity, this);
                
        CallPostInit<IManager>();
    }

    public void Shutdown()
    {
        CallShutdown<IManager>();

        _serviceProvider.ShutdownAllSharpExtensions();
        
        Logger.LogInformation("[DeathrunManager] {colorMessage}", "Unloaded DeathrunManager!");
    }

    public void OnAllModulesLoaded()
    {
        CallOnAllSharpModulesLoaded<IManager>();
    }

    public void OnLibraryConnected(string name) { }

    public void OnLibraryDisconnect(string name) { }
    
    #endregion
    
    #region Injected Instances' Caller methods
    
    private int CallInit<T>() where T : IBaseInterface
    {
        var init = 0;

        foreach (var service in _serviceProvider.GetServices<T>())
        {
            if (!service.Init())
            {
                Logger.LogError("Failed to Init {service}!", service.GetType().FullName);

                return -1;
            }

            init++;
        }

        return init;
    }

    private void CallPostInit<T>() where T : IBaseInterface
    {
        foreach (var service in _serviceProvider.GetServices<T>())
        {
            try
            {
                service.OnPostInit();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occurred while calling PostInit in {m}", service.GetType().Name);
            }
        }
    }

    private void CallShutdown<T>() where T : IBaseInterface
    {
        foreach (var service in _serviceProvider.GetServices<T>())
        {
            try
            {
                service.Shutdown();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occurred while calling Shutdown in {m}", service.GetType().Name);
            }
        }
    }

    private void CallOnAllSharpModulesLoaded<T>() where T : IBaseInterface
    {
        foreach (var service in _serviceProvider.GetServices<T>())
        {
            try
            {
                service.OnAllSharpModulesLoaded();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occurred while calling OnAllSharpModulesLoaded in {m}", service.GetType().Name);
            }
        }
    }

    #endregion
    
    public IDeathrunManagers Managers => _serviceProvider.GetRequiredService<IDeathrunManagers>();
    
    public IDeathrunManagerConfig Config => _serviceProvider.GetRequiredService<IDeathrunManagerConfig>();
}