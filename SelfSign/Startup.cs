
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SelfSign.BL.Commands;
using SelfSign.BL.Interfaces;
using SelfSign.BL.Queries;
using SelfSign.BL.Services;
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
            services.AddMediatR(typeof(AddressQuery).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(CreateDeliveryCommand).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(UpdateDeliveryCommand).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(SnilsUploadCommand).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(PassportUploadCommand).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(CreateSignMeCommand).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(CreateItMonitoringCommand).GetTypeInfo().Assembly);
            services.AddSingleton<IItMonitoringService, ItMonitoringService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddHttpClient();




            services.AddHttpClient("SignMe").SetHandlerLifetime(TimeSpan.FromDays(1));
            services.AddHttpClient("Dadata", httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add("X-Secret", Configuration.GetSection("Dadata")["Secret"]);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {Configuration.GetSection("Dadata")["Api"]}");
            }).SetHandlerLifetime(TimeSpan.FromDays(1));

            services.AddDbContext<ApplicationContext>(x =>
x.UseNpgsql(Configuration.GetConnectionString("Database")));

            services.AddCors();
            services.AddMvc();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(x=>x.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

}
