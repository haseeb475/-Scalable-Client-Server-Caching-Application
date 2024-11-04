using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
  public interface ICache
  {
    /// <summary>
    /// Initializes the cache instance
    /// </summary>
    public void Initialize();
    /// <summary>
    /// Adds the item to the cache.
    /// </summary>
    public void Add(string key, object value);
    /// <summary>
    /// Removes the item from the cache.
    /// </summary>
    public void Remove(string key);
    /// <summary>
    /// Retrieves the item from the cache.
    /// </summary>
    public object Get(string key);
    /// <summary>
    /// Clears the contents of the cache.
    /// </summary>
    public void Clear();
    /// <summary>
    /// Destroys the cache.
    /// </summary>
    public void Dispose();
  }
}
