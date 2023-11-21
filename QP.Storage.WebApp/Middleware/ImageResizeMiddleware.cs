using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NLog;
using QP.Storage.WebApp.Settings;
using SixLabors.ImageSharp;


namespace QP.Storage.WebApp.Middleware;

public class ImageResizeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IFileProvider _fileProvider;
    private readonly ImageResizeSettings _settings;
    private readonly ImageProcessor _imageProcessor;
    private readonly string _rootFolder;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public ImageResizeMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv,
        IOptions<ImageResizeSettings> settings, ImageProcessor imageProcessor)
    {
        _next = next;
        _fileProvider = hostingEnv.WebRootFileProvider;
        _rootFolder = hostingEnv.WebRootPath;
        _settings = settings.Value;
        _imageProcessor = imageProcessor;
    }

    public async Task Invoke(HttpContext context)
    {
        if (_settings.IsResizeAllowed &&
            context.Request.Query.TryGetValue(_settings.WidthParameter, out var width) |
            context.Request.Query.TryGetValue(_settings.SearchParameter, out var size))
        {
            try
            {
                var original = context.Request.Path.Value;
                var originalFileInfo = _fileProvider.GetFileInfo(original);
                var reduceSize = await GetReduceSize(width, originalFileInfo, size);
                
                if (reduceSize == null && originalFileInfo.Exists)
                {
                    await WriteFileToResponse(context, originalFileInfo.PhysicalPath);
                    return;
                }

                var segments = Path.GetDirectoryName(original);
                var extension = Path.GetExtension(original);
                var extensions = _settings.ExtensionsAllowedToResize;

                if (reduceSize != null && 
                    !string.IsNullOrWhiteSpace(extension) &&
                    extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase) &&
                    segments != null)
                {
                    var name = Path.GetFileNameWithoutExtension(original);
                    var pathImage = GetExtractImagePath(name, reduceSize, extension, segments);
                    var fileInfo = _fileProvider.GetFileInfo(pathImage);
                    
                    if (fileInfo is {Exists: true})
                    {
                        await WriteFileToResponse(context, fileInfo.PhysicalPath);
                        return;
                    }

                    if (originalFileInfo.Exists)
                    {
                        var resultPath = await ResizeImage(originalFileInfo, reduceSize, pathImage);
                        await WriteFileToResponse(context, resultPath);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex);
            }
        }

        await _next(context);
    }

    private async Task<string> ResizeImage(IFileInfo originalFileInfo, ReduceSize reduceSize, string pathImage)
    {
        var resultPath = originalFileInfo.PhysicalPath;
        if (reduceSize.ReduceRatio > 0)
        {
            var resizedImagePath = $"{_rootFolder}{pathImage}";
            await _imageProcessor.ResizeImage(resultPath, reduceSize.ReduceRatio, resizedImagePath);
            _logger.Log(LogLevel.Info, $"Resizing image {resultPath} to {resizedImagePath} with ratio {reduceSize.ReduceRatio} completed.");
            resultPath = resizedImagePath;
        }

        return resultPath;
    }

    private string GetExtractImagePath(string name, ReduceSize reduceSize, string extension, string segments)
    {
        var resizedImageName = string.Format(
            _settings.ResizedImageTemplate,
            name,
            reduceSize.Postfix,
            extension
        );
        var pathImage = Path.Combine(segments, resizedImageName);
        return pathImage;
    }

    private static async Task WriteFileToResponse(HttpContext context, string resultPath)
    {
        var result = await File.ReadAllBytesAsync(resultPath);
        await context.Response.BodyWriter.WriteAsync(result);
    }

    private async Task<ReduceSize> GetReduceSize(StringValues width, IFileInfo originalFileInfo, StringValues size)
    {
        ReduceSize result = null;
        if (_settings.ReduceSizes != null)
        {
            if (!string.IsNullOrWhiteSpace(width))
            {
                if (originalFileInfo.Exists && decimal.TryParse(width, out var widthParsed))
                {
                    result = ChooseReduceSize(await GetRatio(originalFileInfo, widthParsed));
                }
            }
            else if (!string.IsNullOrWhiteSpace(size))
            {
                result = _settings.ReduceSizes.SingleOrDefault(
                    w => w.Postfix.Equals(size, StringComparison.InvariantCultureIgnoreCase)
                );
            }
        }
        return result;
    }

    private static async Task<decimal> GetRatio(IFileInfo originalFileInfo, decimal widthParsed)
    {
        using var image = await Image.LoadAsync(originalFileInfo.PhysicalPath);
        return image.Width / widthParsed;
    }

    private ReduceSize ChooseReduceSize(decimal ratio)
    {
        ReduceSize result = null;
        var closestDiff = Math.Abs(ratio - 1);
        foreach (var rs in _settings.ReduceSizes)
        {
            var diff = Math.Abs(rs.ReduceRatio - ratio);
            if (diff < closestDiff)
            {
                result = rs;
            }
        }
        return result;
    }
}