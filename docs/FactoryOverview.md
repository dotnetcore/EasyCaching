# Introduction 

The main purpose of the cache factory is to solve the problem of multiple instances. Multiple instances can be understood as the use of multiple different providers in a project, such as an InMemory with a CSRedis, three InMemory and two InMemory with two CSRedis.

In EasyCaching, all different types of providers can be create by the factory:

- IEasyCachingProvider
- IRedisCachingProvider
- IHybridCachingProvider