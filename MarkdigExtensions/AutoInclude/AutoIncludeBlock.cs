using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markus.MarkdigExtensions.AutoInclude
{
    internal class AutoIncludeBlock : LeafBlock
    {
        public AutoIncludeBlock(BlockParser? parser) : base(parser)
        {
            ProcessInlines = false;
        }

        public StringSlice Filename { get; init; }
        
    }
}
