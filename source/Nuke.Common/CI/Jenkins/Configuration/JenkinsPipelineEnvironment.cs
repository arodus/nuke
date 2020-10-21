using System;
using System.Linq;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public class JenkinsPipelineEnvironment : ConfigurationEntity
    {
        public JenkinsPipelineEnvironmentEntry[] EnvironmentVariables { get; set; } =
            Array.Empty<JenkinsPipelineEnvironmentEntry>();
        
        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock("environment"))
            {
                EnvironmentVariables.ForEach(x => x.Write(writer));
            }
        }
    }
}
