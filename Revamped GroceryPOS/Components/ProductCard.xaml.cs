using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for ProductCard.xaml
    /// </summary>
    public partial class ProductCard : UserControl
    {
        public ProductCard()
        {
            InitializeComponent();
        }

        public ImageSource ImageSource
        {
            get { return image.Source; }    
            set { image.Source = value; }
        }

        public required new string Name
        {
            get { return name.Text; }
            set { name.Text = value; }
        }

        public required string Price
        {
            get { return price.Text; }
            set { price.Text = value; }
        }

        public string Unit
        {
            get { return unit.Text; }
            set { unit.Text = value; }
        }

        public string? Category { get; set; }

        public string? Description { get; set; }

        public required int Quantity { get; set; }
    }
}
