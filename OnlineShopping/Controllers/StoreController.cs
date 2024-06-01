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
    public class StoreController : Controller
    {

        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\admin\source\repos\OnlineShopping\OnlineShopping\App_Data\OnlineShopping.mdf;Integrated Security=True";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewProducts()
        {

            return View();
        }

        public ActionResult ProductDetail(int id)
        {
            using (SqlConnection db = new SqlConnection(connStr))
            {
                db.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM PRODUCT WHERE ID = @ID", db))
                {
                    cmd.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.ID = reader["ID"];
                            ViewBag.NAME = reader["NAME"];
                            ViewBag.DESCRIPTION = reader["DESCRIPTION"];
                            ViewBag.CATEGORY = reader["CATEGORY"];
                            ViewBag.PRICE = reader["PRICE"];
                            ViewBag.QUANTITY = reader["QUANTITY"];
                            ViewBag.IMAGE = reader["IMAGE"];
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                }
            }

            return View();
        }
    }
}