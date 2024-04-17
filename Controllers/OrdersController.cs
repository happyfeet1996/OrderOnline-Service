using Microsoft.AspNetCore.Mvc;

namespace OrderOnline.Controllers
{
    [ApiController]
    [Route("api/ordersManager")]
    public class OrdersController : ControllerBase
    {
        [HttpPost]
        [Route("addOrder")]
        public IActionResult PostAddOrder([FromForm] int userId, OrderStatus orderStatus, DateTime date, List<OrderDetailsDto> orderDetailses)
        {
            try
            {
                Order order = new Order();
                order.Status = orderStatus;
                order.CustomerId = userId;
                order.Date = date;
                string guid = OrdersManager.AddOrder(userId, order, orderDetailses);
                return Ok(guid);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("modifyOrderStatus")]
        public IActionResult PostModifyOrderStatus([FromForm] string orderId, OrderStatus status)
        {
            try
            {
                OrdersManager.ModifyOrderStatus(status, orderId);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
