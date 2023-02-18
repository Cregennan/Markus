using Markus.Configmodels;
using Markus.Exceptions;
using Newtonsoft.Json;

namespace Markus.Services
{
    /// <summary>
    /// Class to work with manifest
    /// </summary>
    internal class ManifestService
    {
        public const String ManifestExtension = ".mks";
        public const String DefaultManifestName = "Project";
        public const int MaxManifestFilenameLength = 100;


        /// <summary>
        /// Find manifest in folder
        /// </summary>
        /// <param name="manifestFolder"></param>
        /// <returns></returns>
        /// <exception cref="ManifestNotFoundException">No manifest in folder</exception>
        /// <exception cref="MultipleManifestsException">Multiple project files found in folder</exception>
        public static String DetectManifestPath(string? manifestFolder)
        {
            ConsoleService.DebugWarning(manifestFolder);
            if (Directory.Exists(manifestFolder))
            {
                string[] paths = Directory.GetFiles(manifestFolder).Where(x => x.EndsWith(ManifestExtension)).ToArray();

                return paths.Length switch
                {
                    0 => throw new ManifestNotFoundException(),
                    1 => paths[0],
                    _ => throw new MultipleManifestsException(),
                };
            }
            throw new ManifestNotFoundException();
            

        }
        
        /// <summary>
        /// Finds manifest in selected folder and parses it
        /// </summary>
        /// <param name="directoryPath"><see cref="Environment.CurrentDirectory"/> by default</param>
        /// <returns></returns>
        /// <exception cref="CorruptedManifestException"></exception>
        public static async Task<Manifest> GetManifest(string? directoryPath = null)
        {   
            String manifestPath = DetectManifestPath(directoryPath);
            
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

        /// <summary>
        /// Saves manifest to current project path
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        /// <exception cref="ManifestAlreadyExistsException"></exception>
        public static async Task SaveManifest(Manifest manifest, string directoryPath, bool overwrite = false)
        {
            
            String serializedManifest = JsonConvert.SerializeObject(manifest);

            String cleanedManifestName = PrepareManifestName(manifest.ProjectName);
            if (cleanedManifestName.Length == 0)
            {
                cleanedManifestName = DefaultManifestName;
            }

            string manifestFilePath = Path.Combine(directoryPath, cleanedManifestName + ManifestExtension);

            ConsoleService.DebugWarning("Путь создания манифеста: " + manifestFilePath);

            bool projectExists = true;
            string existingPath = String.Empty;
            try
            {
                existingPath = DetectManifestPath(directoryPath);
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

            Directory.CreateDirectory(directoryPath);
            await File.WriteAllTextAsync(manifestFilePath, serializedManifest);
        }


        /// <summary>
        /// Filters any unsupported characters from manifest name
        /// </summary>
        /// <param name="manifestName"></param>
        /// <returns></returns>
        public static String PrepareManifestName (String manifestName)
        {
            string filtered = string.Concat(manifestName.Split(Path.GetInvalidFileNameChars()))
                               .TrimEnd('.');

            return filtered.Substring(0, int.Min(50, filtered.Length));
        }

    }
}
