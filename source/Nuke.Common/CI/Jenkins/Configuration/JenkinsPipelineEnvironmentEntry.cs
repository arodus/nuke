using System;
using System.Linq;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public abstract class JenkinsPipelineEnvironmentEntry : ConfigurationEntity
    {
        public string Name { get; set; }
    }
}
