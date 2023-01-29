using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.Services
{
    internal static class HeadingNumerator
    {

        private static int[] levels = new int[6] { 0, 0, 0, 0, 0, 0 };

        private static int level = 0;

        public static String ForCurrentLevel(int currentLevel)
        {
            int dl = currentLevel - level;

            if (dl == 0)
            {
                levels[currentLevel]++;
            }
            else
            {
                for (int i = 0; i < Math.Abs(dl); i++)
                {
                    if (dl < 0)
                    {
                        level--;
                        if (level < 0) level = 0;
                    }
                    if (dl > 0)
                    {
                        level++;
                        if (level > 5) level = 5;
                        levels[level] = 1;
                    }
                }
            }

            var elements = levels.Take(level + 1);

            var result = String.Join(".", elements) + ".";


            return  result;
        }
    }
}
