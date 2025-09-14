
using Domain.Data;
using Domain.Models;
//using Infrastructure.Repositories.Implementations;
//using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace oep
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                builder.Services.AddScoped<TokenService>();
                //builder.Services.AddScoped<IExamRepository, ExamRepository>();
                //builder.Services.AddScoped<IUserRepository, UserRepository>();
                //builder.Services.AddScoped<IAdminRepository, AdminRepository>();
                //builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
                builder.Services.AddControllers();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskManagementAPI", Version = "v1" });

                    // Add JWT Authentication
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter 'Bearer' followed by your JWT token"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
                });

                var JwtKey = builder.Configuration.GetValue<string>("Jwt:Secret");

                builder.Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                   .AddJwtBearer(x =>
                   {
                       x.RequireHttpsMetadata = true;
                       x.SaveToken = true;
                       x.TokenValidationParameters = new TokenValidationParameters
                       {
                           ValidateIssuerSigningKey = true,
                           IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtKey)),

                           ValidateIssuer = true,
                           ValidateAudience = true,
                           ValidIssuer = builder.Configuration["Jwt:Issuer"],
                           ValidAudience = builder.Configuration["Jwt:Audience"],
                       };
                   });

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
                    options.AddPolicy("ExaminerOnly", policy => policy.RequireRole("Examiner"));
                    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                });


                var app = builder.Build();
                //app.UseMiddleware<GlobalExceptionMiddleware>();
                // Configure the HTTP request pipeline.

                if (!args.Contains("ef"))
                {

                    if (app.Environment.IsDevelopment())
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI(options =>
                        {
                            options.ConfigObject.TryItOutEnabled = false;
                        });
                    }
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

    }
}
