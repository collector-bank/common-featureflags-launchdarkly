// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Example.Framework
{
    using System;

    using Collector.Common.FeatureFlags;
    using Collector.Common.FeatureFlags.LaunchDarkly;

    class Program
    {
        static void Main(string[] args)
        {
            var provider = LaunchDarklyFeatureFlagProviderBuilder.CreateFromAppSettings().Build();
           
            var user = new User("my-team-name", "my-user-key")
                          .With("my-custom-attribute", "my-custom-value");

            Console.WriteLine("Press enter to check feature status, 'q' to quit\n");

            var keepRunning = true;
            while (keepRunning)
            {
                var featureEnabledForDefaultUser = provider.IsFeatureEnabled<ExampleFeatureFlag>();
                Console.WriteLine($"Feature enabled for default user: {featureEnabledForDefaultUser}");

                var featureEnabledForSpecificUser = provider.IsFeatureEnabled<ExampleFeatureFlag>(user);
                Console.WriteLine($"Feature enabled for specific user: {featureEnabledForSpecificUser}");

                Console.Write(">");
                var input = Console.ReadLine();

                if (input == null || input.ToLower() == "q")
                    keepRunning = false;
            }
        }
    }
}
