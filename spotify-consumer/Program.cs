using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog;
using spotify_consumer;
using SpotifyAPI.Web;
using System.Net.Http.Headers;
using System.Text.Json;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSerilogElastichApi(builder.Configuration);

builder.Host.UseSerilog(Log.Logger);

builder.Logging.ClearProviders();

builder.Logging.AddConsole();


builder.Services.AddControllers();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class AccessTokenResponse
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
}