using Markus.Configmodels;

namespace Markus.Services
{
    /// <summary>
    /// Storage of data needed for app to run. For example, current parsed manifest or path to current theme
    /// </summary>
    internal class ConfigStore
    {

        public Manifest Manifest { get; private set; }

        public String CurrentFolderPath { get; private set; }

        private ConfigStore(Manifest manifest, String currentFolderPath)
        {
            this.Manifest = manifest;
            this.CurrentFolderPath = currentFolderPath;
        }
        public static ConfigStore Instance { get; private set; } = null;


        public static ConfigStore Setup(Manifest manifest, String CurrentFolderPath)
        {
            if (Instance is not null)
            {
                return Instance;
            }
            Instance = new ConfigStore(manifest, CurrentFolderPath);
            return Instance;
        }


    }
}
