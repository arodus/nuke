using System;
using System.Linq;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public class JenkinsPipelineStage : ConfigurationEntity
    {
        public string Name { get; set; }
        

        
        public JenkinsPipelineStep[] Steps { get; set; }
        public JenkinsPipelinePostActions Post { get; set; }
        
        public JenkinsAgent Agent { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock($"stage('{Name}')"))
            {
                Agent?.Write(writer);
                using (writer.WriteBlock($"steps"))
                {
                    Steps.ForEach(x => x.Write(writer));
                }

                Post?.Write(writer);
            }
        }
    }
}
