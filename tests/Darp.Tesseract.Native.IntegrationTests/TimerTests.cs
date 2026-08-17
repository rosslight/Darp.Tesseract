using Shouldly;
using Xunit;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class TimerTests
{
    [Fact]
    public void TimerCanBeStopped()
    {
        using var timer = new Timer();
        timer.stop();

    }

    [Fact]
    public async Task StopwatchStopsCorrectly()
    {
        using var stopwatch = new Stopwatch();

        var before = System.Diagnostics.Stopwatch.GetTimestamp();
        stopwatch.start();
        await Task.Delay(25, TestContext.Current.CancellationToken);
        stopwatch.stop();
        var duration = System.Diagnostics.Stopwatch.GetElapsedTime(before);
        var elapsedMilliseconds = stopwatch.elapsedMilliseconds();

        elapsedMilliseconds.ShouldBeInRange(25, duration.TotalMilliseconds);
    }
}