// Copyright 2020 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.CI.Jenkins.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Nuke.Common.ValueInjection;

namespace Nuke.Common.CI.Jenkins
{
    public class JenkinsAttribute : ChainedConfigurationAttributeBase
    {
        private readonly JenkinsAgent _agent;

        public JenkinsAttribute(string agentLabel)
        {
            _agent = new JenkinsAgent(agentLabel);
        }

        //todo kubernetes agent
        //public JenkinsAttribute(string kubernetesPodTemplatePath)

        public string[] InvokedTargets { get; set; } = Array.Empty<string>();
        public string[] ImportSecrets { get; set; } = Array.Empty<string>();

        public override HostType HostType => HostType.Jenkins;

        public override IEnumerable<string> GeneratedFiles => new[] { ConfigurationFile };
        public override IEnumerable<string> RelevantTargetNames => InvokedTargets;

        public override string ConfigurationFile => ConfigurationDirectory / "Jenkinsfile";

        //todo make it possible to change dir
        protected virtual AbsolutePath ConfigurationDirectory { get; set; } = NukeBuild.RootDirectory;



        public override CustomFileWriter CreateWriter(StreamWriter streamWriter)
        {
            return new CustomFileWriter(streamWriter, indentationFactor: 2, "//");
        }



        public override ConfigurationEntity GetConfiguration(NukeBuild build, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var stages = relevantTargets
                .Select(x => GetStage(x, relevantTargets))
                .ToArray();



            return new JenkinsPipelineJob
                   {
                       Agent = _agent,
                       BuildCmdPath = BuildCmdPath,
                       Environment = new JenkinsPipelineEnvironment
                                     {
                                         EnvironmentVariables = GetEnvironmentEntries().ToArray()
                                     },
                       Stages = stages,
                       Parameters = GetGlobalParameters(build, relevantTargets).ToArray()
                   };
        }

        protected virtual IEnumerable<JenkinsPipelineEnvironmentEntry> GetEnvironmentEntries()
        {
            yield return new JenkinsPipelineEnvironmentValue("IS_UNIX", "isUnix()");
            foreach (var importSecret in ImportSecrets)
            {
                yield return new JenkinsPipelineEnvironmentCredential(importSecret, importSecret);
            }
        }

        protected virtual JenkinsPipelineStage GetStage(
            ExecutableTarget executableTarget,
            IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var chainLinkTargets = GetInvokedTargets(executableTarget, relevantTargets).ToArray();


            var artifacts = chainLinkTargets
                //todo capture tests
                .SelectMany(x => ArtifactExtensions.ArtifactProducts[x.Definition])
                .Select(x => NukeBuild.RootDirectory.GetUnixRelativePathTo(x))
                .ToList();

            JenkinsPipelinePostActions post = null;
            if (artifacts.Any())
            {
                post = new JenkinsPipelinePostActions
                       {
                           Success = new JenkinsPipelineStep[]
                                     {
                                         new JenkinsArchiveArtifactsStep
                                         {
                                             Artifacts = string.Join(separator: ",", artifacts)
                                         }
                                     }
                       };
            }


            return new JenkinsPipelineStage
                   {
                       Name = chainLinkTargets.Last().Name,
                       Steps = new JenkinsPipelineStep[]
                               {
                                   new JenkinsPipelineRunStep
                                   {
                                       Command = $"nuke '{chainLinkTargets.Select(x => x.Name).JoinSpace()} --skip'"
                                   }
                               },
                       Post = post
                   };
        }



        protected virtual IEnumerable<JenkinsParameter> GetGlobalParameters(NukeBuild build, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            return ValueInjectionUtility
                .GetParameterMembers(build.GetType(), includeUnlisted: false)
                .Except(relevantTargets.SelectMany(x => x.Requirements
                    .Where(y => !(y is Expression<Func<bool>>))
                    .Select(y => y.GetMemberInfo())))
                .Where(x => x.DeclaringType != typeof(NukeBuild) || x.Name == nameof(NukeBuild.Verbosity))
                .Select(x => GetParameter(x, build, required: false));
        }

        protected virtual JenkinsParameter GetParameter(MemberInfo member, NukeBuild build, bool required)
        {
            var attribute = member.GetCustomAttribute<ParameterAttribute>();
            var valueSet = ParameterService.GetParameterValueSet(member, build);

            var defaultValue = member.GetValue(build);
            if (defaultValue != null &&
                (member.GetMemberType() == typeof(AbsolutePath) ||
                 member.GetMemberType() == typeof(Solution) ||
                 member.GetMemberType() == typeof(Octokit.Project)))
                defaultValue = NukeBuild.RootDirectory.GetUnixRelativePathTo(defaultValue.ToString());

            JenkinsParameterType GetParameterType()
            {
                if (member.GetMemberType() == typeof(bool))
                    return JenkinsParameterType.Boolean;
                if (valueSet != null)
                    return JenkinsParameterType.Choice;
                return JenkinsParameterType.String;
            }

            return new JenkinsParameter
                   {
                       Name = member.Name,
                       Description = attribute.Description,
                       Type = GetParameterType(),
                       Options = valueSet?.Select(x => x.Text).ToArray(), //todo text vs object
                       DefaultValue = defaultValue?.ToString(),
                   };
        }

    }
}
