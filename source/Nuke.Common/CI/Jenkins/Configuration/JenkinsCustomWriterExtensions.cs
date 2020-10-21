using System;
using System.Linq;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.Jenkins.Configuration
{
    public static class JenkinsCustomWriterExtensions
    {
        public static IDisposable WriteBlock(this CustomFileWriter writer, string text)
        {
            return DelegateDisposable
                .CreateBracket(() => writer.WriteLine($"{text} {{"), () => writer.WriteLine($"}} // {text}"))
                .CombineWith(writer.Indent());
        }
        public static IDisposable WriteCodeBlock(this CustomFileWriter writer, string text)
        {
            return DelegateDisposable
                .CreateBracket(() => writer.WriteLine($"{text} {{"), () => writer.WriteLine($"}}"))
                .CombineWith(writer.Indent());
        }
    }
}
