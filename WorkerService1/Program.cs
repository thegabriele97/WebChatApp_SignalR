using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.IO;
using WorkerService1.Hub;
using Microsoft.Extensions.FileProviders;

namespace WorkerService1 {
    public class Program {
        public static void Main(string[] args) {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { //configuring web host
                    webBuilder
                        .UseUrls("http://192.168.1.113:80") //binding urls for web server
                        .UseKestrel() //webserver to use
                        .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
                        .ConfigureLogging(logger => {
                            logger.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                            logger.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                        })
                        .Configure(app => { //configuration for kestrel
                            app.UseCors("CorsPolicy"); //Policy for cross-domain requests
                            app.UseRouting();
                            app.UseStaticFiles(new StaticFileOptions {
                                FileProvider = new PhysicalFileProvider(
                                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
                                });
                            app.UseEndpoints(conf => {
                                conf.MapRazorPages();
                                conf.MapHub<MainHub>("/mainHub"); //hub listening on http://localhost/mainHub
                            });
                        })
                        .ConfigureServices(serviceConfig => {
                            serviceConfig.AddSignalR(); //adding supporto to SignalR
                            serviceConfig.AddRazorPages(); //adding support to Razor Pages
                            serviceConfig.AddCors(options => 
                                options.AddPolicy("CorsPolicy", builder => { //adding support for cross-domain requests
                                    builder
                                        .AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .WithOrigins("http://localhost:80") //requests for cross-domain only from this origin
                                        .AllowCredentials();
                                }));
                        });
                })
                .ConfigureServices((hostContext, services) => {
                    services.AddHostedService<Worker>();
                });
    }
}
