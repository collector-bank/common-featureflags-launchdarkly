// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDictionaryCache.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.FeatureFlags.LaunchDarkly.DictionaryCache
{
    using System;
    
    internal interface IDictionaryCache<T>
    {
        T GetOrLoadNewObject(string key, Func<T> loadNewObject);
        
        uint LifeSpan { get; set; }
    }
}