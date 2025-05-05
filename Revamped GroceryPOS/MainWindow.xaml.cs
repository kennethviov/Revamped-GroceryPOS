using Revamped_GroceryPOS.Components;
using Revamped_GroceryPOS.Utilities;
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

        private List<Item> items;
        private List<ProductCard> productCards;
        private DataHandler dh;

        public MainWindow()
        {
            InitializeComponent();

            dh = new DataHandler();
            items = dh.LoadItemsFromDatabase();
            productCards = new List<ProductCard>();

            LoadProducts();
        }

        /*
         * Helper Methods
         * 
         */
        private void LoadProducts()
        {
            StoreToProductCard();

            foreach (var productCard in productCards)
            {
                ProductsWrapPanel.Children.Add(productCard);
            }
        }

        private void StoreToProductCard()
        {
            foreach (var item in items)
            {
                ProductCard productCard = new ProductCard()
                {
                    Name = item.Name,
                    Price = item.Price.ToString("N2"),
                    Unit = item.SoldBy,
                    ImageSource = (item.Image?.Source as BitmapImage) ??
                         new BitmapImage(new Uri("pack://application:,,,/Resources/Products/broken-image.png")),
                    Category = item.Category,
                    Description = item.Description,
                    Quantity = item.Stock
                };

                productCard.Margin = new Thickness(10, 8, 10, 8);

                productCards.Add(productCard);
            }
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

        private void MinimizeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Minimize.Visibility = Visibility.Visible;
        }

        private void MinimizeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Minimize.Visibility = Visibility.Collapsed;
        }

        /*
         * Mouse Events in the SidePanel
         *
         */
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            MainFrameShield.Visibility = Visibility.Visible;
            CollapsedSidePanel.Visibility = Visibility.Visible;
        }



        /*
         * Mouse Events in the Collapsed SidePanel
         *
         */
        private void KeepCollapsedSidePanel_Click(object sender, RoutedEventArgs e)
        {
            MainFrameShield.Visibility = Visibility.Collapsed;
            CollapsedSidePanel.Visibility = Visibility.Collapsed;
        }
    }
}