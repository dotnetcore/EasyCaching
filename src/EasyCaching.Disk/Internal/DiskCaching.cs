namespace EasyCaching.Disk
{
    using System;
    using System.IO;
    using EasyCaching.Core;
    using MessagePack;

    public class DiskCaching : IDiskCaching
    {
        private readonly DiskDbOptions _options;
        private readonly string _name;

        public DiskCaching(string name, DiskDbOptions optionsAccessor)
        {
            ArgumentCheck.NotNull(optionsAccessor, nameof(optionsAccessor));

            _name = name;
            _options = optionsAccessor;
        }

        public bool Exists(string key)
        {
            var path = GetFilePath(key);

            if (!File.Exists(path))
            {
                return false;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var val = MessagePackSerializer.Deserialize<DiskCacheValue>(stream);

                return val.Expiration > DateTimeOffset.UtcNow;
            }
        }

        public byte[] Get(string key)
        {
            var path = GetFilePath(key);

            if (!File.Exists(path)) return null;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var bytes = MessagePackSerializer.Deserialize<DiskCacheValue>(stream);
                return bytes.Value;
            }
        }

        public bool Set(string key, byte[] value, TimeSpan expiresIn)
        {
            var path = GetFilePath(key);

            var cached = new DiskCacheValue(value, (int)expiresIn.TotalSeconds);

            var bytes = MessagePackSerializer.Serialize(cached);

            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
                return true;
            }
        }

        private string GetFilePath(string key)
        {
            var path = Path.Combine(_options.BasePath, $"{key}.dat");
            return path;
        }
    }
}
