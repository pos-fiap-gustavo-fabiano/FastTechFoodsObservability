# FastTechFoods.Observability

[![NuGet](https://img.shields.io/nuget/v/FastTechFoods.Observability.svg)](https://www.nuget.org/packages/FastTechFoods.Observability/)
[![Build Status](https://github.com/seu-usuario/seu-repositorio/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/seu-usuario/seu-repositorio/actions)

Biblioteca de observabilidade para aplicações .NET, focada em telemetria, tracing, métricas e integração facilitada com OpenTelemetry.

---

## ✨ Características

- **Integração com OpenTelemetry**: Suporte a traces, métricas e logs.
- **Exportação para múltiplos destinos**: Console, OTLP (OpenTelemetry Protocol), Prometheus, entre outros.
- **Configuração Simples**: Pensado para microserviços e aplicações modernas .NET.
- **Instrumentação automática**: Para ASP.NET Core, HTTP e EntityFramework Core.
- **Suporte ao Prometheus**: Métricas exportadas automaticamente para Prometheus.
- **HealthChecks integrados**: Monitoramento da saúde da aplicação.
- **Serilog integrado**: Logging estruturado com OpenTelemetry.
- **Pronto para uso em produção**.

---

## 🚀 Instalação

Via NuGet:

```bash
dotnet add package FastTechFoods.Observability
```

---

## 🛠️ Como Usar

### Configuração Básica (Método Simples)

```csharp
using FastTechFoods.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuração básica com parâmetros
builder.Services.AddFastTechFoodsObservability(
    serviceName: "MeuServicoAPI",
    serviceVersion: "1.0.0",
    otlpEndpoint: "http://localhost:4317"
);

var app = builder.Build();

app.Run();
```

### Configuração Avançada com HealthChecks

```csharp
using FastTechFoods.Observability;
using MeuProjeto.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração completa com HealthChecks
builder.Services.AddFastTechFoodsObservabilityAndHealthChecks<MeuDbContext>(
    builder.Configuration
);

var app = builder.Build();

// Configurar endpoints de HealthChecks e Prometheus
app.UseFastTechFoodsHealthChecksUI();

app.Run();
```

### Configuração apenas do Prometheus

```csharp
using FastTechFoods.Observability;

var builder = WebApplication.CreateBuilder(args);

// Apenas métricas do Prometheus
builder.Services.AddFastTechFoodsPrometheus(
    serviceName: "MeuServicoAPI",
    serviceVersion: "1.0.0"
);

var app = builder.Build();

// Endpoint para métricas do Prometheus
app.UseFastTechFoodsPrometheus();

app.Run();
```

### Configuração via appsettings.json

```json
{
  "Observability": {
    "ServiceName": "MeuServicoAPI",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

> Consulte a documentação dos métodos de configuração para customizações avançadas.

---

## 📦 Exportadores Suportados

- Console
- OTLP (gRPC/HTTP)
- Prometheus (métricas)
- Jaeger (via OTLP)
- Serilog com OpenTelemetry
- Outros facilmente configuráveis via OpenTelemetry

## 🎯 Instrumentações Suportadas

- ASP.NET Core (requests, responses, timing)
- HTTP Client (outbound calls)
- EntityFramework Core (database operations)
- Métricas customizadas via OpenTelemetry

## 📊 Endpoints Disponíveis

- `/metrics` - Métricas do Prometheus
- `/health` - Status da aplicação
- `/health-ui` - Interface web para HealthChecks

## 🔧 Métodos de Extensão Disponíveis

### Para IServiceCollection:
- `AddFastTechFoodsObservability()` - Configuração básica
- `AddFastTechFoodsObservabilityAndHealthChecks<TDbContext>()` - Configuração completa
- `AddFastTechFoodsPrometheus()` - Apenas Prometheus

### Para IApplicationBuilder:
- `UseFastTechFoodsHealthChecksUI()` - UI de HealthChecks + Prometheus
- `UseFastTechFoodsPrometheus()` - Apenas endpoint do Prometheus

---

## 📝 Exemplos de Configuração

### Configuração OTLP para Jaeger

```csharp
builder.Services.AddFastTechFoodsObservability(
    serviceName: "MeuServicoAPI",
    serviceVersion: "1.0.0",
    otlpEndpoint: "http://jaeger:14268/api/traces"
);
```

### Configuração com Prometheus e Grafana

```csharp
// No Program.cs
builder.Services.AddFastTechFoodsPrometheus(builder.Configuration);

var app = builder.Build();
app.UseFastTechFoodsPrometheus();

// Métricas estarão disponíveis em http://localhost:5000/metrics
```

### Configuração completa com Docker Compose

```yaml
version: '3.8'
services:
  meuservico:
    image: meuservico:latest
    ports:
      - "5000:5000"
    environment:
      - Observability__ServiceName=MeuServicoAPI
      - Observability__ServiceVersion=1.0.0
      - Observability__OtlpEndpoint=http://jaeger:14268/api/traces
  
  prometheus:
    image: prom/prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
  
  grafana:
    image: grafana/grafana
    ports:
      - "3000:3000"
```

---

## 🧩 Requisitos

- .NET 9.0 ou superior
- Pacotes OpenTelemetry 1.11.x ou superior
- Para Prometheus: endpoint `/metrics` deve estar acessível
- Para HealthChecks: EntityFramework Core (opcional)

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir Issues ou Pull Requests.

---

## 📄 Licença

Este projeto está licenciado sob a licença MIT.

---

## 📢 Observações

- Para uso com .NET 9, utilize versões do OpenTelemetry a partir de 1.11.x e ajuste o `TargetFramework` conforme necessário.

---

> Desenvolvido por FastTechFoods 🥡