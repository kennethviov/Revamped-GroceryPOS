using System;
using System.CodeDom;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Revamped_GroceryPOS.Components
{
    /// <summary>
    /// Interaction logic for CartItem.xaml
    /// </summary>
    public partial class CartItem : UserControl
    {
        public CartItem()
        {
            InitializeComponent();
        }

        public ImageSource Image
        {
            get { return image.Source; }
            set { image.Source = value; }
        }

        public new string Name
        {
            get { return name.Text; }
            set { name.Text = value; }
        }

        public double Price
        {
            get { return double.Parse(price.Text); }
            set { price.Text = value.ToString("N2"); }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                quantity.Text = value.ToString();
                parent?.UpdateCartUI(); // Trigger update here
            }
        }

        private void Incrementer_Click(object sender, RoutedEventArgs e)
        {
            if (Quantity >= MAX_QUANTITY) return;
            Quantity++;
        }

        private void Decrementer_Click(object sender, RoutedEventArgs e)
        {
            if (Quantity <= 0)
            {
                Remove();
                parent?.UpdateCartUI();
                return;
            }
            Quantity--;
        }

        public void Remove()
        {
            var parent = this.Parent as Panel;
            if (parent != null)
            {
                parent.Children.Remove(this);
            }
        }

        public void SetParent(MainWindow parent)
        {
            this.parent = parent;
        }

        private const int MAX_QUANTITY = 100;
        private MainWindow parent;
    }
}
