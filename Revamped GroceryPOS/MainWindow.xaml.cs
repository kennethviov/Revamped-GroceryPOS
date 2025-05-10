using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
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
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Revamped_GroceryPOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<Item> items;
        private List<ProductCard> productCards;
        private List<CartItem> cartItems = new List<CartItem>();
        private List<Transaction> transactions;
        private DataHandler dh;


        public MainWindow()
        {
            InitializeComponent();

            previmg = ClickedAllItems;
            prevbrdr = ReportsPanel;
            dh = new DataHandler();
            
            productCards = new List<ProductCard>();

            LoadProducts();
            LoadCartesianChart();
            LoadTransactions();
            LoadInfoCards(transactions);
            LoadPieChart();
            LoadInventory();
        }

        /*
         * Helper Methods
         * 
         */
        private void LoadProducts()
        {
            StoreToProductCard();

            ProductsWrapPanel.Children.Clear();

            foreach (var productCard in productCards)
            {
                productCard.MouseDoubleClick += ProductCard_Click;
                ProductsWrapPanel.Children.Add(productCard);
            }
        }

        private void StoreToProductCard()
        {
            items = dh.LoadItemsFromDatabase();

            foreach (var item in items)
            {
                ProductCard productCard = new ProductCard()
                {
                    Name = item.Name,
                    Price = item.Price,
                    Unit = item.SoldBy,
                    ImageSource = (item.Image?.Source as BitmapImage) ??
                         new BitmapImage(new Uri("pack://application:,,,/Resources/Products/broken-image.png")),
                    Category = item.Category,
                    Description = item.Description,
                    Quantity = item.Stock
                };

                productCard.Margin = new Thickness(5);

                productCards.Add(productCard);
            }
        }

        private void UpdateSummary()
        {
            double subtotal = Calculators.CalculateSubtotal(cartItems);
            int discountP = Calculators.DetermineDiscountP(subtotal);
            double discount = Calculators.CalculateDiscount(subtotal, discountP);
            double total = Calculators.CalculateTotal(subtotal, discount);

            subtotalTxtblk.Text = "₱ " + subtotal.ToString("N2");
            discountpTxtblk.Text = $"{discountP}%";
            discountTxtblk.Text = "₱ " + discount.ToString("N2");
            totalTxtblk.Text = "₱ " + total.ToString("N2");
        }

        private void DisplayCategory(string category)
        {
            ProductsWrapPanel.Children.Clear();

            if (category == "all items")
            {
                foreach (var product in productCards)
                {
                    ProductsWrapPanel.Children.Add(product);
                }

                return;
            }

            foreach (var product in productCards)
            {
                if (product.Category == category)
                {
                    ProductsWrapPanel.Children.Add(product);
                }
            }
        }

        public void UpdateCartUI()
        {
            Dispatcher.Invoke(() =>
            {
                CartWrapPanel.Children.Clear();

                cartItems.RemoveAll(item => item.Quantity <= 0);

                foreach (var item in cartItems.AsEnumerable().Reverse())
                {
                    CartWrapPanel.Children.Add(item);
                }

                UpdateSummary();
            });
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
         * Product Card Click
         *
         */
        private void ProductCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ProductCard productcard)
            {

                var existingItem = cartItems.FirstOrDefault(item => item.Name == productcard.Name);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                    existingItem.SetParent(this);
                }
                else
                {
                    CartItem item = new CartItem();
                    item.SetParent(this);
                    item.Name = productcard.Name;
                    item.Price = productcard.Price;
                    item.Image = productcard.ImageSource;
                    item.Quantity = 1;

                    item.Margin = new Thickness(0, 5, 0, 0);

                    cartItems.Add(item);
                    CartWrapPanel.Children.Add(item);
                }
            }
            UpdateCartUI();
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

        string currCat = "all items";
        Image previmg;
        Border prevbrdr;
        private void CatBtns_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && !string.IsNullOrEmpty(button.Name))
            {
                string btn = button.Name;

                if (currCat == btn) return;

                currCat = btn;

                if (btn == "vegetablesBtn" || btn == "cvegetablesBtn")
                {
                    DisplayCategory("vegetables");
                    ClickedVegetables.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = ClickedVegetables;
                }
                else if (btn == "meatsBtn" || btn == "cmeatsBtn")
                {
                    DisplayCategory("meats");
                    ClickedMeats.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = ClickedMeats;
                }
                else if (btn == "fruitsBtn" || btn == "cfruitsBtn")
                {
                    DisplayCategory("fruits");
                    ClickedFruits.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = ClickedFruits;
                }
                else if (btn == "drinksBtn" || btn == "cdrinksBtn")
                {
                    DisplayCategory("drinks");
                    ClickedDrinks.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = ClickedDrinks;
                }
                else if (btn == "liquorBtn" || btn == "cliquorBtn")
                {
                    DisplayCategory("liquor");
                    ClickedLiquor.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = ClickedLiquor;
                }
                else if (btn == "allItemsBtn" || btn == "callItemsBtn")
                {
                    DisplayCategory("all items");
                    ClickedAllItems.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = ClickedAllItems;
                }
                else if (btn == "reportsBtn")
                {
                    ReportsPanel.Visibility = Visibility.Visible;
                    clickedreports.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = clickedreports;
                    prevbrdr.Visibility = Visibility.Hidden;
                    prevbrdr = ReportsPanel;
                }
                else if (btn == "inventoryBtn")
                {
                    InventoryPanel.Visibility = Visibility.Visible;
                    clickedinv.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = clickedinv;
                    prevbrdr.Visibility = Visibility.Hidden;
                    prevbrdr = InventoryPanel;
                }
                if (btn[0] == 'c')
                {
                    KeepCollapsedSidePanel_Click(sender, e);
                }
            }
        }

        private void AdminBtn_Click(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Visible;
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

        /*
         * Mouse Events in the Cart
         *
         */
        private void ClearCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is Empty", "Cart Status", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear your order?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CartWrapPanel.Children.Clear();
                    cartItems.Clear();
                    UpdateSummary();
                }
            }
        }

        private void CheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateCartUI();

            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is Empty", "Cart Status", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to checkout?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var ci in cartItems)
                {
                    foreach (var item in items)
                    {
                        if (item.Name == ci.Name)
                        {
                            item.Stock -= ci.Quantity;
                        }
                    }
                }

                double totalAmount = double.Parse(
                    totalTxtblk.Text.Substring(1).Replace(",", ""),
                    System.Globalization.CultureInfo.InvariantCulture);

                int salesID = dh.AddNewSale(totalAmount);
                int percent = int.Parse(Regex.Match(discountpTxtblk.Text, @"\d+\.?\d*").Value);
                dh.AddSalesDetails(salesID, cartItems, percent);
                RefreshAdminPanel();

                Receipt rec = new Receipt(cartItems);
                rec.ShowDialog();

                ProductsWrapPanel.Children.Clear();
                CartWrapPanel.Children.Clear();
                cartItems.Clear();
                productCards.Clear();
                UpdateSummary();
                LoadProducts();
            }
        }




        /* 
         *
         *
         * 
         *
         *
         * Admin Panel
         *
         *
         *
         *
         */
        private List<ItemHolder> ihs = new List<ItemHolder>();

        public ISeries[] MyPieSeries { get; set; }
        public ISeries[] MySeries { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        private void LoadCartesianChart()
        {
            MySeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 2000, 3600, 4500, 5500, 7800 },
                    Name = "Projections",
                    Fill = null,
                    GeometrySize = 8,
                    Stroke = new SolidColorPaint(SKColors.LightBlue, 2)
                },
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new List<string> { "May", "June", "July", "August", "September" },
                    TextSize = 11,
                    LabelsRotation = 45,
                    Name = "Month"
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Amount",
                    Labeler = value => value.ToString("C0"),
                }
            };

            DataContext = this;
        }

        private void LoadPieChart()
        {
            ihs = ihs.OrderByDescending(x => x.Quantity).ToList();

            switch (ihs.Count)
            {
                case 0:
                    break;
                case 1:
                    MyPieSeries = new ISeries[]
                    {
                        new PieSeries<double> { Values = new double[] { ihs[0].Quantity }, Name = ihs[0].Name }
                    };
                    DataContext = this;
                    break;
                case 2:
                    MyPieSeries = new ISeries[]
                    {
                        new PieSeries<double> { Values = new double[] { ihs[0].Quantity }, Name = ihs[0].Name },
                        new PieSeries<double> { Values = new double[] { ihs[1].Quantity }, Name = ihs[1].Name }
                    };
                    DataContext = this;
                    break;
                case 3:
                    MyPieSeries = new ISeries[]
                    {
                        new PieSeries<double> { Values = new double[] { ihs[0].Quantity }, Name = ihs[0].Name },
                        new PieSeries<double> { Values = new double[] { ihs[1].Quantity }, Name = ihs[1].Name },
                        new PieSeries<double> { Values = new double[] { ihs[2].Quantity }, Name = ihs[2].Name }
                    };
                    DataContext = this;
                    break;
                case 4:
                    MyPieSeries = new ISeries[]
                    {
                        new PieSeries<double> { Values = new double[] { ihs[0].Quantity }, Name = ihs[0].Name },
                        new PieSeries<double> { Values = new double[] { ihs[1].Quantity }, Name = ihs[1].Name },
                        new PieSeries<double> { Values = new double[] { ihs[2].Quantity }, Name = ihs[2].Name },
                        new PieSeries<double> { Values = new double[] { ihs[3].Quantity }, Name = ihs[3].Name }
                    };
                    DataContext = this;
                    break;
            }

            if (ihs.Count > 5)
            {
                MyPieSeries = new ISeries[]
                {
                    new PieSeries<double> { Values = new double[] { ihs[0].Quantity }, Name = ihs[0].Name },
                    new PieSeries<double> { Values = new double[] { ihs[1].Quantity }, Name = ihs[1].Name },
                    new PieSeries<double> { Values = new double[] { ihs[2].Quantity }, Name = ihs[2].Name },
                    new PieSeries<double> { Values = new double[] { ihs[3].Quantity }, Name = ihs[3].Name },
                    new PieSeries<double> { Values = new double[] { ihs[4].Quantity }, Name = ihs[4].Name }
                };
                DataContext = this;
            }
        }

        private void LoadTransactions()
        {
            transactions = dh.LoadSalesFromDatabase();

            foreach (var transaction in transactions.AsEnumerable().Reverse())
            {
                transaction.Margin = new Thickness(0, 5, 0, 5);
                transaction.MouseDoubleClick += Transaction_MouseDoubleClick;
                TransactionsWrapPanel.Children.Add(transaction);
            }
        }

        private void Transaction_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Receipt rec = new Receipt((Transaction)sender);
            rec.ShowDialog();
        }

        private void LoadInfoCards(List<Transaction> transactionss)
        {
            ihs = new List<ItemHolder>();
            int totaltransactions = transactionss.Count;
            double totalincome = transactionss.Sum(t => t.TotalAmount);
            int totalitems = 0;

            foreach (var transaction in transactionss)
            {
                Receipt rec = new Receipt(transaction);
                List<CartItem> cis = rec.GetCartItems();
                foreach (var ci in cis)
                {
                    totalitems += ci.Quantity;
                    if (ihs.Any(i => i.Name == ci.Name))
                    {
                        ihs.First(i => i.Name == ci.Name).Quantity += ci.Quantity;
                        continue;
                    }
                    ihs.Add(new ItemHolder()
                    {
                        Name = ci.Name,
                        Quantity = ci.Quantity
                    });
                }
            }
            
            TotalTransactions.Text = totaltransactions.ToString();
            TotalIncome.Text = "₱" + totalincome.ToString("N2");
            TotalItemsSold.Text = totalitems.ToString();
        }

        private void RefreshAdminPanel()
        {
            TransactionsWrapPanel.Children.Clear();
            LoadTransactions();
            LoadInfoCards(transactions);
            LoadInventory();
        }

        private void LoadInventory()
        {
            InventoryPane.Children.Clear();
            foreach (var item in items)
            {
                InventoryItem inventoryItem = new InventoryItem()
                {
                    ID = item.ID,
                    Name = item.Name,
                    Price = item.Price,
                    SoldBy = item.SoldBy,
                    ImageSource = (item.Image?.Source as BitmapImage) ??
                         new BitmapImage(new Uri("pack://application:,,,/Resources/Products/broken-image.png")),
                    Category = item.Category,
                    Stock = item.Stock
                };
                inventoryItem.Margin = new Thickness(0,5,0,5);
                InventoryPane.Children.Add(inventoryItem);
            }
        }

        /*
         * Mouse Events 
         *
         */

        /* Side Panel */
        string ccurrCat = "reports";
        Image cprevimg;
        Border cprevbrdr;
        private void AdmBtns_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && !string.IsNullOrEmpty(button.Name))
            {
                string btn = button.Name;

                if (currCat == btn) return;

                currCat = btn;

                if (btn == "reportsBtn")
                {
                    ReportsPanel.Visibility = Visibility.Visible;
                    clickedreports.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = clickedreports;
                    prevbrdr.Visibility = Visibility.Hidden;
                    prevbrdr = ReportsPanel;
                }
                else if (btn == "inventoryBtn")
                {
                    InventoryPanel.Visibility = Visibility.Visible;
                    clickedinv.Visibility = Visibility.Visible;
                    previmg.Visibility = Visibility.Hidden;
                    previmg = clickedinv;
                    prevbrdr.Visibility = Visibility.Hidden;
                    prevbrdr = InventoryPanel;
                }
                if (btn[0] == 'c')
                {
                    KeepCollapsedSidePanel_Click(sender, e);
                }
            }
        }


        private void StoreBtn_Click(object sender, RoutedEventArgs e)
        {
            AdminPanel.Visibility = Visibility.Hidden;
        }

        private void edititembtn_Click(object sender, RoutedEventArgs e)
        {
            EditItemPanel.Visibility = Visibility.Visible;
        }

        private void additembtn_Click(object sender, RoutedEventArgs e)
        {
            AddItemPanel.Visibility = Visibility.Visible;
        }

        private void deleteitembtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteItemPanel.Visibility = Visibility.Visible;
        }

        private void closeeditpanel_Click(object sender, RoutedEventArgs e)
        {
            EditItemPanel.Visibility = Visibility.Hidden;
        }

        private void closeaddpanel_Click(object sender, RoutedEventArgs e)
        {
            AddItemPanel.Visibility = Visibility.Hidden;
        }


        private void closedelpanel_Click(object sender, RoutedEventArgs e)
        {
            DeleteItemPanel.Visibility = Visibility.Hidden;
        }

        private void savechangesbtn_Click(object sender, RoutedEventArgs e)
        {
            // Check for empty fields
            if (string.IsNullOrWhiteSpace(EditItemID.Text) ||
                string.IsNullOrWhiteSpace(EditItemName.Text) ||
                string.IsNullOrWhiteSpace(EditItemPrice.Text) ||
                string.IsNullOrWhiteSpace(EditItemCat.Text) ||
                string.IsNullOrWhiteSpace(EditItemSoldBy.Text) ||
                string.IsNullOrWhiteSpace(EditItemStock.Text))
            {
                if (string.IsNullOrWhiteSpace(EditItemID.Text))
                {
                    editwarningblk.Text = "Please fill in the Item ID before saving changes.";
                    editwarningblk.Foreground = new SolidColorBrush(Colors.Red);
                    return; // Don't proceed with saving
                }
                
                var item = items.FirstOrDefault(i => i.ID == int.Parse(EditItemID.Text));
                if (item == null)
                {
                    editwarningblk.Text = "Item not found.";
                    editwarningblk.Foreground = new SolidColorBrush(Colors.Red);
                    return; // Don't proceed with saving
                }

                if (string.IsNullOrWhiteSpace(EditItemName.Text))
                {
                    EditItemName.Text = item.Name.ToString();
                }
                if (string.IsNullOrWhiteSpace(EditItemPrice.Text))
                {
                    EditItemPrice.Text = item.Price.ToString();
                }
                if (string.IsNullOrWhiteSpace(EditItemCat.Text))
                {
                    EditItemCat.Text = item.Category.ToString();
                }
                if (string.IsNullOrWhiteSpace(EditItemSoldBy.Text))
                {
                    EditItemSoldBy.Text = item.SoldBy.ToString();
                }
                if (string.IsNullOrWhiteSpace(EditItemStock.Text))
                {
                    EditItemStock.Text = item.Stock.ToString();
                }
            }

            try
            {
                int id = int.Parse(EditItemID.Text);
                string name = EditItemName.Text;
                double price = double.Parse(EditItemPrice.Text);
                string category = EditItemCat.Text;
                string soldby = EditItemSoldBy.Text;
                int stock = int.Parse(EditItemStock.Text);

                bool success = dh.UpdateItem(id, name, price, soldby, stock, category);

                if (success)
                {
                    editwarningblk.Text = "Item updated successfully!";
                    editwarningblk.Foreground = new SolidColorBrush(Colors.Green);
                    ClearAllFields();
                    RefreshUI();
                }
                else
                {
                    editwarningblk.Text = "Item update failed.";
                    editwarningblk.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (FormatException ex)
            {
                editwarningblk.Text = "Invalid input format. Please check numbers.";
                editwarningblk.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void addbtn_Click(object sender, RoutedEventArgs e)
        {
            // Check for empty fields
            if (string.IsNullOrWhiteSpace(AddItemName.Text) ||
                string.IsNullOrWhiteSpace(AddItemPrice.Text) ||
                string.IsNullOrWhiteSpace(AddItemCat.Text) ||
                string.IsNullOrWhiteSpace(AddItemSoldBy.Text) ||
                string.IsNullOrWhiteSpace(AddItemStock.Text))
            {
                addwarningblk.Text = "Please fill in all the fields before adding the item.";
                addwarningblk.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            try
            {
                string name = AddItemName.Text;
                double price = double.Parse(AddItemPrice.Text);
                string category = AddItemCat.Text;
                string soldby = AddItemSoldBy.Text;
                int stock = int.Parse(AddItemStock.Text);

                bool success = dh.AddItem(name, price, soldby, stock, category);

                if (success)
                {
                    addwarningblk.Text = "Item added successfully!";
                    addwarningblk.Foreground = new SolidColorBrush(Colors.Green);
                    ClearAllFields();
                    RefreshUI();
                }
                else
                {
                    addwarningblk.Text = "Failed to add item.";
                    addwarningblk.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (FormatException)
            {
                addwarningblk.Text = "Invalid number format. Please check the price and stock.";
                addwarningblk.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void deletebtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DeleteItemID.Text))
            {
                delwarningblk.Text = "Please fill in all the fields before proceeding.";
                delwarningblk.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            if (!int.TryParse(DeleteItemID.Text, out int id))
            {
                delwarningblk.Text = "Invalid ID format.";
                delwarningblk.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            bool success = dh.DeleteItem(id);
            if (success)
            {
                delwarningblk.Text = "Item deleted successfully!";
                delwarningblk.Foreground = new SolidColorBrush(Colors.Green);
                DeleteItemID.Text = string.Empty;
                ClearAllFields();
                RefreshUI();
            }
            else
            {
                delwarningblk.Text = "Item not found or could not be deleted.";
                delwarningblk.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void DownLoadReport_Click(object sender, RoutedEventArgs e)
        {

            if (FromDate.SelectedDate == null || ToDate.SelectedDate == null)
            {
                if (FromDate.SelectedDate == null && ToDate.SelectedDate == null)
                {
                    LoadTransactions();
                    LoadInfoCards(transactions);
                    return;
                } else
                {
                    MessageBox.Show("Please select a date range.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            List<Transaction> transactionnss = new List<Transaction>();
            TransactionsWrapPanel.Children.Clear();

            DateOnly fromDate = DateOnly.FromDateTime(FromDate.SelectedDate.Value);
            DateOnly toDate = DateOnly.FromDateTime(ToDate.SelectedDate.Value);

            foreach (var transaction in transactions)
            {
                DateOnly date = DateOnly.FromDateTime(transaction.TransactionDateTime);

                if (date >= fromDate && date <= toDate)
                {
                    transaction.Margin = new Thickness(0, 5, 0, 5);
                    transaction.MouseDoubleClick += Transaction_MouseDoubleClick;
                    TransactionsWrapPanel.Children.Add(transaction);
                    transactionnss.Add(transaction);
                }
            }

            LoadInfoCards(transactionnss);

            double totalinc = double.Parse(TotalIncome.Text.ToString().Substring(1));
            string salesFileName = "SalesReport_" + fromDate.ToString("yyyy-MM-dd") + "-" + toDate.ToString("yyyy-MM-dd") + ".pdf";

            Exporter.ExportSalesReportToPDF(
                fromDate: fromDate,
                toDate: toDate,
                transactions: transactionnss,
                totalTransactions: int.Parse(TotalTransactions.Text),
                totalIncome: totalinc,
                totalItems: int.Parse(TotalItemsSold.Text),
                filePath: "C:\\Users\\kenne\\Downloads\\" + salesFileName
            );
        }



        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (FromDate.SelectedDate == null || ToDate.SelectedDate == null)
            {
                if (string.IsNullOrWhiteSpace(FromDate.ToString()) && string.IsNullOrWhiteSpace(ToDate.ToString()))
                {
                    LoadTransactions();
                    LoadInfoCards(transactions);
                    return;
                }
                else
                {
                    MessageBox.Show("Please select a date range.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            List<Transaction> transactionnss = new List<Transaction>();
            TransactionsWrapPanel.Children.Clear();

            DateOnly fromDate = DateOnly.FromDateTime(FromDate.SelectedDate.Value);
            DateOnly toDate = DateOnly.FromDateTime(ToDate.SelectedDate.Value);

            foreach (var transaction in transactions)
            {
                DateOnly date = DateOnly.FromDateTime(transaction.TransactionDateTime);

                if (date >= fromDate && date <= toDate)
                {
                    transaction.Margin = new Thickness(0, 5, 0, 5);
                    transaction.MouseDoubleClick += Transaction_MouseDoubleClick;
                    TransactionsWrapPanel.Children.Add(transaction);
                    transactionnss.Add(transaction);
                }
            }

            LoadInfoCards(transactionnss);
        }

        private void ClearAllFields()
        {
            AddItemName.Text = "";
            AddItemPrice.Text = "";
            AddItemCat.Text = "";
            AddItemSoldBy.Text = "";
            AddItemStock.Text = "";

            EditItemID.Text = "";
            EditItemName.Text = "";
            EditItemPrice.Text = "";
            EditItemCat.Text = "";
            EditItemStock.Text = "";
            EditItemSoldBy.Text = "";

            DeleteItemID.Text = "";
        }

        private void RefreshUI()
        {
            items.Clear();
            productCards.Clear();
            transactions.Clear();
            

            ProductsWrapPanel.Children.Clear();
            TransactionsWrapPanel.Children.Clear();
            InventoryPane.Children.Clear();

            LoadProducts();
            LoadTransactions();
            LoadInfoCards(transactions);
            LoadInventory();
        }
    }
}