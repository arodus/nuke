using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Nuke.Common.DI.Configuration
{
    public class CommandLineConfigurationSource : IConfigurationSource
    {
        public string[] Args { get; set; }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new CommandLineConfigurationProvider(Args);
        }
    }
}
