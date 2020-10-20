using System;
using System.Linq;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.CI.Jenkins
{
    public class JenkinsDeclarativePipelineJob : ConfigurationEntity
    {
        private readonly JenkinsAgent _agent;
        
        public JenkinsDeclarativePipelineStage[] Stages { get; set; } 
        public JenkinsDeclarativePipelineEnvironment Environment { get; set; }
        public string BuildShPath { get; set; }
        public string BuildPs1Path { get; set; }
        public JenkinsDeclarativePipelineJob(JenkinsAgent agent)
        {
            _agent = agent;
        }

        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock("pipeline"))
            {
                _agent.Write(writer);
                Environment?.Write(writer);
                using (writer.WriteBlock("stages"))
                {
                    Stages.ForEach(x => x.Write(writer));
                }
                
            }
            writer.WriteLine();
            WriteNukeFunction(writer);
        }

        private void WriteNukeFunction(CustomFileWriter writer)
        {
            using (writer.WriteCodeBlock("void nuke(String args)"))
            {
                using (writer.WriteCodeBlock("if (Boolean.valueOf(env.IS_UNIX))"))
                {
                    writer.WriteLine($"sh \"{BuildShPath} $args\"");
                }

                using (writer.WriteCodeBlock("else"))
                {
                    writer.WriteLine($"powershell \"{BuildPs1Path} $args\"");
                }
            }
        }
    }
}
