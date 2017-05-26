// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExampleFeatureFlag.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Example
{
    using Collector.Common.FeatureFlags;

    public class ExampleFeatureFlag : FeatureFlag<bool>
    {
        public override string Keyname => "my-feature-flag-key";
    }
}