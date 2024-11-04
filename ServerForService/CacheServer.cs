using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonForServer;
namespace ServerForService
{
    public class CacheServer : ICache
    {
        private object lockObject = new object();
        public Dictionary<string, byte[]> dataStore;
        public void Initialize()
        {
            dataStore = new Dictionary<string, byte[]>();
        }
        /// <summary>
        /// Adds the item to the cache.
        /// </summary>
        public void Add(string key, object value)
        {
            lock (lockObject)
            {
                dataStore.Add(key, value as byte[]);
            }
        }
        /// <summary>
        /// Removes the item from the cache.
        /// </summary>
        public void Remove(string key)
        {
            lock (lockObject)
            {
                dataStore.Remove(key);
            }
        }
        /// <summary>
        /// Retrieves the item from the cache.
        /// </summary>
        public object Get(string key)
        {
            lock (lockObject)
            {
                return dataStore[key];
            }
        }
        /// <summary>
        /// Clears the contents of the cache.
        /// </summary>
        public void Clear()
        {
            lock (lockObject)
            {
                dataStore.Clear();
            }
        }
        /// <summary>
        /// Destroys the cache.
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("Client disconnected");
        }

    }
}
