namespace xmltv_create_test;


// Parses default command line arguments correctly
using System;
using System.Threading.Tasks;
using Xunit;
using TvTv2XmlTv;

public class ProgramTests
{
    [Fact]
    public async Task parses_default_command_line_arguments_correctly()
    {
        // Arrange
        string[] args = new string[] { };

        // Act
        await Program.Main(args);

        // Assert
        // Since the default values are hardcoded, we can check if the file was created with the expected name
        Assert.True(System.IO.File.Exists("xmltv.xml"));
    }

    [Fact]
    public async Task handles_invalid_or_missing_command_line_arguments_gracefully()
    {
        // Arrange
        string[] args = new string[] { "--timezone=Invalid/Timezone", "--days=invalid" };

        // Act
        var exception = await Record.ExceptionAsync(() => Program.Main(args));

        // Assert
        Assert.Null(exception);
    }
}
