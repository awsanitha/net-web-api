using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSdk();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseSdk();
app.UseSdkSwagger();
app.UseAuthorization();
app.MapControllers();

app.Run();
