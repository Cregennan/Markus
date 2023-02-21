using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig.Syntax;
using Markus.Processing;
using Markus.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Markus.Services.MarkdownService;

namespace Markus.Processing
{
    internal static class ListProcessing
    {

        public static void HandleList(ListBlock list, WordprocessingDocument document)
        {

            var documentBody = document.MainDocumentPart.Document.Body;

            //Make level numbering, for example: all "*" for unordered lists or "1. a. iii." for ordered
            WordService.NumberingData numberingData = document.AddDefaultLeveledNumbering(list.IsOrdered);

            int currentNumberingIndex = numberingData.NumberingIndex;

            foreach (var child in list.GetLeveledChildren())
            {

                bool isItemOrdered = child.Block.Order != 0;

                //If order of ListItemBlock differs from order of whole ListBlock 
                //TODO: Fix this, in-flight change of order is not working
                if (!isItemOrdered == list.IsOrdered)
                    numberingData.Levels.ElementAt(child.Level).ChangeLevelOrdering(child.Level, isItemOrdered);

                Paragraph p = documentBody.AppendChild(new Paragraph(
                        //See OpenXML Word documentation for numering docs
                        new NumberingProperties
                        {

                            NumberingId = new NumberingId { Val = currentNumberingIndex },
                            NumberingLevelReference = new NumberingLevelReference { Val = child.Level }
                        }
                    ));
                p.ParagraphProperties ??= new ParagraphProperties();
                p.ParagraphProperties.AppendChild(new ParagraphStyleId { Val = "ListParagraph" });

                Run childRun = p.AppendChild(new Run());

                ParagraphBlock ListItemChildParagraph = child.Block.FirstOrDefault(x => x is ParagraphBlock) as ParagraphBlock;

                ParagraphProcessing.ProcessInlines(ListItemChildParagraph.Inline, ref childRun, document);

            }


        }
        internal static IEnumerable<LeveledListItem> GetLeveledChildren(this ListBlock listBlock)
        {
            //Walk through every children of list block
            IEnumerable<LeveledListItem> dfs(ListBlock list, int level)
            {
                foreach (ListItemBlock block in list)
                {
                    yield return new LeveledListItem { Level = level, Block = block };

                    //If list item contains nested list items
                    foreach (var blockChild in block)
                    {
                        if (blockChild is ListBlock)
                        {
                            //C# does not have "yield foreach return X"
                            foreach (var item in dfs(blockChild as ListBlock, level + 1))
                            {
                                yield return item;
                            }
                        }
                    }
                }
            }

            return dfs(listBlock, 0);

        }

    }
}
