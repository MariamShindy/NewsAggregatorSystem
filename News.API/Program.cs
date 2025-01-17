using Microsoft.AspNetCore.Hosting;
using News.API.Extensions;
using News.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddApplicationsService(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("MyPolicy");
app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers(); 
});
app.Run();
