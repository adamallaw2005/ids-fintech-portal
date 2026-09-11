
namespace MyWebsite_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var jwtKey = builder.Configuration["JwtSettings:Key"]
                ?? throw new InvalidOperationException(
                    "JwtSettings:Key is missing. Set it with .NET User Secrets before starting the API.");

            builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "IDSProductsPortal",
                        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "IDSProductsPortal.Client",
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var id = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                            var role = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                            if (!int.TryParse(id, out var userId))
                            {
                                context.Fail("Invalid user.");
                                return;
                            }

                            var repository = context.HttpContext.RequestServices
                                .GetRequiredService<MyWebsite_API.Repositories.IAuthRepository>();
                            var user = await repository.GetByIdAsync(userId);
                            if (user is null || !user.IsActive || user.RoleName != role)
                            {
                                context.Fail("The account or role has changed. Sign in again.");
                            }
                        }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddSingleton<MyWebsite_API.Data.IDbConnectionFactory, MyWebsite_API.Data.SqlConnectionFactory>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IProductRepository, MyWebsite_API.Repositories.ProductRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IProductService, MyWebsite_API.Services.ProductService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IClientRepository, MyWebsite_API.Repositories.ClientRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IClientService, MyWebsite_API.Services.ClientService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IDeploymentRepository, MyWebsite_API.Repositories.DeploymentRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IDeploymentService, MyWebsite_API.Services.DeploymentService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IEnvironmentRepository, MyWebsite_API.Repositories.EnvironmentRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IEnvironmentService, MyWebsite_API.Services.EnvironmentService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IDeploymentModuleRepository, MyWebsite_API.Repositories.DeploymentModuleRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IDeploymentModuleService, MyWebsite_API.Services.DeploymentModuleService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IProductModuleRepository, MyWebsite_API.Repositories.ProductModuleRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IProductModuleService, MyWebsite_API.Services.ProductModuleService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.ITeamMemberRepository, MyWebsite_API.Repositories.TeamMemberRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.ITeamMemberService, MyWebsite_API.Services.TeamMemberService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IProductResponsibilityRepository, MyWebsite_API.Repositories.ProductResponsibilityRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IProductResponsibilityService, MyWebsite_API.Services.ProductResponsibilityService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IClientResponsibilityRepository, MyWebsite_API.Repositories.ClientResponsibilityRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IClientResponsibilityService, MyWebsite_API.Services.ClientResponsibilityService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IRepositoryLinkRepository, MyWebsite_API.Repositories.RepositoryLinkRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IRepositoryLinkService, MyWebsite_API.Services.RepositoryLinkService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IDocumentRepository, MyWebsite_API.Repositories.DocumentRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IDocumentService, MyWebsite_API.Services.DocumentService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IOverviewRepository, MyWebsite_API.Repositories.OverviewRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IOverviewService, MyWebsite_API.Services.OverviewService>();
            builder.Services.AddSingleton<MyWebsite_API.Services.IPasswordHashService, MyWebsite_API.Services.PasswordHashService>();
            builder.Services.AddSingleton<MyWebsite_API.Services.IJwtTokenService, MyWebsite_API.Services.JwtTokenService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IAuthRepository, MyWebsite_API.Repositories.AuthRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IAuthService, MyWebsite_API.Services.AuthService>();
            builder.Services.AddScoped<MyWebsite_API.Repositories.IUserManagementRepository, MyWebsite_API.Repositories.UserManagementRepository>();
            builder.Services.AddScoped<MyWebsite_API.Services.IUserManagementService, MyWebsite_API.Services.UserManagementService>();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("Frontend");
            app.Use(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (Microsoft.Data.SqlClient.SqlException exception)
                    when (exception.Number is 2601 or 2627 or 547)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = exception.Number == 547
                            ? "This change conflicts with related records or an allowed value. Check the data and try again."
                            : "A record with these details already exists."
                    });
                }
            });
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
