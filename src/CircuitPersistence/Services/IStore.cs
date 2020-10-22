using System.Threading.Tasks;

namespace CircuitPersistence.Services
{
    public interface IStore
    {
        Task SaveValueAsync(string key, string value);
        Task<string> LoadValueAsync(string key);
    }
}
