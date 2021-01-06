## EasyCaching's Features

- More Caching APIs
    1. Get/GetAsync(with data retriever)
    2. Get/GetAsync(without data retriever)
    3. Set/SetAsync
    4. Remove/RemoveAsync
    5. ~~Refresh/RefreshAsync (was removed)~~
    6. RemoveByPrefix/RemoveByPrefixAsync
    7. SetAll/SetAllAsync
    8. GetAll/GetAllAsync
    9. GetByPrefix/GetByPrefixAsync
    10. RemoveAll/RemoveAllAsync
    11. GetCount
    12. Flush/FlushAsync
    13. TrySet/TrySetAsync
    14. GetExpiration/GetExpirationAsync
- Caching Providers(Both local caching and distributed caching)
    1. In-Memory
    2. Memcached
    3. Redis(Based on StackExchange.Redis)
    4. Redis(Based on csredis)
    5. SQLite
    6. Hybrid
    7. Disk
    8. LiteDb
- Caching Interceptors
    1. AspectCore
    2. Castle
- Caching Serializers
    1. BinaryFormatter
    2. MessagePack
    3. Json
    4. Protobuf
    5. System.Text.Json
- ResponseCaching for ASP.NET Core
