using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Microsoft.Data.SqlClient;
using System.Resources;
using System.Reflection;
using System.Data;
using Revamped_GroceryPOS.Utilities;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Revamped_GroceryPOS.Utilities
{
    internal class DataHandler
    {
        readonly string connectionString;
        readonly string itemquery;
        readonly string salesquery;

        public DataHandler()
        {
            connectionString = "Data Source = (LocalDB)\\MSSQLLocalDB; Database = protoDB; Integrated Security = True; Connect Timeout = 30; Encrypt = False; Trust Server Certificate = False; Application Intent = ReadWrite; Multi Subnet Failover = False";
            itemquery = "SELECT\r\n    I.item_id,\r\n    I.item_name,\r\n    I.item_price,\r\n    I.item_unit,\r\n    I.item_stocks,\r\n    C.category_name AS category_description,\r\n    I.item_description\r\nFROM Items I\r\nJOIN Inventory C ON I.category_id = C.category_id";
            salesquery = "SELECT\r\n S.sales_id,\r\n S.sales_date,\r\n S.sales_total\r\n FROM Sales S";
        }

        public List<Item> LoadItemsFromDatabase()
        {
            List<Item> items = new List<Item>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(itemquery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Item item = new Item()
                            {
                                Name = reader["item_name"].ToString() ?? string.Empty,
                                Price = reader["item_price"] != DBNull.Value ? Convert.ToDouble(reader["item_price"]) : 0.0,
                                SoldBy = reader["item_unit"].ToString() ?? string.Empty,
                                Stock = reader["item_stocks"] != DBNull.Value ? Convert.ToInt32(reader["item_stocks"]) : 0,
                                Category = reader["category_description"]?.ToString()?.ToLower(),
                                Description = reader["item_description"]?.ToString()
                            };

                            item.Image = LoadProductImage(item.Name);

                            items.Add(item);
                        }
                    }
                }
            }

            return items;
        }

        private Image LoadProductImage(string productName)
        {
            Console.WriteLine($"Attempting to load image for: {productName}");

            try
            {
                var extensions = new[] { ".png", ".jpg", ".jpeg" };
                foreach (var ext in extensions)
                {
                    var uriString = $"pack://application:,,,/Resources/Products/{productName}{ext}";
                    Console.WriteLine($"Trying URI: {uriString}");

                    try
                    {
                        var uri = new Uri(uriString, UriKind.Absolute);
                        var bitmap = new BitmapImage(uri);
                        Console.WriteLine($"Successfully loaded: {uriString}");
                        return new Image { Source = bitmap };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load {uriString}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error loading image: {ex.Message}");
            }

            // Fallback to broken image
            Console.WriteLine("Using broken-image fallback");
            var fallbackUri = new Uri("pack://application:,,,/Resources/Products/broken-image.png", UriKind.Absolute);
            return new Image { Source = new BitmapImage(fallbackUri) };
        }

        //public List<SalesReport> LoadSalesReport()
        //{
        //    List<SalesReport> salesReports = new List<SalesReport>();
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        using (SqlCommand command = new SqlCommand(salesquery, connection))
        //        {
        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    SalesReport salesReport = new SalesReport()
        //                    {
        //                        SalesID = reader["sales_id"].ToString(),
        //                        DateAndTime = Convert.ToDateTime(reader["sales_date"]).ToString("dd-MM-yyyy HH:mm"),
        //                        Total = Convert.ToDouble(reader["sales_total"]).ToString("C")
        //                    };
        //                    salesReports.Add(salesReport);
        //                }
        //            }
        //        }
        //    }
        //    return salesReports;
        //}

        public void AddNewSale(double totalAmount)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT INTO Sales (sales_total) VALUES (@total)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.Add("@total", SqlDbType.Decimal).Value = Convert.ToDecimal(totalAmount);
                    command.Parameters["@total"].Precision = 10;
                    command.Parameters["@total"].Scale = 2;

                    int rowsAffected = command.ExecuteNonQuery();

                    //if (rowsAffected == 0)
                    //{
                    //    MessageBox.Show("Insert failed.");
                    //}
                    //else
                    //{
                    //    MessageBox.Show("Sale inserted successfully!");
                    //}
                }
            }
        }
    }
}
