﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonForServer
{
  public interface ICache
  {
    /// <summary>
    /// Initializes the cache instance
    /// </summary>
    void Initialize();
    /// <summary>
    /// Adds the item to the cache.
    /// </summary>
    void Add(string key, object value);
    /// <summary>
    /// Removes the item from the cache.
    /// </summary>
    void Remove(string key);
    /// <summary>
    /// Retrieves the item from the cache.
    /// </summary>
    object Get(string key);
    /// <summary>
    /// Clears the contents of the cache.
    /// </summary>
    void Clear();
    /// <summary>
    /// Destroys the cache.
    /// </summary>
    void Dispose();
  }
}
