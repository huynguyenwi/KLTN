using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;
using Shopping.Repository;
using System.Security.Claims;

namespace Shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
	{
		private readonly UserManager<AppUserModel> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public UserController(DataContext dataContext, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
            _dataContext = dataContext;
		}
        /*[HttpGet]
		public async Task<IActionResult> Index()
		{
            //lấy 3 bảng
            var usersWithRoles = await (from u in _dataContext.Users
                                       join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                       join r in _dataContext.Roles on ur.RoleId equals r.Id
                                       select new { User = u, RoleName = r.Name })
                                       .ToListAsync();

			return View(usersWithRoles);
		}*/
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10;

            var usersWithRoles = await (from user in _dataContext.Users
                                        join userRole in _dataContext.UserRoles on user.Id equals userRole.UserId
                                        join role in _dataContext.Roles on userRole.RoleId equals role.Id
                                        select new
                                        {
                                            Id = user.Id,
                                            UserName = user.UserName,
                                            Email = user.Email,
                                            PasswordHash = user.PasswordHash,
                                            PhoneNumber = user.PhoneNumber,
                                            RoleName = role.Name
                                        })
                                        .Skip((pg - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            int recsCount = await _dataContext.Users.CountAsync();
            var pager = new Paginate(recsCount, pg, pageSize);

            ViewBag.Pager = pager;
            ViewBag.LoggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return View(usersWithRoles.Cast<dynamic>().ToList()); // Chuyển về dynamic
        }





        [HttpGet]
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(new AppUserModel());
		}
        [HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserModel user)
        {
			if (ModelState.IsValid)
	 		{
				var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash); //tạo user 
				if(createUserResult.Succeeded)
				{
                    var createUser = await _userManager.FindByEmailAsync(user.Email); //tìm user dựa vào email
                    //var userId = createUser.Id; //lấy user Id
                    var role = _roleManager.FindByIdAsync(user.RoleId);//lấy RoleId

                    //gán quyền
                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        AddIdentityErrors(createUserResult);
                    }

					return RedirectToAction("Index", "User");
				}
				else
				{
                    AddIdentityErrors(createUserResult);
                    return View(user);
				}
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
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //kiểm tra id có tồn tại không
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        { 
            var existingUser = await _userManager.FindByIdAsync(id); //lấy user dựa vào id
            if (existingUser == null)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                //thực hiện update
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);//thực hiện update
                if (updateUserResult.Succeeded)
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }
                
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            TempData["error"] = "Model có một vài thứ đang bị lỗi";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);

            return View(existingUser);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			var deleteResult = await _userManager.DeleteAsync(user);
			if(!deleteResult.Succeeded)
			{
				return View("Error");
			}
            TempData["success"] = "User đã được xóa thành công";
            return RedirectToAction("Index");
        }

        private void AddIdentityErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}
