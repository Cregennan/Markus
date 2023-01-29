using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.Services
{
    internal static class Ticker
    {

        private static Dictionary<String, int> data;

        static Ticker()
        {
            data = new Dictionary<string, int>();
        }
        public static int Get(string key)
        {
            if (!data.ContainsKey(key))
            {
                data[key] = 0;
            }
            
            return data[key];
        }

        public static int Tick(string key)
        {
            if (!data.ContainsKey(key))
            {
                data[key] = 0;
            }
            data[key]++;

            return data[key];
        }
        public static int ReverseTick(string key)
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
