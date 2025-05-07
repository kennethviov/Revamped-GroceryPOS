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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Revamped_GroceryPOS.Components
{
    /// <summary>
    /// Interaction logic for InventoryItem.xaml
    /// </summary>
    public partial class InventoryItem : UserControl
    {
        public InventoryItem()
        {
            InitializeComponent();
        }

        public int? ID
        {
            get { return int.TryParse(id.Text, out int idValue) ? idValue : null; }
            set { id.Text = value?.ToString(); }
        }

        public ImageSource ImageSource
        {
            get { return image.Source; }
            set { image.Source = value; }
        }

        public string Name
        {
            get { return name.Text; }
            set { name.Text = value; }
        }

        public double Price
        {
            get { return double.Parse(price.Text); }
            set { price.Text = value.ToString("N2"); }
        }

        public string? SoldBy
        {
            get { return soldby.Text; }
            set { soldby.Text = value; }
        }

        public string? Category
        {
            get { return category.Text; }
            set { category.Text = value; }
        }

        public int Stock
        {
            get { return int.Parse(stock.Text); }
            set { stock.Text = value.ToString(); }
        }
    }
}
