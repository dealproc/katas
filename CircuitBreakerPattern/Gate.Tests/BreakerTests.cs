using Xunit;

namespace Gate.Tests;

public class BreakerTests {
    private Breaker _sut = new Breaker();

    [Fact]
    public async Task should_succeed() {
        var isTrue = await _sut.Execute(() => Task.FromResult(true));
        Assert.True(isTrue);
    } 
};