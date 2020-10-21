using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public class JenkinsParameter : ConfigurationEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public string[] Options { get; set; }
        
        public JenkinsParameterType Type { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            var noQuotesDefaultValue = false;
            var multiLine = false;
            string typeString;
            switch (Type)
            {
                case JenkinsParameterType.Boolean:
                    noQuotesDefaultValue = true;
                    typeString = "booleanParam";
                    break;
                case JenkinsParameterType.Choice:
                    typeString = "choice";
                    multiLine = true;
                    break;
                case JenkinsParameterType.String:
                    typeString = "string";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Type));
            }

            var args = new List<(string Key, string Value)>
                       {
                           ("name", $"'{Name}'")
                       };


            if (DefaultValue != null)
                args.Add(("defaultValue", noQuotesDefaultValue ? DefaultValue : $"'{DefaultValue}'"));

            if (Options != null)
            {
                var optionsValues = string.Join(", ", Options.Select(x => $"'{x}'"));
                args.Add(("choices", $"[{optionsValues}]"));
            }

            if (Description != null)
                args.Add(("description", $"'{Description.Replace("'", "\\'")}'"));

            if (multiLine)
            {
                writer.WriteLine($"{typeString}(");
                using (writer.Indent())
                {
                    args.ForEach((x, i) => writer.WriteLine($"{x.Key}: {x.Value}{(i == args.Count - 1 ? "" : ",")}"));
                }

                writer.WriteLine(")");
            }
            else
            {
                writer.WriteLine($"{typeString}({string.Join(", ", args.Select(x => $"{x.Key}: {x.Value}"))})");
            }


        }

        private void WriteChoiceParameter(CustomFileWriter writer)
        {
            writer.WriteLine($"choice(name: '{Name}', defaultValue: '{DefaultValue}'");
        }
    }
}
