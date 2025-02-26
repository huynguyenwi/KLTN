using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;
using Shopping.Repository;

namespace Shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize]
	public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {

            if (ModelState.IsValid)
            {
                //code them du lieu
                brand.Slug = brand.Name.Replace(" ", "-");
                var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thương hiệu đã có trong database");
                    return View(brand);
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm thương hiệu thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang bị lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }


            return View(brand);
        }

        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }

        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(BrandModel brand)
         {

             var existed_brand = _dataContext.Brands.Find(brand.Id); //tìm sp theo id brand

             if (ModelState.IsValid)
             {
                 //brand.Slug = brand.Name.Replace(" ", "-");
                 existed_brand.Slug = brand.Name.Replace(" ", "-");

                 //Update other brand properties
                 existed_brand.Name = brand.Name;
                 existed_brand.Description = brand.Description;
                 existed_brand.Status = brand.Status;


                 _dataContext.Update(existed_brand);
                 await _dataContext.SaveChangesAsync();
                 TempData["success"] = "Cập nhật thương hiệu thành công";
                 return RedirectToAction("Index");
             }
             else
             {
                 TempData["error"] = "Model có một vài thứ đang bị lỗi";
                 List<string> errors = new List<string>();
                 foreach (var value in ModelState.Values)
                 {
                     foreach (var error in value.Errors)
                     {
                         errors.Add(error.ErrorMessage);
                     }
                 }
                 string errorMessage = string.Join("\n", errors);
                 return BadRequest(errorMessage);
             }


             return View(brand);
         }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
            var existed_brand = await _dataContext.Brands.FindAsync(brand.Id); // Tìm thương hiệu đang chỉnh sửa

            if (existed_brand == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Tạo Slug từ Name
                string newSlug = brand.Name.Replace(" ", "-").ToLower();

                // Kiểm tra Slug có trùng với thương hiệu khác không
                var existingSlug = await _dataContext.Brands
                    .Where(b => b.Id != brand.Id) // Loại trừ thương hiệu hiện tại
                    .FirstOrDefaultAsync(b => b.Slug == newSlug);

                if (existingSlug != null)
                {
                    ModelState.AddModelError("", "Tên thương hiệu đã tồn tại. Vui lòng chọn tên khác.");
                    return View(brand);
                }

                // Cập nhật dữ liệu
                existed_brand.Slug = newSlug;
                existed_brand.Name = brand.Name;
                existed_brand.Description = brand.Description;
                existed_brand.Status = brand.Status;

                _dataContext.Update(existed_brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thương hiệu thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có lỗi. Vui lòng kiểm tra lại.";
                return View(brand);
            }
        }


        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            if (brand == null)
            {
                return NotFound();
            }

            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thương hiệu đã được xóa thành công";
            return RedirectToAction("Index");
        }

    }
}
