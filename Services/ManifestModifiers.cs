using Markus.Configmodels;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markus.Services
{
    internal static class ManifestModifiers
    {

        private struct ThemeData
        {
            public string Displayname;
            public string Fullpath;
        }

        internal static Manifest handleTemplate(this Manifest manifest, string directoryPath)
        {
            //ConsoleService.ShowWarning("Изменение шаблона проекта");

            string themedefPath = Path.Combine(Utility.ApplicationPath, "Themes", "themeDefinitions.json");
            string themedefText = File.ReadAllText(themedefPath);
            Themes themes = JsonConvert.DeserializeObject<Themes>(themedefText);
            List<ThemeData> themeData = themes.Definitions.Select(x =>
            {
                return new ThemeData
                {
                    Displayname = x.OutputText,
                    Fullpath = Path.Combine(Utility.ApplicationPath, "Themes", $"{x.File}.dotm")
                };
            }).ToList();
            string[] files = System.IO.Directory.GetFiles(directoryPath, "*.dotm");
            themeData.AddRange(files.Select(x =>
            {
                return new ThemeData
                {
                    Fullpath = x,
                    Displayname = $"Шаблон пользователя: " + Path.GetFileName(x)
                };
            }));
            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<String>()
                .Title("Выберите тему для [green] данного проекта [/]")
                .PageSize(10)
                .AddChoices(themeData.Select(x => x.Displayname))
            );
            int index = themeData.Select(x => x.Displayname).ToList().IndexOf(selected);

            AnsiConsole.MarkupLineInterpolated($"Используем [green]{Path.GetFileName(themeData[index].Fullpath)}[/] в качестве шаблона проекта");

            manifest.Template = Path.GetFileName(themeData[index].Fullpath);

            return manifest;
            
        }
        internal static Manifest handleEnumerateHeadings(this Manifest manifest, string directoryPath)
        {
            string[] yesOrNo = new string[] { "Да", "Нет" };

            var doNumbering = AnsiConsole.Prompt(
                new SelectionPrompt<String>()
                .Title("Пронумеровать ли заголовки?")
                .AddChoices(yesOrNo)
                ) == "Да";
            
            //AnsiConsole.MarkupLineInterpolated($"Принято, [green]{(doNumbering ? "" : "не ")}нумеруем[/] заголовки");

            manifest.EnumerateHeadings = doNumbering;

            return manifest;
        }
        internal static Manifest handleIncludeTitle(this Manifest manifest, string directoryPath)
        {

            string[] yesOrNo = new string[] { "Да", "Нет" };

            var includeTitle = AnsiConsole.Prompt(
                new SelectionPrompt<String>()
                .Title("Добавить титульник?")
                .AddChoices(yesOrNo)
                ) == "Да";

            //AnsiConsole.MarkupLineInterpolated($"Принято, [green]{(doNumbering ? "" : "не ")}нумеруем[/] заголовки");

            manifest.IncludeTitle = includeTitle;

            return manifest;

        }
        internal static Manifest handleRecursiveImport(this Manifest manifest, string directoryPath)
        {
            string[] yesOrNo = new string[] { "Да", "Нет" };

            var recursive = AnsiConsole.Prompt(
                new SelectionPrompt<String>()
                .Title("Добавить части в основной файл?")
                .AddChoices(yesOrNo)
                ) == "Да";

            manifest.Recursive = recursive;

            return manifest;

        }
    }
}
