using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;
using Shopping.Repository;

namespace Shopping.Areas.Admin.Controllers
{

	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class OrderController : Controller
	{
		private readonly DataContext _dataContext;
		public OrderController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
        /*public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
		}*/
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<OrderModel> order = _dataContext.Orders.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = order.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = order.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
			var DetailsOrder = await _dataContext.OrderDetails.Include(od=>od.Product).Where(od=>od.OrderCode==ordercode).ToListAsync();
            return View(DetailsOrder);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }
            order.Status = status;

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status đã cập nhật thành công" });
            }catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi cập nhật order status");
            }
        }
        /*  [HttpGet]
           public async Task<IActionResult> Delete(string ordercode)
           {
               var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
               if (order == null)
               {
                   return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
               }

               // Xóa các OrderDetails liên quan trước khi xóa Order
               var orderDetails = _dataContext.OrderDetails.Where(od => od.OrderCode == ordercode);
               _dataContext.OrderDetails.RemoveRange(orderDetails);

               _dataContext.Orders.Remove(order);

               try
               {
                   await _dataContext.SaveChangesAsync();
                   return Ok(new { success = true, message = "Đơn hàng đã được xóa thành công" });
               }
               catch (Exception ex)
               {
                   return StatusCode(500, new { success = false, message = "Lỗi khi xóa đơn hàng" });
               }
           }*/

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }
            try
            {

                //delete order
                _dataContext.Orders.Remove(order);


                await _dataContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }




    }
}
