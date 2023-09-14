using AspNetStatic;
using Sochys.Components;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents();
// .AddServerComponents(); 

bool isStatic = true;
if (isStatic)
{
    builder.Services.AddSingleton<IStaticPagesInfoProvider>(
    new StaticPagesInfoProvider(
    new PageInfo[]
    {
        new("/") { OutFile = "index.html" },
        new("/socha1") { OutFile = "socha1.html" }, new("/socha2") { OutFile = "socha2.html" },

    })
    );
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapRazorComponents<App>();
// .AddServerRenderMode();

string outputPath = @"output";
List<string> ignoredFiles = new() {"app.css"};

if (Directory.Exists(outputPath))
Directory.Delete(outputPath, true);
Directory.CreateDirectory(outputPath);
if (isStatic)
{
    app.GenerateStaticPages(outputPath,
    exitWhenDone: true,
    alwaysDefaultFile: false,
    dontUpdateLinks: true);



}

app.Run();

var searchPattern = "*.html";
var regexPattern = $"""<base\s+href=".*"\s*>""";
var newBase = "/SochyOutTest/";
string newBaseTag = $"""<base href="{newBase}">""";

// Get all .html files from the directory
var files = Directory.GetFiles(outputPath, searchPattern, SearchOption.AllDirectories);

foreach (var file in files)
{
    // Read file content
    var content = File.ReadAllText(file);

    // Replace base tag using regex
    if (Regex.IsMatch(content, regexPattern))
    {
        content = Regex.Replace(content, regexPattern, newBaseTag);
        File.WriteAllText(file, content);
        Console.WriteLine($"Updated: {file}");
    }
}

CopyDirectory("wwwroot", outputPath);

void CopyDirectory(string sourcePath, string destPath)
{
    if (!Directory.Exists(destPath))
    {
        Directory.CreateDirectory(destPath);
    }

    foreach (string file in Directory.GetFiles(sourcePath))
    {
        string filename = Path.GetFileName(file);
        if(ignoredFiles.Contains(filename)) continue;
        string dest = Path.Combine(destPath, filename);
        File.Copy(file, dest, true);
    }

    foreach (string folder in Directory.GetDirectories(sourcePath))
    {
        string dest = Path.Combine(destPath, Path.GetFileName(folder));
        CopyDirectory(folder, dest);
    }
}
