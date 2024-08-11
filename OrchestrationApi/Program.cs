using System.Configuration;
using OrchestrationApi.Contracts;
using OrchestrationApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IEuIssuesBackendClient, EuIssuesBackendClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration.GetValue<string>("ExternalServices__EuIssuesBackendBaseUrl")
            ?? throw new ConfigurationErrorsException(
                "ExternalServices__EuIssuesBackendBaseUrl configuration value is missing."
            )
    );
});

builder.Services.AddHttpClient<IForeignIssuesBackendClient, ForeignIssuesBackendClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration.GetValue<string>("ExternalServices__ForeignIssuesBackendBaseUrl")
            ?? throw new ConfigurationErrorsException(
                "ExternalServices__ForeignIssuesBackendBaseUrl configuration value is missing."
            )
    );
});

builder.Services.AddScoped<IIssueRoutingService, IssueRoutingService>();
builder.Services.AddScoped<IIssuesService, IssuesService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
