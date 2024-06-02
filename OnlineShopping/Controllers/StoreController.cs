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
        public class CartItemViewModel
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Price { get; set; }
        }

        public ActionResult ViewProducts()
        {

            return View();
        }

        public ActionResult Cart(string user)
        {
            var cusid = GetCustomerIdByUsername(user);
            var cartItems = new List<CartItemViewModel>();

            using (SqlConnection db = new SqlConnection(connStr))
            {
                db.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT * FROM CART WHERE CUS_ID = @ID", db))
                {
                    cmd.Parameters.AddWithValue("@ID", cusid);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.ID = reader["ID"];
                            ViewBag.CUS_ID = reader["CUS_ID"];
                            ViewBag.DATE_CREATED = reader["DATE_CREATED"];
                            ViewBag.TOTAL = reader["TOTAL"];
                            ViewBag.QUANTITY = reader["QUANTITY"];
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                }

                using (SqlCommand cmd = new SqlCommand("SELECT * FROM CART_ITEM WHERE CART_ID = @CartId", db))
                {
                    cmd.Parameters.AddWithValue("@CartId", ViewBag.ID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cartItems.Add(new CartItemViewModel
                            {
                                ProductId = Convert.ToInt32(reader["PRODUCT_ID"]),
                                Quantity = Convert.ToInt32(reader["QUANTITY"]),
                                Subtotal = Convert.ToDecimal(reader["SUBTOTAL"]),
                                Price = Convert.ToDecimal(reader["PRICE"])
                            });
                        }
                    }
                }
            }

            ViewBag.CartItems = cartItems;

            return View();
        }

        public ActionResult UpdateCartItem()
        {
            var data = new List<object>();
            try
            {
                var productId = Int32.Parse(Request["productId"]);
                var quantity = Int32.Parse(Request["quantity"]);
                var cartId = Int32.Parse(Request["cartId"]);
                using (SqlConnection db = new SqlConnection(connStr))
                {
                    db.Open();

                    using (SqlCommand cmd = db.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE CART_ITEM SET QUANTITY = @Quantity, SUBTOTAL = @Subtotal WHERE PRODUCT_ID = @ProductId AND CART_ID = @CartId";
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@Subtotal", quantity * GetProductPrice(productId));
                        cmd.Parameters.AddWithValue("@ProductId", productId);
                        cmd.Parameters.AddWithValue("@CartId", cartId);

                        cmd.ExecuteNonQuery();
                    }

                    UpdateCartwithItem(db, cartId);

                    data.Add(new
                    {
                        success = 1,
                        errorMessage = "Updated successfully"
                    });

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "An error occurred while adding the product to the cart: " + ex.Message
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        private bool IsCartEmpty(SqlConnection db, int cartId)
        {
            using (SqlCommand cmd = db.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM CART_ITEM WHERE CART_ID = @CartId";
                cmd.Parameters.AddWithValue("@CartId", cartId);

                return (int)cmd.ExecuteScalar() == 0;
            }
        }
        private void DeleteCart(SqlConnection db, int cartId)
        {
            using (SqlCommand cmd = db.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM CART WHERE ID = @CartId";
                cmd.Parameters.AddWithValue("@CartId", cartId);
                cmd.ExecuteNonQuery();
            }
        }

        public ActionResult DeleteCartItem()
        {
            var data = new List<object>();
            try
            {
                var productId = Int32.Parse(Request["productId"]);
                var cartId = Int32.Parse(Request["cartId"]);
                using (SqlConnection db = new SqlConnection(connStr))
                {
                    db.Open();
                    int currentQuantity = GetCartItemQuantity(db, cartId, productId);

                    UpdateProductQuantity(db, productId, currentQuantity);

                    using (SqlCommand cmd = db.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM CART_ITEM WHERE PRODUCT_ID = @ProductId AND CART_ID = @CartId";
                        cmd.Parameters.AddWithValue("@ProductId", productId);
                        cmd.Parameters.AddWithValue("@CartId", cartId);

                        cmd.ExecuteNonQuery();
                    }

                    UpdateCart(db, cartId);

                    if (IsCartEmpty(db, cartId))
                    {
                        DeleteCart(db, cartId);
                    }

                    data.Add(new
                    {
                        success = 1,
                        errorMessage = "Updated successfully"
                    });

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "An error occurred while adding the product to the cart: " + ex.Message
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        private decimal GetProductPrice(int productId)
        {
            using (SqlConnection db = new SqlConnection(connStr))
            {
                db.Open();
                using (SqlCommand cmd = db.CreateCommand())
                {
                    cmd.CommandText = "SELECT PRICE FROM PRODUCT WHERE ID = @ProductId";
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    return Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
        }
        private int GetCartItemQuantity(SqlConnection db, int cartId, int productId)
        {
            using (SqlCommand cmd = db.CreateCommand())
            {
                cmd.CommandText = "SELECT QUANTITY FROM CART_ITEM WHERE CART_ID = @CartId AND PRODUCT_ID = @ProductId";
                cmd.Parameters.AddWithValue("@CartId", cartId);
                cmd.Parameters.AddWithValue("@ProductId", productId);

                return (int)cmd.ExecuteScalar();
            }
        }

        private void UpdateCartwithItem(SqlConnection db, int cartId)
        {
            using (SqlCommand cmd = db.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "UPDATE CART SET TOTAL = (SELECT SUM(SUBTOTAL) FROM CART_ITEM WHERE CART_ID = @CartId), QUANTITY = (SELECT SUM(QUANTITY) FROM CART_ITEM WHERE CART_ID = @CartId) WHERE ID = @CartId";
                cmd.Parameters.AddWithValue("@CartId", cartId);
                cmd.ExecuteNonQuery();
            }
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

        public ActionResult AddToCart()
        {
            var data = new List<object>();

            try
            {
                var productIdStr = Request["productId"];
                var quantityStr = Request["quantity"];
                var priceStr = Request["price"];
                var username = Session["Username"] as string;

                Console.WriteLine($"Product ID: {productIdStr}, Quantity: {quantityStr}, Price: {priceStr}, Subtotal: {username}");

                if (string.IsNullOrEmpty(productIdStr) || string.IsNullOrEmpty(quantityStr) || string.IsNullOrEmpty(priceStr) || string.IsNullOrEmpty(username))
                {
                    throw new ArgumentNullException("One or more required values are null or empty.");
                }

                var productId = Int32.Parse(productIdStr);
                var quantity = Int32.Parse(quantityStr);
                var price = float.Parse(priceStr);
                var subtotal = quantity * price;
                var customerId = GetCustomerIdByUsername(username);

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        Console.WriteLine($"Product ID: {productId}, Quantity: {quantity}, Price: {price}, Subtotal: {subtotal}, Username: {username}, Customer ID: {customerId}");
                        int cartId = GetCustomerCartId(db, customerId);

                        if (cartId == 0)
                        {
                            cmd.CommandText = "INSERT INTO cart(cus_id, total) VALUES (@cusId, 0); SELECT SCOPE_IDENTITY()";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@cusId", customerId);
                            cartId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        cmd.CommandText = "SELECT COUNT(*) FROM cart_item WHERE product_id = @productId AND cart_id = @cartId";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.Parameters.AddWithValue("@cartId", cartId);
                        int itemCount = (int)cmd.ExecuteScalar();

                        if (itemCount > 0)
                        {
                            cmd.CommandText = "UPDATE cart_item SET quantity = quantity + @quantity, subtotal = subtotal + @subtotal WHERE product_id = @productId AND cart_id = @cartId";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@quantity", quantity);
                            cmd.Parameters.AddWithValue("@subtotal", subtotal);
                            cmd.Parameters.AddWithValue("@productId", productId);
                            cmd.Parameters.AddWithValue("@cartId", cartId);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            cmd.CommandText = "INSERT INTO cart_item(product_id, subtotal, quantity, cart_id, price ) VALUES (@product_id, @subtotal, @quantity, @cartId, @price)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@product_id", productId);
                            cmd.Parameters.AddWithValue("@subtotal", subtotal);
                            cmd.Parameters.AddWithValue("@quantity", quantity);
                            cmd.Parameters.AddWithValue("@cartId", cartId);
                            cmd.Parameters.AddWithValue("@price", price);
                            cmd.ExecuteNonQuery();
                        }

                        UpdateProductQuantity(db, productId, quantity);
                        UpdateCart(db, cartId);
                        data.Add(new
                        {
                            success = 1,
                            message = "Product added to cart successfully."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                data.Add(new
                {
                    success = 0,
                    errorMessage = "An error occurred while adding the product to the cart: " + ex.Message
                });
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        private void UpdateCart(SqlConnection db, int cartId)
        {
            using (var cmd = db.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "UPDATE cart SET total = (SELECT SUM(subtotal) FROM cart_item WHERE cart_id = @cart_id) WHERE Id = @cart_id";
                cmd.Parameters.AddWithValue("@cart_id", cartId);
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                cmd.CommandText = "UPDATE cart SET quantity = quantity + 1 WHERE Id = @cart_id";
                cmd.Parameters.AddWithValue("@cart_id", cartId);
                cmd.ExecuteNonQuery();
            }
        }

        public int GetCustomerIdByUsername(string username)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT ID FROM CUSTOMER WHERE username = @username";
                    cmd.Parameters.AddWithValue("@username", username);

                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("Customer ID not found for the provided username.");
                    }
                }
            }
        }

        private int GetCustomerCartId(SqlConnection db, int customerId)
        {
            int cartId = 0;
            using (var cmd = db.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT id FROM cart WHERE cus_id = @customerId";
                cmd.Parameters.AddWithValue("@customerId", customerId);
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    cartId = Convert.ToInt32(result);
                }
            }
            return cartId;
        }

        private void UpdateProductQuantity(SqlConnection db, int productId, int quantity)
        {
            using (var cmd = db.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "UPDATE PRODUCT SET QUANTITY = QUANTITY - @quantity WHERE ID = @productId";
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}