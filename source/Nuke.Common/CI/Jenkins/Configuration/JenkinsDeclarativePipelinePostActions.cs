using System;
using System.Linq;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.Jenkins
{
    public class JenkinsDeclarativePipelinePostActions : ConfigurationEntity
    {
        public JenkinsDeclarativePipelineStep[] Always { get; set; } = Array.Empty<JenkinsDeclarativePipelineStep>();

        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock("post"))
            {
                if (Always.Length > 0)
                {
                    using (writer.WriteBlock(("always")))
                    {
                        foreach (var jenkinsDeclarativePipelineStep in Always)
                        {
                            jenkinsDeclarativePipelineStep.Write(writer);
                        }
                    }
                }
            }
        }
    }
}