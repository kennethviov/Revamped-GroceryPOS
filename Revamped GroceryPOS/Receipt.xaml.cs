using Revamped_GroceryPOS.Components;
using Revamped_GroceryPOS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Revamped_GroceryPOS
{
    /// <summary>
    /// Interaction logic for Receipt.xaml
    /// </summary>
    public partial class Receipt : Window
    {
        public Receipt(List<CartItem> cartItems)
        {
            InitializeComponent();
            this.cartItems = cartItems;

            DateTime now = DateTime.Now;
            dateandtime.Text = now.ToString();
        }

        public Receipt(Transaction trans)
        {
            InitializeComponent();

            DataHandler handler = new DataHandler();
            this.cartItems = handler.GetCartItemsFromSalesID(trans.TransactionID);
            dateandtime.Text = trans.TransactionDateTime.ToString();
        }

        private void UpdateSummary()
        {
            double subtotal = Calculators.CalculateSubtotal(cartItems);
            int discountP = Calculators.DetermineDiscountP(subtotal);
            double discount = Calculators.CalculateDiscount(subtotal, discountP);
            double total = Calculators.CalculateTotal(subtotal, discount);

            subtotaltxtblk.Text = "₱ " + subtotal.ToString("N2");
            discountptxtblk.Text = $"{discountP}%";
            discounttxtblk.Text = "₱ " + discount.ToString("N2");
            totaltxtblk.Text = "₱ " + total.ToString("N2");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in cartItems)
            {

                if (item.Quantity == 0) continue;

                ReceiptWrapPanel.Children.Add(new ReceiptUnit()
                {
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Amount = item.Price * item.Quantity,
                });
            }

            UpdateSummary();
        }

        private void Backbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public List<CartItem> GetCartItems()
        {
            return cartItems;
        }

        private readonly List<CartItem> cartItems;
    }
}
