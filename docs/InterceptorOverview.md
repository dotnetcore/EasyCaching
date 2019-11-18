# Introduction 

Let's take a look at the simple code below.

```cs
public Product GetProduct(int id)  
{  
    string cacheKey = $"product:{id}";  
      
    var val = _cache.Get<Product>(cacheKey);  
      
    if(val != null)  
        return val;  
      
    val = _db.GetProduct(id);  
      
    if(val != null)  
        _cache.Set<Product>(cacheKey, val, expiration);  
          
    return val;  
} 
```

In fact, the way this method does is very simple:

First, the product information of the corresponding id is taken out from the cache. If it can be retrieved from the cache, the cached data is directly returned. Otherwise, the corresponding product information is searched in the database.

If you can successfully find this product information in the database, you should throw it into the cache. The logic is this.

I believe that most people have written similar code, and the ideas are clearly expressed.

However, writing one or two of these methods may be acceptable. If you have more, you will find it very troublesome, and then you will have the CV Dafa. The result is the ifelse that can be seen everywhere, which is why EasyCaching wants to provide such a function. the reason.

As an example of a log, I believe that everyone will be more clear, the method of input and the input of the parameters are printed out, we certainly can not add one method or one method.

The concept of AOP programming is introduced here!

# EasyCaching provides three strategies for cache interception

EasyCaching disassembles the four common operations of CURD into three strategies.

- Able
- Put
- Evict

Among them, **Able**'s behavior is the same as the above example, which is to check and write, which is C and R in CURD.

The **Able** strategy applies to most query operations. Use with high real-time requirements should be used with caution or prohibited!

Let's take a look at the two strategies of **Put** and **Evict**.

The **Put** policy corresponds to the U (update) operation in CURD.

If a data has been modified, it is also necessary to update its corresponding cached data. If it is updated frequently, it is not recommended!

The **Evict** strategy corresponds to the D (delete) operation in CURD.

If a data is deleted, it is also necessary to clean up its corresponding cached data.

This is the meaning and role of the three strategies.

# How To Implement In EasyCaching

There are three submodules.

1. Cache Key generation rules
2. Intercepting rules
3. Intercept configuration and operation


## Cache Key generation rules

The Key here is automatically generated based on the information (method, parameters) and custom prefix of the method to be intercepted.

For some basic data types, it will be converted into a string and then spliced.

For complex types, like custom classes, EasyCaching provides an ICachable interface that lets the user define the cache key that generates the class.

```cs
public interface ICachable
{
    string CacheKey { get; }
}
```

## Intercepting rules

Attribute is an important part of interception. For the three strategies, three Attributes are provided.

- EasyCachingAble
- EasyCachingPut
- EasyCachingEvict

To intercept this method, just add the above three Attributes to its interface or inherit some of their Attributes.

Here is an example:

```cs
 public interface IDemoService 
 {
    [EasyCachingAble(Expiration = 10)]
    string GetCurrentUtcTime();
 }
```

Some parameter descriptions that can be used

Property | Description | Apply
---|---|---
CacheKeyPrefix | To specify the prefix of your cache key | All
CacheProviderName | To specify which provider you want to use | All
IsHighAvailability | Whether caching opreation will break your method | All
Expiration | To specify the expiration of your cache item，the unit is second | EasyCachingAble and EasyCachingPut
IsAll | Whether remove all the cached items start with the CacheKeyPrefix | EasyCachingEvict only
IsBefore | Remove the cached item before method excute or after method excute | EasyCachingEvict only

> One thing to note here is that if you want to use the Put and Evict policies, you should specify CacheKeyPrefix as much as possible, otherwise the cache will not be updated or deleted correctly. This is dependent on the implementation of the generated cache Key, which specifies that CacheKeyPrefix will ignore method information.


## Intercept configuration and operation

At present, EasyCaching has only one CacheProviderName overall configuration, which is mainly used to specify which Provider to use, which will be overwritten by the attribute of Attribute.

EasyCaching currently offers two implementations, one based on [AspectCore](https://github.com/dotnetcore/AspectCore-Framework) and the other based on [Castle](https://github.com/castleproject/Core)+[Autofac.Extras.DynamicProxy](https://github.com/autofac/Autofac.Extras.DynamicProxy)