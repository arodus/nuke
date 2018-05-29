using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuke.Common.DI.Configuration;
using Nuke.Common.Execution;

namespace Nuke.Common.DI
{
    internal static class Startup
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public static IContainer Setup<T>()
        {
            Configuration = BuildConfiguration(new ConfigurationBuilder());
            var serviceCollection = ConfigureServiceCollection(new ServiceCollection());

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceCollection);
            containerBuilder.RegisterType<T>().As<NukeBuild>().SingleInstance();
            ConfigureContainer(containerBuilder);

            var container = containerBuilder.Build();
            return container;
        }

        private static IConfigurationRoot BuildConfiguration(IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder
                .AddEnvironmentVariables()
                .AddCommandLine(Environment.GetCommandLineArgs())
                .Build();
        }

        private static IServiceCollection ConfigureServiceCollection(IServiceCollection serviceCollection)
        {
            return serviceCollection.AddLogging(ConfigureLogging);
        }

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<InjectionService>().As<IInjectionService>().SingleInstance(); 
            builder.RegisterType<TargetDefinitionService>().As<ITargetDefinitionService>().SingleInstance();
            builder.RegisterType<RequirementService>().As<IRequirementService>().SingleInstance();
            builder.RegisterType<BuildExecutor>().As<IBuildExecutor>().SingleInstance();
            builder.RegisterType<GraphService>().As<IGraphService>().SingleInstance();
            builder.RegisterType<HelpTextService>().As<IHelpTextService>().SingleInstance();
            builder.RegisterType<EnvironmentInfo>().As<IEnvironmentInfo>().SingleInstance();
            builder.Register<ParameterService>(x => new ParameterService(
                    Configuration.GetChildren().ToDictionary(y => y.Key, y => y.Value),
                    Configuration
                        .Providers.OfType<CommandLineConfigurationProvider>()
                        .SingleOrDefault()?
                        .GetAvailableParameters()))
                .As<IParameterService>()
                .SingleInstance();

           

            builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                .AssignableTo<ITargetExecutor>()
                .As<ITargetExecutor>()
                .InstancePerLifetimeScope();
        }

        private static void ConfigureLogging(ILoggingBuilder builder)
        {
#if DEBUG
            var logLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
#else
            var logLevel = Microsoft.Extensions.Logging.LogLevel.Information;

#endif
            logLevel = Configuration.GetValue("LogLevel", logLevel);
            builder
                .AddConsole()
                .SetMinimumLevel(logLevel);
        }
    }
}
