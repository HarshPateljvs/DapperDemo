using DapperDemo.DAL;
using DapperDemo.DAL.Implementation;
using DapperDemo.DAL.Interface;
using DapperDemo.Database;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddSingleton<DbInitializer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();   
var app = builder.Build();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
using var scope = app.Services.CreateScope();
var dbInit = scope.ServiceProvider.GetRequiredService<DbInitializer>();
await dbInit.RunScriptsAsync();

app.Run();

