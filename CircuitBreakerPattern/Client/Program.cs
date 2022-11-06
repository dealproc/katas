var client = new HttpClient();
client.BaseAddress = new Uri("https://localhost:7121");
while(true){
    var response = await client.GetAsync("TimeIs");

    try {
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
    catch {
        Console.WriteLine($"Error Response: {response.StatusCode}");
    }

    await Task.Delay(TimeSpan.FromSeconds(2));
}