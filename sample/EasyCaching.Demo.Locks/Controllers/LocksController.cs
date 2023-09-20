namespace EasyCaching.Demo.Locks.Controllers;

[Route("api/[controller]")]
public class LocksController : Controller
{
    private readonly IDistributedLockFactory _distributedLockFactory;
    
    public LocksController(IDistributedLockFactory distributedLockFactory)
    {
        _distributedLockFactory = distributedLockFactory;
    }

    [HttpPost("distributed-locking")]
    public async Task DistributedLockingOperation(int millisecondsTimeout)
    {
        using var distributedLock = _distributedLockFactory.CreateLock("DefaultRedis", "YourKey");

        try
        {
            if (await distributedLock.LockAsync(millisecondsTimeout))
            {
                // Simulate operation
                Thread.Sleep(2000);
            }
            else
            {
                // Proper error
            }
        }
        catch (Exception ex)
        {
            // log error
            throw;
        }
        finally
        {
            // release lock at the end
            await distributedLock.ReleaseAsync();
        }
    }

    [HttpPost("memory-locking")]
    public async Task MemoryLockingOperation(int millisecondsTimeout)
    {
        using var memoryLock = _distributedLockFactory.CreateLock("DefaultInMemory", "YourKey");

        try
        {
            if (await memoryLock.LockAsync(millisecondsTimeout))
            {
                // Simulate operation
                Thread.Sleep(2000);
            }
            else
            {
                // Proper error
            }
        }
        catch (Exception ex)
        {
            // log error
            throw;
        }
        finally
        {
            // release lock at the end
            await memoryLock.ReleaseAsync();
        }
    }
}
