namespace EasyCaching.Disk
{
    using System;

    public interface IDiskCaching
    {
        bool Set(string key, byte[] value, TimeSpan expiresIn);

        bool Exists(string key);

        byte[] Get(string key);
    }
}
