using Markus.Commands;
using System.Reflection;
using CommandLine;
using Markus.Configmodels;
using Newtonsoft.Json;

namespace Markus
{

    internal class Program
    {
        async static Task Main(string[] args)
        {

            var commands = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IConsoleCommand))).ToArray();

            var result = CommandLine.Parser.Default.ParseArguments(args, commands).WithParsedAsync(
                    async (type) => await (type as IConsoleCommand)?.Execute()
                );
        }
    }
}