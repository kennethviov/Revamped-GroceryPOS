using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for Transaction.xaml
    /// </summary>
    public partial class Transaction : UserControl
    {
        public Transaction()
        {
            InitializeComponent();
        }

        public int TransactionID 
        { 
            get { return int.Parse(transactionId.Text); }
            set { transactionId.Text = value.ToString(); }
        }

        public DateTime TransactionDateTime
        {
            get { return DateTime.Parse(dateandtime.Text); }
            set { dateandtime.Text = value.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public double TotalAmount
        {
            get { return double.Parse(total.Text, NumberStyles.Currency, CultureInfo.CurrentCulture); }
            set { total.Text = value.ToString("C2"); }
        }
    }
}
