using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markus.Configmodels;
using Markus.Services;

namespace Markus.Processing
{
    internal class TitleProcessing
    {
        public static void IncludeTitleIfManifestAllows(Manifest manifest, WordprocessingDocument document)
        {
            
            if (manifest.IncludeTitle is not true)
            {
                return;
            }

            string titleFilename = Path.Combine(ConfigStore.Instance.CurrentFolderPath, "Титульник.docx");

            if (!File.Exists(titleFilename)){
                ConsoleService.ShowWarning("Не найден файл титульника. Титульник не будет включен в результат.");
                return;
            }

            AlternativeFormatImportPart chunk = document.MainDocumentPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML);
            string chunkId = document.MainDocumentPart.GetIdOfPart(chunk);

            using var fileStream = File.Open(titleFilename, FileMode.Open);
            chunk.FeedData(fileStream);

            AltChunk altChunk = new AltChunk { Id = chunkId };
            document.MainDocumentPart.Document.Body.AppendChild(altChunk);
            
        }




    }
}
