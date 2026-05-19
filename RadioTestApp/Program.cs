var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpClient("RadioSpinner", client =>
{
    client.BaseAddress = new Uri("https://radiospinner.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// SPA fallback: serve index.html for client-side routes
app.MapFallbackToFile("index.html");

app.Run();
