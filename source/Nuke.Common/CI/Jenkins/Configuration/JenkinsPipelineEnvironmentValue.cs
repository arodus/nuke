using System;
using System.Linq;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public class JenkinsPipelineEnvironmentValue : JenkinsPipelineEnvironmentEntry
    {
        public JenkinsPipelineEnvironmentValue(){}

        public JenkinsPipelineEnvironmentValue(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Value { get; set; }
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"{Name} = {Value}");
        }
    }
}