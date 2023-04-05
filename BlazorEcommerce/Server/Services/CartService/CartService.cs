using BlazorEcommerce.Shared;
using System.Security.Claims;

namespace BlazorEcommerce.Server.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly DataContext _dataContext;
        private readonly IAuthService _authService;

        public CartService(DataContext dataContext, IAuthService authService)
        {
            _dataContext = dataContext;
            _authService = authService;
        }

        public async Task<ServiceResponse<List<CartProductResponse>>> GetCartProducts(List<CartItem> cartItems)
        {
            var result = new ServiceResponse<List<CartProductResponse>>
            {
                Data = new List<CartProductResponse>()
            };

            foreach (var item in cartItems)
            {
                var product = await _dataContext.Products
                    .Where(p => p.Id == item.ProductId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    continue;
                }

                var productVariant = await _dataContext.ProductVariants
                    .Where(v => v.ProductId == item.ProductId
                    && v.ProductTypeId == item.ProductTypeId)
                    .Include(v => v.ProductType)
                    .FirstOrDefaultAsync();

                if (productVariant == null)
                {
                    continue;
                }

                var cartProduct = new CartProductResponse
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    ImageUrl = product.ImageUrl,
                    Price = productVariant.Price,
                    ProductType = productVariant.ProductType.Name,
                    ProductTypeId = productVariant.ProductTypeId,
                    Quantity = item.Quantity
                };

                result.Data.Add(cartProduct);
            }

            return result;
        }

        public async Task<ServiceResponse<List<CartProductResponse>>> StoreCartItems(List<CartItem> cartItems)
        {
            cartItems.ForEach(cartitem => cartitem.UserId = _authService.GetUserId());
            _dataContext.CartItems.AddRange(cartItems);
            await _dataContext.SaveChangesAsync();

            return await GetDbCartProducts();
        }

        public async Task<ServiceResponse<int>> GetCartItemsCount()
        {
            var count = (await _dataContext.CartItems.Where(ci => ci.UserId == _authService.GetUserId()).ToListAsync()).Count;
            return new ServiceResponse<int> { Data = count };
        }

        public async Task<ServiceResponse<List<CartProductResponse>>> GetDbCartProducts(int? userId = null)
        {
            if (userId == null)
                userId = _authService.GetUserId();

            return await GetCartProducts(await _dataContext.CartItems
                .Where(ci => ci.UserId == userId).ToListAsync());
        }

        public async Task<ServiceResponse<bool>> AddToCart(CartItem cartItem)
        {
            cartItem.UserId = _authService.GetUserId();
            var sameItem = await _dataContext.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == cartItem.ProductId &&
                ci.ProductTypeId == cartItem.ProductTypeId && ci.UserId == cartItem.UserId);
            if (sameItem == null)
            {
                _dataContext.CartItems.Add(cartItem);
            }
            else
            {
                sameItem.Quantity += cartItem.Quantity;
            }

            _dataContext.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true };
        }

        public async Task<ServiceResponse<bool>> UpdateQuantity(CartItem cartItem)
        {
            var dbCartItem = await _dataContext.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == cartItem.ProductId &&
                ci.ProductTypeId == cartItem.ProductTypeId && ci.UserId == _authService.GetUserId());

            if (dbCartItem == null)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Scecess = false,
                    Message = "Cart item does not exist."
                };
            }

            dbCartItem.Quantity = cartItem.Quantity;
            await _dataContext.SaveChangesAsync();

            return new ServiceResponse<bool>
            {
                Data = true
            };
        }

        public async Task<ServiceResponse<bool>> RemoveItemFromCart(int productId, int productTypeId)
        {
            var dbCartItem = await _dataContext.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == productId &&
                ci.ProductTypeId == productTypeId && ci.UserId == _authService.GetUserId());

            if (dbCartItem == null)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Scecess = false,
                    Message = "Cart item does not exist."
                };
            }

            _dataContext.CartItems.Remove(dbCartItem);
            await _dataContext.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true };
        }
    }
}
