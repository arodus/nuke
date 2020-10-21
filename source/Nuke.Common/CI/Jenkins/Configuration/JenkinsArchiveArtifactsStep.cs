using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public class JenkinsArchiveArtifactsStep : JenkinsPipelineStep
    {
        public string Artifacts { get; set; }
        
        public bool? AllowEmptyArchive { get; set; }
        public bool? CaseSensitive { get; set; }
        [CanBeNull] public string Excludes { get; set; }
        public bool? Fingerprint { get; set; }
        public bool? OnlyIfSuccessful { get; set; }
        
        
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"archiveArtifacts('{Artifacts}')");
        }
    }
}
