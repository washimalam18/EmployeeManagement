using Employee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IO;

namespace Employee.Controllers
{
    public class ClientController : Controller
    {
        private readonly string? _connectionString;


        public ClientController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Clients obj)

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
                            cmd.Parameters.AddWithValue("@Street",address.Street ?? (object)DBNull.Value); 
                            cmd.Parameters.AddWithValue("@City",address.City ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@State",address.State ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PinCode",address.PinCode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Country",address.Country ?? (object)DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }                            
                    }
                }

            }                                                                                               

            TempData["success"] = "Client Added succesfully";
            return RedirectToAction("Index");
            
        }

        public IActionResult Edit(int? id)
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

        [HttpPost, ActionName("Edit")]
        public IActionResult EditPost(Clients obj)
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


              

                string deleteQuery = "DELETE FROM ClientAddresses WHERE ClientId = @Id";
                using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
                {
                    deleteCmd.Parameters.AddWithValue("@Id", obj.Id);
                    deleteCmd.ExecuteNonQuery();
                }

                if (obj.Addresses != null && obj.Addresses.Count != 0)
                {
                    foreach (var address in obj.Addresses)
                    {
                        if (string.IsNullOrWhiteSpace(address.Street)) continue;

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

        public IActionResult Delete(int? id)
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

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
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





    }

}

        
