using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using QP.Storage.WebApp.Settings;

namespace QP.Storage.WebApp.Middleware;

public class ImageResizeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IFileProvider _fileProvider;
    private readonly ImageResizeSettings _settings;
    private readonly ImageProcessor _imageImageProcessor;
    private readonly string _rootFolder;

    public ImageResizeMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv,
        IOptions<ImageResizeSettings> settings, ImageProcessor imageImageProcessor)
    {
        _next = next;
        _fileProvider = hostingEnv.WebRootFileProvider;
        _rootFolder = hostingEnv.WebRootPath;
        _settings = settings.Value;
        _imageImageProcessor = imageImageProcessor;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Query.TryGetValue(_settings.QueryParameter, out var size))
        {
            var original = context.Request.Path.Value;
            var segments = Path.GetDirectoryName(original);
            
            var name = Path.GetFileNameWithoutExtension(original);
            var extension = Path.GetExtension(original);

            if (!string.IsNullOrEmpty(extension) &&
                _settings.ExtensionsAllowedToResize.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
            {
                var resizedImageName = $"{name}-{size}{extension}";
                var pathImage = Path.Combine(segments, resizedImageName);
                
                var fileInfo = _fileProvider.GetFileInfo(pathImage);
                string resultPath;
                if (fileInfo.Exists)
                {
                    resultPath = fileInfo.PhysicalPath;

                    var result = File.ReadAllBytes(resultPath);
                    await context.Response.BodyWriter.WriteAsync(result);
                    return;
                }

                var originalFileInfo = _fileProvider.GetFileInfo(original);
                if (originalFileInfo.Exists)
                {
                    var ratio = _settings.ReduceSizes.SingleOrDefault(w =>
                            w.Postfix.Equals(size, StringComparison.InvariantCultureIgnoreCase))
                        ?.ReduceRatio;

                    if (ratio is > 0)
                    {
                        var resizedImagePath = $"{_rootFolder}{pathImage}";
                        _imageImageProcessor.ResizeImage(originalFileInfo.PhysicalPath, ratio.Value,
                            resizedImagePath);
                        
                        resultPath = resizedImagePath;
                    }
                    else
                    {
                        resultPath = originalFileInfo.PhysicalPath;
                    }

                    var result = await File.ReadAllBytesAsync(resultPath);
                    await context.Response.BodyWriter.WriteAsync(result);
                    return;
                }
            }
        }

        await _next(context);
    }
}