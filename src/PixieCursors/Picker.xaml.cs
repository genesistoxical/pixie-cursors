using System.Windows;

namespace PixieCursors
{
    /// <summary>
    /// ColorPicker
    /// </summary>
    public partial class Picker : Window
    {
        public Picker()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
