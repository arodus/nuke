using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Nuke.Common.DI.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, string[] args)
        {
            builder.Add(new CommandLineConfigurationSource { Args = args });
            return builder;
        }

        public static IConfigurationBuilder AddCommandLine(this IConfigurationBuilder builder, Action<CommandLineConfigurationSource> configureSource)
        {
            builder.Add(configureSource);
            return builder;
        }
    }
}
