using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using razorweb.models;
using razorweb.Models;

namespace App.Admin.User
{
    public class EditUserClaimModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly MyBlogContext _context;
        public EditUserClaimModel(MyBlogContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public NotFoundObjectResult OnGet() => NotFound("Không được truy cập");

        public class InputModel
        {
            [Display(Name = "Tên Claim")]
            [Required(ErrorMessage = "Phải nhập {0}")]
            [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự")]
            public string ClaimType { get; set; }

            [Display(Name = "Giá trị")]
            [Required(ErrorMessage = "Phải nhập {0}")]
            [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự")]
            public string ClaimValue { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public AppUser user { get; set; }

        public IdentityUserClaim<string> userClaim { get; set; }

        public async Task<IActionResult> OnGetAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("User không tồn tại");
            return Page();
        }

        public async Task<IActionResult> OnPostAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("User không tồn tại");

            if (!ModelState.IsValid) return Page();

            var claims = _context.UserClaims.Where(c => c.UserId == user.Id);

            if (claims.Any(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Đặt tính này đã có");
                return Page();
            }

            await _userManager.AddClaimAsync(user, new Claim(Input.ClaimType, Input.ClaimValue));
            StatusMessage = "Đã thêm đặt tính cho user";

            return RedirectToPage("./AddRole", new { Id = user.Id });
        }

        public async Task<IActionResult> OnGetEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("User không tồn tại");

            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);

            if (user == null) return NotFound("User không tồn tại");

            Input = new InputModel()
            {
                ClaimType = userClaim.ClaimType,
                ClaimValue = userClaim.ClaimValue,
            };
            return Page();
        }

        public async Task<IActionResult> OnPostEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("User không tồn tại");

            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);

            if (user == null) return NotFound("User không tồn tại");

            if (!ModelState.IsValid) return Page();

            if (_context.UserClaims.Any(c =>
            c.UserId == user.Id
            && c.ClaimType == Input.ClaimType
            && c.ClaimValue == Input.ClaimValue
            && c.Id != userClaim.Id))
            {
                ModelState.AddModelError(string.Empty, "Claim này đã tồn tại");
                return Page(); 
            }

            userClaim.ClaimValue = Input.ClaimValue;
            userClaim.ClaimType = Input.ClaimType;

            await _context.SaveChangesAsync();
            StatusMessage = "Cập nhật claim thành công";

            return RedirectToPage("./AddRole", new { Id = user.Id });
        }
    
        public async Task<IActionResult> OnPostDeleteAsync(int? claimid)
        {
            if (claimid == null) return NotFound("User không tồn tại");

            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);

            if (user == null) return NotFound("User không tồn tại");

            await _userManager.RemoveClaimAsync(user, new Claim(userClaim.ClaimType, userClaim.ClaimValue));

            StatusMessage = "Xóa claim thành công";

            return RedirectToPage("./AddRole", new { Id = user.Id });
        }
    }
}
