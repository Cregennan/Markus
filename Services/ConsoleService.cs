using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Markus.Configmodels;
using Newtonsoft.Json;
using Spectre.Console;

namespace Markus.Services
{
    internal class ConsoleService
    {

        public static void Greeting()
        {
            AnsiConsole.Write(new FigletText("Markus").LeftAligned().Color(Color.White));
        }

        public static void ShowSuccess(string message) => AnsiConsole.MarkupLineInterpolated($"[bold green]{message}[/]");

        public static void ShowError(string message) => AnsiConsole.MarkupLineInterpolated($"[bold red]Ошибка![/][red] {message}[/]");

        public static void ShowWarning(string message) => AnsiConsole.MarkupLineInterpolated($"[bold yellow]{message}[/]");

        public static void ShowSuccess(object data) => ShowSuccess(data.ToString());

        public static void ShowError(object data) => ShowError(data.ToString());

        public static void ShowWarning(object data) => ShowWarning(data.ToString());

        public static void DebugWarning(object data)
        {
            #if DEBUG
                ShowWarning(data);
            #endif
        }
        public static void DebugSuccess(object data)
        {
            #if DEBUG
                ShowSuccess(data);
            #endif
        }
        public static void DebugError(object data)
        {
            #if DEBUG
                ShowError(data);
            #endif
        }

        public static Manifest RequestManifest(string projectName, bool quiet = false)
        {

            string safeEntrypointName = ManifestService.PrepareManifestName(projectName);

            Manifest manifest = new Manifest();
            manifest.ProjectName = projectName;
            manifest.Entrypoint = safeEntrypointName;

            //Рекурсия или нет
            manifest.Recursive = false; //не реализовано

            string themedefPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Themes", "themeDefinitions.json");
            string themedefText = File.ReadAllText(themedefPath);
            Themes themes = JsonConvert.DeserializeObject<Themes>(themedefText);
            List<ThemeData> themeData = themes.Definitions.Select(x =>
            {
                return new ThemeData
                {
                    Displayname = x.OutputText,
                    Fullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Themes", $"{x.File}.dotm")
                };
            }).ToList();
            string[] files = System.IO.Directory.GetFiles(Environment.CurrentDirectory, "*.dotm");
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

            if (!quiet)
            AnsiConsole.MarkupLineInterpolated($"Используем [green]{Path.GetFileName(themeData[index].Fullpath)}[/] в качестве шаблона проекта");
            
            manifest.Template = Path.GetFileName(themeData[index].Fullpath);


            string[] yesOrNo = new string[] { "Да", "Нет" };

            var doNumbering = AnsiConsole.Prompt(
                new SelectionPrompt<String>()
                .Title("Пронумеровать ли заголовки?")
                .AddChoices(yesOrNo)
                ) == "Да";

            if (!quiet)
            AnsiConsole.MarkupLineInterpolated($"Принято, [green]{(doNumbering ? "" : "не ")}нумеруем[/] заголовки");

            manifest.EnumerateHeadings = doNumbering;

            return manifest;
        }

        private struct ThemeData
        {
            public string Displayname;
            public string Fullpath;
        }





    }
}
