using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Howzit.ParseModels;
using Parse;
using System.Text;

namespace Howzit.Pages
{
    public class PostModel : PageModel
    {
        private readonly ParseClient _client;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CharacterCount { get; private set; } = "0/140";
        public ParseUser CurrentUser { get; set; }

        public PostModel(ParseClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IActionResult> OnGetAsync(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;

            // Fetch the current user asynchronously by wrapping the synchronous method
            CurrentUser = await Task.Run(() => _client.GetCurrentUser());

            if (CurrentUser == null)
            {
                // Redirect to login page if no user is logged in
                return RedirectToPage("/Login");
            }

            return Page(); // Return the page if the user is logged in
        }


        public async Task<IActionResult> OnPostAsync(string text, IFormFile image, double latitude, double longitude)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length > 140)
            {
                ModelState.AddModelError("Text", "Post text must be between 1 and 140 characters.");
                return Page();
            }
            // Get the current user
            var currentUser = _client.GetCurrentUser();
            var geoPoint = new ParseGeoPoint(latitude, longitude);
            var post = new HowzitPost
            {
                StatusMessage = text.Trim(),
                Location = geoPoint,
                Timestamp = DateTime.UtcNow.Ticks,
                User = currentUser,
                VoteCount = 0,
                Comments = 0
            };

            if (image != null && image.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var parseFile = new ParseFile(image.FileName, memoryStream.ToArray());
                post.PhotoFile = parseFile;
            }

            var acl = new ParseACL
            {
                PublicReadAccess = true,
                PublicWriteAccess = true
            };
            post.ACL = acl;

            await post.SaveAsync();
            return RedirectToPage("/Main");
        }
    }
}
