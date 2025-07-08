# 🔧 Correção para Health Checks Duplicados

## 🚨 Problema Identificado
Health checks do `ApplicationDbContext` aparecem duplicados na interface do Health Checks UI.

## ✅ Soluções Disponíveis

### **Solução 1: Verificar o Program.cs (Mais Provável)**

O problema mais comum é ter configuração duplicada no `Program.cs`. Verifique se você tem:

```csharp
// ❌ PROBLEMA - Configuração duplicada
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Primeira configuração de health check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(); // DUPLICAÇÃO!

// Segunda configuração (através da biblioteca)
builder.Services.AddFastTechFoodsObservabilityAndHealthChecks<ApplicationDbContext>(builder.Configuration);
```

**✅ CORREÇÃO - Use apenas a biblioteca:**

```csharp
// ✅ CORRETO - Apenas uma configuração
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Não chame AddHealthChecks() manualmente!
// A biblioteca já faz isso:
builder.Services.AddFastTechFoodsObservabilityAndHealthChecks<ApplicationDbContext>(builder.Configuration);
```

### **Solução 2: Usar Nomes Únicos (Já Corrigido na Biblioteca)**

A biblioteca foi atualizada para usar nomes únicos:

- `"database-context"` - Para health check do DbContext
- `"custom-health-check"` - Para health checks customizados

### **Solução 3: Usar Método Específico para Health Checks**

Se você já tem observabilidade configurada em outro lugar e só precisa de health checks:

```csharp
// Use este método se você só quer health checks
builder.Services.AddFastTechFoodsHealthChecksOnly<ApplicationDbContext>("FastTechFoodsAuth.API");
```

## 🔍 Como Debuggar

### 1. Verificar registros no DI Container

Adicione este código temporário no `Program.cs` para ver quantos health checks estão registrados:

```csharp
var app = builder.Build();

// Debug - Verificar quantos health checks estão registrados
using (var scope = app.Services.CreateScope())
{
    var healthCheckService = scope.ServiceProvider.GetService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();
    if (healthCheckService != null)
    {
        var registrations = healthCheckService.GetType()
            .GetField("_checks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(healthCheckService);
        
        Console.WriteLine($"Total Health Checks registrados: {registrations}");
    }
}
```

### 2. Verificar logs de startup

Procure por mensagens como:
- `"Health check service already registered"`
- `"Duplicate health check registration"`

## 📋 Program.cs Corrigido Completo

```csharp
using FastTechFoods.Observability;
using FastTechFoodsAuth.Api.Middleware;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Application.Mapping;
using FastTechFoodsAuth.Application.Services;
using FastTechFoodsAuth.Application.Validators;
using FastTechFoodsAuth.Infra.Context;
using FastTechFoodsAuth.Infra.Repositories;
using FastTechFoodsAuth.Security.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
    Log.Information("Iniciando FastTechFoodsAuth API");
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddFastTechFoodsSwaggerWithJwt("FastTechFoodsAuth API", "v1", "API de autenticação para o sistema FastTechFoods");
    builder.Services.AddFastTechFoodsJwtAuthentication(builder.Configuration);
    builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
    
    // Services
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IUserService, UserService>();

    // Repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRoleRepository, RoleRepository>();

    // Validators
    builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE")
        ?? builder.Configuration.GetConnectionString("Default");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    // ✅ ÚNICA configuração de observabilidade + health checks
    builder.Services.AddFastTechFoodsObservabilityAndHealthChecks<ApplicationDbContext>(builder.Configuration);
    
    // ❌ NÃO FAÇA ISSO - Vai duplicar health checks:
    // builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseFastTechFoodsSecurityAudit();
    
    // ✅ ÚNICA configuração de health checks UI
    app.UseFastTechFoodsHealthChecksUI();
    
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("FastTechFoodsAuth API iniciada com sucesso");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao iniciar a aplicação");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
```

## 🎯 Configuração no appsettings.json

```json
{
  "Observability": {
    "ServiceName": "FastTechFoodsAuth.API",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://localhost:4317"
  },
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=fasttechfoods_auth;Username=postgres;Password=yourpassword"
  }
}
```

## ✅ Resultado Esperado

Após a correção, você deve ver apenas **UM** health check por serviço:

- **API Auth**: ✅ Healthy
  - **database-context**: ✅ Healthy (00:00:01.486837)

## 🆘 Se ainda houver duplicação

1. **Remova todas as chamadas manuais** a `AddHealthChecks()`
2. **Use apenas** `AddFastTechFoodsObservabilityAndHealthChecks<ApplicationDbContext>`
3. **Verifique dependências** que podem estar registrando health checks automaticamente
4. **Reinicie a aplicação** completamente

A duplicação deve desaparecer após seguir essas correções! 🎉
