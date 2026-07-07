using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Add SDK Web API services with assembly prefix for service scanning
builder.Services.AddSdkWebApi("Net.Web.Api.Sdk");

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSdkWebApi();

app.Run();
