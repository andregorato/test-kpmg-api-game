using kpmg_core;
using kpmg_core.Configuration;
using kpmg_core.Interfaces.Cache;
using kpmg_core.Interfaces.Db;
using kpmg_core.Interfaces.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace kpmg_api_game
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var _mongoConfig = new MongoConfig();
            var _redisConfig = new RedisConfig();
            new ConfigureFromConfigurationOptions<MongoConfig>(Configuration.GetSection("MongoConfig")).Configure(_mongoConfig);
            new ConfigureFromConfigurationOptions<RedisConfig>(Configuration.GetSection("RedisConfig")).Configure(_redisConfig);
            services.AddTransient(typeof(IDbClient<Game>), (s => new DbClient(_mongoConfig.Endpoint, _mongoConfig.DatabaseName, _mongoConfig.CollectionName)));
            services.AddTransient(typeof(IGameRepository<Game>), typeof(GameRepository));
            services.AddTransient(typeof(IRedis<Game>), (s => new Redis(_redisConfig.Endpoint, _redisConfig.Port, _redisConfig.Password)));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
