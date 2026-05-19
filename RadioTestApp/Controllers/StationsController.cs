using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RadioTestApp.Controllers;

public record StationEntry(string Id, string Name, string Logo, string Stream);

[ApiController]
[Route("api/stations")]
public class StationsController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [HttpGet("egypt")]
    public async Task<IActionResult> GetEgyptStations()
    {
        var stations = await GetStationsFromRadioSpinner("eg/");
        return Ok(stations);
    }

    [HttpGet("rock")]
    public IActionResult GetRockStations() => Ok(LoadStationsFromFile("Rock"));

    [HttpGet("classic")]
    public IActionResult GetClassicStations() => Ok(LoadStationsFromFile("Classic"));

    private List<StationEntry> LoadStationsFromFile(string category)
    {
        var path = Path.Combine(env.ContentRootPath, "stations.json");
        var json = System.IO.File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<Dictionary<string, List<StationEntry>>>(json, JsonOptions);
        return config?.GetValueOrDefault(category) ?? [];
    }

    private async Task<object[]> GetStationsFromRadioSpinner(string path)
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

        return stationRegex.Matches(html)
            .Select(m => (object)new
            {
                id = m.Groups["id"].Value,
                name = System.Net.WebUtility.HtmlDecode(m.Groups["name"].Value),
                logo = m.Groups["logo"].Value,
                stream = m.Groups["stream"].Value
            })
            .Where(s => !string.IsNullOrEmpty(((dynamic)s).stream))
            .ToArray();
    }
}
