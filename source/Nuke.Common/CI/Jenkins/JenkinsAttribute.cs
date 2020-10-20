// Copyright 2020 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;

namespace Nuke.Common.CI.Jenkins
{
    public class JenkinsAttribute : ChainedConfigurationAttributeBase
    {
        private readonly JenkinsAgent _agent;

        public JenkinsAttribute(string agentLabel)
        {
            _agent = new JenkinsAgent(agentLabel);
        }

        public string[] InvokedTargets { get; set; } = Array.Empty<string>();
        public override HostType HostType => HostType.Jenkins;

        public override IEnumerable<string> GeneratedFiles => new[] { ConfigurationFile };
        public override IEnumerable<string> RelevantTargetNames => InvokedTargets;

        public override string ConfigurationFile => ConfigurationDirectory / "Jenkinsfile";

        protected virtual AbsolutePath ConfigurationDirectory { get; set; } = NukeBuild.RootDirectory;

        protected virtual string BuildShPath =>
            NukeBuild.RootDirectory.GlobFiles("build.sh", "*/build.sh")
                .Select(x => NukeBuild.RootDirectory.GetUnixRelativePathTo(x))
                .FirstOrDefault().NotNull("BuildCmdPath != null");
        
        protected virtual string BuildPs1Path =>
            NukeBuild.RootDirectory.GlobFiles("build.ps1", "*/build.ps1")
                .Select(x => NukeBuild.RootDirectory.GetUnixRelativePathTo(x))
                .FirstOrDefault().NotNull("BuildCmdPath != null");
        
        public override CustomFileWriter CreateWriter(StreamWriter streamWriter)
        {
            return new CustomFileWriter(streamWriter, indentationFactor: 2, "//");
        }

        protected virtual JenkinsDeclarativePipelineStage GetStage(
            ExecutableTarget executableTarget,
            IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var chainLinkTargets = GetInvokedTargets(executableTarget, relevantTargets).ToArray();

            return new JenkinsDeclarativePipelineStage
                   {
                       Name = chainLinkTargets.Last().Name,
                       Steps = new JenkinsDeclarativePipelineStep[]
                               {
                                   new JenkinsDeclarativePipelineRunStep
                                   {
                                       Command = $"nuke '{chainLinkTargets.Select(x => x.Name).JoinSpace()} --skip'"
                                   }
                               }
                   };

        }
        public override ConfigurationEntity GetConfiguration(NukeBuild build, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var stages = relevantTargets
                .Select(x => GetStage(x, relevantTargets))
                .ToArray();
            
            return new JenkinsDeclarativePipelineJob(_agent)
                   {
                       BuildPs1Path = BuildPs1Path,
                       BuildShPath = BuildShPath,
                       Environment = new JenkinsDeclarativePipelineEnvironment
                                     {
                                         EnvironmentVariables = new Dictionary<string, string>
                                                                {
                                                                    {"IS_UNIX", "isUnix()"}
                                                                }
                                     },
                       Stages = stages
                   };
        }
    }
}
