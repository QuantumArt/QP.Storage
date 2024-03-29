﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using QP.Storage.WebApp.Settings;

namespace QP.Storage.WebApp.Middleware
{
    public class FileSizeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFileProvider _fileProvider;
        private readonly FileSizeEndpointSettings _settings;

        public FileSizeMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv, IOptions<FileSizeEndpointSettings> settings)
        {
            _next = next;
            _fileProvider = hostingEnv.WebRootFileProvider;
            _settings = settings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var basePath = _settings.BasePath.StartsWith("/") ?
                _settings.BasePath :
                $"/{_settings.BasePath}";

            if (context.Request.Path.StartsWithSegments(basePath, out PathString subPath))
            {
                var fileInfo = _fileProvider.GetFileInfo(subPath.Value); 
                await context.Response.WriteAsync(fileInfo.Exists ? fileInfo.Length.ToString() : "0");
                return;
            }

            await _next(context);
        }
    }
}
