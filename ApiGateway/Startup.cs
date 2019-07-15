using System;
using System.Collections.Generic;
using System.Threading;
using Bugsnag;
using Bugsnag.AspNet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SharedArea.Utils;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace ApiGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Action<object> signalRPipe)
        {
            Configuration = configuration;
            this._signalRPipe = signalRPipe;
        }

        public IConfiguration Configuration { get; }
        private Action<object> _signalRPipe;

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddJsonOptions(options => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);
            if (!string.IsNullOrEmpty(Variables.BugSnagToken))
            {
                services.AddBugsnag(configuration => { configuration.ApiKey = Variables.BugSnagToken; });
            }
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = 4294967296;
            });
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
            }).AddJsonProtocol((options) =>
            {
                options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.PayloadSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
        }

        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseMvc();
            
            Logger.Setup();
            
            var configs = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>()
                {
                    { "bootstrap.servers" ,  Variables.PairedPeerAddress },
                    { "username" ,  SharedArea.GlobalVariables.KafkaUsername },
                    { "password" ,  SharedArea.GlobalVariables.KafkaPassword },
                }
            };
            
            if (string.IsNullOrEmpty(Variables.BugSnagToken))
            {
                KafkaGatewayExtension.SetupGatewayConsumer<Consumer, KafkaTransport>(configs, _signalRPipe);
            }
            else
            {
                KafkaGatewayExtension.SetupGatewayConsumer<Consumer, KafkaTransport>(configs,
                    new Bugsnag.Client(new Configuration(Variables.BugSnagToken)), _signalRPipe);   
            }
            
            Console.WriteLine("Bus loaded");
        }
    }
}