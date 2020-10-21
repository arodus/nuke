using System;
using System.Linq;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public class JenkinsPipelineEnvironmentCredential : JenkinsPipelineEnvironmentEntry
    {
        public JenkinsPipelineEnvironmentCredential(string name, string credentialName)
        {
            Name = name;
            CredentialsName = credentialName;
        }

        public string CredentialsName { get; set; }
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"{Name} = credentials('{CredentialsName}')");
        }
    }
}
