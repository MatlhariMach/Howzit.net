using Parse;

namespace Howzit.ParseModels
{
    public class Post
    {
        public string Username { get; set; }
        public string StatusMessage { get; set; }
        public int VoteCount { get; set; }
        public string Timestamp { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string Comments { get; set; }

        public ParseFile PhotoFile { get; set; }

        public List<string> UPlist { get; set; }

        public List<string> DWNlist { get; set; }


       



    }


}
