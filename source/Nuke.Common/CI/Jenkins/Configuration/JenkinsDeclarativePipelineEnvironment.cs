using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.CI.Jenkins
{
    public class JenkinsDeclarativePipelineEnvironment : ConfigurationEntity
    {
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock("environment"))
            {
                EnvironmentVariables.ForEach(x => writer.WriteLine($"{x.Key} = {x.Value}"));
            }
        }
    }
}