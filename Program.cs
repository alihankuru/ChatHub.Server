using ChatHub.DataAccessLayer.Context;
using ChatHub.EntityLayer.Concrete;
using DefaultCorsPolicyNugetPackage;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDefaultCors();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddDbContext<ChatDbContext>();
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{

    options.SignIn.RequireConfirmedEmail = true;

}).AddEntityFrameworkStores<ChatDbContext>().AddDefaultTokenProviders();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub.Server.Hubs.ChatHub>("/chat-hub");

app.Run();
