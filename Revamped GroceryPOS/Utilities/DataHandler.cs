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
using Revamped_GroceryPOS.Components;
using System.IO;
using System.Windows;

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
                                ID = reader["item_id"] != DBNull.Value ? Convert.ToInt32(reader["item_id"]) : (int?)null,
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

        private System.Windows.Controls.Image LoadProductImage(string productName)
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
                        return new System.Windows.Controls.Image { Source = bitmap };
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
            return new System.Windows.Controls.Image { Source = new BitmapImage(fallbackUri) };
        }

        public List<Transaction> LoadSalesFromDatabase()
        {
            List<Transaction> salesReports = new List<Transaction>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(salesquery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Transaction salesReport = new Transaction()
                            {
                                TransactionID = Convert.ToInt32(reader["sales_id"]),
                                TransactionDateTime = Convert.ToDateTime(reader["sales_date"]),
                                TotalAmount = Convert.ToDouble(reader["sales_total"])
                            };
                            salesReports.Add(salesReport);
                        }
                    }
                }
            }
            return salesReports;
        }

        public List<CartItem> GetCartItemsFromSalesID(int salesID)
        {
            List<CartItem> cartItems = new List<CartItem>();

            string query = @"
                    SELECT I.item_name, D.price_per_item, D.item_quantity
                    FROM Sales_Details D
                    JOIN Items I ON D.item_name = I.item_id
                    WHERE D.sales_id = @SalesID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SalesID", salesID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CartItem item = new CartItem
                            {
                                Name = reader["item_name"].ToString(),
                                Price = Convert.ToDouble(reader["price_per_item"]),
                                Quantity = Convert.ToInt32(reader["item_quantity"])
                            };

                            cartItems.Add(item);
                        }
                    }
                }
            }

            return cartItems;
        }

        public int AddNewSale(double totalAmount)
        {
            int newSalesID;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertQuery = @"
                    INSERT INTO Sales (sales_total) 
                    VALUES (@total); 
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.Add("@total", SqlDbType.Decimal).Value = Convert.ToDecimal(totalAmount);
                    command.Parameters["@total"].Precision = 10;
                    command.Parameters["@total"].Scale = 2;

                    // ExecuteScalar returns the result of SELECT SCOPE_IDENTITY()
                    newSalesID = (int)command.ExecuteScalar();
                }
            }

            return newSalesID;
        }

        public void AddSalesDetails(int salesID, List<CartItem> cartItems, double discountPercent)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (CartItem item in cartItems)
                {
                    int itemID = GetItemIDByName(item.Name, connection);
                    double subtotal = item.Price * item.Quantity;
                    double discountAmount = subtotal * (discountPercent / 100);

                    // ✅ Check current stock
                    int currentStock = GetCurrentStock(itemID, connection);
                    if (currentStock < item.Quantity)
                    {
                        throw new Exception($"Insufficient stock for item '{item.Name}'. Requested: {item.Quantity}, Available: {currentStock}");
                    }

                    // 📥 Insert sales detail
                    string insertQuery = @"
                        INSERT INTO Sales_Details
                        (sales_id, item_name, item_quantity, price_per_item, sales_subtotal, sales_discount)
                        VALUES
                        (@sales_id, @item_name, @item_quantity, @price_per_item, @sales_subtotal, @sales_discount)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@sales_id", salesID);
                        command.Parameters.AddWithValue("@item_name", itemID);
                        command.Parameters.AddWithValue("@item_quantity", item.Quantity);
                        command.Parameters.AddWithValue("@price_per_item", item.Price);
                        command.Parameters.AddWithValue("@sales_subtotal", subtotal);
                        command.Parameters.AddWithValue("@sales_discount", discountAmount);
                        command.ExecuteNonQuery();
                    }

                    string updateStockQuery = @"
                        UPDATE Items
                        SET item_stocks = item_stocks - @quantity
                        WHERE item_id = @item_id";

                    using (SqlCommand updateCommand = new SqlCommand(updateStockQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                        updateCommand.Parameters.AddWithValue("@item_id", itemID);
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private int GetItemIDByName(string itemName, SqlConnection connection)
        {
            string query = "SELECT item_id FROM Items WHERE item_name = @name";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", itemName);
                object result = command.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);
                else
                    throw new Exception($"Item '{itemName}' not found in database.");
            }
        }

        private int GetCurrentStock(int itemID, SqlConnection connection)
        {
            string query = "SELECT item_stocks FROM Items WHERE item_id = @id";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", itemID);
                object result = command.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);
                else
                    throw new Exception($"Stock info not found for item ID {itemID}.");
            }
        }

        public bool AddItem(string name, double price, string soldBy, int stock, string categoryName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Step 1: Get category_id and inventory_id from Inventory table
                    string lookupQuery = "SELECT category_id, inventory_id FROM Inventory WHERE category_name = @category";
                    int categoryId = 0, inventoryId = 0;

                    using (SqlCommand lookupCommand = new SqlCommand(lookupQuery, connection))
                    {
                        lookupCommand.Parameters.AddWithValue("@category", categoryName);

                        using (SqlDataReader reader = lookupCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                categoryId = Convert.ToInt32(reader["category_id"]);
                                inventoryId = Convert.ToInt32(reader["inventory_id"]);
                            }
                            else
                            {
                                throw new Exception($"Category '{categoryName}' not found in Inventory.");
                            }
                        }
                    }

                    // Step 2: Insert the item into Items table
                    string insertQuery = @"
                INSERT INTO Items 
                    (item_name, item_price, item_unit, item_stocks, category_id, inventory_id)
                VALUES 
                    (@name, @price, @soldBy, @stock, @categoryId, @inventoryId)";

                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@name", name);
                        insertCommand.Parameters.AddWithValue("@price", price);
                        insertCommand.Parameters.AddWithValue("@soldBy", soldBy);
                        insertCommand.Parameters.AddWithValue("@stock", stock);
                        insertCommand.Parameters.AddWithValue("@categoryId", categoryId);
                        insertCommand.Parameters.AddWithValue("@inventoryId", inventoryId);

                        int rowsAffected = insertCommand.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add item:\n{ex.Message}", "Insert Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        public bool UpdateItem(int id, string name, double price, string soldBy, int stock, string categoryName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get the category_id and inventory_id from the Inventory table based on category name
                    string lookupQuery = "SELECT category_id, inventory_id FROM Inventory WHERE category_name = @category";
                    int categoryId = 0, inventoryId = 0;

                    using (SqlCommand lookupCommand = new SqlCommand(lookupQuery, connection))
                    {
                        lookupCommand.Parameters.AddWithValue("@category", categoryName);

                        using (SqlDataReader reader = lookupCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                categoryId = Convert.ToInt32(reader["category_id"]);
                                inventoryId = Convert.ToInt32(reader["inventory_id"]);
                            }
                            else
                            {
                                throw new Exception("Category not found in Inventory table.");
                            }
                        }
                    }

                    // Update the item
                    string updateQuery = @"
                                UPDATE Items
                                SET item_name = @name,
                                    item_price = @price,
                                    item_unit = @soldBy,
                                    item_stocks = @stock,
                                    category_id = @categoryId,
                                    inventory_id = @inventoryId
                                WHERE item_id = @id";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@soldBy", soldBy);
                        command.Parameters.AddWithValue("@stock", stock);
                        command.Parameters.AddWithValue("@categoryId", categoryId);
                        command.Parameters.AddWithValue("@inventoryId", inventoryId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update item:\n{ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool DeleteItem(int itemId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string deleteQuery = "DELETE FROM Items WHERE item_id = @id";

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", itemId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete item:\n{ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void CopyFileToProductResources(string sourceFilePath)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    MessageBox.Show("Source file does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string fileName = Path.GetFileName(sourceFilePath);

                string appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
                string destDir = Path.Combine(appBaseDir, "Resources", "Products");

                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                string destFilePath = Path.Combine(destDir, fileName);

                File.Copy(sourceFilePath, destFilePath, true);

                MessageBox.Show($"File copied to: {destFilePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying file:\n{ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
