using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace PixieCursors
{
    /// <summary>
    /// Config.ini
    /// </summary>
    internal class Config
    {
        internal static bool isIntalled;
        internal static string iniPath;
        internal static string[] iniLines;
        internal static bool restart = false;
        internal static string format = "";

        internal static void CheckPath()
        {
            // Busca el archivo ini en la misma carpeta para saber si
            // Drop Icons está instalado o no, incluso si se tiene una
            // versión instalada y otra portable
            isIntalled = !File.Exists("Config.ini");

            // Establece las rutas de ini y dat, dependiendo de lo anterior
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Drop Icons";
            iniPath = isIntalled ? appdata + "\\Config.ini" : "Config.ini";
            iniLines = File.ReadAllLines(iniPath);

            Console.WriteLine("Drop Icons is installed? " + isIntalled + " - ¿Drop Icons está instalado? " + isIntalled + " - Drop Icons ist installiert? " + isIntalled);
        }

        #region Language
        internal static string currentLan, selecLan;

        internal static void Language()
        {
            // Modificar el idioma de la aplicación en base a Config.ini y establecer
            // el idioma actual en un string para no volveer a leer el archivo
            switch (iniLines[1])
            {
                case "Language = en":
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("");
                    currentLan = "en";
                    selecLan = "en";
                    break;
                case "Language = es":
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es-419");
                    currentLan = "es";
                    selecLan = "es";
                    break;
            }

            Console.WriteLine("Current language: " + currentLan + " - Idioma actual: " + currentLan);
        }

        internal static void CheckLanguage()
        {
            // Si el idioma seleccionado no es el mismo que el actual,
            // cambiarlo y modificar el archivo .ini
            if (selecLan != currentLan)
            {
                switch (selecLan)
                {
                    case "en":
                        iniLines[1] = iniLines[1].Replace(currentLan, "en");
                        File.WriteAllLines(iniPath, iniLines);
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("");
                        currentLan = "en";
                        break;
                    case "es":
                        iniLines[1] = iniLines[1].Replace(currentLan, "es");
                        File.WriteAllLines(iniPath, iniLines);
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es-419");
                        currentLan = "es";
                        break;
                }

                Console.WriteLine("Selected language: " + selecLan + " - Idioma seleccionado: " + selecLan);
                restart = true;
            }
        }
        #endregion
    }
}
