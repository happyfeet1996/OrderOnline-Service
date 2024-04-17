using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderOnline;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // ���� Swagger UI �� JWT �����֤
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
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
        };

    c.AddSecurityRequirement(securityRequirement);
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenParameter.Secret)),
        ValidateIssuer = true,
        ValidIssuer = TokenParameter.Issuer,
        ValidateAudience = true,
        ValidAudience = TokenParameter.Audience,
        ValidateLifetime = true,
    };
});

builder.Services.AddMemoryCache();

/*builder.Services.AddDistributedMemoryCache();  //�������棬AddSession����
builder.Services.AddSession(option =>
{
    option.Cookie.SameSite = SameSiteMode.None;
    option.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    option.IdleTimeout = TimeSpan.FromMinutes(3);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});*/

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(5000);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");

        // ���� Swagger UI �� JWT �����֤�����
        c.DefaultModelsExpandDepth(-1);
        c.DocumentTitle = "Your API Swagger UI";
        c.InjectStylesheet("/swagger-ui/custom.css"); // ��ѡ������Զ�����ʽ
        c.OAuthClientId("swagger-ui");
        c.OAuthAppName("Swagger UI");
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    });
}

app.UseHttpsRedirection();

/*app.UseSession();*/

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

DataManager.CheckDBExist();

app.UseCors("AllowAnyOrigin");
/*Encrypt.MakeRSAKey();
*/
app.Run();
