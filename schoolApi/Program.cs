using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
// --- «” œ⁄«¡ «·ÿ»ﬁ«  »‘ﬂ· ’ÕÌÕ ---
using school.Application.Interfaces;
using school.Application.Services;
using school.Infrastructure.Data;
using school.Infrastructure.Services;
using System.Text;

namespace schoolApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================
            // 1. ≈⁄œ«œ ﬁ«⁄œ… «·»Ì«‰«  (DbContext)
            // ==========================================
            var cont = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<SchoolDbContext>(options =>
            options.UseSqlServer(cont, b => b.MigrationsAssembly("School.Infrastructure")));

            // —»ÿ IApplicationDbContext «·„ÊÃÊœ ›Ì Application »‹ SchoolDbContext «·„ÊÃÊœ ›Ì Infrastructure
            builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SchoolDbContext>());

            // ==========================================
            // 2. —»ÿ «·Œœ„«  (Dependency Injection)
            // ==========================================
            // —»ÿ Œœ„… «· Êﬂ‰ »⁄œ  ⁄œÌ· „”«—« Â«
            builder.Services.AddScoped<IJwtService, JwtService>();
            // ≈÷«›… «·”ÿ— «·ÃœÌœ ·—»ÿ Œœ„… «·„’«œﬁ…
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<ITeacherService, TeacherService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            // „” ﬁ»·« ” ÷Ì› »ﬁÌ… «·Œœ„«  Â‰« „À·:
            // builder.Services.AddScoped<IAdminService, AdminService>();

            // ==========================================
            // 3. ≈⁄œ«œ«  «·‹ JWT (Authentication & Authorization)
            // ==========================================
            var key = builder.Configuration["Jwt:Key"];
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ==========================================
            // 4. ≈⁄œ«œ«  Swagger ·œ⁄„ «· Êﬂ‰
            // ==========================================
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "schoolApi",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "√œŒ· «· Êﬂ‰ »Â–« «·‘ﬂ·: Bearer {your token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // «· — Ì» Â‰« „Â„ Ãœ«: Authentication À„ Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}