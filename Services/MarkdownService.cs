using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

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
                .Build();

        }

        /// <summary>
        /// Get all Markdig blocks from current markdown document
        /// </summary>
        /// <param name="filepath">Path to markdown file</param>
        /// <param name="recursive">Indicates to add all documents from </param>
        /// <param name="processedPaths"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<MarkdownObject>> GetMarkdownTokens(string filepath, bool recursive = false, HashSet<string>? processedPaths = null)
        {

            //This functionality is not implemented yet: recursive include_once like behaivor of markdown parser
            if (recursive && processedPaths == null)
            {
                processedPaths = new HashSet<string>();
            }

            string markdown = await File.ReadAllTextAsync(filepath);


            MarkdownDocument document = Markdown.Parse(markdown, Pipeline);

            foreach(var child in document.First().Descendants())
            {
                ConsoleService.ShowError(child);
            }

            return document;
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
                            ProcessInlines(heading.Inline, ref firstRun, document);

                        }
                        break;

                    case ParagraphBlock paragraphBlock:
                        {
                            //Append new paragraph and run
                            var currentRun = new Run();
                            var paragraph = currentParagraph ?? documentBody.AppendChild(new Paragraph(currentRun));

                            ProcessInlines(paragraphBlock.Inline, ref currentRun, document);
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
                            HandleList(list, document);
                            break;
                        }

                       
                    default:
                        //Message will be written if block is not implemented and app is in debug
                        ConsoleService.DebugWarning($"Блок {block} не реализован");
                        break;

                }
            }

        }

        /// <summary>
        /// Process all inlines. If instance of inline has child inlines itself, function will be recursively called for all of them.
        /// </summary>
        /// <param name="children">Collection of inlines, for example: children of block element</param>
        /// <param name="currentRun"></param>
        /// <param name="document">Current Wordprocessing document</param>
        /// <param name="currentInlineProperties">Run modifiers such as <see cref="Bold"> and </see><seealso cref="Italic"/>. Effects will be applied to selected run</param>
        private static void ProcessInlines(IEnumerable<Inline>? children, ref Run currentRun, WordprocessingDocument document, object[] currentInlineProperties = null)
        {

            foreach (Inline child in children)
            {
                switch (child)
                {
                    case LiteralInline literal:
                        {
                            
                            currentRun
                                .ApplyEmphasisEffects(currentInlineProperties) //Append bold and italic styles
                                .AppendChild(new Text(literal.ToString()) { Space = SpaceProcessingModeValues.Preserve });
                            break;
                        }

                    case EmphasisInline emphasis:
                        {
                            HandleEmphasis(emphasis, ref currentRun, document, currentInlineProperties);
                            break;
                        }
                        //LinkInline may represend simple link, image or linkreference
                    case LinkInline link:
                        {

                            if (link.IsImage)
                            {
                                HandleImageLink(link, ref currentRun, document);
                                break;
                            }
                            if (link.IsShortcut)
                            {
                                ConsoleService.DebugWarning("Элемент <LinkReference> не реализован");
                                break;
                            }

                            HandleBasicLink(link, ref currentRun, document);

                            break;
                        }
                    case CodeInline code:
                        {

                            currentRun.AppendChild(new Text(code.Content));

                            //Apply styles for custom paragraph type
                            currentRun.RunProperties ??= new RunProperties();
                            currentRun.RunProperties.AppendChild(new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas" });
                            currentRun.RunProperties.AppendChild(new FontSize { Val = "24" });
                            currentRun.RunProperties.AppendChild(new FontSizeComplexScript { Val = "24" });
                            break;

                        }
                    case LineBreakInline lineBreak:
                        {
                            currentRun.AppendChild(new Break());
                            break;
                        }
                    default:
                            ConsoleService.DebugWarning($"Элемент {child} не реализован");
                        break;
                }
            }        
        }

        public static void HandleList(ListBlock list, WordprocessingDocument document)
        {

            var documentBody = document.MainDocumentPart.Document.Body;

            //Make level numbering, for example: all "*" for unordered lists or "1. a. iii." for ordered
            WordService.NumberingData numberingData = document.AddDefaultLeveledNumbering(list.IsOrdered);

            int currentNumberingIndex = numberingData.NumberingIndex;

            foreach (var child in list.GetLeveledChildren()) {

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

                ProcessInlines(ListItemChildParagraph.Inline, ref childRun, document);

            }


        }


        public static void HandleBasicLink(LinkInline link, ref Run currentRun, WordprocessingDocument document)
        {

            //ConsoleService.DebugError(link.Url);
            //ConsoleService.DebugError(link.IsShortcut);

            Uri uri = new Uri(link.Url);
            string text = string.Join("", link.Where(x => x is LiteralInline).Select(x => x.ToString()));

            HyperlinkRelationship rl = document.MainDocumentPart.AddHyperlinkRelationship(uri, true);
            string relId = rl.Id;


            Paragraph p = currentRun.GetParentParagraph();

            Hyperlink hl = p.AppendChild(new Hyperlink(

                new Run(
                    new RunProperties(
                        new RunStyle { Val = "Hyperlink" }
                    ),
                    new Text($"{text}") { Space = SpaceProcessingModeValues.Preserve}
                    )
                )
            { History = true, Id = relId }
            );


            /*Content of paragraph after adding hyperlink (html pseudocode):
             * <run> text of run *hyperlink should be here* text after hyperlink </run> <hyperlink />
             * Should be like this:
             * <run> text of run <hyperlink /> text after hyperlink </run>
             * 
             * We add another run after hyperlink and split the content of old one in two like this:
             * <run> text of run </run> <hyperlink /> <run> text after hyperlink </run>
             * 
             * Possible loss of emphasis. Should be covered with tests
             */

            currentRun = p.AppendChild(new Run());

        }

        public static void HandleEmphasis(EmphasisInline emphasis, ref Run currentRun, WordprocessingDocument document, object[] existingEffects)
        {

            //Paragraph of current run
            Paragraph currentParagraph = currentRun.GetParentParagraph();

            //Effects (modifiers) such as Bold, Italic
            object[] currentEffects = GetEmphasisEffects(emphasis.DelimiterCount, emphasis.DelimiterChar);

            //Run in WordprocessingDocument cannot contain another run. 
            //Because of that, nested effects (bold in italic) should be broken down into series of runs
            foreach(Inline inline in emphasis)
            {
                //Insert new run after current and set "pointer to current run" to it
                currentRun = currentParagraph.InsertAfter(new Run(), currentRun);

                //Merge effects of current emphasis with effects of previous emphasises (higher in markdown tree)
                object[] mergedEffects = (existingEffects ??= new object[] { }).Union(currentEffects ?? new object[] { }).ToArray();

                //Process this inline with old and new effects
                ProcessInlines(new[] { inline }, ref currentRun, document, existingEffects.Union(currentEffects ?? new object[] { }).ToArray());

            }
            //Add new run after the last to cancel effects of current emphasis
            currentRun = currentParagraph.InsertAfter(new Run(), currentRun);
        }

        
        //Process link as image
        public static void HandleImageLink(LinkInline link, ref Run previousRun, WordprocessingDocument document)
        {
            MainDocumentPart mainPart = document.MainDocumentPart;
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            //See OpenXML documentation
            Paragraph currentParagraph = previousRun.GetParentParagraph().InsertAfterSelf(new Paragraph());
            currentParagraph.ParagraphProperties ??= new ParagraphProperties();
            currentParagraph.ParagraphProperties.AppendChild(new Justification { Val = JustificationValues.Center });
            currentParagraph.ParagraphProperties.AppendChild(new Indentation { FirstLine = "0" });
            Run currentRun = currentParagraph.AppendChild(new Run());

            string path = link.Url;
            Stream? imageData = new MemoryStream();
            long Width = 0;
            long Height = 0;
            bool processed = true;

            //Check if link to image is URL to internet resource
            imageData = Utility.TryGetJpegStreamExternal(path, out string message, out Height,out Width);
            processed = imageData is not null;

            //Check if link to image is to file in current project directory
            if (!processed)
            {
                imageData = Utility.TryGetJpegStreamLocal(Path.Combine(Environment.CurrentDirectory, path), out string message2, out Height, out Width);

                processed = imageData is not null;
            }
            //Check if link is absolute path to image on disk
            if (!processed)
            {
                imageData = Utility.TryGetJpegStreamLocal(path, out string message1, out Height, out Width);
                processed = imageData is not null;
            }
            //Fail
            if (!processed)
            {
                currentRun.AppendChild(new Text($"Не удалось найти изображение по адресу {path}"));
                return;
            }

            imageData.Position = 0;
            imagePart.FeedData(imageData);

            //Calc new sizes of image for it to fit in margins of page
            WordService.CalcFitImageSizes(Width, Height, document, out long newWidth, out long newHeight);

            //Insert image into ImageDefinitions part, see OpenXML docs
            var drawing = WordService.MakeImage(mainPart.GetIdOfPart(imagePart), newWidth, newHeight);
            currentRun.AppendChild(drawing);

            //Apply some styles to paragraph of caption
            Paragraph captionParagraph = currentParagraph.InsertAfterSelf(new Paragraph());
            captionParagraph.ParagraphProperties ??= new ParagraphProperties();
            captionParagraph.ParagraphProperties.AppendChild(new ParagraphStyleId { Val = "Caption" });
            captionParagraph.ParagraphProperties.AppendChild(new Indentation { FirstLine = "0" });
            captionParagraph.ParagraphProperties.AppendChild(new Justification { Val = JustificationValues.Center });

            captionParagraph.AppendChild(new Run(new Text("Рисунок ") { Space = SpaceProcessingModeValues.Preserve }));
            captionParagraph.AppendChild(new SimpleField(
                    new Run(
                        new RunProperties(
                            new NoProof() { Val = true }
                            ),
                        new Text(
                            //Get current image number in document
                            Ticker.Up("Pictures").ToString()
                            )
                        )
                )
            //Instruction is rule how to make link reference to this image
            { Instruction = @"SEQ Рисунок \* ARABIC" });
            captionParagraph.AppendChild(new Run(new Text(" - ") { Space = SpaceProcessingModeValues.Preserve }));
            captionParagraph.AppendChild(new Run(new Text(link.FirstChild.ToString())));

            previousRun = captionParagraph.InsertAfterSelf(new Paragraph()).AppendChild(new Run());
            


        }

        //Parse markdown characters (e.g. **, *)
        public static object[] GetEmphasisEffects(int count, char delimiter)
        {

            bool isItalic = (count == 1) && (delimiter == '*' || delimiter == '_');
            bool isBold = (count == 2) && (delimiter == '*' || delimiter == '_');
            //TODO: Implement additional emphasis effects
            bool isStrikeThrough = (count == 1) && (delimiter == '~');

            List<object> effects = new List<object>();

            if (isBold) effects.Add(new Bold());
            if (isItalic) effects.Add(new Italic());

            return effects.ToArray();
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
