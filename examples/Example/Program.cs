// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Example
{
    using System;

    using Collector.Common.FeatureFlags;
    using Collector.Common.FeatureFlags.LaunchDarkly;

    using Microsoft.Extensions.Configuration;

    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var section = configuration.GetSection("LaunchDarkly");

            var provider = LaunchDarklyFeatureFlagProviderBuilder.CreateFromConfigSection(section).Build();
           
            var user = new User("my-team-name", "my-user-key")
                          .With("my-custom-attribute", "my-custom-value");

            var featureEnabledForDefaultUser = provider.IsFeatureEnabled<ExampleFeatureFlag>();
            Console.WriteLine($"Feature enabled for default user: {featureEnabledForDefaultUser}");

            var featureEnabledForSpecificUser = provider.IsFeatureEnabled<ExampleFeatureFlag>(user);
            Console.WriteLine($"Feature enabled for specific user: {featureEnabledForSpecificUser}");

            Console.ReadLine();
        }
    }
}
