using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using System.Text;
using Howzit.ParseModels;
using Newtonsoft.Json.Linq;
using Parse;

namespace Howzit.Pages
{
    public class MainModel : PageModel
    {
        private readonly ParseClient _client;
        public List<HowzitPost> Posts { get; set; } = new List<HowzitPost>();
        public string SortBy { get; set; }
        public ParseUser CurrentUser { get; private set; }
        public MainModel(ParseClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        //Quey base on geopoint

        public async Task<IActionResult> OnGetAsync(string sortBy = "new", double? latitude = null, double? longitude = null, double radiusInKm = 5)
        {
            SortBy = sortBy;
            var query = _client.GetQuery<HowzitPost>();
            CurrentUser = _client.GetCurrentUser();
            query.Include("user");

            if (sortBy == "hot")
            {
                query.OrderByDescending("vote");

            }
            else
            {
                query.OrderByDescending("createdAt");
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                var userLocation = new ParseGeoPoint(latitude.Value, longitude.Value);
                query = query.WhereNear("location", userLocation);
            }

            var results = await query.FindAsync();
            Posts = results.ToList();

            // Return JSON for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return new JsonResult(Posts);
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                // Redirect to the target page with latitude and longitude
                return RedirectToPage("/Post", new { latitude = latitude.Value, longitude = longitude.Value });
            }
            // Return the full page for non-AJAX requests
          
            return Page();
        }

        public async Task<IActionResult> OnPostVote(string postId, bool? isUpvote, bool? isDwnvote)
        {
            if (string.IsNullOrEmpty(postId))
                return BadRequest("Invalid post ID.");

            var post = await _client.GetQuery<HowzitPost>()
                                    .WhereEqualTo("objectId", postId)
                                    .FirstOrDefaultAsync();

            if (post == null)
                return NotFound();

            if (isUpvote == true)
            {
                await post.UpvoteAsync(_client);
            }
            else if (isDwnvote == true)
            {
                await post.DownvoteAsync(_client);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string postId)
        {
            if (string.IsNullOrEmpty(postId))
                return BadRequest("Invalid post ID.");

            var post = await _client.GetQuery<HowzitPost>()
                                    .WhereEqualTo("objectId", postId)
                                    .FirstOrDefaultAsync();

            if (post == null)
                return NotFound();

            var currentUser = _client.GetCurrentUser();
            if (currentUser == null || post.User.ObjectId != currentUser.ObjectId)
                return Forbid();

            await post.DeleteAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEdit(string postId, string statusMessage)
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(statusMessage))
                return BadRequest("Invalid post data.");

            var post = await _client.GetQuery<HowzitPost>()
                                    .WhereEqualTo("objectId", postId)
                                    .FirstOrDefaultAsync();

            if (post == null)
                return NotFound();

            // Check if the current user is the owner of the post
            var currentUser = _client.GetCurrentUser();
            if (currentUser == null || post.User.ObjectId != currentUser.ObjectId)
                return Unauthorized();

            // Update the post content and timestamp
            post.StatusMessage = statusMessage;
            post["updatedAt"] = DateTime.UtcNow;

            await post.SaveAsync();

            return RedirectToPage();
        }
    }
}

