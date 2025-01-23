using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parse.Infrastructure;
using Parse;
namespace Howzit.Pages
{
    
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        private readonly ParseClient _client;

        public LoginModel(ParseClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required.";
                return Page();
            }

            try
            {
                // Authenticate the user
                ParseUser user = await _client.LogInAsync(Username, Password);
                if (user != null)
                {
                    // Redirect to the main feed if login is successful
                    return RedirectToPage("/Main");
                   // return RedirectToPage("/FeedNoGeoModel");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Invalid username or password.";
            }

            return Page();
        }
    }

}
