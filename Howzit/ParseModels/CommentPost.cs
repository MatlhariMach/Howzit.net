using Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzit.ParseModels
{
    [ParseClassName("Comments")]
    public class CommentPost : ParseObject
    {
        [ParseFieldName("text")]
        public string StatusMessage
        {
            get => GetProperty<string>() ?? "No message";
            set => SetProperty(value);
        }

        [ParseFieldName("user")]
        public ParseUser User
        {
            get => GetProperty<ParseUser>();
            set => SetProperty(value);
        }

        [ParseFieldName("timestamp")]
        public long Timestamp
        {
            get => GetProperty<long>();
            set => SetProperty(value);
        }

        [ParseFieldName("vote")]
        public int Vote
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        [ParseFieldName("list")]
        public IList<string> UPlist
        {
            get => GetProperty<List<string>>() ?? new List<string>();
            set => SetProperty(value);
        }

        [ParseFieldName("list2")]
        public IList<string> DWNlist
        {
            get => GetProperty<List<string>>() ?? new List<string>();
            set => SetProperty(value);
        }

        [ParseFieldName("Pointer")]
        public string Pointer
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        private DateTime? _customCreatedAt;

        public new DateTime? CreatedAt
        {
            get => base.CreatedAt ?? _customCreatedAt ?? DateTime.UtcNow;
            set => _customCreatedAt = value; 
        }



        public string GetTimeAgo()
        {
            if (CreatedAt == null)
                return "Unknown time";

            var timeSpan = DateTime.UtcNow - CreatedAt.Value;

            if (timeSpan.TotalSeconds < 60)
                return $"{(int)timeSpan.TotalSeconds} seconds ago";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";

            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }

        private bool isVoting = false; // Prevent simultaneous clicks

        public async Task UpvoteAsync(ParseClient client)
        {
            if (isVoting) return; // Prevent duplicate rapid clicks
            isVoting = true;

            try
            {
                var currentUser = client.GetCurrentUser();
                if (currentUser == null)
                    throw new InvalidOperationException("No user is currently logged in.");

                var userId = currentUser.ObjectId;

                // Ensure lists are initialized
                var upList = UPlist ?? new List<string>();
                var dwnList = DWNlist ?? new List<string>();

                // Check if the user is voting on their own post
                if (upList.Contains(userId))
                {
                    Console.WriteLine("You have already upvoted this post.");
                }
                else if (dwnList.Contains(userId))
                {
                    // Neutralize the downvote and add an upvote
                    dwnList.Remove(userId);
                    upList.Add(userId);
                    Increment("vote", +1); // Remove -1, then add +1
                }
                else
                {
                    // Normal upvote
                    upList.Add(userId);
                    Increment("vote", +1);
                }

                // Save changes
                UPlist = upList;
                DWNlist = dwnList;
                await SaveAsync();
            }
            finally
            {
                isVoting = false;
            }
        }

        public async Task DownvoteAsync(ParseClient client)
        {
            if (isVoting) return; // Prevent duplicate rapid clicks
            isVoting = true;

            try
            {
                var currentUser = client.GetCurrentUser();
                if (currentUser == null)
                    throw new InvalidOperationException("No user is currently logged in.");

                var userId = currentUser.ObjectId;

                // Ensure lists are initialized
                var upList = UPlist ?? new List<string>();
                var dwnList = DWNlist ?? new List<string>();

                // Check if the user is voting on their own post
                if (dwnList.Contains(userId))
                {
                    Console.WriteLine("You have already downvoted this post.");
                }
                else if (upList.Contains(userId))
                {
                    // Neutralize the upvote and add a downvote
                    upList.Remove(userId);
                    dwnList.Add(userId);
                    Increment("vote", -1); // Remove +1, then subtract -1
                }
                else
                {
                    // Normal downvote
                    dwnList.Add(userId);
                    Increment("vote", -1);
                }

                // Save changes
                UPlist = upList;
                DWNlist = dwnList;
                await SaveAsync();


            }
            finally
            {
                isVoting = false;
            }
        }
   
        public static async Task<List<CommentPost>> GetCommentsForPostAsync(ParseClient client, string postId)
        {
            if (string.IsNullOrEmpty(postId))
                throw new ArgumentException("Post ID cannot be null or empty.", nameof(postId));

            var query = client.GetQuery<CommentPost>()
                              .Include("user")
                              .WhereEqualTo("Pointer", postId)
                              .OrderByDescending("createdAt")
                              .Limit(100);

            var results = await query.FindAsync();
            return results.ToList();
        }
    }
}
