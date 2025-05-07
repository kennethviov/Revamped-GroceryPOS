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
    /// Interaction logic for ReceiptUnit.xaml
    /// </summary>
    public partial class ReceiptUnit : UserControl
    {
        public ReceiptUnit()
        {
            InitializeComponent();
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

        public int Quantity
        {
            get { return int.Parse(quantity.Text); }
            set { quantity.Text = value.ToString(); }
        }

        public double Amount
        {
            get { return double.Parse(amount.Text); }
            set { amount.Text = value.ToString("N2"); }
        }
    }
}
