using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace OnlineShopping.Controllers
{
    public class AccountController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\admin\source\repos\OnlineShopping\OnlineShopping\App_Data\OnlineShopping.mdf;Integrated Security=True";

        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>
        {
            { "admin", "password123" },
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
                return RedirectToAction("Index", "Store");
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

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT INTO customer(lname, fname, email, password, username, date_of_birth, gender)"
                        + "VALUES (@cus_lname, @cus_fname, @cus_email, @cus_pass, @cus_username, @cus_dob, @cus_gender)";//yoursqlcommand
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
                };
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}
