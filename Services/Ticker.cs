using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.Services
{
    /// <summary>
    /// Class that implements Ticker - any number of independent counters that can be increased or decreased by one.
    /// </summary>
    internal static class Ticker
    {

        private static Dictionary<String, int> data;

        static Ticker()
        {
            data = new Dictionary<string, int>();
        }

        /// <summary>
        /// Get current number in counter by its key
        /// </summary>
        /// <param name="key">Key of counter</param>
        /// <returns>Current number stored in counter</returns>
        public static int Get(string key)
        {
            if (!data.ContainsKey(key))
            {
                data[key] = 0;
            }
            
            return data[key];
        }

        /// <summary>
        /// Increases number stored in counter by one and returns it
        /// </summary>
        /// <param name="key">Key of counter</param>
        /// <returns></returns>
        public static int Up(string key)
        {
            if (!data.ContainsKey(key))
            {
                data[key] = 0;
            }
            data[key]++;

            return data[key];
        }

        /// <summary>
        /// Decreases number stored in counter by one and returns it
        /// </summary>
        /// <param name="key">Key of counter</param>
        /// <returns></returns>
        public static int Down(string key)
        {
            if (!data.ContainsKey(key))
            {
                data[key] = 0;
            }
            data[key]--;

            return data[key];
        }

    }
}
