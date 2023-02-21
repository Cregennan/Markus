using CommandLine;
using Markus.Commands;
using System.Reflection;

namespace Markus
{

    internal class Program
    {
        async static Task Main(string[] args)
        {

            var commands = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IConsoleCommand))).ToArray();

            var result = await CommandLine.Parser.Default.ParseArguments(args, commands).WithParsedAsync(
                    (type) => (type as IConsoleCommand)?.Execute()
                );
        }
    }
}