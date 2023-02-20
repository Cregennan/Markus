using Markdig;
using Markus.MarkdigExtensions.AutoInclude;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.MarkdigExtensions
{
    internal static class ExtensionUsings
    {
        public static MarkdownPipelineBuilder UseAutoInclude(this MarkdownPipelineBuilder builder)
        {
            if (!builder.BlockParsers.Contains<AutoIncludeParser>())
            {
                builder.BlockParsers.Insert(0, new AutoIncludeParser());
            }
            return builder;
        }

    }
}
