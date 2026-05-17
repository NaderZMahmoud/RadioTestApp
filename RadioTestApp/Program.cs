using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("RadioSpinner", client =>
{
    client.BaseAddress = new Uri("https://radiospinner.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
});
builder.Services.AddHttpClient("RadioBrowser", client =>
{
    client.BaseAddress = new Uri("https://de1.api.radio-browser.info/");
    client.DefaultRequestHeaders.Add("User-Agent", "RadioTestApp/1.0");
});

var app = builder.Build();

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

app.MapGet("/", () => Results.Content(BuildPage("egypt", "🇪🇬 Egyptian Radio", "/api/stations/egypt"), "text/html"));
app.MapGet("/rock", () => Results.Content(BuildPage("rock", "🎸 Rock Radio", "/api/stations/rock"), "text/html"));

app.Run();

static string BuildPage(string activeTab, string title, string apiUrl)
{
    return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>{{title}}</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: system-ui, sans-serif; background: #1a1a2e; color: #eee; min-height: 100vh; display: flex; flex-direction: column; align-items: center; padding: 0; }
        nav { width: 100%; background: #16213e; display: flex; justify-content: center; gap: 0; padding: 0; position: sticky; top: 0; z-index: 20; }
        nav a { color: #aaa; text-decoration: none; padding: 1rem 2rem; font-weight: bold; transition: color 0.2s, border-color 0.2s; border-bottom: 3px solid transparent; }
        nav a:hover { color: #fff; }
        nav a.active { color: #e94560; border-bottom-color: #e94560; }
        .content { padding: 2rem; width: 100%; display: flex; flex-direction: column; align-items: center; }
        h1 { margin-bottom: 1.5rem; color: #e94560; }
        #player-bar { position: sticky; top: 52px; z-index: 10; background: #16213e; padding: 1rem; border-radius: 8px; margin-bottom: 1.5rem; width: 100%; max-width: 900px; text-align: center; }
        #now-playing { margin-bottom: 0.5rem; font-weight: bold; }
        audio { width: 100%; }
        .station-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 1rem; width: 100%; max-width: 900px; list-style: none; }
        .station-grid li { background: #16213e; border-radius: 8px; cursor: pointer; display: flex; flex-direction: column; align-items: center; justify-content: center; aspect-ratio: 1; padding: 0.75rem; text-align: center; transition: background 0.2s, transform 0.2s; }
        .station-grid li:hover { background: #0f3460; transform: scale(1.05); }
        .station-grid li.active { outline: 3px solid #e94560; }
        .station-grid img { width: 64px; height: 64px; border-radius: 8px; object-fit: cover; background: #333; margin-bottom: 0.5rem; }
        .station-grid .placeholder { width: 64px; height: 64px; border-radius: 8px; background: #333; margin-bottom: 0.5rem; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; }
        .station-grid span { font-size: 0.75rem; overflow: hidden; text-overflow: ellipsis; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }
        .loading { color: #aaa; grid-column: span 5; text-align: center; }
    </style>
</head>
<body>
    <nav>
        <a href="/" class="{{(activeTab == "egypt" ? "active" : "")}}">🇪🇬 Egyptian</a>
        <a href="/rock" class="{{(activeTab == "rock" ? "active" : "")}}">🎸 Rock</a>
    </nav>
    <div class="content">
        <h1>{{title}}</h1>
        <div id="player-bar">
            <div id="now-playing">Select a station to play</div>
            <audio id="audio" controls></audio>
        </div>
        <ul class="station-grid" id="stations"><li class="loading">Loading stations...</li></ul>
    </div>
    <script>
        const audio = document.getElementById('audio');
        const nowPlaying = document.getElementById('now-playing');
        const list = document.getElementById('stations');

        fetch('{{apiUrl}}')
            .then(r => r.json())
            .then(stations => {
                list.innerHTML = '';
                stations.forEach(s => {
                    const li = document.createElement('li');
                    const imgHtml = s.logo
                        ? `<img src="${s.logo}" onerror="this.outerHTML='<div class=\\'placeholder\\'>📻</div>'" alt=""/>`
                        : `<div class="placeholder">📻</div>`;
                    li.innerHTML = `${imgHtml}<span>${s.name}</span>`;
                    li.addEventListener('click', () => {
                        document.querySelectorAll('.station-grid li').forEach(el => el.classList.remove('active'));
                        li.classList.add('active');
                        audio.src = s.stream;
                        audio.play();
                        nowPlaying.textContent = '▶ ' + s.name;
                    });
                    list.appendChild(li);
                });
            })
            .catch(() => { list.innerHTML = '<li class="loading">Failed to load stations.</li>'; });
    </script>
</body>
</html>
""";
}

record RadioBrowserStation(string stationuuid, string name, string url_resolved, string? favicon);
