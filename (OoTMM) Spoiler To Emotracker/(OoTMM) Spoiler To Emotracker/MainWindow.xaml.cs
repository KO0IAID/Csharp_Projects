using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OoTMMSpoilerToTracker.SpoilerLog.Controller;
using OoTMMSpoilerToTracker.Tracker;
using OoTMMSpoilerToTracker.Tracker.Controller;

namespace _OoTMM__Spoiler_To_Emotracker
{
    public partial class MainWindow : Window
    {
        Tracker tracker = new();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging with left mouse button
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // Double-click to toggle maximize/restore
                    this.WindowState = this.WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;
                }
                else
                {
                    this.DragMove();
                }
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        private void MainGrid_DragOver(object sender, DragEventArgs e)
        {
            // Only allow if data contains files
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Show copy cursor
            }
            else
            {
                e.Effects = DragDropEffects.None; // No drop allowed
            }

            e.Handled = true;
        }

        private void MainGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string message = "Dropped files:\n" + string.Join("\n", files);
                MessageBox.Show(message, "Files Dropped", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            e.Handled = true;
        }

    }
}