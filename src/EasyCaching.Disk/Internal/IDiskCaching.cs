//namespace EasyCaching.Disk
//{
//    using System;
//    using System.Collections.Generic;

//    public interface IDiskCaching
//    {
//        bool Set(string key, byte[] value, TimeSpan expiresIn);

//        int SetAll<T>(IDictionary<string, T> values, TimeSpan expiresIn);

//        bool Exists(string key);

//        byte[] Get(string key);

//        bool Remove(string key);
//    }
//}
