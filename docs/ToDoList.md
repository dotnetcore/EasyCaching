# Todo List

## Caching Providers

- [x] Memory
- [x] Redis(Based on [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis))
- [x] Redis(Based on [csredis](https://github.com/2881099/csredis))
- [x] SQLite
- [x] Memcached
- [x] Hybrid(Combine local caching and distributed caching)
- [x] Disk
- [x] LiteDB
- [ ] Others...

## Basic Caching API

- [x] Get/GetAsync(with data retriever)
- [x] Get/GetAsync(without data retriever)
- [x] Set/SetAsync
- [x] Remove/RemoveAsync
- [x] Refresh/RefreshAsync
- [x] RemoveByPrefix/RemoveByPrefixAsync
- [x] SetAll/SetAllAsync
- [x] GetAll/GetAllAsync
- [x] GetByPrefix/GetByPrefixAsync
- [x] RemoveAll/RemoveAllAsync
- [x] GetCount
- [x] Flush/FlushAsync
- [x] TrySet/TrySetAsync
- [x] GetExpiration/GetExpirationAsync
- [ ] Others...

## Serializer Extensions 

- [x] BinaryFormatter
- [x] MessagePack
- [x] Json
- [x] ProtoBuf
- [x] System.Text.Json
- [ ] Others...

## Caching Interceptor

- [x] AspectCore
- [x] Castle
- [ ] Others ..
    
1. EasyCachingAble
2. EasyCachingPut
3. EasyCachingEvict

> Note: Not support Hybird Caching provider yet.

## Caching Bus

- [x] Redis (Based on [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis))
- [x] Redis (Based on [csredis](https://github.com/2881099/csredis))
- [x] RabbitMQ 

## Others

- [x] Configuration
- [x] Caching Region (one region with an instance of provider)
- [x] Caching Statistics
- [ ] UI Manager
- [x] Logger
- [ ] Caching Warm Up 
- [ ] ...