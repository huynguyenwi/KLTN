﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;
using Shopping.Repository;

namespace Shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public ContactController(DataContext dataContext, IWebHostEnvironment webHostEnviroment)
        {
            _dataContext = dataContext;
            _webHostEnviroment = webHostEnviroment;
        }
        public IActionResult Index()
        {
            var contact = _dataContext.Contact.ToList();
            return View(contact);
        }
        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contact.FirstOrDefaultAsync();
            return View(contact);
        }
  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            var existed_contact = _dataContext.Contact.FirstOrDefault();


            if (ModelState.IsValid)
            {


                if (contact.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/logo");
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await contact.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_contact.LogoImg = imageName;
                }

                existed_contact.Name = contact.Name;
                existed_contact.Email = contact.Email;
                existed_contact.Description = contact.Description;
                existed_contact.Phone = contact.Phone;
                existed_contact.Map = contact.Map;


                _dataContext.Update(existed_contact);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thông tin web thành công";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
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
            return View(contact);
        }
    }
}
