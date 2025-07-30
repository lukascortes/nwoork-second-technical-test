using Microsoft.EntityFrameworkCore;
using TimeOffManager.Data;
using TimeOffManager.Services; // ← Asegúrate de tener este using
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Configuración del string de conexión usando SqliteConnectionStringBuilder
var connectionStringBuilder = new SqliteConnectionStringBuilder
{
    DataSource = "timeoff.db",
    Cache = SqliteCacheMode.Shared,
    Mode = SqliteOpenMode.ReadWriteCreate,
    DefaultTimeout = 5 // en segundos
};

var connectionString = connectionStringBuilder.ToString();

// ⬇️ REGISTROS DE SERVICIOS

// Base de datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Servicio JWT
builder.Services.AddScoped<JwtService>(); // ⬅️ Esta línea es esencial

// Controladores
builder.Services.AddControllers();

// CORS (ajusta si necesitas algo más estricto)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "clave_jwt_por_defecto_123456";
var key = Encoding.ASCII.GetBytes(jwtKey);

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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// ⬇️ CONFIGURACIÓN DE LA APP

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Crea la base de datos si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
