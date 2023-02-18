using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Markdig.Syntax.Inlines;
using Markus.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.Processing
{
    internal static class ParagraphProcessing
    {
        /// <summary>
        /// Process all inlines. If instance of inline has child inlines itself, function will be recursively called for all of them.
        /// </summary>
        /// <param name="children">Collection of inlines, for example: children of block element</param>
        /// <param name="currentRun"></param>
        /// <param name="document">Current Wordprocessing document</param>
        /// <param name="currentInlineProperties">Run modifiers such as <see cref="Bold"> and </see><seealso cref="Italic"/>. Effects will be applied to selected run</param>
        internal static void ProcessInlines(IEnumerable<Inline>? children, ref Run currentRun, WordprocessingDocument document, object[] currentInlineProperties = null)
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
                                LinkProcessing.HandleImageLink(link, ref currentRun, document);
                                break;
                            }
                            if (link.IsShortcut)
                            {
                                ConsoleService.DebugWarning("Элемент <LinkReference> не реализован");
                                break;
                            }

                            LinkProcessing.HandleBasicLink(link, ref currentRun, document);
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

        internal static void HandleEmphasis(EmphasisInline emphasis, ref Run currentRun, WordprocessingDocument document, object[] existingEffects)
        {

            //Paragraph of current run
            Paragraph currentParagraph = currentRun.GetParentParagraph();

            //Effects (modifiers) such as Bold, Italic
            object[] currentEffects = GetEmphasisEffects(emphasis.DelimiterCount, emphasis.DelimiterChar);

            //Run in WordprocessingDocument cannot contain another run. 
            //Because of that, nested effects (bold in italic) should be broken down into series of runs
            foreach (Inline inline in emphasis)
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

        //Parse markdown characters (e.g. **, *)
        internal static object[] GetEmphasisEffects(int count, char delimiter)
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

    }
}
