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
    internal static class LinkProcessing
    {
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
                    new Text($"{text}") { Space = SpaceProcessingModeValues.Preserve }
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
            imageData = Utility.TryGetJpegStreamExternal(path, out string message, out Height, out Width);
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


    }
}
