using Employee.Models;
using Employee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;


namespace Employee.Controllers
{
    public class ClientController : Controller
    {
        private readonly string? _connectionString;
        private readonly PdfService _pdfService;
        private readonly ExcelService _excelService;
        private readonly WordService _wordService;


        public ClientController(IConfiguration configuration, PdfService pdfService, ExcelService excelService, WordService wordService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _pdfService = pdfService;
            _excelService = excelService;
            _wordService = wordService;

        }
        public IActionResult Index()
        {
            try
            {
                List<Clients> client = new List<Clients>();
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Clients";
                    SqlCommand sqlCommand = new SqlCommand(query, con);
                    con.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        client.Add(new Clients()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString()

                        });
                    }

                    return View(client);

                }

            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public IActionResult Create()
        {
            
            try
            {
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public IActionResult Create(Clients obj)

        {
            try
            {
                if (obj.Name == null)
                {
                    return View(obj);
                }

                int clientId;

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO Clients (Name, Email, Role) " +
                                   "VALUES (@Name, @Email, @Role); " +
                                   "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", obj.Name);
                        cmd.Parameters.AddWithValue("@Email", !obj.Email.IsNullOrEmpty() ? obj.Email : "");
                        cmd.Parameters.AddWithValue("@Role", !obj.Role.IsNullOrEmpty() ? obj.Role : "");


                        con.Open();
                        clientId = (int)cmd.ExecuteScalar();

                    }



                    if (obj.Addresses != null && obj.Addresses.Count != 0)
                    {
                        foreach (var address in obj.Addresses)
                        {
                            if (string.IsNullOrWhiteSpace(address.Street)) continue;

                            string addresquery = @"INSERT INTO ClientAddresses (ClientId, Street, City, State, PinCode, Country)
                                     VALUES (@ClientId, @Street, @City, @State, @PinCode, @Country);";

                            using (SqlCommand cmd = new SqlCommand(addresquery, con))
                            {
                                cmd.Parameters.AddWithValue("@ClientId", clientId);
                                cmd.Parameters.AddWithValue("@Street", address.Street ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@City", address.City ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@State", address.State ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@PinCode", address.PinCode ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Country", address.Country ?? (object)DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                }

                TempData["success"] = "Client Added succesfully";
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }

            

        }

        public IActionResult Edit(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    List<ClientAddresses> addres = new List<ClientAddresses>();
                    Clients client = null;

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {

                        string addressquery = "SELECT * FROM ClientAddresses WHERE ClientId = @Id";
                        using (SqlCommand sqlCommand = new SqlCommand(addressquery, con))
                        {
                            sqlCommand.Parameters.AddWithValue("@Id", id);
                            con.Open();

                            SqlDataReader addresreader = sqlCommand.ExecuteReader();
                            while (addresreader.Read())
                            {
                                addres.Add(new ClientAddresses()
                                {
                                    Id = Convert.ToInt32(addresreader["Id"]),
                                    Street = addresreader["Street"].ToString(),
                                    City = addresreader["City"].ToString(),
                                    State = addresreader["State"].ToString(),
                                    PinCode = addresreader["PinCode"].ToString(),
                                    Country = addresreader["Country"].ToString()

                                });
                            }

                            con.Close();

                        }



                        string query = "SELECT * FROM clients WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            con.Open();

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    client = new Clients
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        Name = reader["Name"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        Role = reader["role"].ToString(),
                                        Addresses = addres,

                                    };
                                }
                            }

                        }

                    }

                    if (client == null)
                    {
                        return NotFound();
                    }

                    return View(client);


                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            


        }

        [HttpPost, ActionName("Edit")]
        public IActionResult EditPost(Clients obj)
        {
            try
            {
                if (obj.Name.IsNullOrEmpty())
                {
                    return View(obj);
                }
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = @"UPDATE Clients 
                        SET Name = @Name, Email = @Email, Role = @Role
                        WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", obj.Name);
                        cmd.Parameters.AddWithValue("@Email", !obj.Email.IsNullOrEmpty() ? obj.Email : "");
                        cmd.Parameters.AddWithValue("@Role", !obj.Role.IsNullOrEmpty() ? obj.Role : "");
                        cmd.Parameters.AddWithValue("@Id", obj.Id);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    foreach (var address in obj.Addresses)
                    {
                        if (address.IsDeleted && address.Id > 0)
                        {
                            try
                            {
                                string deleteQuery = @"DELETE FROM ClientAddresses 
                                       WHERE Id = @AddressId AND ClientId = @ClientId";

                                using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                                {
                                    cmd.Parameters.AddWithValue("@AddressId", address.Id);
                                    cmd.Parameters.AddWithValue("@ClientId", obj.Id);

                                    cmd.ExecuteNonQuery();
                                }

                                continue;
                            }
                            catch (Exception ex)
                            {
                                TempData["AlertMessage"] = "This address : " + address.Street.ToString() + ", " + address.City.ToString() + "(" + address.PinCode.ToString() + "), " + address.State.ToString() + ", " + address.Country.ToString() + " is linked to an existing order and cannot be deleted.";

                                return View(obj);
                            }

                        }
                        if (string.IsNullOrWhiteSpace(address.Street)) continue;

                        if (address.Id > 0)
                        {
                            string updateQuery = @"UPDATE ClientAddresses 
                               SET Street = @Street, City = @City, State = @State, PinCode = @PinCode, Country = @Country
                               WHERE Id = @AddressId AND ClientId = @ClientId";

                            using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@Street", address.Street ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@City", address.City ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@State", address.State ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@PinCode", address.PinCode ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Country", address.Country ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@AddressId", address.Id);
                                cmd.Parameters.AddWithValue("@ClientId", obj.Id);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string addquery = @"INSERT INTO ClientAddresses (ClientId, Street, City, State, PinCode, Country)
                                    VALUES (@ClientId, @Street, @City, @State, @PinCode, @Country);";

                            using (SqlCommand cmd = new SqlCommand(addquery, con))
                            {
                                cmd.Parameters.AddWithValue("@ClientId", obj.Id);
                                cmd.Parameters.AddWithValue("@Street", address.Street ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@City", address.City ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@State", address.State ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@PinCode", address.PinCode ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Country", address.Country ?? (object)DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }

                    }

                }

                TempData["success"] = "Client Updated succesfully";
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }

            




        }

        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    List<ClientAddresses> addres = new List<ClientAddresses>();
                    Clients client = null;

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        string addresquery = "SELECT * FROM ClientAddresses WHERE ClientId = @Id";
                        using (SqlCommand sqlCommand = new SqlCommand(addresquery, con))
                        {
                            sqlCommand.Parameters.AddWithValue("@Id", id);
                            con.Open();

                            SqlDataReader addresreader = sqlCommand.ExecuteReader();
                            while (addresreader.Read())
                            {
                                addres.Add(new ClientAddresses()
                                {
                                    Id = Convert.ToInt32(addresreader["Id"]),
                                    Street = addresreader["Street"].ToString(),
                                    City = addresreader["City"].ToString(),
                                    State = addresreader["State"].ToString(),
                                    PinCode = addresreader["PinCode"].ToString(),
                                    Country = addresreader["Country"].ToString()

                                });
                            }

                            con.Close();

                        }
                        string query = "SELECT * FROM Clients WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);

                            con.Open();

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    client = new Clients
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        Name = reader["Name"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        Role = reader["Role"].ToString(),
                                        Addresses = addres,

                                    };
                                }
                            }

                        }
                    }

                    if (client == null)
                    {
                        return NotFound();
                    }

                    return View(client);
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {

                    string selectQuery = "SELECT COUNT(*) FROM Clients WHERE Id = @Id";
                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, con))
                    {
                        selectCmd.Parameters.AddWithValue("@Id", id);
                        con.Open();

                        int count = (int)selectCmd.ExecuteScalar();
                        if (count == 0)
                        {
                            return NotFound();
                        }

                        string deleteQuery = "DELETE FROM clients WHERE Id = @Id";
                        using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
                        {
                            deleteCmd.Parameters.AddWithValue("@Id", id);
                            deleteCmd.ExecuteNonQuery();
                        }
                    }
                }
                TempData["success"] = "Client Deleted succesfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Since this client has placed orders, please delete their orders before deleting the client";
                return RedirectToAction("Index");
            }

        }

        public IActionResult Order(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    List<Order> orders = new List<Order>();

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {


                        string query = @"SELECT o.*, a.*, c.* FROM [Order] o INNER JOIN ClientAddresses a ON o.AddressId = a.Id INNER JOIN Clients c ON o.ClientId = c.Id WHERE o.ClientId = @Id";

                        con.Open();
                        using (SqlCommand sqlCommand = new SqlCommand(query, con))
                        {
                            sqlCommand.Parameters.AddWithValue("@Id", id);
                            SqlDataReader reader = sqlCommand.ExecuteReader();


                            while (reader.Read())
                            {
                                orders.Add(new Order()
                                {

                                    Id = Convert.ToInt32(reader["Id"]),
                                    ClientId = Convert.ToInt32(reader["ClientId"]),
                                    Address = reader["Street"].ToString() + ", " + reader["City"].ToString() + "(" + reader["PinCode"].ToString() + "), " + reader["State"].ToString() + ", " + reader["Country"].ToString(),
                                    OrderTotal = reader["OrderTotal"].ToString(),
                                    items = new List<Item>(),
                                    client = new Clients
                                    {
                                        Name = reader["Name"].ToString(),
                                        Email = reader["Email"].ToString(),
                                    }

                                });

                            }


                        }
                        con.Close();



                        con.Open();
                        string itemQuery = "SELECT  o.Id,  o.AddressId, o.OrderTotal, i.Id AS ItemId,  i.Quantity, p.Name AS ProductName, p.Rate AS ProductRate, (p.Rate * i.Quantity) AS ItemTotal FROM  [Order] o JOIN Item i ON o.Id = i.OrderId JOIN  Product p ON i.ProductId = p.Id WHERE o.Id = @OrderId;";
                        foreach (var order in orders)
                        {
                            using (SqlCommand cmdItems = new SqlCommand(itemQuery, con))
                            {
                                cmdItems.Parameters.AddWithValue("@OrderId", order.Id);

                                using (SqlDataReader itemReader = cmdItems.ExecuteReader())
                                {
                                    while (itemReader.Read())
                                    {
                                        var item = new Item
                                        {
                                            Id = Convert.ToInt32(itemReader["ItemId"]),

                                            ProductName = itemReader["ProductName"].ToString(),
                                            ProductRate = itemReader["ProductRate"].ToString(),
                                            Quantity = Convert.ToInt32(itemReader["Quantity"]),
                                            ItemTotal = itemReader["ItemTotal"].ToString()


                                        };

                                        order.items.Add(item);
                                    }
                                }
                            }
                        }




                    }


                    if (orders.Count != 0)
                    {
                        return View(orders);
                    }
                    else
                    {
                        orders.Add(new Order()
                        {
                            Id = Convert.ToInt32(0),
                            ClientId = Convert.ToInt32(id)
                        });
                        return View(orders);
                    }


                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            



        }

        public IActionResult CreateOrder(int? id)
        {
            try
            {
                Order order = new Order();
                List<ClientAddresses> clientAddresses = new List<ClientAddresses>();
                List<Product> products = new List<Product>();

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM ClientAddresses WHERE ClientId = @Id";
                    SqlCommand sqlCommand = new SqlCommand(query, con);
                    con.Open();
                    sqlCommand.Parameters.AddWithValue("@Id", id);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        clientAddresses.Add(new ClientAddresses()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ClientId = Convert.ToInt32(reader["ClientId"]),
                            Street = reader["Street"].ToString(),
                            City = reader["City"].ToString(),
                            State = reader["State"].ToString(),
                            PinCode = reader["PinCode"].ToString(),
                            Country = reader["Country"].ToString(),
                            Address = reader["Street"].ToString() + ", " + reader["City"].ToString() + "(" + reader["PinCode"].ToString() + "), " + reader["State"].ToString() + ", " + reader["Country"].ToString()

                        });
                    }
                    con.Close();


                    con.Open();

                    string itemQuery = "SELECT * FROM Product";
                    SqlCommand itemSqlCommand = new SqlCommand(itemQuery, con);

                    itemSqlCommand.Parameters.AddWithValue("@Id", id);
                    SqlDataReader itemreader = itemSqlCommand.ExecuteReader();
                    while (itemreader.Read())
                    {
                        products.Add(new Product()
                        {
                            Id = Convert.ToInt32(itemreader["Id"]),
                            Name = itemreader["Name"].ToString(),
                            Rate = Convert.ToDecimal(itemreader["Rate"])
                        });
                    }

                }


                order.ClientId = Convert.ToInt32(id);
                order.Addersses = clientAddresses;
                order.Products = products;
                return View(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }


        }

        [HttpPost, ActionName("CreateOrder")]
        public IActionResult CreateOrder(Order obj)
        {
            try
            {
                int orderId;

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = @"
                INSERT INTO [Order] (ClientId, AddressId, OrderTotal)
                VALUES (@ClientId, @AddressId, @OrderTotal);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";


                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ClientId", obj.ClientId);
                        cmd.Parameters.AddWithValue("@AddressId", obj.AddressId);
                        cmd.Parameters.AddWithValue("@OrderTotal", obj.OrderTotal);


                        con.Open();
                        orderId = (int)cmd.ExecuteScalar();

                    }



                    if (obj.items != null && obj.items.Count != 0)
                    {

                        foreach (var item in obj.items)
                        {
                            if (item.ProductId == 0) continue;

                            string itemquery = "INSERT INTO Item (OrderId, ProductId, Quantity, ItemTotal) " +
                                   "VALUES (@OrderId, @ProductId, @Quantity, @ItemTotal)";

                            using (SqlCommand cmd = new SqlCommand(itemquery, con))
                            {
                                cmd.Parameters.AddWithValue("@OrderId", orderId);
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@ItemTotal", item.ItemTotal);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                }

                TempData["success"] = "Order placed succesfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }



        }

        public IActionResult ViewOrder(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    Order order = new Order();
                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {


                        string query = @"SELECT o.*, a.*, c.* FROM [Order] o INNER JOIN ClientAddresses a ON o.AddressId = a.Id INNER JOIN Clients c ON o.ClientId = c.Id WHERE o.Id = @Id";

                        con.Open();
                        using (SqlCommand sqlCommand = new SqlCommand(query, con))
                        {
                            sqlCommand.Parameters.AddWithValue("@Id", id);
                            SqlDataReader reader = sqlCommand.ExecuteReader();


                            while (reader.Read())
                            {

                                order.Id = Convert.ToInt32(reader["Id"]);
                                order.ClientId = Convert.ToInt32(reader["ClientId"]);
                                order.Address = reader["Street"].ToString() + ", " + reader["City"].ToString() + "(" + reader["PinCode"].ToString() + "), " + reader["State"].ToString() + ", " + reader["Country"].ToString();
                                order.OrderTotal = reader["OrderTotal"].ToString();
                                order.client = new Clients
                                {
                                    Name = reader["Name"].ToString(),
                                    Email = reader["Email"].ToString(),
                                };



                            }


                        }
                        con.Close();



                        con.Open();
                        string itemQuery = "SELECT  o.Id,  o.AddressId, o.OrderTotal, i.Id AS ItemId,  i.Quantity, p.Name AS ProductName, p.Rate AS ProductRate, i.ItemTotal FROM  [Order] o JOIN Item i ON o.Id = i.OrderId JOIN  Product p ON i.ProductId = p.Id WHERE o.Id = @OrderId;";
                        using (SqlCommand cmdItems = new SqlCommand(itemQuery, con))
                        {
                            cmdItems.Parameters.AddWithValue("@OrderId", order.Id);

                            using (SqlDataReader itemReader = cmdItems.ExecuteReader())
                            {
                                while (itemReader.Read())
                                {
                                    var item = new Item
                                    {
                                        Id = Convert.ToInt32(itemReader["ItemId"]),
                                        ProductName = itemReader["ProductName"].ToString(),
                                        ProductRate = itemReader["ProductRate"].ToString(),
                                        Quantity = Convert.ToInt32(itemReader["Quantity"]),
                                        ItemTotal = itemReader["ItemTotal"].ToString()


                                    };

                                    order.items.Add(item);
                                }
                            }
                        }

                    }

                    return View(order);




                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            

        }
        public IActionResult EditOrder(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return BadRequest("Missing orderId");
                }

                Order order = new Order();
                List<ClientAddresses> clientAddresses = new List<ClientAddresses>();
                List<Product> products = new List<Product>();
                List<Item> items = new List<Item>();


                using (SqlConnection con = new SqlConnection(_connectionString))
                {




                    string orderquery = "SELECT [Order].*, ClientAddresses.* FROM  [Order] JOIN  ClientAddresses ON [Order].AddressId = ClientAddresses.Id WHERE [Order].Id = @OrderId;";
                    SqlCommand ordersqlCommand = new SqlCommand(orderquery, con);
                    con.Open();
                    ordersqlCommand.Parameters.AddWithValue("@OrderId", id);
                    SqlDataReader orderreader = ordersqlCommand.ExecuteReader();
                    while (orderreader.Read())
                    {
                        order.Id = Convert.ToInt32(orderreader["Id"]);
                        order.ClientId = Convert.ToInt32(orderreader["ClientId"]);
                        order.AddressId = Convert.ToInt32(orderreader["AddressId"]);
                        order.Address = orderreader["Street"].ToString() + ", " + orderreader["City"].ToString() + "(" + orderreader["PinCode"].ToString() + "), " + orderreader["State"].ToString() + ", " + orderreader["Country"].ToString();
                        order.OrderTotal = orderreader["OrderTotal"].ToString();
                    }
                    con.Close();

                    string query = "SELECT * FROM ClientAddresses WHERE ClientId = @Id";
                    SqlCommand sqlCommand = new SqlCommand(query, con);
                    con.Open();
                    sqlCommand.Parameters.AddWithValue("@Id", order.ClientId);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        clientAddresses.Add(new ClientAddresses()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ClientId = Convert.ToInt32(reader["ClientId"]),
                            Street = reader["Street"].ToString(),
                            City = reader["City"].ToString(),
                            State = reader["State"].ToString(),
                            PinCode = reader["PinCode"].ToString(),
                            Country = reader["Country"].ToString(),
                            Address = reader["Street"].ToString() + ", " + reader["City"].ToString() + "(" + reader["PinCode"].ToString() + "), " + reader["State"].ToString() + ", " + reader["Country"].ToString()

                        });
                    }
                    con.Close();


                    string itemquery = "SELECT item.*, product.Name, product.Rate FROM item JOIN  product ON item.ProductId = product.Id WHERE item.OrderId = @OrderId;";
                    SqlCommand itemSqlCommand = new SqlCommand(itemquery, con);
                    con.Open();
                    itemSqlCommand.Parameters.AddWithValue("@OrderId", id);
                    SqlDataReader itemreader = itemSqlCommand.ExecuteReader();
                    while (itemreader.Read())
                    {
                        items.Add(new Item()
                        {
                            Id = Convert.ToInt32(itemreader["Id"]),
                            ProductId = Convert.ToInt32(itemreader["ProductId"]),
                            Quantity = Convert.ToInt32(itemreader["Quantity"]),
                            ItemTotal = itemreader["ItemTotal"].ToString(),
                            ProductName = itemreader["Name"].ToString(),
                            ProductRate = itemreader["Rate"].ToString(),

                        });

                    }
                    con.Close();

                    con.Open();

                    string productQuery = "SELECT * FROM Product";
                    SqlCommand productSqlCommand = new SqlCommand(productQuery, con);
                    SqlDataReader productreader = productSqlCommand.ExecuteReader();
                    while (productreader.Read())
                    {
                        products.Add(new Product()
                        {
                            Id = Convert.ToInt32(productreader["Id"]),
                            Name = productreader["Name"].ToString(),
                            Rate = Convert.ToDecimal(productreader["Rate"])
                        });
                    }





                }

                order.items = items;
                order.Addersses = clientAddresses;
                order.Products = products;
                return View(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public IActionResult EditOrderItem(Order obj)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    string updateOrderQuery = @"UPDATE [Order] SET AddressId = @AddressId, OrderTotal = @OrderTotal WHERE Id = @OrderId";

                    using (SqlCommand cmd = new SqlCommand(updateOrderQuery, con))
                    {

                        cmd.Parameters.AddWithValue("@AddressId", obj.AddressId);
                        cmd.Parameters.AddWithValue("@OrderTotal", obj.OrderTotal);
                        cmd.Parameters.AddWithValue("@OrderId", obj.Id);

                        cmd.ExecuteNonQuery();
                    }






                    string deleteItemsQuery = "DELETE FROM Item WHERE OrderId = @OrderId";
                    using (SqlCommand cmd = new SqlCommand(deleteItemsQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", obj.Id);
                        cmd.ExecuteNonQuery();
                    }


                    if (obj.items != null && obj.items.Count != 0)
                    {
                        foreach (var item in obj.items)
                        {
                            if (item.ProductId == 0) continue;

                            string insertItemQuery = @"INSERT INTO Item (OrderId, ProductId, Quantity, ItemTotal)
                                                       VALUES (@OrderId, @ProductId, @Quantity, @ItemTotal)";

                            using (SqlCommand cmd = new SqlCommand(insertItemQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@OrderId", obj.Id);
                                cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@ItemTotal", item.ItemTotal);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    string deleteOrderQuery = "DELETE FROM [Order] WHERE OrderTotal = '0.00'";

                    using (SqlCommand cmd = new SqlCommand(deleteOrderQuery, con))
                    {

                        cmd.ExecuteNonQuery();
                    }





                }

                TempData["success"] = "Order updated successfully ";
                return RedirectToAction("Order", "Client", new { id = obj.ClientId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            
        }

        public IActionResult DeleteOrder(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return BadRequest("Missing orderId");
                }

                Order order = new Order();
                List<Item> items = new List<Item>();


                using (SqlConnection con = new SqlConnection(_connectionString))
                {

                    string orderquery = "SELECT [Order].*, ClientAddresses.* FROM  [Order] JOIN  ClientAddresses ON [Order].AddressId = ClientAddresses.Id WHERE [Order].Id = @OrderId;";
                    SqlCommand ordersqlCommand = new SqlCommand(orderquery, con);
                    con.Open();
                    ordersqlCommand.Parameters.AddWithValue("@OrderId", id);
                    SqlDataReader orderreader = ordersqlCommand.ExecuteReader();
                    while (orderreader.Read())
                    {
                        order.Id = Convert.ToInt32(orderreader["Id"]);
                        order.ClientId = Convert.ToInt32(orderreader["ClientId"]);
                        order.AddressId = Convert.ToInt32(orderreader["AddressId"]);
                        order.Address = orderreader["Street"].ToString() + ", " + orderreader["City"].ToString() + "(" + orderreader["PinCode"].ToString() + "), " + orderreader["State"].ToString() + ", " + orderreader["Country"].ToString();
                        order.OrderTotal = orderreader["OrderTotal"].ToString();
                    }
                    con.Close();




                    string itemquery = "SELECT item.*, product.Name, product.Rate FROM item JOIN  product ON item.ProductId = product.Id WHERE item.OrderId = @OrderId;";
                    SqlCommand itemSqlCommand = new SqlCommand(itemquery, con);
                    con.Open();
                    itemSqlCommand.Parameters.AddWithValue("@OrderId", id);
                    SqlDataReader itemreader = itemSqlCommand.ExecuteReader();
                    while (itemreader.Read())
                    {
                        items.Add(new Item()
                        {
                            Id = Convert.ToInt32(itemreader["Id"]),
                            ProductId = Convert.ToInt32(itemreader["ProductId"]),
                            Quantity = Convert.ToInt32(itemreader["Quantity"]),
                            ItemTotal = itemreader["ItemTotal"].ToString(),
                            ProductName = itemreader["Name"].ToString(),
                            ProductRate = itemreader["Rate"].ToString(),

                        });

                    }





                }

                order.items = items;



                return View(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public IActionResult DeleteOrderItem(Order obj)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    string deleteItemsQuery = "DELETE FROM Item WHERE OrderId = @OrderId";
                    using (SqlCommand cmd = new SqlCommand(deleteItemsQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", obj.Id);
                        cmd.ExecuteNonQuery();
                    }


                    string deleteOrderQuery = "DELETE FROM [Order] WHERE Id = @OrderId";

                    using (SqlCommand cmd = new SqlCommand(deleteOrderQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", obj.Id);
                        cmd.ExecuteNonQuery();
                    }





                }

                TempData["success"] = "Order delete successfully ";
                return RedirectToAction("Order", "Client", new { id = obj.ClientId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }


            
        }

        public IActionResult DownloadPdf(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    Order order = new Order();
                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {


                        string query = @"SELECT o.*, a.*, c.* FROM [Order] o INNER JOIN ClientAddresses a ON o.AddressId = a.Id INNER JOIN Clients c ON o.ClientId = c.Id WHERE o.Id = @Id";

                        con.Open();
                        using (SqlCommand sqlCommand = new SqlCommand(query, con))
                        {
                            sqlCommand.Parameters.AddWithValue("@Id", id);
                            SqlDataReader reader = sqlCommand.ExecuteReader();


                            while (reader.Read())
                            {

                                order.Id = Convert.ToInt32(reader["Id"]);
                                order.ClientId = Convert.ToInt32(reader["ClientId"]);
                                order.Address = reader["Street"].ToString() + ", " + reader["City"].ToString() + "(" + reader["PinCode"].ToString() + "), " + reader["State"].ToString() + ", " + reader["Country"].ToString();
                                order.OrderTotal = reader["OrderTotal"].ToString();
                                order.client = new Clients
                                {
                                    Name = reader["Name"].ToString(),
                                    Email = reader["Email"].ToString(),
                                };



                            }


                        }
                        con.Close();



                        con.Open();
                        string itemQuery = "SELECT  o.Id,  o.AddressId, o.OrderTotal, i.Id AS ItemId,  i.Quantity, p.Name AS ProductName, p.Rate AS ProductRate, i.ItemTotal FROM  [Order] o JOIN Item i ON o.Id = i.OrderId JOIN  Product p ON i.ProductId = p.Id WHERE o.Id = @OrderId;";
                        using (SqlCommand cmdItems = new SqlCommand(itemQuery, con))
                        {
                            cmdItems.Parameters.AddWithValue("@OrderId", order.Id);

                            using (SqlDataReader itemReader = cmdItems.ExecuteReader())
                            {
                                while (itemReader.Read())
                                {
                                    var item = new Item
                                    {
                                        Id = Convert.ToInt32(itemReader["ItemId"]),
                                        ProductName = itemReader["ProductName"].ToString(),
                                        ProductRate = itemReader["ProductRate"].ToString(),
                                        Quantity = Convert.ToInt32(itemReader["Quantity"]),
                                        ItemTotal = itemReader["ItemTotal"].ToString()


                                    };

                                    order.items.Add(item);
                                }
                            }
                        }

                    }

                    var excelBytes = _excelService.GenerateOrderExcel(order);


                    var pdfBytes = _pdfService.GenerateOrderPdf(order);
                    return File(pdfBytes, "application/pdf", $"Order_{order.Id}.pdf");

                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }


        public IActionResult DownloadExcel(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    Order order = new Order();

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {


                        string query = @"SELECT o.*, a.*, c.* FROM [Order] o INNER JOIN ClientAddresses a ON o.AddressId = a.Id INNER JOIN Clients c ON o.ClientId = c.Id WHERE o.Id = @Id";

                        con.Open();
                        using (SqlCommand sqlCommand = new SqlCommand(query, con))
                        {
                            sqlCommand.Parameters.AddWithValue("@Id", id);
                            SqlDataReader reader = sqlCommand.ExecuteReader();


                            while (reader.Read())
                            {

                                order.Id = Convert.ToInt32(reader["Id"]);
                                order.ClientId = Convert.ToInt32(reader["ClientId"]);
                                order.Address = reader["Street"].ToString() + ", " + reader["City"].ToString() + "(" + reader["PinCode"].ToString() + "), " + reader["State"].ToString() + ", " + reader["Country"].ToString();
                                order.OrderTotal = reader["OrderTotal"].ToString();
                                order.client = new Clients
                                {
                                    Name = reader["Name"].ToString(),
                                    Email = reader["Email"].ToString(),
                                };



                            }


                        }
                        con.Close();



                        con.Open();
                        string itemQuery = "SELECT  o.Id,  o.AddressId, o.OrderTotal, i.Id AS ItemId,  i.Quantity, p.Name AS ProductName, p.Rate AS ProductRate, i.ItemTotal FROM  [Order] o JOIN Item i ON o.Id = i.OrderId JOIN  Product p ON i.ProductId = p.Id WHERE o.Id = @OrderId;";
                        using (SqlCommand cmdItems = new SqlCommand(itemQuery, con))
                        {
                            cmdItems.Parameters.AddWithValue("@OrderId", order.Id);

                            using (SqlDataReader itemReader = cmdItems.ExecuteReader())
                            {
                                while (itemReader.Read())
                                {
                                    var item = new Item
                                    {
                                        Id = Convert.ToInt32(itemReader["ItemId"]),
                                        ProductName = itemReader["ProductName"].ToString(),
                                        ProductRate = itemReader["ProductRate"].ToString(),
                                        Quantity = Convert.ToInt32(itemReader["Quantity"]),
                                        ItemTotal = itemReader["ItemTotal"].ToString()


                                    };

                                    order.items.Add(item);
                                }
                            }
                        }

                    }

                    var excelBytes = _excelService.GenerateOrderExcel(order);
                    return File(excelBytes,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"Order_{order.Id}.xlsx"
                    );



                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        public IActionResult DownloadWord(int? id)
        {
            try
            {

                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    Order order = new Order();

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {


                        string query = @"SELECT o.*, a.*, c.* FROM [Order] o INNER JOIN ClientAddresses a ON o.AddressId = a.Id INNER JOIN Clients c ON o.ClientId = c.Id WHERE o.Id = @Id";

                        con.Open();
                        using (SqlCommand sqlCommand = new SqlCommand(query, con))
                        {
                            sqlCommand.Parameters.AddWithValue("@Id", id);
                            SqlDataReader reader = sqlCommand.ExecuteReader();


                            while (reader.Read())
                            {

                                order.Id = Convert.ToInt32(reader["Id"]);
                                order.ClientId = Convert.ToInt32(reader["ClientId"]);
                                order.Address = reader["Street"].ToString() + ", " + reader["City"].ToString() + "(" + reader["PinCode"].ToString() + "), " + reader["State"].ToString() + ", " + reader["Country"].ToString();
                                order.OrderTotal = reader["OrderTotal"].ToString();
                                order.client = new Clients
                                {
                                    Name = reader["Name"].ToString(),
                                    Email = reader["Email"].ToString(),
                                };



                            }


                        }
                        con.Close();



                        con.Open();
                        string itemQuery = "SELECT  o.Id,  o.AddressId, o.OrderTotal, i.Id AS ItemId,  i.Quantity, p.Name AS ProductName, p.Rate AS ProductRate, i.ItemTotal FROM  [Order] o JOIN Item i ON o.Id = i.OrderId JOIN  Product p ON i.ProductId = p.Id WHERE o.Id = @OrderId;";
                        using (SqlCommand cmdItems = new SqlCommand(itemQuery, con))
                        {
                            cmdItems.Parameters.AddWithValue("@OrderId", order.Id);

                            using (SqlDataReader itemReader = cmdItems.ExecuteReader())
                            {
                                while (itemReader.Read())
                                {
                                    var item = new Item
                                    {
                                        Id = Convert.ToInt32(itemReader["ItemId"]),
                                        ProductName = itemReader["ProductName"].ToString(),
                                        ProductRate = itemReader["ProductRate"].ToString(),
                                        Quantity = Convert.ToInt32(itemReader["Quantity"]),
                                        ItemTotal = itemReader["ItemTotal"].ToString()


                                    };

                                    order.items.Add(item);
                                }
                            }
                        }

                    }

                    var wordBytes = _wordService.GenerateOrderWord(order);

                    return File(wordBytes,
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                $"Order_{order.Id}.docx"
                    );



                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }


        }






    }
}

        
