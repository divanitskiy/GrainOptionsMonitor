using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using TestSilo.Grains;
using Xunit;

namespace GrainOptionsMonitor.Tests.ConfigurationGrain
{
    public class ConfigurationGrainTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ConfigurationGrainTests(CustomWebApplicationFactory factory) =>
            _factory = factory;

        [Fact]
        public async Task ConfigurationGrain_GetValue_ShouldReturnUpdatedValue()
        {
            // Arrange
            const int expectedNumber = 23;
            const string expectedString = "Zion";

            var clusterClient = _factory.Services.GetService<IClusterClient>();
            var configurationGrain = clusterClient.GetGrain<IConfigurationGrain>(0);

            // Act
            await configurationGrain.SetValue(new ConfigurationGrainState
            {
                Number = expectedNumber,
                String = expectedString
            });

            var configuration = await configurationGrain.GetValue();

            // Assert
            Assert.Equal(expectedNumber, configuration.Number);
            Assert.Equal(expectedString, configuration.String);
        }

        [Fact]
        public async Task OptionsMonitor_CurrentValue_ShouldReturnUpdatedValue()
        {
            // Arrange
            const int expectedNumber = 23;
            const string expectedString = "Zion";

            var clusterClient = _factory.Services.GetService<IClusterClient>();
            var configurationGrain = clusterClient.GetGrain<IConfigurationGrain>(0);

            await configurationGrain.SetValue(new ConfigurationGrainState
            {
                Number = 55,
                String = "123 asd"
            });

            // Act
            await configurationGrain.SetValue(new ConfigurationGrainState
            {
                Number = expectedNumber,
                String = expectedString
            });

            // wait some time until options monitor is updated
            await Task.Delay(TimeSpan.FromSeconds(15));

            var optionsMonitor = _factory.Services.GetService<IOptionsMonitor<ConfigurationGrainState>>();
            var configuration = optionsMonitor.CurrentValue;

            // Assert
            Assert.Equal(expectedNumber, configuration.Number);
            Assert.Equal(expectedString, configuration.String);
        }

        [Fact]
        public async Task OptionsMonitor_OnChange_ShouldReturnUpdatedValue()
        {
            // Arrange
            const int expectedNumber = 23;
            const string expectedString = "Zion";

            var clusterClient = _factory.Services.GetService<IClusterClient>();
            var configurationGrain = clusterClient.GetGrain<IConfigurationGrain>(0);

            await configurationGrain.SetValue(new ConfigurationGrainState
            {
                Number = 55,
                String = "123 asd"
            });

            // Act
            await configurationGrain.SetValue(new ConfigurationGrainState
            {
                Number = expectedNumber,
                String = expectedString
            });

            var optionsMonitor = _factory.Services.GetService<IOptionsMonitor<ConfigurationGrainState>>();
            
            // Assert
            optionsMonitor.OnChange(configuration =>
            {
                Assert.Equal(expectedNumber, configuration.Number);
                Assert.Equal(expectedString, configuration.String);
            });

            // wait some time until options monitor is updated
            await Task.Delay(TimeSpan.FromSeconds(15));
        }
    }
}
