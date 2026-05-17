using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("RadioSpinner", client =>
{
    client.BaseAddress = new Uri("https://radiospinner.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

async Task<IResult> GetStations(IHttpClientFactory httpClientFactory, string path)
{
    var client = httpClientFactory.CreateClient("RadioSpinner");
    var html = await client.GetStringAsync(path);

    var stationRegex = new Regex(
        @"data-station-id=""(?<id>[^""]+)""\s+" +
        @"data-station-name=""(?<name>[^""]+)""\s+" +
        @"data-station-url=""(?<url>[^""]+)""\s+" +
        @"data-logo-url=""(?<logo>[^""]*)""\s+" +
        @"data-stream-url-1=""(?<stream>[^""]*)""",
        RegexOptions.Singleline);

    var stations = stationRegex.Matches(html)
        .Select(m => new
        {
            id = m.Groups["id"].Value,
            name = System.Net.WebUtility.HtmlDecode(m.Groups["name"].Value),
            logo = m.Groups["logo"].Value,
            stream = m.Groups["stream"].Value
        })
        .Where(s => !string.IsNullOrEmpty(s.stream))
        .ToList();

    return Results.Json(stations);
}

app.MapGet("/api/stations/egypt", (IHttpClientFactory f) => GetStations(f, "eg/"));
app.MapGet("/api/stations/rock", () =>
{
    var stations = new[]
    {
        new { id = "pink-floyd", name = "Exclusively Pink Floyd", logo = "https://exclusive.radio/img/logos/pinkfloyd.jpg", stream = "https://streaming.exclusive.radio/er/pinkfloyd/icecast.audio" },
        new { id = "metallica", name = "Exclusively Metallica", logo = "https://exclusive.radio/img/logos/metallica.jpg", stream = "https://nl4.mystreaming.net/er/metallica/icecast.audio" },
        new { id = "iron-maiden", name = "Exclusively Iron Maiden", logo = "https://exclusive.radio/img/logos/ironmaiden.jpg", stream = "https://nl4.mystreaming.net/er/ironmaiden/icecast.audio" },
        new { id = "dream-theater", name = "Dream Theater (Prog Rock Radio)", logo = "", stream = "https://s2.ssl-stream.com/radio/8180/radio.mp3" },
        new { id = "dire-straits", name = "Exclusively Dire Straits", logo = "https://exclusive.radio/img/logos/direstraits.jpg", stream = "https://streaming.exclusive.radio/er/direstraits/icecast.audio" },
        new { id = "acdc", name = "Exclusively AC/DC", logo = "https://exclusive.radio/img/logos/acdc.jpg", stream = "https://nl4.mystreaming.net/er/acdc/icecast.audio" },
        new { id = "savatage", name = "Savatage (Metal Detector)", logo = "", stream = "https://ice4.somafm.com/metal-128-aac" },
        new { id = "blackmores-night", name = "Blackmore's Night (Prog Rock)", logo = "", stream = "https://theproganrockmachine.out.airtime.pro/theproganrockmachine_b" },
    };

    return Results.Json(stations);
});

// SPA fallback: serve index.html for client-side routes
app.MapFallbackToFile("index.html");

app.Run();
