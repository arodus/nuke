using System;
using System.Linq;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.Jenkins
{
    public class JenkinsDeclarativePipelineRunStep : JenkinsDeclarativePipelineStep
    {
        public string Command { get; set; }
        
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine(Command);
        }
    }
}