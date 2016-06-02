using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Assignment4
{
    class Program
    {
        //displays menu options
        static void displayMenu()
        {
            Console.WriteLine("1. Display product description, price, and quantity of Product");
            Console.WriteLine("2. Create new order");
            Console.WriteLine("3. Display price and description for order");
            Console.WriteLine("4. Print order description for customer");
            Console.WriteLine("5. Count and order total for all customers");
            Console.WriteLine("6. Count and total order greater that $100 per cust.");
            Console.WriteLine("7. Total revenue for all products");
            Console.WriteLine("8. Top 5 selling products by revenue");
            Console.WriteLine("9. Top 5 selling products by volume");
            Console.WriteLine("10. Insert new product");
            Console.WriteLine("11. Update inventory for item");
            Console.WriteLine("12. Update product description");
            Console.WriteLine("13. Delete item from inventory");
            Console.WriteLine("14. Quit");
        }

        //print product list
        static void printProductList(MySqlConnection conn, DataTable table)
        {
            try
            {
                string sql = @"
                                SELECT *
                                FROM HW_Product;";
                MySqlDataAdapter da = new MySqlDataAdapter (sql, conn);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //1 - list product details
        static void disDescPriceQuantity(MySqlConnection conn, DataTable table)
        {
            Console.WriteLine("Enter product ID: ");
            string id = Console.ReadLine();

            try
            {
                string sql = @"
                                SELECT Description, Price, Inventory 
                                FROM HW_Product 
                                WHERE Product_ID = @ID;";
                MySqlDataAdapter da = new MySqlDataAdapter (sql, conn);

                da.SelectCommand.Parameters.AddWithValue("@ID", id);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //2 - new order
        static void newOrder(MySqlConnection conn, MySqlDataAdapter da)
        {
            Console.WriteLine("Enter first name ");
            string first = Console.ReadLine();
            Console.WriteLine("Enter last name  ");
            string last = Console.ReadLine();
            Console.WriteLine("Enter product ID: ");
            string product = Console.ReadLine();
            Console.WriteLine("Enter quantity: ");
            string quantity = Console.ReadLine();

            try
            {
                string cSql = @"
                                INSERT INTO HW_Customers(FirtName, LastName) 
                                VALUE (@First, @Last); 
                                SELECT LAST_INSERT_ID();";
                MySqlCommand custCommand = new MySqlCommand(cSql, conn);
                da.InsertCommand = custCommand;
                da.InsertCommand.Parameters.AddWithValue("@First", first);
                da.InsertCommand.Parameters.AddWithValue("@Last", last);

                conn.Open();
                var custID = da.InsertCommand.ExecuteScalar();

                try
                {
                    string oSql = @"
                                INSERT INTO HW_Orders(Customer_ID, Product_ID, Quantity) 
                                VALUE (@CustID, @prodID, @Quantity);";
                    MySqlCommand orderCommand = new MySqlCommand(oSql, conn);
                    da.InsertCommand = orderCommand;
                    da.InsertCommand.Parameters.AddWithValue("@CustID", custID);
                    da.InsertCommand.Parameters.AddWithValue("@prodID", product);
                    da.InsertCommand.Parameters.AddWithValue("@Quantity", quantity);

                    da.InsertCommand.ExecuteNonQuery();

                    string uSql = @"
                                    UPDATE HW_Product 
                                    SET Inventory = Inventory - @Quantity
                                    WHERE Product_ID = @prodID;";
                    MySqlCommand updateCommand = new MySqlCommand(uSql, conn);
                    da.UpdateCommand = updateCommand;
                    da.UpdateCommand.Parameters.AddWithValue("@Quantity", quantity);
                    da.UpdateCommand.Parameters.AddWithValue("@prodID", product);
                    da.UpdateCommand.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            conn.Close();
        }

        //3 - product for a given order
        static void disPriceDesc(MySqlConnection conn, DataTable table)
        {
            Console.WriteLine("Enter Order ID: ");
            string order = Console.ReadLine();
            try
            {
                string sql = @"
                                SELECT Price * HW_Orders.Quantity, Description
                                FROM HW_Product
                                INNER JOIN HW_Orders ON HW_Orders.Product_ID = HW_Product.Product_ID
                                WHERE HW_Orders.Order_ID = @orderID;";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                da.SelectCommand.Parameters.AddWithValue("@orderID", order);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //4 print order description
        static void disOrderDescription(MySqlConnection conn, DataTable table)
        {
            Console.WriteLine("Enter Customer ID: ");
            string customer = Console.ReadLine();
            try
            {
                string sql = @"
                                SELECT Description
                                from HW_Product
                                INNER JOIN HW_Orders ON HW_Orders.Product_ID = HW_Product.Product_ID
                                  INNER JOIN HW_Customers ON HW_Customers.Customer_ID = HW_Orders.Customer_ID
                                WHERE HW_Customers.Customer_ID = @customer;";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                da.SelectCommand.Parameters.AddWithValue("@customer", customer);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //5 count and order total
       static void countTotalCust(MySqlConnection conn, DataTable table)
        {
            try
            {
                string sql = @"
                                SELECT HW_Customers.FirtName, HW_Customers.LastName, HW_Orders.Quantity, Price * HW_Orders.Quantity
                                from HW_Product
                                INNER JOIN HW_Orders ON HW_Orders.Product_ID = HW_Product.Product_ID
                                  INNER JOIN HW_Customers ON HW_Customers.Customer_ID = HW_Orders.Customer_ID
                                GROUP BY HW_Customers.Customer_ID;";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //6 display customer orders greater than 100
        static void countGreat100(MySqlConnection conn, DataTable table)
        {
            try
            {
                string sql = @"
                                SELECT HW_Customers.FirtName, HW_Customers.LastName, HW_Orders.Quantity, Price * HW_Orders.Quantity
                                from HW_Product
                                INNER JOIN HW_Orders ON HW_Orders.Product_ID = HW_Product.Product_ID
                                  INNER JOIN HW_Customers ON HW_Customers.Customer_ID = HW_Orders.Customer_ID
                                WHERE Price * HW_Orders.Quantity > 100
                                GROUP BY HW_Customers.Customer_ID;";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //7 - total revenu
        static void totalRevenue(MySqlConnection conn, DataTable table)
        {
            try
            {
                string sql = @"
                                SELECT SUM(Price * HW_Orders.Quantity)
                                FROM HW_Product
                                INNER JOIN HW_Orders ON HW_Orders.Product_ID = HW_Product.Product_ID;";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //8 - top 5 revenue
        static void top5Revenue(MySqlConnection conn, DataTable table)
        {
            try
            {
                string sql = @"
                                SELECT Description, SUM(Price * HW_Orders.Quantity)
                                FROM HW_Product
                                INNER JOIN HW_Orders ON HW_Orders.Product_ID = HW_Product.Product_ID
                                GROUP BY Description
                                ORDER BY SUM(Price * HW_Orders.Quantity) DESC LIMIT 5;";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //9 - top 5 volume
        static void top5Volume(MySqlConnection conn, DataTable table)
        {
            try
            {
                string sql = @"
                                SELECT Description, SUM(Quantity)
                                FROM HW_Product
                                INNER JOIN HW_Orders ON HW_Orders.Product_ID = HW_Product.Product_ID
                                GROUP BY Description
                                ORDER BY SUM(Quantity) DESC LIMIT 5;";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                table.Reset();
                da.Fill(table);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            printTable(table);
        }

        //10 - add new product
        static void addProduct(MySqlConnection conn, MySqlDataAdapter da)
        {
            Console.WriteLine("Enter product description: ");
            string desc = Console.ReadLine();
            Console.WriteLine("Enter price:  ");
            string price = Console.ReadLine();
            Console.WriteLine("Enter quantity to add: ");
            string inventory = Console.ReadLine();

            try
            {
                string sql = @"
                                INSERT INTO HW_Product(Description, Price, Inventory) 
                                VALUE (@description, @price, @quantity);";
                MySqlCommand command = new MySqlCommand(sql, conn);
                da.InsertCommand = command;
                da.InsertCommand.Parameters.AddWithValue("@description", desc);
                da.InsertCommand.Parameters.AddWithValue("@price", price);
                da.InsertCommand.Parameters.AddWithValue("@quantity", inventory);

                conn.Open();
                da.InsertCommand.ExecuteNonQuery();
                conn.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //11 - update inventory
        static void updateInventory(MySqlConnection conn, MySqlDataAdapter da)
        {
            Console.WriteLine("Enter product ID: ");
            string id = Console.ReadLine();
            Console.WriteLine("Enter number to add: ");
            string quantity = Console.ReadLine();

            try
            {
                string sql = @"
                                UPDATE HW_Product 
                                SET Inventory = Inventory + @Quantity
                                WHERE Product_ID = @ID;";
                MySqlCommand command = new MySqlCommand(sql, conn);
                da.UpdateCommand = command;
                da.UpdateCommand.Parameters.AddWithValue("@Quantity", quantity);
                da.UpdateCommand.Parameters.AddWithValue("@ID", id);

                conn.Open();
                da.UpdateCommand.ExecuteNonQuery();
                conn.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //12 - update description
        static void updateDesc(MySqlConnection conn, MySqlDataAdapter da)
        {
            Console.WriteLine("Enter product ID: ");
            string id = Console.ReadLine();
            Console.WriteLine("Enter new description: ");
            string desc = Console.ReadLine();

            try
            {
                string sql = @"
                                UPDATE HW_Product 
                                SET Description = @Description
                                WHERE Product_ID = @ID;";
                MySqlCommand command = new MySqlCommand(sql, conn);
                da.UpdateCommand = command;
                da.UpdateCommand.Parameters.AddWithValue("@Description", desc);
                da.UpdateCommand.Parameters.AddWithValue("@ID", id);

                conn.Open();
                da.UpdateCommand.ExecuteNonQuery();
                conn.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //13 - delete product
        static void deleteProduct(MySqlConnection conn, MySqlDataAdapter da)
        {
            Console.WriteLine("Enter Product ID: ");
            string id = Console.ReadLine();

            try
            {
                string sql = @"
                                DELETE FROM HW_Product 
                                WHERE Product_ID = @ID;";
                MySqlCommand command = new MySqlCommand(sql, conn);
                da.UpdateCommand = command;
                da.UpdateCommand.Parameters.AddWithValue("@ID", id);

                conn.Open();
                da.UpdateCommand.ExecuteNonQuery();
                conn.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //outputs database through use of data table
        static void printTable(DataTable table)
        {
            Console.WriteLine("--------------------------------------------------------------------------");
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; ++i)
                {
                    Console.Write(row[i].ToString() + "\t\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine("--------------------------------------------------------------------------");

        }

        static void Main(string[] args)
        {
            string connectionString = "Database=acsm_a06ea3234231add;Data Source=us-cdbr-azure-east-a.cloudapp.net;User Id=bede392e3315a5;Password=28477764";
            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dt = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();

            //test intial connection
            try
            {
                connection.Open();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            connection.Close();
            Console.WriteLine("Connection established, Press any key to continue...");
            Console.ReadKey(); 
            Console.Clear();

            //display options until user wants to quit
            int menuChoice = 0;
            while (menuChoice != 14)
            {
                printProductList(connection, dt);
                displayMenu();
                Console.WriteLine("Enter choice: ");
                menuChoice = int.Parse(Console.ReadLine());

                switch (menuChoice)
                {
                    //1
                    case 1:
                        disDescPriceQuantity(connection, dt);
                        break;
                    //2
                    case 2:
                        newOrder(connection, adapter);
                        break;
                    //3
                    case 3:
                        disPriceDesc(connection, dt);
                        break;
                    //4
                    case 4:
                        disOrderDescription(connection, dt);
                        break;
                    //5
                    case 5:
                        countTotalCust(connection, dt);
                        break;
                    //6
                    case 6:
                        countGreat100(connection, dt);
                        break;
                    //7
                    case 7:
                        totalRevenue(connection, dt);
                        break;
                    //8
                    case 8:
                        top5Revenue(connection, dt);
                        break;
                    //9
                    case 9:
                        top5Volume(connection, dt);
                        break;
                    //10
                    case 10:
                        addProduct(connection, adapter);
                        break;
                    //11
                    case 11:
                        updateInventory(connection, adapter);
                        break;
                    //12
                    case 12:
                        updateDesc(connection, adapter);
                        break;
                    //12
                    case 13:
                        deleteProduct(connection, adapter);
                        break;
                    case 14:
                        break;
                    default:
                        Console.WriteLine("Invalid selection");
                        break;
                }
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}
