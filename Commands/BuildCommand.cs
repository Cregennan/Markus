using CommandLine;
using DocumentFormat.OpenXml.Packaging;
using Markdig.Syntax;
using Markus.Configmodels;
using Markus.Exceptions;
using Markus.Processing;
using Markus.Services;
using Newtonsoft.Json;
using Spectre.Console;

namespace Markus.Commands
{

    [Verb("build", isDefault: false, HelpText = "Сборка проекта")]
    internal class BuildCommand : IConsoleCommand
    {

        [Option('q', "quiet", Required = false, Default = false, HelpText = "Тихая работа, без вывода большей части текста")]
        public bool _quiet { get; set; }

        [Option("path", Default = null, Required = false, HelpText = "Путь к папке проекта")]
        public string? _path { get; set; }

        ///<inheritdoc />
        public async Task Execute()
        {

            //TODO: Extract reading of template file into another method
            
            try
            {
                if (!_quiet){
                    ConsoleService.Greeting();
                }

                //Получение манифеста

                string directoryPath = _path == null ? Environment.CurrentDirectory : Path.GetFullPath(_path);

                Manifest manifest = await ManifestService.GetManifest(directoryPath);

                if (!_quiet)
                    ConsoleService.ShowSuccess($"Собираем проект {manifest.ProjectName}...");

                ConfigStore.Setup(manifest, directoryPath);

                //TODO: Make --project parameter to manually select path where app will be executed
                string entrypointPath = Path.Combine(directoryPath, manifest.Entrypoint);
                string templateFromProjectPath = Path.Combine(Environment.CurrentDirectory, manifest.Template);
                string templateFromApplicationPath = Path.Combine(Utility.ApplicationPath, "Themes", manifest.Template);

                string selectedTemplatePath = String.Empty;

                if (File.Exists(templateFromProjectPath))
                {
                    selectedTemplatePath = templateFromProjectPath;
                    if(!_quiet)
                        AnsiConsole.MarkupLineInterpolated($"Используется пользовательский шаблон {manifest.Template}");
                }
                else if (File.Exists(templateFromApplicationPath))
                {
                    selectedTemplatePath = templateFromApplicationPath;
                    if (!_quiet)
                        AnsiConsole.MarkupLineInterpolated($"Используется системный шаблон {manifest.Template}");
                }
                else
                {   
                    //Get themes configuration
                    Themes themes = JsonConvert.DeserializeObject<Themes>(
                        File.ReadAllText(Path.Combine(Utility.ApplicationPath, "Themes", "themeDefinitions.json"))
                        )!;

                    
                    Theme? theme = themes.Definitions.Where(x => x.Default).FirstOrDefault();

                    //No theme in theme definition is pointed as "default" 
                    //Can happen if some developer forgets to set one of themes to "default" or in result of user actions
                    //TODO: Pack all content of Themes folder in archive (zip maybe). This will require to find another way to insert template into WordprocessingDocument
                    if (theme is null)
                    {
                        ConsoleService.ShowError("Ошибка в параметрах приложения");
                        AnsiConsole.MarkupLineInterpolated($"Не найден шаблон по умолчанию, попробуйте выбрать другой шаблон");
                        return;
                    }
                    selectedTemplatePath = Path.Combine(Utility.ApplicationPath, "Themes", theme.File + ".dotm");
                    if (!_quiet)
                    {
                        ConsoleService.ShowWarning("Не удалось найти файл шаблона");
                        AnsiConsole.MarkupLineInterpolated($"Используется системный шаблон по умолчанию: {theme.OutputText}");
                    }
                        

                }

                bool doAutoInclude = manifest.Recursive is true;

                ConsoleService.DebugWarning("Включен Include_once");

                IEnumerable<MarkdownObject> tokens = MarkdownService.GetMarkdownTokens(entrypointPath + ".md", doAutoInclude);

                using (var document = WordprocessingDocument.CreateFromTemplate(selectedTemplatePath))
                {

                    TitleProcessing.IncludeTitleIfManifestAllows(manifest, document);
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
