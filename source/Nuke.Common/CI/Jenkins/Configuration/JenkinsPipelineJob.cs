using System;
using System.Linq;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public class JenkinsPipelineJob : ConfigurationEntity
    {
        public JenkinsAgent Agent { get; set; }
        
        public string BuildCmdPath { get; set; }
        public JenkinsPipelineStage[] Stages { get; set; } 
        public JenkinsPipelineEnvironment Environment { get; set; }
        
        public JenkinsParameter[] Parameters { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock("pipeline"))
            {
                Agent.Write(writer);

                if (Parameters != null)
                {
                    using (writer.WriteBlock("parameters"))
                    {
                        Parameters.ForEach(x => x.Write(writer));
                    }
                }
                
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
                    writer.WriteLine($"sh (\"{BuildCmdPath} $args\")");
                }

                using (writer.WriteCodeBlock("else"))
                {
                    writer.WriteLine($"powershell (\"{BuildCmdPath} $args\")");
                }
            }
        }
    }
}
