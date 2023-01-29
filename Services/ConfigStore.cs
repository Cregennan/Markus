using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markus.Configmodels;
using System.Drawing;

namespace Markus.Services
{
    internal class ConfigStore
    {

        public Manifest Manifest { get; private set; }

        public String CurrentTheme {get; private set;}

        private ConfigStore(Manifest manifest, String themePath)
        {
            this.Manifest = manifest;
            this.CurrentTheme = themePath;
        }

        public static ConfigStore Instance { get; } = null;

        public static ConfigStore Setup(Manifest manifest, String themePath)
        {
            if (Instance is not null)
            {
                return Instance;
            }
            return new ConfigStore(manifest, themePath);
        }


    }
}
