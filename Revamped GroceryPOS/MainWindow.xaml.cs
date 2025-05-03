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

namespace Revamped_GroceryPOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetStage();
        }

        private void SetStage()
        {
            Close.Visibility = Visibility.Collapsed;
        }

        /*  
         *  Mouse events in the Docker
         *
        */

        private void Docker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Close.Visibility = Visibility.Visible;
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Close.Visibility = Visibility.Collapsed;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
    }
}