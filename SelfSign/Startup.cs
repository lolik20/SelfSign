
using MediatR;
using Microsoft.EntityFrameworkCore;
using SelfSign.BL.Queries;
using SelfSign.DAL;
using System.Reflection;

namespace SelfSign
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(IsRequestedQuery).GetTypeInfo().Assembly);


            services.AddDbContext<ApplicationContext>(x =>
x.UseNpgsql(Configuration.GetConnectionString("Database")));

            services.AddCors(p => p.AddPolicy("corsapp", builder =>
             {
                 builder.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader();
             }));
            services.AddMvc();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment()||env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors("corsapp");
           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }

}
