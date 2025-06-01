using System;
using System.Windows;

namespace PixieCursors
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Config.CheckPath();
            Config.Language();           
            InitializeComponent();
        }
    }
}
