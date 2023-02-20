using CommandLine;
using Markus.Configmodels;
using Markus.Exceptions;
using Markus.Services;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.Commands
{
    [Verb("config", false, HelpText = "Настройка проекта")]
    internal class ConfigureCommand : IConsoleCommand
    {
        [Option("path", Default =null, Required = false, HelpText ="Путь к папке проекта")]
        public string? _path { get; set; }

        private Dictionary<string, string> _parameters = new Dictionary<string, string>
        {
            { "Нумерация заголовков", "enumerateHeadings" },
            { "Используемый шаблон", "usedTemplate"  },
            { "Добавление титульника", "includeTitle" }
        };

        private string _exitSelection = "> Выход";
        private Dictionary<string, Func<Manifest, string, Manifest>> _handlers = new Dictionary<string, Func<Manifest, string, Manifest>>();

        public ConfigureCommand()
        {
            _handlers.Add("usedTemplate", ManifestModifiers.handleTemplate);
            _handlers.Add("enumerateHeadings", ManifestModifiers.handleEnumerateHeadings);
            _handlers.Add("includeTitle", ManifestModifiers.handleIncludeTitle);
        }

        public async Task Execute()
        {
            ConsoleService.Greeting();
            

            string directoryPath = _path == null ? Environment.CurrentDirectory : Path.GetFullPath(_path);
            try
            {
                Manifest manifest = await ManifestService.GetManifest(directoryPath);

                ConfigStore.Setup(manifest, directoryPath);

                ConsoleService.ShowWarning("Редактирование проекта " + manifest.ProjectName);

                var parameters = _parameters.Keys.Concat(new[] { _exitSelection });

                while (true)
                {
                    string result = AnsiConsole.Prompt<string>(
                        new SelectionPrompt<string>()
                        .Title("Выберите параметр для изменения")
                        .PageSize(10)
                        .AddChoices(parameters)
                        );
                    if (result == _exitSelection)
                    {
                        break;
                    }
                    string command = _parameters[result];

                    if (!_handlers.ContainsKey(command))
                    {
                        ConsoleService.ShowError("Неверный параметр для редактирования");
                        break;
                    }
                    _handlers[command](manifest, directoryPath);
                }
                ConsoleService.ShowSuccess("Изменения сохранены");

                await ManifestService.SaveManifest(manifest, directoryPath, true);
            }
            catch (ManifestNotFoundException)
            {
                ConsoleService.ShowError("Проект не найден");
            }
            




        }
    }
}
