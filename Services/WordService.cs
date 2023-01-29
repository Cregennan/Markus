using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Drawing = DocumentFormat.OpenXml.Drawing;
using DrawingWordprocessing = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DrawingPictures = DocumentFormat.OpenXml.Drawing.Pictures;
using System.Dynamic;

namespace Markus.Services
{

    /// <summary>
    /// Service to manipulate WordprocessingDocument and its content
    /// </summary>
    internal static class WordService
    {

        internal struct NumberingData
        {
            public IEnumerable<Level> Levels;
            public AbstractNum AbstractNumbering;
            public NumberingInstance NumberingInstance;
            public int NumberingIndex;
        }

        /// <summary>
        /// Centimeters to dxa conversion constant.
        /// </summary>
        public const double CentimetersToDxa = 2410d / 4.25d;

        /// <summary>
        /// Number formats for each level in ordered list.
        /// </summary>
        public static readonly NumberFormatValues[] OrderedNumberingOrder;

        /// <summary>
        /// Number formats for each level in unordered list.
        /// </summary>
        public static readonly NumberFormatValues[] UnorderedNumberingOrder;

        /// <summary>
        /// Numbering text for each level in ordered list.
        /// </summary>
        public static readonly string[] OrderedNumberingText;

        /// <summary>
        /// Number formats for each level in unordered list.
        /// </summary>
        public static readonly string[] UnorderedNumberingText;

        
        static WordService()
        {
            //As defined in Word's russian locale
            OrderedNumberingOrder = new NumberFormatValues[9] {
                NumberFormatValues.Decimal,
                NumberFormatValues.RussianLower,
                NumberFormatValues.LowerRoman,
                NumberFormatValues.Decimal,
                NumberFormatValues.RussianLower,
                NumberFormatValues.LowerRoman,
                NumberFormatValues.Decimal,
                NumberFormatValues.RussianLower,
                NumberFormatValues.LowerRoman
            };

            UnorderedNumberingOrder = Enumerable.Repeat(NumberFormatValues.Bullet, 9).ToArray();

            OrderedNumberingText = Enumerable.Range(1, 9).Select(x => $"%{x}.").ToArray();

            UnorderedNumberingText = Enumerable.Repeat("●", 9).ToArray();

        }


        /// <summary>
        /// Function returns first parent with type Wordprocessing.Paragraph in parent tree.
        /// </summary>
        /// <param name="element">OpenXml element, such as Run, Hyperlink etc.</param>
        internal static Paragraph? GetParentParagraph(this OpenXmlElement element)
        {
            OpenXmlElement current = element;

            while(!(current is Paragraph || current is null))
            {
                current = current.Parent;
            }

            return current as Paragraph;
        }

        /// <summary>
        /// Adds new numbering system to document and returns it's data
        /// </summary>
        /// <param name="document">Current <see cref="WordprocessingDocument"/></param>
        /// <param name="isOrdered">Is list should be ordered or not</param>
        /// <returns></returns>
        internal static NumberingData AddDefaultLeveledNumbering(this WordprocessingDocument document, bool isOrdered)
        {

            NumberingDefinitionsPart numberingPart = document.MainDocumentPart.NumberingDefinitionsPart;


            NumberFormatValues[] currentNumberingFormat = isOrdered ? OrderedNumberingOrder: UnorderedNumberingOrder;
            string[] currentNumberingText = isOrdered ? OrderedNumberingText: UnorderedNumberingText;


            //Make numbering levels with curresponding format and levelText
            var levels = currentNumberingFormat.Select((x, i) =>
                            new Level(
                                new StartNumberingValue { Val = 1},    
                                new LevelText { Val = currentNumberingText[i] },
                                new NumberingFormat { Val = x },
                                new ParagraphProperties (
                                    //TODO: Change later to current GOST indentation
                                    new Indentation { Hanging = "360", Left = $"{ 1429 + i * 500}"}
                                )
                            )
                            { LevelIndex = i }
                        );


            //See OpenXML docs
            var abstractNumberingId = numberingPart.Numbering.Elements<AbstractNum>().Count() + 1;
            var abstractNumbering = new AbstractNum(levels) { AbstractNumberId = abstractNumberingId };

            if (abstractNumberingId == 1)
            {
                numberingPart.Numbering.Append(abstractNumbering);
            }
            else
            {
                AbstractNum lastAbstractNum = numberingPart.Numbering.Elements<AbstractNum>().Last();
                numberingPart.Numbering.InsertAfter(abstractNumbering, lastAbstractNum);
            }

            var numberId = numberingPart.Numbering.Elements<NumberingInstance>().Count() + 1;
            NumberingInstance numberingInstance1 = new NumberingInstance(
                                                        new AbstractNumId() { Val = abstractNumberingId }
                                                   ) { NumberID = numberId };

            if (numberId == 1)
            {
                numberingPart.Numbering.Append(numberingInstance1);
            }
            else
            {
                var lastNumberingInstance = numberingPart.Numbering.Elements<NumberingInstance>().Last();
                numberingPart.Numbering.InsertAfter(numberingInstance1, lastNumberingInstance);
            }

            return new NumberingData { Levels = levels, NumberingInstance = numberingInstance1, AbstractNumbering = abstractNumbering, NumberingIndex = numberId};

        }

        //TODO: Not working
        internal static void ChangeLevelOrdering(this Level level, int itemLevel, bool shouldBeOrdered)
        {
    
            LevelText levelText = level.Elements<LevelText>().First();
            NumberingFormat numberingFormat = level.Elements<NumberingFormat>().First();

            levelText.Val = (shouldBeOrdered ? OrderedNumberingText[itemLevel] : UnorderedNumberingText[itemLevel]);
            numberingFormat.Val = (shouldBeOrdered ? OrderedNumberingOrder[itemLevel] : UnorderedNumberingOrder[itemLevel]);

        }

        /// <summary>
        /// Apply Emphasis effects provided by e.g. <see cref="MarkdownService.GetEmphasisEffects">GetEmphasisEffects</see>
        /// </summary>
        /// <param name="currentRun">Run which effect will be applied to</param>
        /// <param name="effects">List of effects</param>
        /// <returns></returns>
        internal static Run ApplyEmphasisEffects(this Run currentRun, object[] effects)
        {
            currentRun.RunProperties ??= new RunProperties();

            if (effects is not null)
            {
                foreach (object effect in effects)
                {

                    switch (effect)
                    {
                        case Bold:
                            currentRun.RunProperties.AppendChild(new Bold { Val = true });
                            break;

                        case Italic:
                            currentRun.RunProperties.AppendChild(new Italic { Val = true });
                            break;

                        default: break;
                    }
                }
            }
            
            return currentRun;
        }


        /// <summary>
        /// Totally copy-pasted method. It just creates a lot of stuff to make image insertion work. 
        /// 
        /// You should only pass <paramref name="relationshipId"/> of relation that you made in <see cref="ImagePart">Image Part</see>[[ section of <see cref="WordprocessingDocument">document</see>
        /// </summary>
        /// <param name="relationshipId"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        internal static DocumentFormat.OpenXml.Wordprocessing.Drawing MakeImage(string relationshipId, long Width, long Height)
        {
            return new DocumentFormat.OpenXml.Wordprocessing.Drawing(
            new DrawingWordprocessing.Inline(
                new DrawingWordprocessing.Extent() { Cx = Width, Cy = Height },
                new DrawingWordprocessing.EffectExtent()
                {
                    LeftEdge = 0L,
                    TopEdge = 0L,
                    RightEdge = 0L,
                    BottomEdge = 0L
                },
                new DrawingWordprocessing.DocProperties()
                {
                    Id = (UInt32Value)1U,
                    Name = "Picture 1"
                },  
                new DrawingWordprocessing.NonVisualGraphicFrameDrawingProperties(
                    new Drawing.GraphicFrameLocks() { NoChangeAspect = true }),
                new Drawing.Graphic(
                    new Drawing.GraphicData(
                        new DrawingPictures.Picture(
                            new DrawingPictures.NonVisualPictureProperties(
                                new DrawingPictures.NonVisualDrawingProperties()
                                {
                                    Id = (UInt32Value)0U,
                                    Name = "New Bitmap Image.jpg"
                                },
                                new DrawingPictures.NonVisualPictureDrawingProperties()),
                            new DrawingPictures.BlipFill(
                                new Drawing.Blip(
                                    new Drawing.BlipExtensionList(
                                        new Drawing.BlipExtension()
                                        {
                                            Uri =
                                              "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                        })
                                )
                                {
                                    Embed = relationshipId,
                                    CompressionState =
                                    Drawing.BlipCompressionValues.Print
                                },
                                new Drawing.Stretch(
                                    new Drawing.FillRectangle())),
                            new DrawingPictures.ShapeProperties(
                                new Drawing.Transform2D(
                                    new Drawing.Offset() { X = 0L, Y = 0L },
                                    new Drawing.Extents() { Cx = Width, Cy = Height }
                                   ),
                                new Drawing.PresetGeometry(
                                    new Drawing.AdjustValueList()
                                )
                                { Preset = Drawing.ShapeTypeValues.Rectangle }))
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
            )
            {
                DistanceFromTop = (UInt32Value)0U,
                DistanceFromBottom = (UInt32Value)0U,
                DistanceFromLeft = (UInt32Value)0U,
                DistanceFromRight = (UInt32Value)0U,
                EditId = "50D07946"
            });
        }

        internal static void CalcFitImageSizes(long Width, long Height, WordprocessingDocument document, out long newWidth, out long newHeight)
        {
                
            double ratio = (Width * 1.0) / (Height * 1.0);

            //one inch is 914400 EMU
            //one inch is 1140 twips
            //one inch is 96 pixels

            var body = document.MainDocumentPart.Document.Body;
            var section = body.Elements<SectionProperties>().First();
            var pageSize = section.Elements<PageSize>().First();


            //full document sizes in twips
            long documentWidth = pageSize.Width;
            long documentHeight = pageSize.Height;

            //margins in twips
            var pageMargins = section.Elements<PageMargin>().First();
            long marginX = (long)pageMargins.Left + (long)pageMargins.Right;
            long marginY = (long)pageMargins.Top + (long)pageMargins.Bottom;


            long availableWidthTwips = documentWidth - marginX;
            long availableHeightTwips = documentHeight - marginY;

            //available space in emus
            ulong availableWidth = (ulong)((availableWidthTwips / 1440d) * 914400d);
            ulong availableHeight = (ulong)((availableHeightTwips / 1440d) * 914400d);

            double documentRatio = (availableWidth * 1d) / (availableHeight * 1d);

            //Choose to fit by width or height (similar to background-fit: contain in css)
            if (ratio > documentRatio)
            {
                newWidth = (long)Math.Min(availableWidth, (ulong)Width);
                newHeight = (long)(newWidth / ratio);
            }
            else
            {
                newHeight = (long)Math.Min(availableHeight, (ulong)Height);
                newWidth = (long)(newHeight * ratio);
            }
        }

    }
}
