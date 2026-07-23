using RevitMcp.Core.Config;

namespace RevitMcp.Tests;

public sealed class RevitMcpSettingsValidationTests
{
    [Fact]
    public void Validate_DefaultSettings_ReturnsNoErrors()
    {
        var settings = new RevitMcpSettings();

        var errors = settings.Validate();

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_InvalidPort_ReturnsError()
    {
        var settings = new RevitMcpSettings
        {
            Server = new ServerSettings
            {
                Port = 80
            }
        };

        var errors = settings.Validate();

        Assert.Contains(errors, e => e.Contains("port", StringComparison.OrdinalIgnoreCase));
    }
}
