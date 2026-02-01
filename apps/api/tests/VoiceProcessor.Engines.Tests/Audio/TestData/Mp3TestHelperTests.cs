using FluentAssertions;

namespace VoiceProcessor.Engines.Tests.Audio.TestData;

public class Mp3TestHelperTests
{
    [Fact]
    public void CreateMinimalMp3_ShouldGenerateValidMp3()
    {
        var mp3Data = Mp3TestHelper.CreateMinimalMp3(1000);
        
        mp3Data.Should().NotBeNull();
        mp3Data.Length.Should().BeGreaterThan(0);
        Mp3TestHelper.IsValidMp3(mp3Data).Should().BeTrue();
    }
    
    [Fact]
    public void CreateMinimalMp3_ShouldBeUnder5KB()
    {
        var mp3Data = Mp3TestHelper.CreateMinimalMp3(1000);
        
        mp3Data.Length.Should().BeLessThan(5 * 1024);
    }
    
    [Fact]
    public void CreateShortMp3Chunk_ShouldGenerateValidMp3()
    {
        var mp3Data = Mp3TestHelper.CreateShortMp3Chunk();
        
        mp3Data.Should().NotBeNull();
        Mp3TestHelper.IsValidMp3(mp3Data).Should().BeTrue();
    }
    
    [Fact]
    public void IsValidMp3_ShouldReturnFalseForInvalidData()
    {
        var invalidData = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        
        Mp3TestHelper.IsValidMp3(invalidData).Should().BeFalse();
    }
    
    [Fact]
    public void IsValidMp3_ShouldReturnFalseForNull()
    {
        Mp3TestHelper.IsValidMp3(null!).Should().BeFalse();
    }
    
    [Fact]
    public void IsValidMp3_ShouldReturnFalseForEmptyArray()
    {
        Mp3TestHelper.IsValidMp3(Array.Empty<byte>()).Should().BeFalse();
    }
}
