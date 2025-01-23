using Howzit.ParseModels;
using Parse.Infrastructure;
using Parse;

var builder = WebApplication.CreateBuilder(args);

// Create and configure ParseClient
var parseClient = new ParseClient(new ServerConnectionData
{
    ApplicationID = "xxxxxxxxxxxxxxx", // Replace with your Back4App App ID
    ServerURI = "https://parseapi.back4app.com/", // Replace with Back4App's Server URI
    Key = "xxxxxxxxxxxxx" // Replace with your Back4App Client Key
});
parseClient.Publicize(); // Makes ParseClient globally accessible  



// Register custom ParseObject subclasses
parseClient.RegisterSubclass(typeof(HowzitPost));
parseClient.RegisterSubclass(typeof(CommentPost));

// Add ParseClient to the DI container
builder.Services.AddSingleton(parseClient);

builder.Services.AddHttpClient();

// Add Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
