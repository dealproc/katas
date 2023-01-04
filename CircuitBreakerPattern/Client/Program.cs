var client = new HttpClient();
client.BaseAddress = new Uri("https://localhost:7121");
while(true) {
    HttpResponseMessage response = default;

    try {
        response = await client.GetAsync("TimeIs").TimeoutAfter(TimeSpan.FromSeconds(1));
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
    catch {
        Console.WriteLine($"Error Response: {response?.StatusCode}");
    }

    await Task.Delay(TimeSpan.FromSeconds(2));
}

static class Extensions {
    public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout, CancellationTokenSource cancellationTokenSource = default) {
        if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            return await task;
        else {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            throw new TimeoutException();
        }
    }
}