using System.Text;
using System.Threading.RateLimiting;
using api.Database;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Middlewares;
using api.Repositories;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

ConfigHelper.Initialize(builder.Configuration);

// Thiết lập DbContext với MySQL
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseMySql(ConfigHelper.ConnectionString,
    new MySqlServerVersion(new Version(9, 2, 0))));

// Thiết lập CORS cho phép frontend Vue.js truy cập API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins(ConfigHelper.AdminUrl, ConfigHelper.UserUrl)
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .AllowAnyHeader()
                        .AllowCredentials());
});

// Thiết lập response cho các lỗi 400 (Bad Request) với thông báo
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return new BadRequestObjectResult(new { Success = false, Message = "Invalid data", Error = errors });
    };
});

// Thiết lập JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "smart";
    options.DefaultChallengeScheme = "smart";
    options.DefaultScheme = "smart";
    options.DefaultForbidScheme = "smart";
    options.DefaultSignInScheme = "smart";
    options.DefaultSignOutScheme = "smart";
})
.AddPolicyScheme("smart", "Smart Auth Selector", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        string origin = HttpContextHelper.UserOrigin;

        if (string.IsNullOrEmpty(origin) || origin.Contains("unknown"))
        {
            return "both";
        }
        else if (!string.IsNullOrEmpty(origin) && origin.Contains(ConfigHelper.AdminUrl))
        {
            return "admin";
        }

        return "user";
    };
})
.AddJwtBearer("admin",
    options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = CookieHelper.AdminAccessToken;
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.JwtSecretKey)),
            ValidIssuer = ConfigHelper.JwtIssuer,
            ValidAudience = ConfigHelper.JwtAudience,
        };
    }
).AddJwtBearer("user",
    options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = CookieHelper.UserAccessToken;
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.JwtSecretKey)),
            ValidIssuer = ConfigHelper.JwtIssuer,
            ValidAudience = ConfigHelper.JwtAudience,
        };
    }
)
.AddJwtBearer("both",
    options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var adminToken = CookieHelper.AdminAccessToken;
                if (!string.IsNullOrEmpty(adminToken))
                {
                    context.Token = adminToken;
                    return Task.CompletedTask;
                }

                var userToken = CookieHelper.UserAccessToken;
                if (!string.IsNullOrEmpty(userToken))
                {
                    context.Token = userToken;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception != null && !string.IsNullOrEmpty(CookieHelper.AdminAccessToken) && !string.IsNullOrEmpty(CookieHelper.UserAccessToken))
                {
                }
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.JwtSecretKey)),
            ValidIssuer = ConfigHelper.JwtIssuer,
            ValidAudience = ConfigHelper.JwtAudience,
        };
    }
);

builder.Services.AddRateLimiter(options =>
{
    // Global limiter theo IP để bảo vệ toàn bộ hệ thống
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Lấy địa chỉ IP từ HttpContext
        // var remoteIpAddress = HttpContextHelper.UserIP;
        var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = httpContext.Request.Path.ToString().ToLower();

        // Áp dụng policy khác nhau dựa trên đường dẫn
        if (path.EndsWith("/verify"))
        {
            return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress, _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 35,
                SegmentsPerWindow = 5,
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
        }
        else if (path.Contains("/auth/"))
        {
            return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress, _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 10,
                SegmentsPerWindow = 6,
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
        }
        else if (path.Contains("/admin/"))
        {
            return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress, _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 20,
                SegmentsPerWindow = 5,
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
        }
        else
        {
            return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress, _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 30,
                SegmentsPerWindow = 5,
                QueueLimit = 1,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
        }
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Message = "Too many requests. Please try again later"
        }, cancellationToken: token);
    };
});


builder.Services.AddMemoryCache(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
    options.SizeLimit = 15000;
});

// Đăng ký các repository và service
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProductLineRepository, ProductLineRepository>();
builder.Services.AddScoped<IProductLineService, ProductLineService>();
// builder.Services.AddScoped<ITagRepository, TagRepository>();
// builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddHttpClient(); // For OpenAI API calls

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRateLimiter();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
CookieHelper.Configure(httpContextAccessor);
HttpContextHelper.Configure(httpContextAccessor);
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.MapControllers();
app.Run();
