using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using shoper.Models;

namespace shoper.Controllers
{
    public class HomeController : Controller
    {
        private readonly Model1 db;

        public HomeController()
        {
            db = new Model1();
        }

        public ActionResult Index(int? id)
        {
            var products = db.Products.ToList();
            if (id.HasValue)
            {
                var user = db.Customers.Find(id);
                if (user != null)
                {
                    ViewBag.UserName = user.CustomerID;
                }
            }
            return View(products);
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

        //HTTP get /Home/Register
        public ActionResult Register()
        {
            return View();
        }

        //HTTP Post /Home/Register
        [HttpPost]
        public ActionResult Register(FormCollection form)
        {
            Customer customer = new Customer();
            customer.FullName = form["FullName"];
            customer.Sex = form["Sex"];
            customer.Email = form["Email"];
            customer.Phone = form["Phone"];
            customer.Username = form["Username"];
            customer.Password = form["Password"];

            // Tạo CustomerID tự động
            int maxId = db.Customers.Any() ? db.Customers.Max(c => c.CustomerID) : 0;
            customer.CustomerID = maxId + 1;

            db.Customers.Add(customer);
            db.SaveChanges();
            return RedirectToAction("LogIn");
        }

        //HTTP get /Home/LogIn
        public ActionResult LogIn()
        {
            return View();
        }

        //HTTP Post /Home/LogIn
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var users = db.Customers.Where(u => u.Username == username && u.Password == password).ToList();
            if (users.Any())
            {
                foreach (var user in users)
                {
                    if (user != null)
                    {
                        // Đăng nhập thành công
                        Session["CustomerID"] = user.CustomerID;    //Lưu ID của người dùng
                        Session["FullName"] = user.FullName;    //Lưu Họ Và Tên của người dùng
                        Session["Sex"] = user.Sex;  //Lưu Giới Tính của người dùng
                        Session["Email"] = user.Email;  //Lưu địa chỉ Email của người dùng
                        Session["Phone"] = user.Phone;  //Lưu SĐT của người dùng
                        Session["Username"] = user.Username;    //Lưu Tên Đăng Nhập của người dùng
                        Session["Password"] = user.Password;    //Lưu Password của người dùng
                        var redirectUrl = Session["RedirectUrl"];
                        if (redirectUrl != null)
                        {
                            Session["RedirectUrl"] = null;
                            return Redirect(redirectUrl.ToString());
                        }
                        // Chuyển hướng đến trang Index với ID người dùng
                        return RedirectToAction("Index", new { id = user.CustomerID });
                    }
                }
            }
            else
            {
                // Đăng nhập thất bại, hiển thị lỗi
                ViewBag.Message = "Username or password is incorrect or does not exist.";
            }
            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session["Username"] = null;
            Session["Password"] = null;
            Session["Sex"] = null;
            Session["CustomerID"] = null; // Xóa ID của người dùng
            return RedirectToAction("Index");
        }

        public ActionResult Hot_Products()
        {
            var bestSellers = db.Products.Where(p => p.BestSeller == true).ToList();
            return View(bestSellers);
        } 

        public ActionResult Best_Seller_Products()
        {
            var hotproducts = db.Products.Where(p => p.HotProduct == true).ToList();
            return View(hotproducts);
        }
        

       

        public ActionResult Product_Detail(int id)
        {
            ViewBag.ProductID = id;
            var productDT = db.ProductDetailPages.ToList();
            return View(productDT); // Truyền model tới view
        }

        public ActionResult Dress()
        {
            var dress = db.Products.Where(p => p.Dress == false).ToList();
            if (dress == null)
            {
                dress = new List<Product>();
            }
            return View(dress);
        }

        public ActionResult Sexy_Nightgown()
        {
            var Sexy_Nightgown = db.Products.Where(p => p.Sexy_Nightgown == true).ToList();
            if (Sexy_Nightgown == null)
            {
                Sexy_Nightgown = new List<Product>();
            }
            return View(Sexy_Nightgown);
        }

        private int GetCustomerIdFromSession()
        {
            // Assuming you have a way to get the customer ID from the session
            return (int)Session["CustomerId"];
        }

        public ActionResult ShoppingCart()
        {
            // Lấy ID của khách hàng từ Session
            int customerId = (int)Session["CustomerID"];

            // Lấy tất cả các mục trong giỏ hàng của khách hàng
            var cartItems = db.Carts.Where(c => c.CustomerID == customerId).ToList();

            // Truyền dữ liệu giỏ hàng đến view
            return View(cartItems);
        }
        
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {
            // Lấy ID của khách hàng từ Session
            int customerId = (int)Session["CustomerID"];
            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingCart = db.Carts.FirstOrDefault(c => c.ProductID == productId && c.CustomerID == customerId);
            if (existingCart != null)
            {
                // Nếu có, cập nhật số lượng
                existingCart.Quantity += quantity;
                db.Entry(existingCart).State = EntityState.Modified;
            }
            else
            {
                
                // Nếu chưa, thêm sản phẩm mới vào giỏ
                var newCart = new Cart
                {
                    CustomerID = customerId,
                    ProductID = productId,
                    Quantity = quantity
                };
                db.Carts.Add(newCart);
            }

            // Lưu thay đổi vào database
            db.SaveChanges();

            // Chuyển hướng người dùng về trang xem giỏ hàng hoặc tiếp tục mua sắm
            return RedirectToAction("ShoppingCart");
        }
        //
        public ActionResult AddProduct(int productId)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if user is not logged in
            }

            int customerId = (int)Session["CustomerID"]; // Assuming CustomerID is stored in session

            var cartItem = db.Carts.FirstOrDefault(c => c.ProductID == productId && c.CustomerID == customerId);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
                db.Entry(cartItem).State = EntityState.Modified;
            }
            else
            {
                cartItem = new Cart 
                { 
                    ProductID = productId, 
                    Quantity = 1, 
                    CustomerID = customerId 
                };
                db.Carts.Add(cartItem);
            }
            db.SaveChanges();

            return RedirectToAction("ShoppingCart"); // Redirect back to the cart view
        }
        public ActionResult DropProduct(int productId)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int customerId = (int)Session["CustomerID"];

            var cartItem = db.Carts.FirstOrDefault(c => c.ProductID == productId && c.CustomerID == customerId);
            if (cartItem != null && cartItem.Quantity > 1)
            {
                cartItem.Quantity -= 1;
                db.Entry(cartItem).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("ShoppingCart");
        }
        public ActionResult RemoveProduct(int productId)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int customerId = (int)Session["CustomerID"];

            var cartItem = db.Carts.FirstOrDefault(c => c.ProductID == productId && c.CustomerID == customerId);
            if (cartItem != null)
            {
                db.Carts.Remove(cartItem);
                db.SaveChanges();
            }
            return RedirectToAction("ShoppingCart");
        }

        public ActionResult PurchaseOder()
        {
            // Kiểm tra Session để lấy CustomerId
            int? customerId = GetCustomerIdFromSession();

            if (customerId == null)
            {
                // Nếu không tồn tại CustomerId trong Session, có thể xử lý bằng cách chuyển hướng hoặc hiển thị thông báo lỗi
                // Ví dụ:
                TempData["ErrorMessage"] = "Không có thông tin khách hàng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Home"); // Chuyển hướng đến trang đăng nhập
            }

            // Lấy danh sách các đơn hàng đã mua của CustomerId từ cơ sở dữ liệu
            List<paymentorderdetail> purchaseHistory = db.paymentorderdetails.Where(p => p.CustomerID == customerId).ToList();

            // Kiểm tra xem Session["Username"] có tồn tại hay không
            if (Session["Username"] != null)
            {
                // Nếu có tồn tại Session["Username"], truyền danh sách này vào view
                return View("PurchaseOder", purchaseHistory);
            }
            else
            {
                // Nếu không tồn tại Session["Username"], có thể xử lý bằng cách chuyển hướng hoặc hiển thị thông báo lỗi
                TempData["ErrorMessage"] = "Phiên làm việc hết hạn hoặc không có thông tin người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Home"); // Chuyển hướng đến trang đăng nhập
            }
        }


        [HttpPost]
        public ActionResult Checkout()
        {
            int customerId = GetCustomerIdFromSession();

            // Lấy danh sách các sản phẩm được chọn từ form
            string[] selectedProducts = Request.Form.GetValues("selectedProducts");

            if (selectedProducts == null || selectedProducts.Length == 0)
            {
                ModelState.AddModelError("", "Please select at least one product to checkout.");
                return View("YourCartView", GetCartItems(customerId)); // Chuyển hướng lại view giỏ hàng nếu không có sản phẩm nào được chọn
            }

            // Tính tổng tiền cần thanh toán
            decimal totalAmount = 0;

            foreach (var productIdStr in selectedProducts)
            {
                int productId = Convert.ToInt32(productIdStr);
                var cartItem = db.Carts.FirstOrDefault(c => c.CustomerID == customerId && c.ProductID == productId);

                if (cartItem != null)
                {
                    // Tính tổng tiền cho từng sản phẩm
                    // Assuming Price is not nullable
                    decimal productTotal = 0;

                    if (cartItem.Quantity.HasValue && cartItem.Product != null && cartItem.Product.Price.HasValue)
                    {
                        productTotal = cartItem.Quantity.Value * cartItem.Product.Price.Value;
                    }
                    else
                    {
                        // Xử lý trường hợp dữ liệu thiếu
                        ModelState.AddModelError("", "Invalid data for calculating product total.");
                        return View("YourCartView", GetCartItems(customerId));
                    }
                    totalAmount += productTotal;

                    // Lưu thông tin đơn hàng vào bảng OrderDetail
                    paymentorderdetail paymentorderdetails = new paymentorderdetail
                    {
                        ProductID = cartItem.ProductID.Value,
                        Quantity = cartItem.Quantity.Value,
                        UnitPrice = cartItem.Product.Price.Value, // Assuming Price is not nullable
                        CustomerID = GetCustomerIdFromSession()
                    };
                    db.paymentorderdetails.Add(paymentorderdetails);
                    
                    // Xóa các mục trong giỏ hàng sau khi đã thanh toán
                    db.Carts.Remove(cartItem);
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();

            // Chuyển hướng đến view hoặc trang thanh toán thành công
            ViewBag.TotalAmount = totalAmount;
            return View("Checkout"); // Chuyển hướng đến view thanh toán thành công
        }


        // Phương thức này dùng để lấy CustomerId từ session
        private int GetCustomerIdFromSession1()
        {
            // Kiểm tra xem session có tồn tại CustomerId không
            if (Session["CustomerId"] != null && Session["CustomerId"] is int)
            {
                return (int)Session["CustomerId"];
            }

            // Nếu không có trong session, có thể xử lý tùy thuộc vào logic của bạn
            // Ví dụ: nếu không có, có thể chuyển hướng về trang đăng nhập hoặc trang chủ
            // return 0; // hoặc throw new Exception("Customer ID not found in session");
            // Tùy vào yêu cầu của bạn để xử lý trường hợp này
            // Trong ví dụ này, giả sử sẽ throw Exception để thông báo lỗi
            throw new Exception("Customer ID not found in session");
        }
       
        // Phương thức này dùng để lấy danh sách sản phẩm trong giỏ hàng của khách hàng
        private List<Cart> GetCartItems(int customerId)
        {
            return db.Carts.Include(c => c.Product).Where(c => c.CustomerID == customerId).ToList();
        }

        //HTTP get /Home/My_Account
        public ActionResult My_Account()
        {
            return View();
        }

        //HTTP Post /Home/My_Account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult My_Account(string Username, string Email, string Phone, string Sex)
        {
            int customerId = (int)Session["CustomerID"];

            // Lấy ID người dùng từ cơ sở dữ liệu sử dụng CustomerId
            var customer = db.Customers.Find(customerId);
            if (customer != null)
            {
                // Cập nhật thuộc tính của người dùng bằng các giá trị từ form
                customer.Username = Username;
                customer.Email = Email;
                customer.Phone = Phone;
                customer.Sex = Sex;

                // Lưu thay đổi vào trong database
                db.SaveChanges();

                // Tùy chọn, cập nhật các giá trị trong phiên làm việc (session)
                Session["Username"] = Username;
                Session["Email"] = Email;
                Session["Phone"] = Phone;
                Session["Sex"] = Sex;
                // Thông báo cho người dùng
                ViewBag.Message = "Account information updated successfully.";
            }
            else
            {
                ViewBag.Message = "Customer not found.";
            }

            // Chuyển hướng tới một view hoặc trả về một view với thông báo
            return View();
        }
    }
}
