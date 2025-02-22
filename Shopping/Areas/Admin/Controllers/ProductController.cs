using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;
using Shopping.Repository;

namespace Shopping.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p=>p.Id).Include(p=>p.Category).Include(p => p.Brand).ToListAsync());
        }
 
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if(ModelState.IsValid)
            {
                //code them du lieu
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong database");
                    return View(product);
                }
               
                if(product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }
                
                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang bị lỗi";
                List<string> errors = new List<string>();
                foreach(var value in ModelState.Values)
                {
                    foreach(var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }


            return View(product);
        }

       
        public async Task<IActionResult> Edit(int Id)
        {
            ProductModel product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            var existed_product = _dataContext.Products.Find(product.Id); //tìm sp theo id product
            
            if (ModelState.IsValid)
            { 
                product.Slug = product.Name.Replace(" ", "-");
                
                if (product.ImageUpload != null)
                {            
                    //upload new image
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    //delete old picture
                    string oldfilePath = Path.Combine(uploadDir, existed_product.Image);

                    try
                    {
                        if (System.IO.File.Exists(oldfilePath))
                        {
                            System.IO.File.Delete(oldfilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi xóa product image");
                    }

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_product.Image = imageName;
    
                }

                //Update other product properties
                existed_product.Name = product.Name;
                existed_product.Description = product.Description;
                existed_product.Price = product.Price;
                existed_product.CategoryId = product.CategoryId;
                existed_product.BrandId = product.BrandId;

                _dataContext.Update(existed_product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
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


            return View(product);
        }

        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(int Id, ProductModel product)
         {
             ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
             ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

             if (ModelState.IsValid)
             {
                 // Lấy sản phẩm cũ từ database
                 var existingProduct = await _dataContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Id);
                 if (existingProduct == null)
                 {
                     return NotFound();
                 }

                 // Giữ nguyên ảnh cũ nếu không có ảnh mới
                 if (product.ImageUpload != null)
                 {
                     string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                     string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                     string filePath = Path.Combine(uploadDir, imageName);

                     FileStream fs = new FileStream(filePath, FileMode.Create);
                     await product.ImageUpload.CopyToAsync(fs);
                     fs.Close();

                     // Gán ảnh mới
                     product.Image = imageName;
                 }
                 else
                 {
                     // Giữ nguyên ảnh cũ
                     product.Image = existingProduct.Image;
                 }

                 product.Slug = product.Name.Replace(" ", "-");

                 _dataContext.Update(product);
                 await _dataContext.SaveChangesAsync();
                 TempData["success"] = "Cập nhật sản phẩm thành công";
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
         }*/

        public async Task<IActionResult> Delete(int Id)
        {
            ProductModel product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
            {
                return NotFound();
            }

            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
            string oldfilePath = Path.Combine(uploadDir, product.Image);

            try
            {
                if (System.IO.File.Exists(oldfilePath))
                {
                    System.IO.File.Delete(oldfilePath);
                }
            }catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi xóa product image");
            }
           
            
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm đã được xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
