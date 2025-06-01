using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixieCursors
{
    /// <summary>
    /// About, Licenses, Theme, Format and Language
    /// </summary>
    public partial class About : Window
    {
        private void ImageResize_info()
        {
            License.Text = File.ReadAllText(@"Docs\LazZiyaImageResize.txt");
            Description.Content = "LazZiya.ImageResize: Image resizing tool for .Net applications";
        }

        private void PC_info()
        {
            License.Text = File.ReadAllText(@"Docs\Pixie Cursors.txt");
            Description.Content = Properties.Resources.AppToCreateYourOwnCustomCursors;
        }

        public About()
        {
            InitializeComponent();
        }

        private void Arrows(Label visibleArrw, Label opacityBtn)
        {
            // Oculta todas las flechas y restaura la opacidad
            // para mostrar y cambiar la opacidad de la que se indique
            Arrw_1.Visibility = Visibility.Hidden;
            Arrw_2.Visibility = Visibility.Hidden;
            Arrw_3.Visibility = Visibility.Hidden;
            Arrw_Handy.Visibility = Visibility.Hidden;
            Arrw_Pixel.Visibility = Visibility.Hidden;
            Arrw_Teeny.Visibility = Visibility.Hidden;

            Btn_1.Opacity = 1;
            Btn_2.Opacity = 1;
            Btn_3.Opacity = 1;
            Btn_Handy.Opacity = 1;
            Btn_Pixel.Opacity = 1;
            Btn_Teeny.Opacity = 1;

            visibleArrw.Visibility = Visibility.Visible;
            opacityBtn.Opacity = 0.7;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Elegir el idioma, bordes redondeados y TopMost
            // dependiendo del archivo Config.ini
            switch (Config.currentLan)
            {
                case "es":
                    Lang.Content = Properties.Resources.LanguageEspañol;
                    break;
                case "en":
                    Lang.Content = Properties.Resources.LanguageEnglish;
                    break;
            }
        }

        private void Btn_1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (Btn_1.Content)
            {
                case "Pixie Cursors":
                    PC_info();
                    break;
                case "ImageResize":
                    ImageResize_info();
                    break;
            }
            Arrows(Arrw_1, Btn_1);
        }

        private void Btn_2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (Btn_2.Content)
            {
                case "Noto Music":
                    License.Text = File.ReadAllText(@"Docs\Noto Music\OFL.txt");
                    Description.Content = "Global font collection for writing in all modern and ancient languages";
                    break;
                case "ImageResize +":
                    License.Text = File.ReadAllText(@"Docs\LazZiyaImageResize +.txt");
                    Description.Content = "LazZiya.ImageResize Dependencies";
                    break;
            }
            Arrows(Arrw_2, Btn_2);
        }

        private void Btn_3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            License.Text = File.ReadAllText(@"Docs\FolderBrowserEx.txt");
            Description.Content = "Library to use the Folder Browser in .NET";
            Arrows(Arrw_3, Btn_3);
        }

        private void Btn_Teeny_MouseDown(object sender, MouseButtonEventArgs e)
        {
            License.Text = File.ReadAllText(@"Docs\Teenyicons.txt");
            Description.Content = "Tiny minimal 1px icons";
            Arrows(Arrw_Teeny, Btn_Teeny);
        }

        private void Btn_Iconizer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            License.Text = File.ReadAllText(@"Docs\PixelArtTool.txt");
            Description.Content = "Homemade Pixel Art Tool (WIP)";
            Arrows(Arrw_Pixel, Btn_Pixel);
        }

        private void Btn_Handy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            License.Text = File.ReadAllText(@"Docs\HandyControls.txt");
            Description.Content = "Based on HandyControl and includes more controls and features";
            Arrows(Arrw_Handy, Btn_Handy);
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            if (Arrw_1.Visibility == Visibility.Visible)
            {
                switch (Btn_1.Content)
                {
                    case "Pixie Cursors":
                        _ = Process.Start("https://genesistoxical.github.io/pixie-cursors/");
                        break;
                    case "ImageResize":
                        _ = Process.Start("https://github.com/LazZiya/ImageResize");
                        break;
                }
            }
            else if (Arrw_2.Visibility == Visibility.Visible)
            {
                switch (Btn_2.Content)
                {
                    case "Noto Music":
                        _ = Process.Start("https://fonts.google.com/noto/specimen/Noto+Sans");
                        break;
                    case "ImageResize +":
                        _ = Process.Start("https://nuget.org/packages/LazZiya.ImageResize/#dependencies-body-tab");
                        break;
                }
            }
            else if (Arrw_3.Visibility == Visibility.Visible)
            {
                _ = Process.Start("https://github.com/evaristocuesta/FolderBrowserEx");
            }
            else if (Arrw_Teeny.Visibility == Visibility.Visible)
            {
                _ = Process.Start("https://teenyicons.com/");
            }
            else if (Arrw_Pixel.Visibility == Visibility.Visible)
            {
                _ = Process.Start("https://github.com/unitycoder/PixelArtTool");
            }
            else if (Arrw_Handy.Visibility == Visibility.Visible)
            {
                _ = Process.Start("https://github.com/ghost1372/HandyControls");
            }
        }

        private void Caret_Click(object sender, RoutedEventArgs e)
        {
            // Cambiar y/o mostrar los tres lenguajes al cliquear flechitas.
            string LangText = Lang.Content.ToString();

            switch (LangText)
            {
                case string _ when LangText.Contains("English"):
                    Lang.Content = Properties.Resources.LanguageEspañol;
                    Config.selecLan = "es";
                    break;
                case string _ when LangText.Contains("Español"):
                    Lang.Content = Properties.Resources.LanguageEnglish;
                    Config.selecLan = "en";
                    break;
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            // Cambiar el idioma si es necesario
            Config.CheckLanguage();

            // Si el idioma cambió, se deberá "reiniciar" la ventana principal
            // para actualizar el idioma
            if (Config.restart)
            {
                Console.WriteLine("Restarting - Reiniciando");
                MainWindow updatedMain = new MainWindow
                {
                    Owner = (MainWindow)Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                updatedMain.Show();
                updatedMain.Owner = null;
                ((MainWindow)Application.Current.MainWindow).Close();
                Application.Current.MainWindow = updatedMain;
                Config.restart = false;
            }
            else
            {
                Close();
            }
        }

        private void Back_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Btn_1.Content = "ImageResize";
            Btn_2.Content = "ImageResize +";
            Btn_3.Visibility = Visibility.Hidden;
            Btn_Handy.Visibility = Visibility.Hidden;
            Btn_Pixel.Visibility = Visibility.Hidden;
            Btn_Teeny.Visibility = Visibility.Hidden;

            ImageResize_info();
            Arrows(Arrw_1, Btn_1);

            if (Back_Next.Content.ToString() == Properties.Resources.Back)
            {
                Btn_1.Content = "Pixie Cursors";
                Btn_2.Content = "Noto Music";
                Btn_3.Visibility = Visibility.Visible;
                Btn_Handy.Visibility = Visibility.Visible;
                Btn_Pixel.Visibility = Visibility.Visible;
                Btn_Teeny.Visibility = Visibility.Visible;
                Back_Next.Content = Properties.Resources.Next;

                PC_info();
            }
            else
            {
                Back_Next.Content = Properties.Resources.Back;
            }
        }
    }
}