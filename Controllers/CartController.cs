using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tuan3.Data;
using Tuan3.Models;
using Tuan3.Repositories;

namespace Tuan3.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(IProductRepository productRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        private List<CartItem> GetCartItems()
        {
            var cartSession = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartSession))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(cartSession);
        }

        private void SaveCartItems(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }

        public IActionResult Index()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCartItems(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartItems(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                ApplicationUserId = user.Id,
                ApplicationUser = user,
                ShippingAddress = "Nhập địa chỉ giao hàng của bạn", // Default or user's address
                PhoneNumber = "Nhập SĐT", // Default or user's phone
                TotalPrice = cart.Sum(c => c.Price * c.Quantity),
                OrderDetails = cart.Select(c => new OrderDetail
                {
                    ProductId = c.ProductId,
                    Count = c.Quantity,
                    Price = c.Price
                }).ToList()
            };

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                order.TotalPrice = cart.Sum(c => c.Price * c.Quantity);
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cart)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Count = item.Quantity,
                        Price = item.Price
                    });
                }
                await _context.SaveChangesAsync();

                // Clear cart
                HttpContext.Session.Remove("Cart");

                return RedirectToAction(nameof(OrderSuccess));
            }

            return View(order);
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
