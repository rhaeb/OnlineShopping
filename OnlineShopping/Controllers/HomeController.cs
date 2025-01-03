﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Globalization;
using OnlineShopping.Filters;
using System.Text.RegularExpressions;

namespace OnlineShopping.Controllers
{
    [AdminAuthorization]
    public class HomeController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\admin\source\repos\OnlineShopping\OnlineShopping\App_Data\OnlineShopping.mdf;Integrated Security=True";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public class Customer
        {
            public int Id { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Username { get; set; }
            public string DateOfBirth { get; set; }
            public string Gender { get; set; }
        }
        public class OrderItem
        {
            public int ProductID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal { get; set; }
        }

        public class ProductViewModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string Image { get; set; }
        }

        public ActionResult ListAllProducts()
        {
            return View();
        }

        public ActionResult ListAllCustomers()
        {
            return View();
        }
        public ActionResult ListAllOrders()
        {
            return View();
        }

        //orders
        public ActionResult DeleteOrder(int id)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM [orders] WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Message"] = "Order deleted successfully.";
            return RedirectToAction("ListAllOrders");
        }

        public ActionResult ViewOrderItems(int orderId)
        {
            List<OrderItem> orderItems = new List<OrderItem>();

            using (SqlConnection db = new SqlConnection(connStr))
            {
                db.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT OI.product_id, P.NAME, P.DESCRIPTION, P.CATEGORY, P.PRICE, OI.quantity, OI.subtotal
                  FROM order_item OI
                  JOIN PRODUCT P ON OI.product_id = P.ID
                  WHERE OI.order_id = @orderId", db))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orderItems.Add(new OrderItem
                            {
                                ProductID = (int)reader["product_id"],
                                Name = reader["NAME"].ToString(),
                                Description = reader["DESCRIPTION"].ToString(),
                                Category = reader["CATEGORY"].ToString(),
                                Price = (decimal)reader["PRICE"],
                                Quantity = (int)reader["quantity"],
                                Subtotal = (decimal)reader["subtotal"]
                            });
                        }
                    }
                }
            }

            ViewBag.OrderID = orderId;
            ViewBag.OrderItems = orderItems;

            return View();
        }


        public ActionResult UpdateDelete()
        {
            return View();
        }

        public ActionResult CustomerEntryForm()
        {
            ViewBag.Message = "Your customer entry page.";

            return View();
        }
        public ActionResult CreateCustomer()
        {
            ViewBag.Message = "Your customer entry page.";

            return View();
        }

        public ActionResult UpdateDeleteProduct() 
        {

            return View();
        }


        public ActionResult GetProductById(string id)
        {
            var product = new ProductViewModel();

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM PRODUCT WHERE ID = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product.Id = reader["ID"].ToString();
                            product.Name = reader["NAME"].ToString();
                            product.Description = reader["DESCRIPTION"].ToString();
                            product.Price = Convert.ToDecimal(reader["PRICE"]);
                            product.Quantity = Convert.ToInt32(reader["QUANTITY"]);
                            product.Image = reader["IMAGE"].ToString();
                        }
                        else
                        {
                            return Json(new { success = false, message = "No Entry Found!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            return Json(new { success = true, data = product }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ProductView()
        {

            if (Request["display"] != null)
            {
                var id = Request["prodId"];
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT * FROM PRODUCT WHERE ID='" + id + "'";
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var pname = reader["IMAGE"];
                                ViewData["pname"] = pname;
                                ViewData["name"] = reader["NAME"];
                            }
                            else
                            {
                                Response.Write("<script>alert('No Entry Found!')</script>");
                            }
                        }

                    }
                }
            }

            return View();
        }

        [HttpGet]
        public FileResult Image(string filename)
        {
            var folder = "";
            var filepath = "";
            try
            {
                folder = "c:\\Uploads";
                filepath = Path.Combine(folder, filename);
                if (System.IO.File.Exists(filepath))
                {

                }
            }
            catch (Exception)
            {
                throw new FileNotFoundException("File not found");
            }
            var mime = System.Web.MimeMapping.GetMimeMapping(Path.GetFileName(filename));
            Response.Headers.Add("content-disposition", "inline");
            return new FilePathResult(filepath, mime);
        }

        public ActionResult CustomerView()
        {
            return View();
        }

        private bool IsImageFile(string extension)
        {
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp";
        }

        [HttpPost]
        public ActionResult ProductEntry(FormCollection collection, HttpPostedFileBase img)
        {
            try
            {
                if (img == null || img.ContentLength == 0)
                {
                    throw new Exception("No file uploaded.");
                }
                string imag = Path.GetFileName(img.FileName);
                var extension = Path.GetExtension(img.FileName).ToLower();
                int filesize = img.ContentLength;
                if (!IsImageFile(extension))
                {
                    throw new Exception("Invalid file type. Only images (jpg, jpeg, png, gif, bmp) are allowed.");
                }
                const int maxSize = 25 * 1024 * 1024;
                if (filesize > maxSize)
                {
                    throw new Exception("File size exceeds the maximum allowed limit (25MB).");
                }
                string logpath = "c:\\Uploads";
                string filepath = Path.Combine(logpath, imag);
                img.SaveAs(filepath);

                var pname = collection["pName"].Trim();
                if (string.IsNullOrWhiteSpace(pname))
                {
                    throw new Exception("Product Name cannot be empty or spaces only.");
                }

                var price = 0f;
                if (!float.TryParse(collection["price"], out price) || price <= 0)
                {
                    throw new Exception("Invalid price format or negative value.");
                }

                var description = collection["description"];
                var quantity = 0;
                if (!int.TryParse(collection["quantity"], out quantity) || quantity < 0)
                {
                    throw new Exception("Invalid quantity format or non-positive value.");
                }

                var category = collection["category"];

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = " INSERT INTO PRODUCT(NAME,DESCRIPTION,CATEGORY,PRICE,QUANTITY,IMAGE) "
                                        + " VALUES (@pname,@description,@category,@price,@quantity,@file)";
                        cmd.Parameters.AddWithValue("@pname", pname);
                        cmd.Parameters.AddWithValue("@description", description);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@file", imag);

                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr > 0)
                        {
                            Response.Write("<script>alert('Product entry is created')</script>");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message.Replace("'", "\\'");
                Response.Write($"<script>alert('Invalid input: {errorMessage}');</script>");
            }
            return View("ListAllProducts");
        }

        public ActionResult ProductEntryForm()
        {
            return View();
        }


        public ActionResult EditCustomer(int id)
        {
            string query = "SELECT * FROM CUSTOMER WHERE ID = @id";

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = new SqlCommand(query, db))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.cusID = reader["ID"].ToString();
                            ViewBag.FirstName = reader["FNAME"].ToString();
                            ViewBag.LastName = reader["LNAME"].ToString();
                            ViewBag.Email = reader["EMAIL"].ToString();
                            ViewBag.Password = reader["PASSWORD"].ToString();
                            ViewBag.Username = reader["USERNAME"].ToString();
                            ViewBag.DateOfBirth = reader["DATE_OF_BIRTH"].ToString();
                            ViewBag.Gender = reader["GENDER"].ToString();
                        }
                    }
                }
            }

            return View();
        }

        public ActionResult CustomerAddEntry()
        {
            var data = new List<object>();
            var lname = Request["lname"]?.Trim();
            var fname = Request["fname"]?.Trim();
            var email = Request["email"];
            var pass = Request["pass"];
            var username = Request["username"];
            var dob = Request["dob"];
            var gender = Request["gender"];

            if (string.IsNullOrWhiteSpace(lname))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Last Name cannot be empty or spaces only."
                });
                return Json(data, JsonRequestBehavior.AllowGet); 
            }

            if (string.IsNullOrWhiteSpace(fname))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "First Name cannot be empty or spaces only."
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (username.ToLower() == "admin")
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Username is invalid."
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (pass.Length < 6)
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Password should at least be 6 characters long."
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$");
            if (!passwordRegex.IsMatch(pass))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one number, and be at least six characters long."
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (!IsUsernameUnique(username))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Username is already taken. Please choose a different username."
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (!IsEmailUnique(email))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Email is already taken."
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT INTO customer(lname, fname, email, password, username, date_of_birth, gender)"
                                    + "VALUES (@cus_lname, @cus_fname, @cus_email, @cus_pass, @cus_username, @cus_dob, @cus_gender)";
                    cmd.Parameters.AddWithValue("@cus_fname", fname);
                    cmd.Parameters.AddWithValue("@cus_lname", lname);
                    cmd.Parameters.AddWithValue("@cus_email", email);
                    cmd.Parameters.AddWithValue("@cus_pass", pass);
                    cmd.Parameters.AddWithValue("@cus_username", username);
                    cmd.Parameters.AddWithValue("@cus_dob", dob);
                    cmd.Parameters.AddWithValue("@cus_gender", gender);

                    var ctr = cmd.ExecuteNonQuery();
                    if (ctr >= 1)
                    {
                        data.Add(new
                        {
                            success = 1
                        });
                    }
                    else
                    {
                        data.Add(new
                        {
                            success = 0
                        });
                    }
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        private bool IsUsernameUnique(string username)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT COUNT(*) FROM customer WHERE username = @username";
                    cmd.Parameters.AddWithValue("@username", username);

                    int count = (int)cmd.ExecuteScalar();
                    return count == 0;
                }
            }
        }
        private bool IsEmailUnique(string email)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT COUNT(*) FROM customer WHERE email = @email";
                    cmd.Parameters.AddWithValue("@email", email);

                    int count = (int)cmd.ExecuteScalar();
                    return count == 0;
                }
            }
        }
        /////////////////////search customer

        public ActionResult SearchCustomer()
        {
            var data = new List<object>();
            var id = Request["idno"];
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = " SELECT * FROM CUSTOMER "
                                    + " WHERE ID ='" + id + "'";
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        data.Add(new
                        {
                            mess = 0,
                            name = reader["FNAME"].ToString(),
                            email = reader["EMAIL"].ToString(),
                            pass = reader["PASSWORD"].ToString()
                        });
                    }
                    else
                    {
                        data.Add(new
                        {
                            mess = 1
                        });
                    }

                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        //////////////////update
        public ActionResult CustomerUpdate()
        {
            var data = new List<object>();
            var idno = Request["idno"];
            var name = Request["name"];
            var email = Request["email"];
            var pass = Request["pass"];

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "UPDATE CUSTOMER SET "
                                    + " FNAME = @name, "
                                    + " EMAIL = @email, "
                                    + " PASSWORD = @pass "
                                    + " WHERE ID='" + idno + "'";
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@pass", pass);
                    var ctr = cmd.ExecuteNonQuery();
                    if (ctr > 0)
                    {
                        data.Add(new
                        {
                            mess = 0

                        });
                    }
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CustomerUpdateEdit()
        {
            var data = new List<object>();
            var idno = Request["id"];
            var lname = Request["lname"]?.Trim();
            var fname = Request["fname"]?.Trim();
            var email = Request["email"]?.Trim();
            var pass = Request["pass"];
            var uname = Request["uname"]?.Trim();
            var dob = Request["dob"];
            var gender = Request["gender"];

            if (string.IsNullOrWhiteSpace(lname))
            {
                data.Add(new { mess = 1, error = "Last name cannot be empty or spaces only." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(fname))
            {
                data.Add(new { mess = 1, error = "First name cannot be empty or spaces only." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                data.Add(new { mess = 1, error = "Invalid email address." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (pass.Length < 6)
            {
                data.Add(new { mess = 1, error = "Password should be at least 6 characters long." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$");
            if (!passwordRegex.IsMatch(pass))
            {
                data.Add(new { mess = 1, error = "Password must contain at least one lowercase letter, one uppercase letter, one number, and be at least six characters long." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(uname))
            {
                data.Add(new { mess = 1, error = "Username cannot be empty or spaces only." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (!IsValidDate(dob))
            {
                data.Add(new { mess = 1, error = "Invalid date of birth." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(gender) || (gender != "M" && gender != "F" && gender != "O"))
            {
                data.Add(new { mess = 1, error = "Invalid gender value." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE CUSTOMER SET "
                                        + "FNAME = @fname, "
                                        + "LNAME = @lname, "
                                        + "EMAIL = @email, "
                                        + "PASSWORD = @pass, "
                                        + "USERNAME = @uname, "
                                        + "DATE_OF_BIRTH = @dob, "
                                        + "GENDER = @gender "
                                        + "WHERE ID = @idno";
                        cmd.Parameters.AddWithValue("@idno", idno);
                        cmd.Parameters.AddWithValue("@fname", fname);
                        cmd.Parameters.AddWithValue("@lname", lname);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@pass", pass);
                        cmd.Parameters.AddWithValue("@uname", uname);
                        cmd.Parameters.AddWithValue("@dob", dob);
                        cmd.Parameters.AddWithValue("@gender", gender);

                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr > 0)
                        {
                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "Update failed." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                data.Add(new { mess = 1, error = ex.Message });
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidDate(string dateStr)
        {
            return DateTime.TryParse(dateStr, out _);
        }


        ///////////////delete
        public ActionResult CustomerDelete()
        {
            var data = new List<object>();
            var id = Request["id"];
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM CUSTOMER WHERE ID = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    var ctr = cmd.ExecuteNonQuery();
                    if (ctr > 0)
                    {
                        data.Add(new
                        {
                            mess = 0
                        });
                    }

                }

            }
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        public ActionResult DeleteCustomer(int id)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM CUSTOMER WHERE ID = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("ListAllCustomers","Home");
        }
        /////////////////////search product

        public ActionResult SearchProduct()
        {
            var data = new List<object>();
            var id = Request["idno"];
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = " SELECT * FROM PRODUCT "
                                    + " WHERE ID ='" + id + "'";
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        data.Add(new
                        {
                            mess = 0,
                            name = reader["NAME"].ToString(),
                            desc = reader["DESCRIPTION"].ToString(),
                            price = reader["PRICE"].ToString(),
                            qty = reader["QUANTITY"].ToString()
                        });
                    }
                    else
                    {
                        data.Add(new
                        {
                            mess = 1
                        });
                    }

                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        ////product update
        [HttpPost]
        public ActionResult ProductUpdate(FormCollection collection, HttpPostedFileBase image)
        {
            var data = new List<object>();
            try
            {
                var idno = collection["idno"];
                var name = collection["name"];
                var desc = collection["desc"];
                var priceStr = collection["price"];
                var qtyStr = collection["qty"];

                string imageFileName = null;
                if (image != null && image.ContentLength > 0)
                {
                    imageFileName = Path.GetFileName(image.FileName);
                    var extension = Path.GetExtension(imageFileName).ToLower();
                    int filesize = image.ContentLength;

                    if (!IsImageFile(extension))
                    {
                        throw new Exception("Invalid file type. Only images (jpg, jpeg, png, gif, bmp) are allowed.");
                    }

                    const int maxSize = 25 * 1024 * 1024;
                    if (filesize > maxSize)
                    {
                        throw new Exception("File size exceeds the maximum allowed limit (25MB).");
                    }

                    string logpath = "c:\\Uploads";
                    string filepath = Path.Combine(logpath, imageFileName);
                    image.SaveAs(filepath);
                }

                if (!float.TryParse(priceStr, out float price) || price < 0)
                {
                    throw new Exception("Invalid price format or negative value.");
                }

                if (!int.TryParse(qtyStr, out int qty) || qty < 0)
                {
                    throw new Exception("Invalid quantity format or negative value.");
                }

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE PRODUCT SET "
                                        + "NAME = @name, "
                                        + "DESCRIPTION = @desc, "
                                        + "PRICE = @price, "
                                        + "QUANTITY = @qty";
                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            cmd.CommandText += ", IMAGE = @image";
                            cmd.Parameters.AddWithValue("@image", imageFileName);
                        }
                        cmd.CommandText += " WHERE ID = @idno";
                        cmd.Parameters.AddWithValue("@idno", idno);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@desc", desc);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@qty", qty);

                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr > 0)
                        {
                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "Update failed." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                data.Add(new { mess = 1, error = ex.Message });
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditProduct(int id)
        {
            string query = "SELECT * FROM PRODUCT WHERE ID = @id";

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = new SqlCommand(query, db))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.id = reader["ID"].ToString();
                            ViewBag.name = reader["NAME"].ToString();
                            ViewBag.description = reader["DESCRIPTION"].ToString();
                            ViewBag.category = reader["CATEGORY"].ToString();
                            ViewBag.price = reader["PRICE"].ToString();
                            ViewBag.quantity = reader["QUANTITY"].ToString();
                            ViewBag.image = reader["IMAGE"].ToString();
                        }
                    }
                }
            }

            return View();
        }
        [HttpPost]
        public ActionResult ProductUpdateEdit(FormCollection collection, HttpPostedFileBase image)
        {
            var data = new List<object>();
            try
            {
                var idno = collection["idno"];
                var name = collection["name"]?.Trim();
                var desc = collection["desc"]?.Trim();
                var category = collection["category"];
                var priceStr = collection["price"];
                var qtyStr = collection["qty"];

                if (string.IsNullOrWhiteSpace(name))
                {
                    data.Add(new { mess = 1, error = "Name cannot be empty or spaces only." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(desc))
                {
                    data.Add(new { mess = 1, error = "Description cannot be empty or spaces only." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                if (!float.TryParse(priceStr, out float price) || price < 0)
                {
                    data.Add(new { mess = 1, error = "Invalid price format or negative value." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                if (!int.TryParse(qtyStr, out int qty) || qty <= 0)
                {
                    data.Add(new { mess = 1, error = "Invalid quantity format or non-positive value." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                string imageFileName = null;
                if (image != null && image.ContentLength > 0)
                {
                    imageFileName = Path.GetFileName(image.FileName);
                    var extension = Path.GetExtension(imageFileName).ToLower();
                    int filesize = image.ContentLength;

                    if (!IsImageFile(extension))
                    {
                        data.Add(new { mess = 1, error = "Invalid file type. Only images (jpg, jpeg, png, gif, bmp) are allowed." });
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }

                    const int maxSize = 25 * 1024 * 1024;
                    if (filesize > maxSize)
                    {
                        data.Add(new { mess = 1, error = "File size exceeds the maximum allowed limit (25MB)." });
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }

                    string logpath = "c:\\Uploads";
                    string filepath = Path.Combine(logpath, imageFileName);
                    image.SaveAs(filepath);
                }

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE PRODUCT SET "
                                        + "NAME = @name, "
                                        + "DESCRIPTION = @desc, "
                                        + "CATEGORY = @category, "
                                        + "PRICE = @price, "
                                        + "QUANTITY = @qty";
                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            cmd.CommandText += ", IMAGE = @image";
                            cmd.Parameters.AddWithValue("@image", imageFileName);
                        }
                        cmd.CommandText += " WHERE ID = @idno";
                        cmd.Parameters.AddWithValue("@idno", idno);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@desc", desc);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@qty", qty);

                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr > 0)
                        {
                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "Update failed." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                data.Add(new { mess = 1, error = ex.Message });
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        ///////////////product delete
        public ActionResult ProductDelete()
        {
            var data = new List<object>();
            var idno = Request["idno"];
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM PRODUCT WHERE ID='" + idno + "'";
                    var ctr = cmd.ExecuteNonQuery();
                    if (ctr > 0)
                    {
                        data.Add(new
                        {
                            mess = 0
                        });
                    }

                }

            }
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        public ActionResult DeleteProduct(int id)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE FROM PRODUCT WHERE ID = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("ListAllProducts", "Home");
        }
    }
}