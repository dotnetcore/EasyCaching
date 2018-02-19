namespace EasyCaching.Demo.Memcached
{
    using EasyCaching.Memcached;
    using EasyCaching.Serialization.MessagePack;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDefaultMessagePackSerializer();
            services.AddDefaultMemcached(op=>
            {                
                op.AddServer("127.0.0.1",11211);
                //specify the Transcoder use messagepack .
                op.Transcoder = new FormatterTranscoder(new DefaultMessagePackSerializer()) ;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDefaultMemcached();

            app.UseMvc();
        }
    }
}
