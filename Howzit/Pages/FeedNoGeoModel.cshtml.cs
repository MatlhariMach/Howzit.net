using Howzit.ParseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parse;

namespace Howzit.Pages
{
    public class FeedNoGeoModel : PageModel
    {
        private readonly ParseClient _client;

        public List<HowzitPost> Posts { get; set; } = new List<HowzitPost>();
        public string SortBy { get; set; }

        public ParseUser CurrentUser { get; private set; }

        public FeedNoGeoModel(ParseClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task OnGetAsync(string sortBy = "new")
        {
            

            SortBy = sortBy;

            var query = _client.GetQuery<HowzitPost>();
            CurrentUser = _client.GetCurrentUser();

            query.Include("user");

            if (sortBy == "hot")
            {
                query.OrderByDescending("vote"); // Sort by votes
            }
            else
            {
                query.OrderByDescending("createdAt"); // Sort by creation date
            }  
            Posts = (List<HowzitPost>)await HowzitPost.GetPostsAsync(_client);
            // Posts = (List<HowzitPost>)await query.FindAsync();
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
