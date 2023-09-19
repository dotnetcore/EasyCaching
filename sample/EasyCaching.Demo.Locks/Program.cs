var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddEasyCaching(option =>
{
    //use memory cache
    option.UseInMemory()
    .UseMemoryLock(); // use memory lock

    //use redis cache
    option.UseRedis(config =>
    {
        config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
        config.DBConfig.SyncTimeout = 10000;
        config.DBConfig.AsyncTimeout = 10000;
        config.SerializerName = "NewtonsoftJson";
    })
    .WithJson()//with josn serialization
    .UseRedisLock(); // use distributed lock
});

#region How Inject Distributed and Memory lock

// inject to use distributed lock
builder.Services.AddSingleton<IDistributedLockFactory, RedisLockFactory>();

// inject to use memory lock
builder.Services.AddSingleton<IDistributedLockFactory, MemoryLockFactory>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
