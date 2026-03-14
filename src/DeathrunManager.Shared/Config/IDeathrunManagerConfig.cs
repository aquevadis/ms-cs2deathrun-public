
using DeathrunManager.Shared.Data;

namespace DeathrunManager.Shared.Config;

public interface IDeathrunManagerConfig
{
    IDeathrunManagerConfigStructure GetConfig { get; }
}