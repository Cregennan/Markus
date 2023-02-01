using Markus.Configmodels;

namespace Markus.Services
{
    /// <summary>
    /// Storage of data needed for app to run. For example, current parsed manifest or path to current theme
    /// </summary>
    internal class ConfigStore
    {

        public Manifest Manifest { get; private set; }

        public String CurrentTheme { get; private set; }

        private ConfigStore(Manifest manifest, String themePath)
        {
            this.Manifest = manifest;
            this.CurrentTheme = themePath;
        }

        public static ConfigStore Instance { get; private set; } = null;

        public static ConfigStore Setup(Manifest manifest, String themePath)
        {
            if (Instance is not null)
            {
                return Instance;
            }
            Instance = new ConfigStore(manifest, themePath);
            return Instance;
        }


    }
}
