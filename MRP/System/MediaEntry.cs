namespace MRP.System
{
    public class MediaEntry
    {
        public Guid Id { get; set; }

        public User Creator { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public MediaType Type { get; set; }

        public int ReleaseYear { get; set; }

        public List<string> Genres { get; set; } = new();

        public int AgeRestriction { get; set; }

        public List<Rating> Ratings { get; set; } = new();

        public List<User> FavoritedBy { get; set; } = new();

        public enum MediaType
        {
            Unknown = 0,
            Movie,
            Series,
            Game
        }

        public double AverageScore
        {
            get
            {
                if (Ratings.Count == 0)
                    return 0;

                return Ratings.Average(r => r.Stars);
            }
        }


    }
}