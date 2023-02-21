using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.Exceptions
{
    /// <summary>
    /// Is thrown when another project exists in current folder
    /// </summary>
    internal class ManifestAlreadyExistsException : Exception
    {
    }
}
