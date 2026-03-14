using DeathrunManager.Shared.Config;
using DeathrunManager.Shared.Managers;

namespace DeathrunManager.Shared;

public interface IDeathrunManager
{
    #region Identity

    /// <summary>
    /// Represents the unique identity of the <see cref="IDeathrunManager"/> interface.
    /// </summary>
    /// <remarks>
    /// This property uniquely identifies the <see cref="IDeathrunManager"/> interface and can be used
    /// for purposes such as registration, discovery, or logging.
    /// The returned value is derived from the fully qualified name of the interface or its name if the full name is null.
    /// </remarks>
    static string Identity => typeof(IDeathrunManager).FullName ?? nameof(IDeathrunManager);

    #endregion

    public IDeathrunManagerConfig Config { get; }
    
    public IDeathrunManagers Managers { get; }
}
