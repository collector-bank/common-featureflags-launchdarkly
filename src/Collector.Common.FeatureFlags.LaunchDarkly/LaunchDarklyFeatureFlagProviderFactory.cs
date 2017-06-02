namespace Collector.Common.FeatureFlags.LaunchDarkly
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;

    using global::LaunchDarkly.Client;

    using Microsoft.Extensions.Logging;

    using Configuration = global::LaunchDarkly.Client.Configuration;

    public class LaunchDarklyFeatureFlagProviderFactory
    {
        private const int DEFAULT_POLLING_INTERVAL = 60;
        private const int DEFAULT_REPORT_USAGE_INTERVAL = 2;
        private const int DEFAULT_REPORT_USAGE_BUFFER = 500;
        private const int MAX_REPORT_USAGE_INTERVAL = 9;
        private static LaunchDarklyFeatureFlagProvider launchDarklyFeatureFlagProvider;

        public LaunchDarklyFeatureFlagProvider Create(string localKeyPath = null, ILoggerFactory loggerFactory = null, bool cacheFlagValuesForDefaultUser = false)
        {
            switch (GetValueFromAppSettingsKey("LaunchDarkly:Strategy")?.ToUpper())
            {
                case "STATIC":
                    return launchDarklyFeatureFlagProvider ?? (launchDarklyFeatureFlagProvider = CreateNew(localKeyPath, loggerFactory, cacheFlagValuesForDefaultUser));
                default:
                    return CreateNew(localKeyPath, loggerFactory, cacheFlagValuesForDefaultUser);
            }
        }

        private LaunchDarklyFeatureFlagProvider CreateNew(string localKeyPath, ILoggerFactory loggerFactory, bool cacheFlagValuesForDefaultUser)
        {
            var sdkKey = GetAndEnsureValueFromAppSettingsKey("LaunchDarkly:SdkKey");

            if (sdkKey.ToUpper() == "LOCAL")
            {
                if (localKeyPath == null)
                    localKeyPath = GetValueFromAppSettingsKey("LaunchDarkly:LocalKeyPath");

                if (localKeyPath == null)
                    throw new ArgumentNullException(nameof(localKeyPath), message: "To run locally you need to provide a localKeyPath, either by parameter or by value in config LaunchDarkly:LocalKeyPath");

                if (!File.Exists(localKeyPath))
                    throw new ConfigurationErrorsException($"To run locally you need to have a file '{localKeyPath}' that contains your LaunchDarkly SDK key.");

                sdkKey = File.ReadAllText(localKeyPath).Trim();
            }

            var pollingInterval = GetPollingInterval();

            var configuration = Configuration.Default(sdkKey)
                                             .WithPollingInterval(TimeSpan.FromSeconds(pollingInterval))
                                             .WithEventQueueFrequency(TimeSpan.FromSeconds(GetReportUsageFrequency()))
                                             .WithEventQueueCapacity(GetReportUsageBuffer());
            if (loggerFactory != null)
                configuration = configuration.WithLoggerFactory(loggerFactory);

            return new LaunchDarklyFeatureFlagProvider(configuration, cacheFlagValuesForDefaultUser);
        }

        private int GetPollingInterval()
        {
            var pollingIntervalSetting = GetValueFromAppSettingsKey("LaunchDarkly:PollingIntervalSeconds");
            int pollingInterval;
            if (!int.TryParse(pollingIntervalSetting, out pollingInterval))
                pollingInterval = DEFAULT_POLLING_INTERVAL;

            return EnsureHealthyPollingInterval(pollingInterval);
        }

        private static int EnsureHealthyPollingInterval(int pollingInterval)
        {
            return Math.Max(pollingInterval, 5);
        }

        private int GetReportUsageFrequency()
        {
            var reportUsageInterval = GetValueFromAppSettingsKey("LaunchDarkly:ReportUsageInterval");
            int interval;
            if (!int.TryParse(reportUsageInterval, out interval))
                interval = DEFAULT_REPORT_USAGE_INTERVAL;

            return EnsureHealthyReportUsageInterval(interval);
        }

        private static int EnsureHealthyReportUsageInterval(int interval)
        {
            var adjustedInterval = Math.Max(interval, 1);

            return Math.Min(adjustedInterval, MAX_REPORT_USAGE_INTERVAL);
        }

        private int GetReportUsageBuffer()
        {
            var reportUsageBuffer = GetValueFromAppSettingsKey("LaunchDarkly:ReportUsageBufferSize");
            int buffer;
            if (!int.TryParse(reportUsageBuffer, out buffer))
                buffer = DEFAULT_REPORT_USAGE_BUFFER;

            return EnsureHealthyReportUsageBuffer(buffer);
        }

        private static int EnsureHealthyReportUsageBuffer(int buffer)
        {
            return Math.Max(buffer, 100);
        }

        private string GetValueFromAppSettingsKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var str = ConfigurationManager.AppSettings[key];

            if (!string.IsNullOrWhiteSpace(str))
                str = str.Replace("&amp;", "&");

            return str;
        }

        private string GetAndEnsureValueFromAppSettingsKey(string key)
        {
            var fromAppSettingsKey = GetValueFromAppSettingsKey(key);

            if (fromAppSettingsKey != null)
                return fromAppSettingsKey;

            throw new KeyNotFoundException($"The key '{key}' is not found in the config file.");
        }
    }
}