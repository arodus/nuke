using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public class JenkinsPipelinePostActions : ConfigurationEntity
    {
        public JenkinsPipelineStep[] Always { get; set; } = Array.Empty<JenkinsPipelineStep>();
        public JenkinsPipelineStep[] Success { get; set; } =  Array.Empty<JenkinsPipelineStep>();

        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock("post"))
            {
                
                WriteWhenNotEmpty(nameof(Success).ToLowerInvariant(), Success, writer);
                WriteWhenNotEmpty(nameof(Always).ToLowerInvariant(), Always, writer);
            }
        }

        private void WriteWhenNotEmpty(string action, IReadOnlyCollection<JenkinsPipelineStep> steps, CustomFileWriter writer)
        {
            if (steps.Count <= 0)
                return;
            
            using (writer.WriteBlock(action))
            {
                foreach (var JenkinsPipelineStep in steps)
                {
                    JenkinsPipelineStep.Write(writer);
                }
            }
        }
    }
}
