using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using wordman.SQLite;

namespace wordman
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
                options.EnableForHttps = true;
            });

            services
                .AddMvc()
                .AddRazorOptions(options =>
            {
                options.ViewLocationFormats.Clear();
                options.ViewLocationFormats.Add("Views/themes/bootstrap/{1}/{0}.cshtml");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            using (var context = new WordContext())
            {
                context.Database.Migrate();
            }

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages(async ctx =>
            {
                ctx.HttpContext.Response.ContentType = "text/html";
                int statusCode = ctx.HttpContext.Response.StatusCode;
                string statusDescription = ReasonPhrases.GetReasonPhrase(statusCode);
                await ctx.HttpContext.Response.WriteAsync("<html lang=\"ko\">");
                await ctx.HttpContext.Response.WriteAsync("<head><style>body {text-align:center;} .desc{font-size:12pt;}</style></head>");
                await ctx.HttpContext.Response.WriteAsync($"<body>\r\n<h1>{statusCode} {statusDescription}</h1>\r\n");
                await ctx.HttpContext.Response.WriteAsync("<hr>\r\n");
                await ctx.HttpContext.Response.WriteAsync($"<span class=\"desc\">{App.Version}</span>\r\n");
                await ctx.HttpContext.Response.WriteAsync("</body></html>");
            });

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{action=index}", defaults: new { controller = "main" });
            });
        }
    }
}
