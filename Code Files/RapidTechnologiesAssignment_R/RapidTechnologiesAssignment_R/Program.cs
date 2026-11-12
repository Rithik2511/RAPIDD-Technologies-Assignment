
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();

// Register HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//Serve static files embedded inside the executable
var embeddedProvider = new ManifestEmbeddedFileProvider(
    Assembly.GetExecutingAssembly(),
    "wwwroot"
);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = embeddedProvider
});

// Configure MVC routing
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Configure local hosting
const string applicationUrl = "http://127.0.0.1:5000";

app.Urls.Clear();
app.Urls.Add(applicationUrl);

// Automatically open the browser
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = applicationUrl,
            UseShellExecute = true
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unable to open browser: {ex.Message}");
    }
});

// Start the web server
app.Run();
