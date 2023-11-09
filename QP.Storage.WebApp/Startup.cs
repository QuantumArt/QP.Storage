using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QP.Storage.WebApp.Middleware;
using QP.Storage.WebApp.Settings;

namespace QP.Storage.WebApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FileSizeEndpointSettings>(Configuration.GetSection("FileSizeEndpointSettings"));
            services.Configure<ImageResizeSettings>(Configuration.GetSection("ImageResizeSettings"));

            services.AddTransient<ImageProcessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMiddleware<FileSizeMiddleware>();
            app.UseMiddleware<ReduceSettingsMiddleware>();
            app.UseMiddleware<ImageResizeMiddleware>();

            app.UseStaticFiles();
        }
    }
}
