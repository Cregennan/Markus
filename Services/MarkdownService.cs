using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markus.MarkdigExtensions;
using Markus.MarkdigExtensions.AutoInclude;
using Markus.Processing;
using MarkTables = Markdig.Extensions.Tables;

namespace Markus.Services
{
    /// <summary>
    /// Class to work with Markdown 
    /// </summary>
    internal static class MarkdownService
    {

        private static MarkdownPipeline Pipeline;

        internal struct LeveledListItem
        {
            public int Level;
            public ListItemBlock Block;

        }
        
        static MarkdownService()
        {
            Pipeline = new MarkdownPipelineBuilder()
                .UseFootnotes()
                .UseAdvancedExtensions()
                .UseAutoInclude()
                .Build();

        }

        /// <summary>
        /// Get all Markdig blocks from current markdown document
        /// </summary>
        /// <param name="filepath">Path to markdown file</param>
        /// <param name="recursive">Indicates to add all documents from </param>
        /// <param name="processedPaths"></param>
        /// <returns></returns>
        public static IEnumerable<MarkdownObject> GetMarkdownTokens(string filepath, bool recursive = false, HashSet<string>? processedPaths = null)
        {

            //This will create a set of already processed paths to markdown files to prevent invinite recursive include.
            if (processedPaths is null)
            {
                processedPaths = new HashSet<string>();
            }
            string markdown = File.ReadAllText(filepath);

            MarkdownDocument document = Markdown.Parse(markdown, Pipeline);

            processedPaths.Add(filepath);

            foreach (MarkdownObject token in document)
            {
                
                if(token is not AutoIncludeBlock)
                {
                    yield return token;
                    continue;
                }

                //If block is link to another file
                if (!recursive)
                {
                    continue;
                }

                //If recursive mode is on

                string filename = (token as AutoIncludeBlock).Filename.ToString();
                string directoryPath = Path.GetDirectoryName(filepath)!;
                string anotherFilePath = Path.Combine(directoryPath, filename + ".md");

                if (!File.Exists(anotherFilePath))
                    continue;
                
                //If this file was already processed
                if (processedPaths!.Contains(anotherFilePath))
                    continue;

                processedPaths!.Add(anotherFilePath);

                //Get all tokens from referenced file
                foreach (MarkdownObject subToken in GetMarkdownTokens(anotherFilePath, true, processedPaths))
                    yield return subToken;
                
            }
        }

        public static void ProcessBlocks(IEnumerable<MarkdownObject> blocks, WordprocessingDocument document, Paragraph currentParagraph = null, object[] blockProcesingOptions = null)
        {

            var documentBody = document.MainDocumentPart.Document.Body;

            foreach (MarkdownObject block in blocks)
            {
                switch (block)
                {
                    
                    case HeadingBlock heading:
                        {

                            //Append new paragraph and insert new run into it
                            var firstRun = new Run();
                            var paragraph = currentParagraph ?? documentBody.AppendChild(new Paragraph(firstRun));

                            string headingNumber = String.Empty;
                            if ((bool)ConfigStore.Instance.Manifest.EnumerateHeadings!)
                            {
                                headingNumber = HeadingNumerator.ForCurrentLevel(heading.Level - 1);
                            }

                            firstRun.AppendChild(new Text($"{headingNumber} "){ Space = SpaceProcessingModeValues.Preserve });

                            paragraph.ParagraphProperties ??= new ParagraphProperties();
                            paragraph.ParagraphProperties.AppendChild(new ParagraphStyleId { Val = $"Heading{heading.Level}" });

                            //Walk through new inlines in this heading block
                            ParagraphProcessing.ProcessInlines(heading.Inline, ref firstRun, document);

                        }
                        break;

                    case ParagraphBlock paragraphBlock:
                        {
                            //Append new paragraph and run
                            var currentRun = new Run();
                            var paragraph = currentParagraph ?? documentBody.AppendChild(new Paragraph(currentRun));

                            ParagraphProcessing.ProcessInlines(paragraphBlock.Inline, ref currentRun, document);
                        }
                        break;

                    //Horizontal line
                    case ThematicBreakBlock thematicBreakBlock:
                        {
                            documentBody.AppendChild(new Paragraph(
                                    new Run(),
                                    new ParagraphProperties(
                                        //Space after line
                                        new SpacingBetweenLines { After = "360" },
                                        new ParagraphBorders(
                                            new BottomBorder { Val = BorderValues.Single, Color = "auto", Size = 12U, Space = 1U }
                                        )
                                    )
                                ));
                            break;
                        }
                    //Список
                    case ListBlock list:
                        {
                            ListProcessing.HandleList(list, document);
                            break;
                        }

                    case MarkTables.Table table:
                        {
                            TableProcessing.ProcessTable(table, document);

                            break;
                        }
                       
                    default:
                        //Message will be written if block is not implemented and app is in debug
                        ConsoleService.DebugWarning($"Блок {block} не реализован");
                        break;

                }
            }

        }

    }
}
