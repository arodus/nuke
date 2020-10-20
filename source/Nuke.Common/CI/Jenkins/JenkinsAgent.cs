using System;
using System.Linq;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.Jenkins
{
    public class JenkinsAgent : ConfigurationEntity
    {
        public string Label { get; set; }

        public JenkinsAgent(string label)
        {
            Label = label;
        }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"agent {{ label '{Label}' }}");
        }
    }
}