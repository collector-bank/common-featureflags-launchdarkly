// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LaunchDarklyFeatureFlagProvider.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Collector.Common.FeatureFlags.LaunchDarkly
{
    using System.Collections.Generic;

    using LaunchDarklyClient = global::LaunchDarkly.Client;

    using Newtonsoft.Json.Linq;

    public class LaunchDarklyFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly LaunchDarklyClient.ILdClient _ldClient;
        private readonly User _defaultUser;

        public LaunchDarklyFeatureFlagProvider(LaunchDarklyClient.Configuration configuration)
        {
            _ldClient = new LaunchDarklyClient.LdClient(configuration);
            _defaultUser = new User("Default", "Default");
        }

        public bool IsFeatureEnabled<T>(User user, bool defaultValue = false) where T : FeatureFlag<bool>, new()
        {
            var ldUser = CreateUser(user);
            return _ldClient.BoolVariation(new T().Keyname, ldUser, defaultValue);
        }

        public bool IsFeatureEnabled<T>(bool defaultValue = false) where T : FeatureFlag<bool>, new()
        {
            return IsFeatureEnabled<T>(_defaultUser, defaultValue);
        }

        public bool GetBoolFlag<T>(User user, bool defaultValue = false) where T : FeatureFlag<bool>, new()
        {
            var ldUser = CreateUser(user);
            return _ldClient.BoolVariation(new T().Keyname, ldUser, defaultValue);
        }

        public bool GetBoolFlag<T>(bool defaultValue = false) where T : FeatureFlag<bool>, new()
        {
           return GetBoolFlag<T>(_defaultUser, defaultValue);
        }

        public int GetIntFlag<T>(User user, int defaultValue) where T : FeatureFlag<int>, new()
        {
            var ldUser = CreateUser(user);
            return _ldClient.IntVariation(new T().Keyname, ldUser, defaultValue);
        }

        public int GetIntFlag<T>(int defaultValue) where T : FeatureFlag<int>, new()
        {
            return GetIntFlag<T>(_defaultUser, defaultValue);
        }

        public string GetStringFlag<T>(User user, string defaultValue) where T : FeatureFlag<string>, new()
        {
            var ldUser = CreateUser(user);
            return _ldClient.StringVariation(new T().Keyname, ldUser, defaultValue);
        }

        public string GetStringFlag<T>(string defaultValue) where T : FeatureFlag<string>, new()
        {
            return GetStringFlag<T>(_defaultUser, defaultValue);
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