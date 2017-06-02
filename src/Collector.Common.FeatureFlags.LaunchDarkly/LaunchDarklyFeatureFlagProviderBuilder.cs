using System;
using System.Configuration;
using System.IO;
using LaunchDarkly.Client;
using Configuration = LaunchDarkly.Client.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Collector.Common.FeatureFlags.LaunchDarkly
{
    public class LaunchDarklyFeatureFlagProviderBuilder
    {
        private const int MIN_POLLING_INTERVAL = 5;
        private const int DEFAULT_POLLING_INTERVAL = 60;

        private const int DEFAULT_REPORT_USAGE_INTERVAL = 2;

        private const int MIN_REPORT_USAGE_BUFFER = 100;
        private const int DEFAULT_REPORT_USAGE_BUFFER = 500;

        private const int MIN_REPORT_USAGE_INTERVAL = 1;
        private const int MAX_REPORT_USAGE_INTERVAL = 9;

        private readonly Configuration _configuration;
        private LaunchDarklyFeatureFlagProvider _singleton;
        private bool _cacheFlagValuesForDefaultUser;

        private LaunchDarklyFeatureFlagProviderBuilder(string sdkKey)
        {
            _configuration = Configuration.Default(sdkKey)
                .WithPollingInterval(TimeSpan.FromSeconds(DEFAULT_POLLING_INTERVAL))
                .WithEventQueueFrequency(TimeSpan.FromSeconds(DEFAULT_REPORT_USAGE_INTERVAL))
                .WithEventQueueCapacity(DEFAULT_REPORT_USAGE_BUFFER);
        }

        public static LaunchDarklyFeatureFlagProviderBuilder CreateWithKey(string sdkKey)
        {
            return new LaunchDarklyFeatureFlagProviderBuilder(sdkKey);
        }

        public static LaunchDarklyFeatureFlagProviderBuilder CreateWithLocalFile(string localKeyPath)
        {
            if (!File.Exists(localKeyPath))
            {
                throw new ConfigurationErrorsException($"To run locally you need to have a file '{localKeyPath}' that contains your LaunchDarkly SDK key.");
            }

            var sdkKey = File.ReadAllText(localKeyPath).Trim();

            return new LaunchDarklyFeatureFlagProviderBuilder(sdkKey);
        }

        public static LaunchDarklyFeatureFlagProviderBuilder CreateFromAppSettings()
        {
            LaunchDarklyFeatureFlagProviderBuilder builder;

            var sdkKey = GetValueFromAppSettingsKey<string>("LaunchDarkly:SdkKey");
            if (sdkKey == "LOCAL")
            {
                var localKeyPath = GetValueFromAppSettingsKey<string>("LaunchDarkly:LocalKeyPath");
                builder = CreateWithLocalFile(localKeyPath);
            }
            else
            {
                builder = CreateWithKey(sdkKey);
            }

            var reportUsageBuffer = GetValueFromAppSettingsKey<int>("LaunchDarkly:ReportUsageBufferSize");
            if (reportUsageBuffer > 0)
            {
                builder = builder.WithReportUsageBuffer(reportUsageBuffer);
            }

            var reportUsageInterval = GetValueFromAppSettingsKey<int>("LaunchDarkly:ReportUsageInterval");
            if (reportUsageInterval > 0)
            {
                builder = builder.WithReportUsageInterval(reportUsageBuffer);
            }

            var pollingIntervalSetting = GetValueFromAppSettingsKey<int>("LaunchDarkly:PollingIntervalSeconds");
            if (pollingIntervalSetting > 0)
            {
                builder = builder.WithPollingInterval(pollingIntervalSetting);
            }

            return builder;
        }

        public static LaunchDarklyFeatureFlagProviderBuilder CreateFromConfigSection(IConfigurationSection configurationSection)
        {
            LaunchDarklyFeatureFlagProviderBuilder builder;

            var sdkKey = configurationSection.GetValue<string>("SdkKey");
            if (sdkKey == "LOCAL")
            {
                var localKeyPath = configurationSection.GetValue<string>("LocalKeyPath");
                builder = CreateWithLocalFile(localKeyPath);
            }
            else
            {
                builder = CreateWithKey(sdkKey);
            }

            var reportUsageBuffer = configurationSection.GetValue<int>("ReportUsageBufferSize");
            if (reportUsageBuffer > 0)
            {
                builder = builder.WithReportUsageBuffer(reportUsageBuffer);
            }

            var reportUsageInterval = configurationSection.GetValue<int>("ReportUsageInterval");
            if (reportUsageInterval > 0)
            {
                builder = builder.WithReportUsageInterval(reportUsageBuffer);
            }

            var pollingIntervalSetting = configurationSection.GetValue<int>("PollingIntervalSeconds");
            if (pollingIntervalSetting > 0)
            {
                builder = builder.WithPollingInterval(pollingIntervalSetting);
            }

            return builder;
        }

        public IFeatureFlagProvider Build()
        {
            return _singleton ?? (_singleton = new LaunchDarklyFeatureFlagProvider(_configuration, _cacheFlagValuesForDefaultUser));
        }

        public LaunchDarklyFeatureFlagProviderBuilder WithPollingInterval(int seconds)
        {
            _configuration.WithPollingInterval(TimeSpan.FromSeconds(EnsureHealthyPollingInterval(seconds)));
            return this;
        }

        public LaunchDarklyFeatureFlagProviderBuilder WithCachingOfFlagValuesForDefaultUser()
        {
            _cacheFlagValuesForDefaultUser = true;
            return this;
        }

        private static int EnsureHealthyPollingInterval(int pollingInterval)
        {
            return Math.Max(pollingInterval, MIN_POLLING_INTERVAL);
        }

        public LaunchDarklyFeatureFlagProviderBuilder WithReportUsageInterval(int seconds)
        {
            _configuration.WithEventQueueFrequency(TimeSpan.FromSeconds(EnsureHealthyReportUsageInterval(seconds)));
            return this;
        }

        private static int EnsureHealthyReportUsageInterval(int interval)
        {
            var adjustedInterval = Math.Max(interval, MIN_REPORT_USAGE_INTERVAL);

            return Math.Min(adjustedInterval, MAX_REPORT_USAGE_INTERVAL);
        }

        public LaunchDarklyFeatureFlagProviderBuilder WithReportUsageBuffer(int bufferSize)
        {
            _configuration.WithEventQueueCapacity(EnsureHealthyReportUsageBuffer(bufferSize));
            return this;
        }

        private static int EnsureHealthyReportUsageBuffer(int buffer)
        {
            return Math.Max(buffer, MIN_REPORT_USAGE_BUFFER);
        }

        public LaunchDarklyFeatureFlagProviderBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            _configuration.WithLoggerFactory(loggerFactory);
            return this;
        }

        private static T GetValueFromAppSettingsKey<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var str = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrWhiteSpace(str))
            {
                str = str.Replace("&amp;", "&");
            }
            else
            {
                return default(T);
            }

            return (T)Convert.ChangeType(str, typeof(T));
        }

        private static string GetAndEnsureValueFromAppSettingsKey(string key)
        {
            var fromAppSettingsKey = GetValueFromAppSettingsKey<string>(key);

            if (fromAppSettingsKey != null)
            {
                return fromAppSettingsKey;
            }

            throw new KeyNotFoundException($"The key '{key}' is not found in the config file.");
        }

    }
}
