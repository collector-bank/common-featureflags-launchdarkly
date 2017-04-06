using Microsoft.Extensions.Configuration;
using Xunit;

namespace Collector.Common.FeatureFlags.LaunchDarkly.Tests
{
    public class LaunchDarklyFeatureFlagProviderBuilderTests
    {
        [Fact]
        public void CreateFromAppSettings_WithValidSection_ShouldReturnProvider()
        {
            var configuration = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json")
                 .Build();

            var section = configuration.GetSection("LaunchDarkly");

            var provider = LaunchDarklyFeatureFlagProviderBuilder.CreateFromConfigSection(section).Build();

            Assert.NotNull(provider);
        }
    }
}
