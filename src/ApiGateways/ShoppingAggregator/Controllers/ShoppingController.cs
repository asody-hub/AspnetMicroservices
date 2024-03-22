using Microsoft.AspNetCore.Mvc;
using Shopping.Aggregator.Models;
using Shopping.Aggregator.Services;
using System.Net;
using System.Runtime.CompilerServices;

namespace Shopping.Aggregator.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ShoppingController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly IBasketService _basketService;
    private readonly IOrderService _orderService;

    public ShoppingController(ICatalogService catalogService, IBasketService basketService, IOrderService orderService)
    {
        _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
        _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    [HttpGet("{userName}", Name = "GetShopping")]
    [ProducesResponseType(typeof(ShoppingModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingModel>> GetShopping(string userName)
    {
        var basket = await _basketService.GetBasket(userName);
        foreach (var basketItem in basket.Items) 
        {
            var product = await _catalogService.GetCatalog(basketItem.ProductId);

            basketItem.ProductName = product.Name;
            basketItem.Category = product.Category;
            basketItem.Summary = product.Summary;
            basketItem.Description = product.Description;
            basketItem.ImageFile = product.ImageFile;
        }
        var orders = await _orderService.GetOrdersByUserName(userName);

        var shoppingModel = new ShoppingModel
        {
            UserName = userName,
            BasketWithProducts = basket,
            Orders = orders
        };

        return Ok(shoppingModel);
    }
}
