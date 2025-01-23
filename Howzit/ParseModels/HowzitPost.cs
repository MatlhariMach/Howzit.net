using Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzit.ParseModels
{
    [ParseClassName("Posts")]
    public class HowzitPost : ParseObject
    {
        // Properties mapped to Parse fields
        [ParseFieldName("text")]
        public string StatusMessage
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [ParseFieldName("user")]
        public ParseUser User
        {
            get => GetProperty<ParseUser>();
            set => SetProperty(value);
        }

        [ParseFieldName("vote")]
        public int VoteCount
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        [ParseFieldName("timestamp")]
        public long Timestamp
        {
            get => GetProperty<long>();
            set => SetProperty(value);
        }


        [ParseFieldName("location")]
        public ParseGeoPoint Location
        {
            get => GetProperty<ParseGeoPoint>();
            set => SetProperty(value);
        }

        [ParseFieldName("comments")]
        public int Comments
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        [ParseFieldName("photo")]
        public ParseFile PhotoFile
        {
            get => GetProperty<ParseFile>();
            set => SetProperty(value);
        }

        [ParseFieldName("list")]
        public List<string> UPlist
        {
            get => GetProperty<List<string>>() ?? new List<string>();
            set => SetProperty(value);
        }

        [ParseFieldName("list2")]
        public List<string> DWNlist
        {
            get => GetProperty<List<string>>() ?? new List<string>();
            set => SetProperty(value);
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








        public string GetTimeAgo()
        {
            if (CreatedAt == null)
                return "Unknown time";

            var timeSpan = DateTime.UtcNow - CreatedAt.Value; // Use `.Value` to access the DateTime value of a nullable type.

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


        // Computed properties
        public string Username => User?.Username; // Automatically derived
        public DateTime? CreatedAt => base.CreatedAt; // Provided by ParseObject

        // Static method for querying HowzitPost objects
        public static async Task<List<HowzitPost>> GetPostsAsync(ParseClient client)
        {
            var query = client.GetQuery<HowzitPost>()
                              .Include("user") // Ensures user data is included in the query
                              .OrderByDescending("createdAt")
                              .Limit(100);

            var results = await query.FindAsync();

            // The Parse SDK will already map these into HowzitPost objects
            return results.ToList();
        }

    }
}
