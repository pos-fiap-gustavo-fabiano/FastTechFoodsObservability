# Exemplo de Configuração com Prometheus

Este documento demonstra como configurar o FastTechFoods.Observability com Prometheus para monitoramento de métricas.

## Configuração Básica

### 1. Program.cs

```csharp
using FastTechFoods.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuração básica com Prometheus
builder.Services.AddFastTechFoodsPrometheus(
    serviceName: "FastTechFoods.API",
    serviceVersion: "1.0.0"
);

// Ou usando configuração do appsettings.json
// builder.Services.AddFastTechFoodsPrometheus(builder.Configuration);

var app = builder.Build();

// Ativa o endpoint /metrics para o Prometheus
app.UseFastTechFoodsPrometheus();

app.Run();
```

### 2. appsettings.json

```json
{
  "Observability": {
    "ServiceName": "FastTechFoods.API",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

## Configuração Completa com HealthChecks

### Program.cs

```csharp
using FastTechFoods.Observability;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração completa: OpenTelemetry + Prometheus + HealthChecks
builder.Services.AddFastTechFoodsObservabilityAndHealthChecks<ApplicationDbContext>(
    builder.Configuration
);

var app = builder.Build();

// Configura endpoints: /health, /health-ui, /metrics
app.UseFastTechFoodsHealthChecksUI();

app.Run();
```

## Configuração do Prometheus

### prometheus.yml

```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'fasttechfoods-api'
    static_configs:
      - targets: ['localhost:5000']
    metrics_path: '/metrics'
    scrape_interval: 5s
```

## Docker Compose para Ambiente Completo

```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Observability__ServiceName=FastTechFoods.API
      - Observability__ServiceVersion=1.0.0
      - Observability__OtlpEndpoint=http://jaeger:14268/api/traces
    depends_on:
      - prometheus
      - jaeger

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--storage.tsdb.retention.time=200h'
      - '--web.enable-lifecycle'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana-storage:/var/lib/grafana

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
      - "14268:14268"
    environment:
      - COLLECTOR_OTLP_ENABLED=true

volumes:
  grafana-storage:
```

## Métricas Disponíveis

O FastTechFoods.Observability expõe automaticamente as seguintes métricas:

### Métricas do ASP.NET Core
- `http_server_duration` - Duração das requisições HTTP
- `http_server_request_size` - Tamanho das requisições
- `http_server_response_size` - Tamanho das respostas

### Métricas do HTTP Client
- `http_client_duration` - Duração das chamadas HTTP externas
- `http_client_request_size` - Tamanho das requisições externas

### Métricas Customizadas
Você pode adicionar métricas customizadas usando OpenTelemetry:

```csharp
using System.Diagnostics.Metrics;

public class OrderService
{
    private static readonly Meter Meter = new("FastTechFoods.OrderService");
    private static readonly Counter<int> OrdersProcessed = Meter.CreateCounter<int>("orders_processed_total");

    public async Task ProcessOrder(Order order)
    {
        // Processar pedido
        await ProcessOrderLogic(order);
        
        // Incrementar métrica
        OrdersProcessed.Add(1, new KeyValuePair<string, object?>("status", "success"));
    }
}
```

## Acessando as Métricas

- **Métricas do Prometheus**: `http://localhost:5000/metrics`
- **HealthChecks**: `http://localhost:5000/health`
- **HealthChecks UI**: `http://localhost:5000/health-ui`
- **Prometheus UI**: `http://localhost:9090`
- **Grafana**: `http://localhost:3000` (admin/admin)

## Dashboards Recomendados

Para Grafana, recomendamos os seguintes dashboards:

1. **ASP.NET Core**: ID 10915
2. **Prometheus**: ID 2
3. **Node Exporter**: ID 1860

## Alertas do Prometheus

### rules.yml

```yaml
groups:
  - name: fasttechfoods.rules
    rules:
      - alert: HighErrorRate
        expr: rate(http_server_duration_count{status=~"5.."}[5m]) > 0.1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High error rate detected"
          description: "Error rate is above 10% for 5 minutes"

      - alert: HighLatency
        expr: histogram_quantile(0.95, rate(http_server_duration_bucket[5m])) > 0.5
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High latency detected"
          description: "95th percentile latency is above 500ms"
```

## Troubleshooting

### Métricas não aparecem no Prometheus

1. Verifique se o endpoint `/metrics` está acessível
2. Confirme a configuração do `prometheus.yml`
3. Verifique os logs do Prometheus para erros de scraping

### HealthChecks falham

1. Verifique a conectividade com o banco de dados
2. Confirme que o DbContext está registrado no DI
3. Verifique as configurações de connection string

### Alta latência nas métricas

1. Ajuste o `scrape_interval` no Prometheus
2. Considere usar sampling para traces
3. Verifique a performance do exportador OTLP
