using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.GraphQL.Mutations;
using TerminoApp_NewBackend.GraphQL.Queries;
using TerminoApp_NewBackend.GraphQL.Types;
using TerminoApp_NewBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// 📌 Logging (ako želiš detaljnije logove za debug)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// 1) EF Core – SAMO AddDbContext (scoped). NEMA AddDbContextFactory.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) JWT auth
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSection["Key"] ?? throw new Exception("JwtSettings:Key nedostaje");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // samo za DEV
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
        ValidIssuer = jwtIssuer,
        ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

// 3) JwtService
builder.Services.AddSingleton<JwtService>();

// 4) EmailService – Outlook SMTP konfiguracija
builder.Services.AddSingleton(provider =>
    new EmailService(
        host: "smtp.office365.com",     // Outlook SMTP host
        port: 587,                      // TLS port
        username: "",
        password: "",
        fromEmail: ""
    )
);

// 5) GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<ServiceType>()
    .AddType<UserType>()
    .AddType<UserInputType>()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

// Swagger (opcionalno)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL("/graphql");

app.Run();