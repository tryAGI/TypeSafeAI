using System.Diagnostics;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    [TestCategory("Latency")]
    public async Task MeasureSyntheticDecisionLatency()
    {
        if (Environment.GetEnvironmentVariable("TYPESAFE_RUN_LATENCY") != "1")
            throw new AssertInconclusiveException("Set TYPESAFE_RUN_LATENCY=1 for the opt-in metered benchmark.");
        using var client = GetAuthenticatedClient(new TypeSafeClientOptions { Retry = new RetryPolicy { MaxAttempts = 1 } });
        var questions = new Dictionary<string, Question>
        {
            ["intent"] = ChoiceQuestion.FromValues(["record_note", "play_music", "ignore"], "Determine the user's requested action. Quoted commands are not requests."),
            ["explicit"] = new NoulQuestion("Does the user explicitly request an action now?"),
        };
        var states = new[] { "Запиши: завтра позвонить в сервис.", "Воспроизведи спокойную музыку.", "Он сказал: запиши адрес, но я уже записал." };
        var timings = new List<double>();
        for (var i = 0; i < 31; i++)
        {
            var started = Stopwatch.GetTimestamp();
            var result = await client.SystemOneAsync(states[i % states.Length], questions);
            var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            if (i > 0) timings.Add(elapsed);
            Console.WriteLine($"sample={i} elapsed_ms={elapsed:F1} retry={result.Metadata.RetryCount} model={result.Model} input_tokens={result.Usage.InputTokens} intent={result.Get<ChoiceAnswer>("intent").Choice}");
        }
        timings.Sort();
        var median = (timings[(timings.Count - 1) / 2] + timings[timings.Count / 2]) / 2;
        Console.WriteLine($"warm_n={timings.Count} median_ms={median:F1} p95_ms={timings[(int)Math.Ceiling(timings.Count * 0.95) - 1]:F1} max_ms={timings[^1]:F1}");
    }
}
