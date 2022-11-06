using Xunit;

namespace Gate.Tests;

public class BreakerTests {
    private Breaker<bool> _sut = new Breaker<bool>();

    [Fact]
    public async Task should_succeed() {
        var isTrue = await _sut.Execute(() => Task.FromResult(true));
        Assert.True(isTrue);
    } 
};