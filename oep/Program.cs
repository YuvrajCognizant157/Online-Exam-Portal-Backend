using Domain.Data;
using Domain.Models;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using System.Security.Claims;

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
                builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings"));

                builder.Services.AddTransient<EmailService>();

                builder.Services.AddScoped<TokenService>();
                builder.Services.AddScoped<IExamRepository, ExamRepository>();
                builder.Services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
                builder.Services.AddScoped<IUserRepository, UserRepository>();
                builder.Services.AddScoped<IAdminRepository, AdminRepository>();
                builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
                builder.Services.AddScoped<IExamFeedbackRepository, ExamFeedbackRepository>();

                builder.Services.AddScoped<ITopicRepository, TopicRepository>();
                builder.Services.AddScoped<IResultRespository, ResultRepository>();
                builder.Services.AddScoped<IQuestionFeedbackRepository, QuestionFeedbackRepository>();
                builder.Services.AddScoped<IAuthRepository, AuthRepository>();
                builder.Services.AddScoped<IJwtDecoderService, TokenService>();
                builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
                builder.Services.AddSingleton<PasswordHashingService>();
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        sqlServerOptions => sqlServerOptions.CommandTimeout(120)
                    )
                );
                builder.Services.AddControllers()
                    .AddJsonOptions(x =>
                                    x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

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

                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                builder.Host.UseSerilog();


                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAngularApp", policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });


                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.ConfigObject.TryItOutEnabled = false;
                    });
                }

                app.UseRouting();
                app.UseCors("AllowAngularApp");
                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();


                app.MapControllers();

                Log.Information("Starting up....");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed ::: Internal Server Error.");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

    }


}
