using Howzit.ParseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parse;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Howzit.Pages
{
    public class CommentsModel : PageModel
    {
        private readonly ParseClient _parseClient;
        public HowzitPost Post { get; set; }
        public List<CommentPost> Comments { get; set; }
        public ParseUser CurrentUser { get;  set; }
        public CommentsModel(ParseClient parseClient)
        {
            _parseClient = parseClient ?? throw new ArgumentNullException(nameof(parseClient));
        }

        public async Task OnGetAsync(string postId)
        {
            CurrentUser = _parseClient.GetCurrentUser();

            if (string.IsNullOrEmpty(postId))
            {
                ViewData["PostId"] = null; // Ensure postId is set in ViewData
                Post = null;
                Comments = null;
                return;
            }

            ViewData["PostId"] = postId; // Store postId for the form

            //Fetch the main post
            Post = await _parseClient.GetQuery<HowzitPost>()
                                     .WhereEqualTo("objectId", postId)
                                     .FirstOrDefaultAsync(); 

            if (Post == null)
            {
                Comments = null;
                return;
            }
            // Fetch comments for the post
            Comments = await CommentPost.GetCommentsForPostAsync(_parseClient, postId);
        }

        public async Task<IActionResult> OnPostCommentVote(string commentId, bool? isUpvote, bool? isDwnvote)
        {
            if (string.IsNullOrEmpty(commentId))
                return BadRequest("Invalid comment ID.");

            var comment = await _parseClient.GetQuery<CommentPost>()
                                            .WhereEqualTo("objectId", commentId)
                                            .FirstOrDefaultAsync();

            if (comment == null)
                return NotFound("Comment not found.");

            if (isUpvote == true)
                await comment.UpvoteAsync(_parseClient);
            else if (isDwnvote == true)
                await comment.DownvoteAsync(_parseClient);

            //  return RedirectToPage(new { postId = ViewData["PostId"] });

            return RedirectToPage();
        }


 

        public async Task<IActionResult> OnPostAddCommentAsync(string CommentText, string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                return BadRequest("Post ID is missing.");

            if (string.IsNullOrWhiteSpace(CommentText))
                return RedirectToPage();

            // Fetch the post by ID
            var post = await _parseClient.GetQuery<HowzitPost>()
                                         .WhereEqualTo("objectId", postId)
                                         .FirstOrDefaultAsync();

            if (post == null)
                return NotFound("Post not found.");

            // Get the current user
            var currentUser = _parseClient.GetCurrentUser();
            if (currentUser == null)
                return NotFound("User must be logged in to add a comment.");

            // Create the new comment
            var newComment = new CommentPost
            {
                StatusMessage = CommentText,
                Vote = 0, // Default vote count
                CreatedAt = DateTime.UtcNow,
                Pointer = post.ObjectId, // Associate the comment with the fetched post
                User = currentUser // Use the current user
            };

            // Save the new comment
            await newComment.SaveAsync();

            // Increment the Comments count on the original post
            post.Comments += 1;
            await post.SaveAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string commentId)
        {
            if (string.IsNullOrEmpty(commentId))
                return BadRequest("Invalid comment ID.");

            // Fetch the comment by its ID
            var comment = await _parseClient.GetQuery<CommentPost>()
                                            .WhereEqualTo("objectId", commentId)
                                            .FirstOrDefaultAsync();

            if (comment == null)
                return NotFound("Comment not found.");

            var currentUser = _parseClient.GetCurrentUser();
            if (currentUser == null || comment.User.ObjectId != currentUser.ObjectId)
                return Forbid();

            // Fetch the parent post associated with the comment
            var post = await _parseClient.GetQuery<HowzitPost>()
                                         .WhereEqualTo("objectId", comment.Pointer)
                                         .FirstOrDefaultAsync();

            if (post != null)
            {
                // Decrement the comment count
                post.Comments -= 1;
                await post.SaveAsync();
            }

            // Delete the comment
            await comment.DeleteAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditCommentAsync(string commentId, string statusMessage)
        {
            if (string.IsNullOrEmpty(commentId) || string.IsNullOrEmpty(statusMessage))
                return BadRequest("Invalid comment data.");

            var comment = await _parseClient.GetQuery<CommentPost>()
                                            .WhereEqualTo("objectId", commentId)
                                            .FirstOrDefaultAsync();

            if (comment == null)
                return NotFound();

            var currentUser = _parseClient.GetCurrentUser();
            if (currentUser == null || comment.User.ObjectId != currentUser.ObjectId)
                return Unauthorized();

            comment.StatusMessage = statusMessage;
            comment["updatedAt"] = DateTime.UtcNow;

            await comment.SaveAsync();

            return RedirectToPage();
            // return RedirectToPage(new { postId = ViewData["PostId"] });
        }


        public async Task<IActionResult> OnPostEdit(string postId, string statusMessage)
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(statusMessage))
                return BadRequest("Invalid post data.");

            var post = await _parseClient.GetQuery<HowzitPost>()
                                    .WhereEqualTo("objectId", postId)
                                    .FirstOrDefaultAsync();

            if (post == null)
                return NotFound();

            // Check if the current user is the owner of the post
            var currentUser = _parseClient.GetCurrentUser();
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
