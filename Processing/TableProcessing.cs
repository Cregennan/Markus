using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig.Syntax;
using Markus.Services;
using MarkTables = Markdig.Extensions.Tables;

namespace Markus.Processing
{
    internal static class TableProcessing
    {

        internal static void ProcessTable(MarkTables.Table markTable, WordprocessingDocument document)
        {
            var body = document.MainDocumentPart.Document.Body;
            var table = body.AppendChild(new Table());

            table.AppendChild(new TableProperties
            {
                TableWidth = new TableWidth { Width = "0", Type = TableWidthUnitValues.Auto },
                TableStyle = new TableStyle { Val = "TableGrid" }
            });

            foreach(MarkTables.TableRow row in markTable)
            {
                var wordRow = table.AppendChild(new TableRow());
                if (row.IsHeader)
                {
                    wordRow.AppendChild(new TableRowProperties ()).AppendChild(new ConditionalFormatStyle { FirstRow = true });
                }
                int cellCounter = 0;
                foreach(MarkTables.TableCell cell in row)
                {
                    var wordCell = wordRow.AppendChild(new TableCell());
                    if (cellCounter == 0)
                    {
                        wordCell.AppendChild(new TableCellProperties { ConditionalFormatStyle = new ConditionalFormatStyle { FirstColumn = true } });
                    }
                    var paragraph = wordCell.AppendChild(new Paragraph());
                    var run = paragraph.AppendChild(new Run());
                    foreach(ParagraphBlock pBlock in cell)
                    {
                        ParagraphProcessing.ProcessInlines(pBlock.Inline, ref run, document);
                    }
                    cellCounter++;
                }
            }
        }

    }
}
