using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.MarkdigExtensions.AutoInclude
{
    internal class AutoIncludeParser : BlockParser
    {

        private readonly string _openingSequence = "@@[[";
        private readonly string _closingSequence = "]]";

        private readonly char _openingCharacter = '@';
        private readonly char _firstClosingCharacter = ']';

        public AutoIncludeParser() {

            OpeningCharacters = new[] { _openingCharacter };
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            if (processor.IsCodeIndent)
            {
                return BlockState.None;
            }
            
            var sourceColumn = processor.Column;
            var line = processor.Line;
            var sourcePosition = line.Start;
            var c = processor.NextChar();

            int index = 1;


            while(c != '\0' && index < _openingSequence.Length)
            {
                if (c != _openingSequence[index])
                {
                    processor.Column = sourceColumn;
                    processor.Line = line;
                    return BlockState.None;
                }
                index++;
                c = processor.NextChar();
            }
            
            //Exit if invalid opening sequence
            if (index != _openingSequence.Length)
            {
                processor.Column = sourceColumn;
                processor.Line = line;
                return BlockState.None;
            }

            StringBuilder sb = new StringBuilder();

            if (c != _firstClosingCharacter)
            {
                sb.Append(c);
                while (c != _firstClosingCharacter)
                {
                    c = processor.NextChar();
                }
            }
            index = 0;

            while(c != '\0' && index < _closingSequence.Length)
            {
                if (c != _closingSequence[index])
                {
                    processor.Column = sourceColumn;
                    processor.Line = line;
                    return BlockState.None;
                }
                index++;
                c = processor.NextChar();
            }
            if (index != _closingSequence.Length)
            {
                processor.Column = sourceColumn;
                processor.Line = line;
                return BlockState.None;
            }

            var autoIncludeBlock = new AutoIncludeBlock(this)
            {
                Column = sourceColumn,
                Span = { Start = sourcePosition },
                Filename = new StringSlice(sb.ToString())
            };
            processor.GoToColumn(sourceColumn + _openingSequence.Length + sb.Length + _closingSequence.Length + 1);
            processor.NewBlocks.Push(autoIncludeBlock);

            autoIncludeBlock.Span.End = processor.Line.End;

            return BlockState.Break;
        }
    }
}
