// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryCache.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.FeatureFlags.LaunchDarkly.DictionaryCache
{
    using System;
    using System.Collections.Generic;

    internal class DictionaryCache<T> : IDictionaryCache<T>
    {
        private static readonly IDictionary<string, TimeLimitedContainer> Cache = new Dictionary<string, TimeLimitedContainer>();

        // ReSharper disable StaticFieldInGenericType
        private static readonly object Lock = new object(); // suppressed because we want one lock per generic type.
                                                            // ReSharper restore StaticFieldInGenericType
                                                            
        public DictionaryCache()
        {
            LifeSpan = 60;
        }
        
        public uint LifeSpan { get; set; }
        
        public static void Clear()
        {
            lock (Lock)
            {
                Cache.Clear();
            }
        }
        
        public T GetOrLoadNewObject(string key, Func<T> loadNewObject)
        {
            lock (Lock)
            {
                if (!Cache.ContainsKey(key))
                    Cache[key] = new TimeLimitedContainer(LifeSpan);

                return Cache[key].GetOrLoadNewObject(loadNewObject);
            }
        }

        private class TimeLimitedContainer
        {
            private readonly uint _lifespanInSeconds;

            private T _loadedObject;

            private DateTimeOffset _validUntil;

            public TimeLimitedContainer(uint lifespanInSeconds)
            {
                _lifespanInSeconds = lifespanInSeconds;
            }

            public T GetOrLoadNewObject(Func<T> loadNewObject)
            {
                if (_validUntil < DateTimeOffset.UtcNow)
                {
                    _loadedObject = loadNewObject();
                    _validUntil = DateTimeOffset.UtcNow.AddSeconds(_lifespanInSeconds);
                }

                return _loadedObject;
            }
        }
    }
}

