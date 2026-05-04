using FluentValidation;
using FluentValidation.AspNetCore;
using Ticketing.Application.Handlers;
using TransportPlatform.Infrastructure.Common.Auth;
using TransportPlatform.Infrastructure.Common.Observability;
using TransportPlatform.Ticketing.Application.Commands;
using TransportPlatform.Ticketing.Application.Queries;
using TransportPlatform.Ticketing.Domain.Exceptions;
using TransportPlatform.Ticketing.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Logging (Serilog → OTel → Grafana Loki) ──────────────────────────────────
builder.Host.AddTransportLogging(builder.Configuration, "transport-ticketing");

// ── Infrastructure (DB, repositories, messaging) ──────────────────────────────
builder.Services.AddTicketingInfrastructure(builder.Configuration);

// ── Auth (JWT validation, UserContext, permission policies) ───────────────────
builder.Services.AddTransportAuth(builder.Configuration);

// ── Observability (OTel traces + metrics → Grafana Tempo / Prometheus) ────────
builder.Services.AddTransportObservability(builder.Configuration, "transport-ticketing");

// ── Application handlers (no MediatR — registered directly) ──────────────────
builder.Services.AddScoped<ReserveTicketHandler>();
builder.Services.AddScoped<CancelTicketHandler>();
builder.Services.AddScoped<ValidateTicketHandler>();
builder.Services.AddScoped<GetTicketByIdHandler>();
builder.Services.AddScoped<GetUserTicketsHandler>();
builder.Services.AddScoped<GetAvailableRoutesHandler>();
builder.Services.AddScoped<GetRouteByIdHandler>();

// ── Validation ────────────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── API ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
}).AddApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TransportPlatform Ticketing API", Version = "v1" });
    c.AddServer(new() { Url = "/" });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Database init (migrate + seed on startup in Development) ─────────────────
if (app.Environment.IsDevelopment())
{
    await app.Services.InitialiseDatabaseAsync();
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseTransportUserContext();
app.UseAuthorization();

// ── Global exception handler ──────────────────────────────────────────────────
app.UseExceptionHandler(error =>
{
    error.Run(async context =>
    {
        var feature = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        var (status, message) = feature?.Error switch
        {
            BusinessRuleException ex => (400, ex.Message),
            TicketNotFoundException ex => (404, ex.Message),
            RouteNotFoundException ex => (404, ex.Message),
            _ => (500, "An unexpected error occurred.")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
