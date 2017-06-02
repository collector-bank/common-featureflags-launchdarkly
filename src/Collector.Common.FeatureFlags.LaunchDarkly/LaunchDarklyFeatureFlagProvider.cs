// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LaunchDarklyFeatureFlagProvider.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Collector.Common.FeatureFlags.LaunchDarkly
{
    using System;
    using System.Collections.Generic;

    using Collector.Common.FeatureFlags.LaunchDarkly.DictionaryCache;

    using LaunchDarklyClient = global::LaunchDarkly.Client;

    using Newtonsoft.Json.Linq;

    public class LaunchDarklyFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly bool _cacheFlagValuesForDefaultUser;
        private readonly LaunchDarklyClient.ILdClient _ldClient;
        private readonly User _defaultUser;
        private readonly IDictionaryCache<object> _defaultUserFlagCache = new DictionaryCache<object>();

        public LaunchDarklyFeatureFlagProvider(LaunchDarklyClient.Configuration configuration, bool cacheFlagValuesForDefaultUser = false)
        {
            _cacheFlagValuesForDefaultUser = cacheFlagValuesForDefaultUser;
            _ldClient = new LaunchDarklyClient.LdClient(configuration);
            _defaultUser = new User("Default", "Default");
            _defaultUserFlagCache.LifeSpan = (uint)configuration.EventQueueFrequency.TotalSeconds;
        }

        public bool IsFeatureEnabled<T>(User user, bool defaultValue = false) where T : FeatureFlag<bool>, new()
        {
            var ldUser = CreateUser(user);
            return _ldClient.BoolVariation(new T().Keyname, ldUser, defaultValue);
        }

        public bool IsFeatureEnabled<T>(bool defaultValue = false) where T : FeatureFlag<bool>, new()
        {
            return GetFlagForDefaultUser<bool>(typeof(T).FullName, () => IsFeatureEnabled<T>(_defaultUser, defaultValue));
        }

        public bool GetBoolFlag<T>(User user, bool defaultValue = false) where T : FeatureFlag<bool>, new()
        {
            var ldUser = CreateUser(user);
            return _ldClient.BoolVariation(new T().Keyname, ldUser, defaultValue);
        }

        public bool GetBoolFlag<T>(bool defaultValue = false) where T : FeatureFlag<bool>, new()
        {
            return GetFlagForDefaultUser<bool>(typeof(T).FullName, () => GetBoolFlag<T>(_defaultUser, defaultValue));
        }

        public int GetIntFlag<T>(User user, int defaultValue) where T : FeatureFlag<int>, new()
        {
            var ldUser = CreateUser(user);
            return _ldClient.IntVariation(new T().Keyname, ldUser, defaultValue);
        }

        public int GetIntFlag<T>(int defaultValue) where T : FeatureFlag<int>, new()
        {
            return GetFlagForDefaultUser<int>(typeof(T).FullName, () => GetIntFlag<T>(_defaultUser, defaultValue));
        }

        public string GetStringFlag<T>(User user, string defaultValue) where T : FeatureFlag<string>, new()
        {
            var ldUser = CreateUser(user);
            return _ldClient.StringVariation(new T().Keyname, ldUser, defaultValue);
        }

        public string GetStringFlag<T>(string defaultValue) where T : FeatureFlag<string>, new()
        {
            return GetFlagForDefaultUser<string>(typeof(T).FullName, () => GetStringFlag<T>(_defaultUser, defaultValue));
        }

        private TReturn GetFlagForDefaultUser<TReturn>(string flagName, Func<object> loadFunc)
        {
            if (_cacheFlagValuesForDefaultUser)
            {
                return (TReturn)_defaultUserFlagCache.GetOrLoadNewObject(flagName, loadFunc);
            }

            return (TReturn)loadFunc();
        }

        private LaunchDarklyClient.User CreateUser(User user)
        {
            return new LaunchDarklyClient.User(user.Key)
                         {
                             Email = user.Email,
                             Country = user.CountryCode,
                             Custom = GetCustomAttributes(user.CustomAttributes)
                         };
        }

        private Dictionary<string, JToken> GetCustomAttributes(IDictionary<string, object> customParameters)
        {
            var customValues = new Dictionary<string, JToken>();

            if (customParameters == null)
                return customValues;

            foreach (var customParameter in customParameters)
            {
                customValues.Add(customParameter.Key, new JValue(customParameter.Value));
            }

            return customValues;
        }
    }
}