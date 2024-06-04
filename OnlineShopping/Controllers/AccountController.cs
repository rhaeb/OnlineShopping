using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;


namespace OnlineShopping.Controllers
{
    public class AccountController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\admin\source\repos\OnlineShopping\OnlineShopping\App_Data\OnlineShopping.mdf;Integrated Security=True";

        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>
        {
            { "admin", "Password123" },
        };

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginForm(string usernameOrEmail, string password)
        {
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username or email and password are required";
                return View("Login");
            }
            if (Users.ContainsKey(usernameOrEmail) && Users[usernameOrEmail] == password)
            {
                Session["Username"] = usernameOrEmail;
                Session["IsAuthenticated"] = true;
                Session["IsAdmin"] = (usernameOrEmail == "admin");
                return RedirectToAction("Index", "Home");
            }

            if (IsValidCustomer(usernameOrEmail, password))
            {
                Session["Username"] = usernameOrEmail;
                Session["IsAuthenticated"] = true;
                Session["IsAdmin"] = false;
                return RedirectToAction("ViewProducts", "Store");
            }

            ViewBag.Error = "Invalid username or password";
            return View("Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        private bool IsValidCustomer(string usernameOrEmail, string password)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT COUNT(*) FROM CUSTOMER WHERE (username = @usernameOrEmail OR email = @usernameOrEmail) AND password = @password";
                    cmd.Parameters.AddWithValue("@usernameOrEmail", usernameOrEmail);
                    cmd.Parameters.AddWithValue("@password", password);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public ActionResult SignUp()
        {
            return View();
        }

        public ActionResult SignUpForm()
        {
            var data = new List<object>();
            var lname = Request["lname"];
            var fname = Request["fname"];
            var email = Request["email"];
            var pass = Request["pass"];
            var username = Request["username"];
            var dob = Request["dob"];
            var gender = Request["gender"];

            if (string.IsNullOrWhiteSpace(lname) || string.IsNullOrWhiteSpace(fname) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(dob) ||
                string.IsNullOrWhiteSpace(gender))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "All fields are required."
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (username.ToLower() == "admin")
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Username 'admin' is invalid."
                });
            }

            if (pass.Length < 6)
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Password should be at least 6 characters long."
                });
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$");
            if (!passwordRegex.IsMatch(pass))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number."
                });
            }

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Invalid email format."
                });
            }

            if (data.Count > 0)
            {
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            if (!IsUsernameUnique(username))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Username is already taken. Please choose a different username."
                });
            }

            if (!IsEmailUnique(email))
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "Email is already taken."
                });
            }

            if (data.Count > 0)
            {
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT INTO customer (lname, fname, email, password, username, date_of_birth, gender) " +
                                      "VALUES (@cus_lname, @cus_fname, @cus_email, @cus_pass, @cus_username, @cus_dob, @cus_gender)";
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
                            success = 0,
                            errorMessage = "An error occurred while creating your account. Please try again."
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


    }
}
