using CommandLine;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using Markus.Configmodels;
using Markus.Exceptions;
using Markus.Services;
using Markdig.Syntax;
using Newtonsoft.Json;
using Spectre.Console;
using System.Diagnostics;
using System.Reflection;

namespace Markus.Commands
{

    [Verb("build", isDefault: false, HelpText = "Сборка проекта")]
    internal class BuildCommand : IConsoleCommand
    {

        [Option('q', "quiet", Required = false, Default = false, HelpText = "Тихая работа, без вывода большей части текста")]
        public bool _quiet { get; set; }


        public async Task Execute()
        {

            try
            {
                if (!_quiet){
                    ConsoleService.Greeting();
                }
                    
                //Получение манифеста
                Manifest manifest = await ManifestService.GetManifest();

                if (!_quiet)
                ConsoleService.ShowSuccess($"Собираем проект {manifest.ProjectName}...");


                string entrypointPath = Path.Combine(Environment.CurrentDirectory, manifest.Entrypoint);
               

                string projectThemePath = Path.Combine(Environment.CurrentDirectory, manifest.Template);
                string executableThemePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Themes", manifest.Template);

                string templatePath = String.Empty;

                if (File.Exists(projectThemePath))
                {
                    templatePath = projectThemePath;
                    if(!_quiet)
                        AnsiConsole.MarkupLineInterpolated($"Используется пользовательский шаблон {manifest.Template}");
                }
                else if (File.Exists(executableThemePath))
                {
                    templatePath = executableThemePath;
                    if (!_quiet)
                        AnsiConsole.MarkupLineInterpolated($"Используется системный шаблон {manifest.Template}");
                }
                else
                {
                    Themes themes = JsonConvert.DeserializeObject<Themes>(
                        File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Themes", "themeDefinitions.json"))
                        )!;

                    Theme? theme = themes.Definitions.Where(x => x.Default).FirstOrDefault();
                    if (theme is null)
                    {
                        ConsoleService.ShowError("Не удалось найти файл шаблона");
                        AnsiConsole.MarkupLineInterpolated($"Попробуйте проверить файлы или создать новый проект");
                        return;
                    }
                    templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Themes", theme.File + ".md");
                    if (!_quiet)
                        AnsiConsole.MarkupLineInterpolated($"Используется системный шаблон по умолчанию: {theme.OutputText}");

                }
                ConfigStore.Setup(manifest, templatePath);
                IEnumerable<MarkdownObject> tokens = await MarkdownService.GetMarkdownTokens(entrypointPath + ".md");

                using (var document = WordprocessingDocument.CreateFromTemplate(templatePath))
                {

                    MarkdownService.ProcessBlocks(tokens, document);
                    document.SaveAs(entrypointPath + ".docx");

                }
                if(!_quiet)
                ConsoleService.ShowSuccess("Готово!");

            }
            catch (ManifestNotFoundException)
            {
                ConsoleService.ShowError("Проект в выбранной папке не найден");
            }
            catch (CorruptedThemeFileException)
            {
                ConsoleService.ShowError("Файл темы неверен");
            }
            catch (IOException)
            {
                ConsoleService.ShowError("Не удалось сохранить файл. Возможно, у вас открыт Word.");
            }

        }
    }
}
