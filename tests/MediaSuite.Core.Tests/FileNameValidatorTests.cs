using MediaSuite.Core.FileSystem;
using Xunit;

namespace MediaSuite.Core.Tests;

public class FileNameValidatorTests
{
    [Theory]
    [InlineData("Sunday Service")]
    [InlineData("Example File - Page 1")]
    public void ValidNames_Pass(string name)
    {
        Assert.True(FileNameValidator.Validate(name).IsValid);
    }

    [Fact]
    public void EmptyName_Fails()
    {
        Assert.False(FileNameValidator.Validate("").IsValid);
    }

    [Theory]
    [InlineData("bad<name")]
    [InlineData("bad>name")]
    [InlineData("bad:name")]
    [InlineData("bad\"name")]
    [InlineData("bad/name")]
    [InlineData("bad\\name")]
    [InlineData("bad|name")]
    [InlineData("bad?name")]
    [InlineData("bad*name")]
    public void InvalidCharacters_Fail(string name)
    {
        Assert.False(FileNameValidator.Validate(name).IsValid);
    }

    [Fact]
    public void TrailingSpace_Fails()
    {
        Assert.False(FileNameValidator.Validate("Name ").IsValid);
    }

    [Fact]
    public void TrailingPeriod_Fails()
    {
        Assert.False(FileNameValidator.Validate("Name.").IsValid);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("con")]
    [InlineData("LPT1")]
    [InlineData("COM3")]
    public void ReservedDeviceNames_Fail(string name)
    {
        Assert.False(FileNameValidator.Validate(name).IsValid);
    }

    [Fact]
    public void NameOverMaxLength_Fails()
    {
        var name = new string('a', 101);

        Assert.False(FileNameValidator.Validate(name, maxLength: 100).IsValid);
    }

    [Fact]
    public void NameAtMaxLength_Passes()
    {
        var name = new string('a', 100);

        Assert.True(FileNameValidator.Validate(name, maxLength: 100).IsValid);
    }
}
