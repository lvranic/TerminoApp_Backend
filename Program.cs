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

// (opcionalno, ali korisno za Npgsql DateTime ponašanje)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 1) EF Core – KORISTIMO *FACTORY* (bez AddDbContext!)
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Missing connection string 'DefaultConnection'.");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connStr));
// Ako želiš pool: builder.Services.AddPooledDbContextFactory<AppDbContext>(...);
// ali i dalje NEMOJ dodavati AddDbContext uz to.

// 2) JWT auth
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSection["Key"] ?? throw new Exception("JwtSettings:Key missing");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // dev
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

// 4) GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<UserType>()
    .AddType<UserInputType>()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true);

// Swagger / API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ⬇️ redoslijed je bitan
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL(); // /graphql

app.Run();