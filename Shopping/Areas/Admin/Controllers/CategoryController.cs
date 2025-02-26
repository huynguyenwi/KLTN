using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;
using Shopping.Repository;

namespace Shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize]
	public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {

            if (ModelState.IsValid)
            {
                //code them du lieu
                category.Slug = category.Name.Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã có trong database");
                    return View(category);
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm danh mục thành công";
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


            return View(category);
        }

        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            return View(category);
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {

            var existed_category = _dataContext.Categories.Find(category.Id); //tìm sp theo id category

            if (ModelState.IsValid)
            {
                //category.Slug = category.Name.Replace(" ", "-");
                existed_category.Slug = category.Name.Replace(" ", "-");

                //Update other category properties
                existed_category.Name = category.Name;
                existed_category.Description = category.Description;
                existed_category.Status = category.Status;
               

                _dataContext.Update(existed_category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật danh mục thành công";
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


            return View(category);
        }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            var existed_category = await _dataContext.Categories.FindAsync(category.Id); // Tìm danh mục đang chỉnh sửa

            if (ModelState.IsValid)
            {
                // Tạo Slug mới từ Name
                category.Slug = category.Name.Replace(" ", "-").ToLower();

                // Kiểm tra Slug mới có trùng với danh mục khác không
                var existingSlug = await _dataContext.Categories
                    .Where(c => c.Id != category.Id) // Loại trừ danh mục đang chỉnh sửa
                    .FirstOrDefaultAsync(c => c.Slug == category.Slug);

                if (existingSlug != null)
                {
                    ModelState.AddModelError("", "Tên danh mục đã tồn tại. Vui lòng chọn tên khác.");
                    return View(category);
                }

                // Cập nhật Slug và các thuộc tính khác
                existed_category.Slug = category.Slug;
                existed_category.Name = category.Name;
                existed_category.Description = category.Description;
                existed_category.Status = category.Status;

                _dataContext.Update(existed_category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật danh mục thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài lỗi!";
                return View(category);
            }
        }


        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            //CategoryModel category = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Id == Id);
            if (category == null)
            {
                return NotFound();
            }

            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Danh mục đã được xóa thành công";
            return RedirectToAction("Index");
        }

    }
}
