using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircuitPersistence.Services
{
    public class InMemoryStore : IStore
    {
        private Dictionary<string, string> _store = new Dictionary<string, string>();

        public Task SaveValueAsync(string key, string value)
        {
            _store[key] = value;
            return Task.CompletedTask;
        }

        public Task<string> LoadValueAsync(string key)
        {
            _store.TryGetValue(key, out string value);
            return Task.FromResult(value ?? null);
        }
    }
}
