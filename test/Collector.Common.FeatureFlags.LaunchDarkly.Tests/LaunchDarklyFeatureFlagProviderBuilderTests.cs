#if NET452
using System.Configuration;
using System.Collections.Generic;
#elif NETCOREAPP2_0
using Microsoft.Extensions.Configuration;
#endif

using NUnit.Framework;

namespace Collector.Common.FeatureFlags.LaunchDarkly.Tests
{
    [TestFixture]
    public class LaunchDarklyFeatureFlagProviderBuilderTests
    {

#if NET452
        [Test]
        public void CreateFromAppSettings_WithAppConfig_ShouldReturnProvider()
        {
            var provider = LaunchDarklyFeatureFlagProviderBuilder.CreateFromAppSettings().Build();

            Assert.NotNull(provider);
        }

#elif NETCOREAPP2_0
        [Test]
        public void CreateFromConfigSection_WithValidSection_ShouldReturnProvider()
        {
            var configuration = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json")
                 .Build();

            var section = configuration.GetSection("LaunchDarkly");

            var provider = LaunchDarklyFeatureFlagProviderBuilder.CreateFromConfigSection(section).Build();

            Assert.NotNull(provider);
        }
#endif
    }
}
