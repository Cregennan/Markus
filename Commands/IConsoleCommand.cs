using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.Commands
{
    /// <summary>
    /// Any command in this cli app should implement this interface. 
    /// </summary>
    internal interface IConsoleCommand
    {

        /// <summary>
        /// Command entrypoint
        /// </summary>
        /// <returns>Task of anything, result of method will not be used.</returns>
        Task Execute();


    }
}
