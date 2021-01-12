using System.Threading.Tasks;

namespace CircuitPersistence.Services
{
    public interface IStore
    {
        Task SaveValueAsync(string key, string value);
        Task<string> LoadValueAsync(string key);
    }

    public interface ISingletonStore : IStore {}
    public interface IScopeStore : IStore {}
    public interface IComponentStore : IStore {}
    public interface ISessionStore : IStore {}
}
