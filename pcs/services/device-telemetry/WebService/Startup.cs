// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.Auth;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics.ILogger;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService
{
    public class Startup
    {
        // Initialized in `Startup`
        public IConfigurationRoot Configuration { get; }

        // Initialized in `ConfigureServices`
        public IContainer ApplicationContainer { get; private set; }

        private ActionsAgent.IAgent actionsAgent;
        private readonly CancellationTokenSource agentsRunState;

        // Invoked by `Program.cs`
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddIniFile("appsettings.ini", optional: false, reloadOnChange: true);
            this.Configuration = builder.Build();
            this.agentsRunState = new CancellationTokenSource();
        }

        // This is where you register dependencies, add services to the
        // container. This method is called by the runtime, before the
        // Configure method below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Setup (not enabling yet) CORS
            services.AddCors();

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();

            // Prepare DI container
            this.ApplicationContainer = DependencyResolution.Setup(services);

            // Print some useful information at bootstrap time
            this.PrintBootstrapInfo(this.ApplicationContainer);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method is called by the runtime, after the ConfigureServices
        // method above. Use this method to add middleware.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            ICorsSetup corsSetup,
            IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));

            // Check for Authorization header before dispatching requests
            app.UseMiddleware<AuthMiddleware>();

            // Enable CORS - Must be before UseMvc
            // see: https://docs.microsoft.com/en-us/aspnet/core/security/cors
            corsSetup.UseMiddleware(app);

            app.UseMvc();

            // Start agent threads
            appLifetime.ApplicationStarted.Register(this.StartAgents);
            appLifetime.ApplicationStopping.Register(this.StopAgents);

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(() => this.ApplicationContainer.Dispose());
        }

        private void PrintBootstrapInfo(IContainer container)
        {
            var log = container.Resolve<ILogger>();
            log.Info("Web service started", () => new { Uptime.ProcessId });
        }

        private void StartAgents()
        {
            this.actionsAgent = this.ApplicationContainer.Resolve<ActionsAgent.IAgent>();
            this.actionsAgent.RunAsync(this.agentsRunState.Token);
        }

        private void StopAgents()
        {
            this.agentsRunState.Cancel();
        }
    }
}