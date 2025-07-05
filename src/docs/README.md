# FastTechFoods.Observability

[![NuGet](https://img.shields.io/nuget/v/FastTechFoods.Observability.svg)](https://www.nuget.org/packages/FastTechFoods.Observability/)
[![Build Status](https://github.com/seu-usuario/seu-repositorio/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/seu-usuario/seu-repositorio/actions)

Biblioteca de observabilidade para aplicações .NET, focada em telemetria, tracing, métricas e integração facilitada com OpenTelemetry.

---

## ✨ Características

- **Integração com OpenTelemetry**: Suporte a traces, métricas e logs.
- **Exportação para múltiplos destinos**: Console, OTLP (OpenTelemetry Protocol), entre outros.
- **Configuração Simples**: Pensado para microserviços e aplicações modernas .NET.
- **Instrumentação automática**: Para ASP.NET Core, HTTP e EntityFramework Core.
- **Pronto para uso em produção**.

---

## 🚀 Instalação

Via NuGet:

```bash
dotnet add package FastTechFoods.Observability
```

---

## 🛠️ Como Usar

1. **Adicione a biblioteca ao seu projeto.**
2. **Configure os serviços no `Program.cs`:**

```csharp
using FastTechFoods.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastTechFoodsObservability(options =>
{
    // Configure endpoints, sampling, exporters, etc.
});

var app = builder.Build();

app.UseFastTechFoodsObservability();

app.Run();
```

> Consulte a documentação dos métodos de configuração para customizações avançadas.

---

## 📦 Exportadores Suportados

- Console
- OTLP (gRPC/HTTP)
- Jaeger (via OTLP)
- Outros facilmente configuráveis via OpenTelemetry

---

## 🎯 Suporte a Instrumentações

- ASP.NET Core
- HTTP Client
- EntityFramework Core

---

## 📝 Exemplo de Configuração OTLP

```csharp
builder.Services.AddFastTechFoodsObservability(options =>
{
    options.UseOtlpExporter("http://localhost:4317");
});
```

---

## 🧩 Requisitos

- .NET 8.0 ou superior
- Pacotes OpenTelemetry até 1.10.x (para .NET 8)

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