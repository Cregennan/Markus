using CommandLine;
using Markus.Configmodels;
using Markus.Exceptions;
using Markus.Services;
using Spectre.Console;

namespace Markus.Commands
{

    [Verb("new", isDefault: false, HelpText = "Создание нового проекта")]
    internal class NewProjectCommand : IConsoleCommand
    {

        [Value(0, MetaName = "name",  Required = true, HelpText = "Название проекта")]
        public String _name { get; set; }

        [Option('f', "force", Required = false, Default = false, HelpText = "Обход ограничений, к примеру создание нового проекта поверх старого")]
        public bool _force { get; set; }

        [Option('q', "quiet", Required = false, Default = false, HelpText = "Тихая работа, без вывода большей части текста")]
        public bool _quiet { get; set; }

        [Option("path", Default = null, Required = false, HelpText = "Путь к папке проекта")]
        public string? _path { get; set; }


        /// <inheritdoc/>
        public async Task Execute()
        {

            //Name provided by user should be filtered from any NTFS unsupported characters.
            string entryName = ManifestService.PrepareManifestName(_name);
            if (!_quiet)
            {
                ConsoleService.Greeting();
                AnsiConsole.WriteLine($"Создаем проект {this._name}...");
            }
            
            Manifest manifest = ConsoleService.RequestManifest(entryName);

            try
            {
                string directoryPath = _path == null ? Environment.CurrentDirectory : Path.GetFullPath( _path );

                await ManifestService.SaveManifest(manifest, directoryPath, _force);

                

                //Copy example files into new project folder
                Directory.CreateDirectory(Path.Combine(directoryPath, "images"));
                File.Copy(
                    Path.Combine(Utility.ApplicationPath, "Example", "alexanderPushnoy.jpg"),
                    Path.Combine(directoryPath, "images", "alexanderPushnoy.jpg")
                    );

                File.Copy(
                    Path.Combine(Utility.ApplicationPath, "Example", "example.md"),
                    Path.Combine(directoryPath, $"{manifest.Entrypoint}.md")
                );
                
                if (!_quiet)
                AnsiConsole.MarkupLineInterpolated($"[bold green] Проект {manifest.ProjectName} создан. Используйте [/] [dim green]`markus build`[/][bold green] для сборки проекта.[/]");


            }
            catch (ManifestAlreadyExistsException)
            {
                ConsoleService.ShowError("В данной папке уже существует другой проект! Удалите существующий или повторите команду с параметром --force");
            }
            catch (CorruptedManifestException)
            {
                ConsoleService.ShowError("Не удалось создать проект");
            }
        }
    }
}
