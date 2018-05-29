using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Nuke.Common.Utilities;

namespace Nuke.Common.DI.Configuration
{
    public class CommandLineConfigurationProvider : ConfigurationProvider
    {
        /// <summary>
        /// The command line arguments.
        /// </summary>
        protected IEnumerable<string> Args { get; private set; }

        public CommandLineConfigurationProvider(string[] args)
        {
            Args = args.NotNull();

        }

        public override void Load()
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var current = string.Empty;
            var values = new List<string>();
            foreach (var commandlineArg in Args)
            {
                if (commandlineArg.StartsWith("-"))
                {
                    data.Add(current, values.Join(separator: ' '));
                    current = commandlineArg.TrimStart('-');
                    values = new List<string>();
                }
                else
                {
                    values.Add(commandlineArg);
                }
            }
            data.Add(current, values.Join(separator: ' '));
            Data = data;
        }

        public IReadOnlyCollection<string> GetAvailableParameters()
        {
            return Data.Keys.ToArray();
        }
    }
}
