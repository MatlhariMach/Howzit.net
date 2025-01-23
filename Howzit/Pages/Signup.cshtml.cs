using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parse.Infrastructure;
using Parse;

namespace Howzit.Pages
{
    public class SignupModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Email { get; set; }

        public string ErrorMessage { get; set; }

        private readonly ParseClient _client;

        public SignupModel(ParseClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            try
            {
                var user = new ParseUser
                {
                    Username = Username,
                    Password = Password,
                    Email = Email
                };

                await user.SignUpAsync();

                // Redirect to the login page after successful signup
                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Sign-up failed: " + ex.Message;
                return Page();
            }
        }
    }
}
