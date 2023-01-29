using Markus.Configmodels;
using Markus.Exceptions;
using Newtonsoft.Json;
using System.Text;

namespace Markus.Services
{
    internal class ManifestService
    {


        public const String ManifestExtension = ".gdproj";
        public const String DefaultManifestName = "Project";
        public const int MaxManifestFilenameLength = 100;


        public static String DetectManifestPath(string? manifestFolder)
        {
            string[] paths = Directory.GetFiles(manifestFolder).Where(x => x.EndsWith(ManifestExtension)).ToArray();

            return paths.Length switch
            {
                0 => throw new ManifestNotFoundException(),
                1 => paths[0],
                _ => throw new MultipleManifestsException(),
            };

        }

        public static async Task<Manifest> GetManifest(string? manifestFolder = null)
        {

            manifestFolder ??= Environment.CurrentDirectory;

            String manifestPath = DetectManifestPath(manifestFolder);
            
            Manifest? manifest = null;
            String text = await File.ReadAllTextAsync(manifestPath);

            try
            {
                manifest = JsonConvert.DeserializeObject<Manifest>(text);
            }
            catch (JsonException)
            {
                throw new CorruptedManifestException();
            }
            return manifest;
        }

        public static async Task SaveManifest(Manifest manifest, bool overwrite = false)
        {

            String serialized = JsonConvert.SerializeObject(manifest);

            String cleanManifestName = PrepareManifestName(manifest.ProjectName);
            if (cleanManifestName.Length == 0)
            {
                cleanManifestName = DefaultManifestName;
            }

            String path = Path.Combine(Environment.CurrentDirectory, cleanManifestName + ManifestExtension);
            

            bool projectExists = true;
            string existingPath = String.Empty;
            try
            {
                existingPath = DetectManifestPath(Environment.CurrentDirectory);
                
            }
            catch (ManifestNotFoundException)
            {
                projectExists = false;
            }

            if (projectExists)
            {
                if (overwrite)
                {
                    File.Delete(existingPath);
                }
                else
                {
                    throw new ManifestAlreadyExistsException();
                }
            }

            await File.WriteAllTextAsync(path, serialized);
        }

        public static String PrepareManifestName (String manifestName)
        {
            string filtered = string.Concat(manifestName.Split(Path.GetInvalidFileNameChars()))
                               .TrimEnd('.');

            return filtered.Substring(0, int.Min(50, filtered.Length));
        }

    }
}
