using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Assignment2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Assignment2.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignment2.Areas.Identity.Pages.Account
{
    /// <author>Following class was originally scaffolded through the Identity API </author>
    /// <summary>
    /// Handles the backend of user registration 
    /// </summary>
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public RegisterInputModel RegisterInput { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// Class to represent and transport the input the user provides to register
        /// </summary>
        public class RegisterInputModel : Customer
        {
            [Required]
            [Display(Name = "Login ID")]
            [Range(1000, 9999)] // Is 4 digits
            public int LoginId { get; set; }
            
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var rnd = new Random();
                var customer = new Customer
                {
                    CustomerId = await GenerateCustomerId(),
                    Name = RegisterInput.Name,
                    Tfn = RegisterInput.Tfn,
                    Address = RegisterInput.Address,
                    City = RegisterInput.City,
                    State = RegisterInput.State,
                    PostCode = RegisterInput.PostCode,
                    Phone = RegisterInput.Phone
                };
                await _context.Customer.AddAsync(customer);

                var user = new ApplicationUser
                {
                    UserName = RegisterInput.LoginId.ToString(), 
                    Email = RegisterInput.Email, 
                    Customer = customer
                };
                var result = await _userManager.CreateAsync(user, RegisterInput.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(RegisterInput.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = RegisterInput.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        
        /// <summary>
        /// Generates a unique customer id
        /// </summary>
        /// <returns>The unique customer id</returns>
        private async Task<int> GenerateCustomerId()
        {
            
            var rnd = new Random();
            int customerId;
            
            // Keep generating a customer id until we get one that isn't used
            do
            {
                customerId = rnd.Next(1000, 9999);// Generate a 4 digit number as needed for a customer id
            } while (await _context.Customer.AnyAsync(c => c.CustomerId == customerId)); // Check that the id isn't already used

            return customerId;
        }
    }
}
