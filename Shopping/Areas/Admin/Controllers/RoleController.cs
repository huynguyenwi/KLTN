using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;
using Shopping.Repository;

namespace Shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(DataContext dataContext, RoleManager<IdentityRole> roleManager)
        {
            _dataContext = dataContext;
            _roleManager = roleManager;
        }
        /* public async Task<IActionResult> Index()
         {
             return View(await _dataContext.Roles.OrderByDescending(p => p.Id).ToListAsync());
         }*/
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<IdentityRole> role = _dataContext.Roles.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = role.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //brand.Skip(20).Take(10).ToList()

            var data = role.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
        [HttpGet]
        public IActionResult Create()
		{
			return View();
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            if(!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            return Redirect("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role = await _roleManager.FindByIdAsync(id);
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound();
            }    
            if(ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(id);

                if(role == null) {
                    return NotFound();
                }

                role.Name = model.Name;

                try
                {
                    await _roleManager.UpdateAsync(role);
                    TempData["success"] = "Update Role thành công.";
                    return RedirectToAction("Index");
                }catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi trong việc update");
                }
            }

            return View(model ?? new IdentityRole { Id = id});
        }


        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
               return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);

            if(role == null)
            {
                return NotFound();
            }

            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Role đã xóa thành công.";
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa role.");
            }

            return RedirectToAction("Index");
        }


    }
}
