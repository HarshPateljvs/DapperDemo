    using DapperDemo.API.Data;
    using DapperDemo.API.Middleware;
    using DapperDemo.API.Validators;
    using DapperDemo.DAL;
    using DapperDemo.DAL.Implementation;
    using DapperDemo.DAL.Interface;
    using DapperDemo.Database;
    using DapperDemo.Domain.Entities;
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using System.Text;

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.Configure<MyAppSettings>(builder.Configuration.GetSection(nameof(MyAppSettings)));
    builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptionsMonitor<MyAppSettings>>().CurrentValue);
    builder.Services.AddControllers();
    builder.Services.AddSingleton<SqlConnectionFactory>();
    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<IAuthService, AuthService>();
    //builder.Services.AddSingleton<DbInitializer>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();   
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(AppServicesHelper.Config.DapperConnectinString));
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddValidatorsFromAssemblyContaining<EmployeeValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<EmployeeRequestValidator>();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddAuthorization();



    builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    var app = builder.Build();
    app.UseMiddleware<RequestLoggingMiddleware>();
    AppServicesHelper.Services = app.Services;
    app.MapControllers();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();
    //using var scope = app.Services.CreateScope();
    //var dbInit = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    //await dbInit.RunScriptsAsync();
    app.Run();

