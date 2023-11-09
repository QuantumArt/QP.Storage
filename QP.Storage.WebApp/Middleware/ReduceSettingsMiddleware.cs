using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using QP.Storage.WebApp.Settings;

namespace QP.Storage.WebApp.Middleware;

public class ReduceSettingsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ImageResizeSettings _settings;
   

    public ReduceSettingsMiddleware(RequestDelegate next, IOptions<ImageResizeSettings> settings)
    {
        _next = next;
        _settings = settings.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        var basePath = _settings.BasePath.StartsWith("/") ?
            _settings.BasePath :
            $"/{_settings.BasePath}";

        if (context.Request.Path.StartsWithSegments(basePath) && _settings.ReduceSizes.Length > 0)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(_settings, serializeOptions));
            return;
        }

        await _next(context);
    }
}