
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SelfSign.BL.Commands;
using SelfSign.BL.Interfaces;
using SelfSign.BL.Queries;
using SelfSign.BL.Services;
using SelfSign.DAL;
using System.Reflection;
using System.Text;

namespace SelfSign
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public  static IConfigurationSection JwtSection { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSection = configuration.GetSection("Jwt");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidIssuer = JwtSection["Issuer"],
                    ValidateAudience = false,
                    ValidAudience = JwtSection["Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtSection["Key"])),
                    ValidateIssuerSigningKey = true,
                };

            });
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
            services.AddTransient<IHistoryService, HistoryService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddHttpClient();




            services.AddHttpClient("SignMe").SetHandlerLifetime(TimeSpan.FromDays(1));
            services.AddHttpClient("ItMonitoring", httpClient =>
            {
            });
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors(x => x.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

}
