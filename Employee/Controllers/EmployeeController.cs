using Employee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace Employee.Controllers
{
    public class EmployeeController : Controller
    {
       
        
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly string _connectionString;


        public EmployeeController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Home()
        {
            try
            {
                List<Emp> employees = new List<Emp>();
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Employee";
                    SqlCommand sqlCommand = new SqlCommand(query, con);
                    con.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        employees.Add(new Emp()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Department = reader["Department"].ToString(),
                            JoiningDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["JoiningDate"])),
                            Url = reader["Url"].ToString()

                        });
                    }

                    return View(employees);

                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
          
        }

        public IActionResult Index()
        {
            try
            {
                List<Emp> employees = new List<Emp>();
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Employee";
                    SqlCommand sqlCommand = new SqlCommand(query, con);
                    con.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        employees.Add(new Emp()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Department = reader["Department"].ToString(),
                            JoiningDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["JoiningDate"])),
                            Url = reader["Url"].ToString()

                        });
                    }

                    return View(employees);

                }
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        [HttpPost]
        public IActionResult Create(Emp obj,IFormFile? file)
        {
            try
            {
                if (obj.Name.IsNullOrEmpty() || obj.Email.IsNullOrEmpty() || obj.Department.IsNullOrEmpty() || obj.JoiningDate == null)
                {
                    return View(obj);
                }

                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    String profilePath = Path.Combine(wwwRootPath, @"images\profile");

                    using (var fileStream = new FileStream(Path.Combine(profilePath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Url = @"\images\profile\" + fileName;
                }
                else
                {
                    obj.Url = "https://placehold.co/500x600";
                }

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO Employee (Name, Email, Department, JoiningDate, Url) " +
                                   "VALUES (@Name, @Email, @Department, @JoiningDate, @Url)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", obj.Name);
                        cmd.Parameters.AddWithValue("@Email", obj.Email);
                        cmd.Parameters.AddWithValue("@Department", obj.Department);
                        cmd.Parameters.AddWithValue("@JoiningDate", obj.JoiningDate);
                        cmd.Parameters.AddWithValue("@Url", obj.Url);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["success"] = "Employee Added succesfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
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
                    Emp employee = null;

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        string query = "SELECT * FROM Employee WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            con.Open();

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    employee = new Emp
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        Name = reader["Name"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        Department = reader["Department"].ToString(),
                                        JoiningDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["JoiningDate"])),
                                        Url = reader["Url"].ToString()
                                    };
                                }
                            }
                        }
                    }

                    if (employee == null)
                    {
                        return NotFound();
                    }

                    return View(employee);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        public IActionResult Details(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }

                Emp employee = null;

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Employee WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                employee = new Emp
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Department = reader["Department"].ToString(),
                                    JoiningDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["JoiningDate"])),
                                    Url = reader["Url"].ToString()
                                };
                            }
                        }
                    }
                }

                if (employee == null)
                {
                    return NotFound();
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

           
        }
        

        [HttpPost, ActionName("Edit")]
        public IActionResult EditPost(Emp obj,IFormFile? file)
        {
            try
            {
                if (obj.Name.IsNullOrEmpty() || obj.Email.IsNullOrEmpty() || obj.Department.IsNullOrEmpty() || obj.JoiningDate == null)
                {
                    return View(obj);
                }

                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    String profilePath = Path.Combine(wwwRootPath, @"images\profile");

             string wwwRootPath = _webHostEnvironment.WebRootPath;
             if (file != null)
             {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                String profilePath = Path.Combine(wwwRootPath, @"images\profile");

                 using (var fileStream = new FileStream(Path.Combine(profilePath, fileName), FileMode.Create))
                 {
                    file.CopyTo(fileStream);
                 }
                 obj.Url = @"\images\profile\" + fileName;


                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        string query = @"UPDATE Employee 
                    SET Name = @Name, Email = @Email, Department = @Department, 
                    JoiningDate = @JoiningDate, Url = @Url 
                    WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Name", obj.Name);
                            cmd.Parameters.AddWithValue("@Email", obj.Email);
                            cmd.Parameters.AddWithValue("@Department", obj.Department);
                            cmd.Parameters.AddWithValue("@JoiningDate", obj.JoiningDate);
                            cmd.Parameters.AddWithValue("@Url", obj.Url);
                            cmd.Parameters.AddWithValue("@Id", obj.Id);

                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                }

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = @"UPDATE Employee 
                SET Name = @Name, Email = @Email, Department = @Department, 
                JoiningDate = @JoiningDate 
                WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                       cmd.Parameters.AddWithValue("@Name", obj.Name);
                       cmd.Parameters.AddWithValue("@Email", obj.Email);
                       cmd.Parameters.AddWithValue("@Department", obj.Department);
                       cmd.Parameters.AddWithValue("@JoiningDate", obj.JoiningDate);
                       cmd.Parameters.AddWithValue("@Url", obj.Url);
                       cmd.Parameters.AddWithValue("@Id", obj.Id);

                       con.Open();
                       cmd.ExecuteNonQuery();
                    }
                 }

             }

             using (SqlConnection con = new SqlConnection(_connectionString))
             {
                string query = @"UPDATE Employee 
                SET Name = @Name, Email = @Email, Department = @Department, 
                JoiningDate = @JoiningDate 
                WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                   cmd.Parameters.AddWithValue("@Name", obj.Name);
                   cmd.Parameters.AddWithValue("@Email", obj.Email);
                   cmd.Parameters.AddWithValue("@Department", obj.Department);
                   cmd.Parameters.AddWithValue("@JoiningDate", obj.JoiningDate);
                   cmd.Parameters.AddWithValue("@Id", obj.Id);

                   con.Open();
                   cmd.ExecuteNonQuery();
                }
             }

                TempData["success"] = "Employee Updated succesfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
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



                    Emp employee = null;

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        string query = "SELECT * FROM Employee WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);

                            con.Open();

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    employee = new Emp
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        Name = reader["Name"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        Department = reader["Department"].ToString(),
                                        JoiningDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["JoiningDate"])),
                                        Url = reader["Url"].ToString()
                                    };
                                }
                            }
                        }
                    }

                    if (employee == null)
                    {
                        return NotFound();
                    }

                    return View(employee);
                }
            }
            catch (Exception ex)
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
                    string selectQuery = "SELECT COUNT(*) FROM Employee WHERE Id = @Id";
                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, con))
                    {
                        selectCmd.Parameters.AddWithValue("@Id", id);
                        con.Open();

                        int count = (int)selectCmd.ExecuteScalar();
                        if (count == 0)
                        {
                            return NotFound();
                        }

                        string deleteQuery = "DELETE FROM Employee WHERE Id = @Id";
                        using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
                        {
                            deleteCmd.Parameters.AddWithValue("@Id", id);
                            deleteCmd.ExecuteNonQuery();
                        }
                    }
                }
                TempData["success"] = "Employee Deleted succesfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }




    }

}

        
