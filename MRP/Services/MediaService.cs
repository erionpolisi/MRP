using MRP.System;

namespace MRP.Services
{
    public class MediaService
    {

    }

    public static class MediaRepository
    {
        private static readonly Dictionary<Guid, MediaEntry> _media = new();

        public static void Add(MediaEntry entry) => _media[entry.Id] = entry;

        public static MediaEntry? Get(Guid id)
            => _media.TryGetValue(id, out var m) ? m : null;

        public static bool Exists(Guid id)
            => _media.ContainsKey(id);

        public static IEnumerable<MediaEntry> GetAll()
            => _media.Values;

        public static void Delete(Guid id)
            => _media.Remove(id);
    }
}
