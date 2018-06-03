using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Nuke.Common.DI.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.OutputSinks;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.DI
{



    internal interface IBuildExecutor
    {
        int Execute<T>(Expression<Func<T, Target>> defaultTargetExpression)
            where T : NukeBuild;
    }

    internal interface IInjectionService
    {
        void InjectValues(NukeBuild nukeBuild);
    }

    internal class InjectionService : IInjectionService
    {
        private readonly ILogger<InjectionService> _logger;
        private readonly IConfiguration _configuration;

        public InjectionService(ILogger<InjectionService> logger, IEnvironmentInfo environmentInfo)
        {
            _logger = logger;
        }

        public void InjectValues(NukeBuild build)
        {
            var anyInjected = false;

            var injectionMembers = build.GetInjectionMembers()
                .OrderByDescending(x => x.GetCustomAttribute<ParameterAttribute>() != null);

            foreach (var member in injectionMembers)
            {
                var attributes = member.GetCustomAttributes().OfType<InjectionAttributeBase>().ToList();
                if (attributes.Count == 0)
                    continue;
                ControlFlow.Assert(attributes.Count == 1, $"Member '{member.Name}' has multiple injection attributes applied.");

                var attribute = attributes.Single();
                var memberType = (member as FieldInfo)?.FieldType ?? ((PropertyInfo) member).PropertyType;
                var value = attribute.GetValue(member.Name, memberType);
                if (value == null)
                    continue;

                var valueType = value.GetType();
                ControlFlow.Assert(memberType.IsAssignableFrom(valueType),
                    $"Field '{member.Name}' must be of type '{valueType.Name}' to get its valued injected from '{attribute.GetType().Name}'.");
                ReflectionService.SetValue(build, member, value);

                anyInjected = true;
            }

            if (anyInjected)
                Logger.Log();
        }
    }

    internal class RequirementService : IRequirementService
    {
        public RequirementService()
        {
        }

        public void ValidateRequirements(IReadOnlyCollection<TargetDefinition> executionList)
        {

        }
    }

    internal interface IRequirementService
    {
        void ValidateRequirements(IReadOnlyCollection<TargetDefinition> executionList);
    }

    internal interface ITargetDefinitionService
    {
        string[] InvokedTargets { get; }
        string[] SkippedTargets { get; }
        string[] ExecutingTargets { get; }
        IReadOnlyCollection<TargetDefinition> GetExecutingTargets(NukeBuild build);
    }

    internal class TargetDefinitionService : ITargetDefinitionService
    {

        public TargetDefinitionService()
        {

        }

        public string[] InvokedTargets { get; }
        public string[] SkippedTargets { get; }
        public string[] ExecutingTargets { get; }

        public IReadOnlyCollection<TargetDefinition> GetExecutingTargets(NukeBuild build)
        {
            // ControlFlow.Assert(build.TargetDefinitions.All(x => !x.Name.EqualsOrdinalIgnoreCase(BuildExecutor.DefaultTarget)),
            //       "The name 'default' cannot be used as target name.");
            return default(IReadOnlyCollection<TargetDefinition>);
        }
    }

    public interface ITargetExecutor
    {
    }

    public class TargetExecutor : ITargetExecutor
    {
    }

    internal class BuildExecutor : IBuildExecutor
    {
        private readonly NukeBuild _buildInstance;
        private readonly IInjectionService _injectionService;
        private readonly IGraphService _graphService;
        private readonly ITargetDefinitionService _targetDefinitionService;
        private readonly IRequirementService _requirementService;
        private readonly ILogger _logger;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHelpTextService _helpTextService;

        public BuildExecutor(
            NukeBuild buildInstance,
            IInjectionService injectionService,
            IGraphService graphService,
            ITargetDefinitionService targetDefinitionService,
            IRequirementService requirementService,
            ILogger<BuildExecutor> logger,
            ILifetimeScope lifetimeScope,
            IHelpTextService helpTextService)
        {
            _buildInstance = buildInstance;
            _injectionService = injectionService;
            _graphService = graphService;
            _targetDefinitionService = targetDefinitionService;
            _requirementService = requirementService;
            _logger = logger;
            _lifetimeScope = lifetimeScope;
            _helpTextService = helpTextService;
        }

        public int Execute<T>(Expression<Func<T, Target>> defaultTargetExpression)
            where T : NukeBuild
        {
            _logger.LogInformation("NUKE");
            _logger.LogInformation($"Version: {GetType().GetTypeInfo().Assembly.GetVersionText()}");
            _logger.LogInformation("Host: TBA");
            _logger.LogInformation("");

            var executionList = default(IReadOnlyCollection<TargetDefinition>);
            try
            {
                _injectionService.InjectValues(_buildInstance);
                HandleEarlyExits();

                executionList = _targetDefinitionService.GetExecutingTargets(_buildInstance);
                _requirementService.ValidateRequirements(executionList);
                Execute(executionList);

                return 0;
            }
            catch (Exception e)
            {

            }



            return 0;
        }

        private void Execute(IReadOnlyCollection<TargetDefinition> targetDefinitions)
        {
            foreach (var targetDefinition in targetDefinitions)
            {
                using (_lifetimeScope.BeginLifetimeScope(targetDefinition))
                {
                    var executor = _lifetimeScope.Resolve(targetDefinition.TargetExecutorType);
                }
            }
        }

        private void HandleEarlyExits()
        {
            if (_buildInstance.Help)
            {
                _logger.LogInformation(_helpTextService.GetTargetsText());
                _logger.LogInformation(_helpTextService.GetTargetsText());
            }

            if (_buildInstance.Graph)
                _graphService.ShowGraph();

            if (_buildInstance.Help || _buildInstance.Graph)
                Environment.Exit(exitCode: 0);
        }

    }

    public interface IEnvironmentInfo
    {

    }

    public interface IParameterService
    {

    }
    public class EnvironmentInfo : IEnvironmentInfo
    {
        private readonly IParameterService _parameterService;
        private readonly ILogger _logger;

        public EnvironmentInfo(IParameterService parameterService, ILogger logger)
        {
            _parameterService = parameterService;
            _logger = logger;
        }

        public PathConstruction.AbsolutePath BuildAssemblyDirectory
        {
            get
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                ControlFlow.Assert(entryAssembly.GetTypes().Any(x => x.IsSubclassOf(typeof(NukeBuild))),
                    $"{entryAssembly} doesn't contain a NukeBuild class.");
                return (PathConstruction.AbsolutePath) Path.GetDirectoryName(entryAssembly.Location).NotNull();
            }
        }

        public PathConstruction.AbsolutePath BuildProjectDirectory
        {
            get
            {
                var buildProjectDirectory = new DirectoryInfo(BuildAssemblyDirectory)
                    .DescendantsAndSelf(x => x.Parent)
                    .Select(x => x.GetFiles("*.csproj", SearchOption.TopDirectoryOnly)
                        .SingleOrDefaultOrError($"Found multiple project files in '{x}'."))
                    .FirstOrDefault(x => x != null)
                    ?.DirectoryName;
                return (PathConstruction.AbsolutePath) buildProjectDirectory.NotNull("buildProjectDirectory != null");
            }
        }
    }



}
