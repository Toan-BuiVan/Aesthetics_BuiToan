﻿
using Aesthetics.DataAccess.NetCore.Dapper;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Impliment;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject;
using Aesthetics.DTO.NetCore.DataObject.Model.Momo;
using ASP_NetCore_Aesthetics.Filter;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using ASP_NetCore_Aesthetics.Services.MomoServices;
using ASP_NetCore_Aesthetics.Services.SenderMail;
using ASP_NetCore_Aesthetics.Services.VnPaySevices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
//Configuration Login Google Account


// Add services to the container.
builder.Services.AddDbContext<DB_Context>(options =>
			   options.UseSqlServer(configuration.GetConnectionString("Aesthetics_ConnString")));

//Momo API Payment
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = false,
		ValidateAudience = false,
		ValidateLifetime = false,
		ValidateIssuerSigningKey = false,
		ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
		ValidAudience = builder.Configuration["Jwt:ValidAudience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
	};
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<IAccountRepository,AccountRepository>();
builder.Services.AddTransient<IUserSessionRepository,UserSessionRepository>();
builder.Services.AddTransient<IApplicationDbConnection,ApplicationDbConnection>();
builder.Services.AddTransient<IUserRepository,UserRepository>();
builder.Services.AddTransient<ISupplierRepository, SupplierRepository>();
builder.Services.AddTransient<ITypeProductsOfServicesRepository, TypeProductsOfServicesRepository>();
builder.Services.AddTransient<IServicessRepository, ServicessRepository>();
builder.Services.AddTransient<IBookingsRepository, BookingsRepository>();
builder.Services.AddTransient<IClinicRepository, ClinicRepository>();
builder.Services.AddTransient<IClinic_StaffRepository, Clinic_StaffRepository>();
builder.Services.AddTransient<IProductsRepository, ProductsRepository>();
builder.Services.AddTransient<ICommentRepository, CommentRepository>();
builder.Services.AddTransient<ICartProductRepository, CartProductRepositoty>();
builder.Services.AddTransient<IVouchersRepository, VouchersRepository>();
builder.Services.AddTransient<IWalletsRepository, WalletsRepository>();
builder.Services.AddTransient<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddTransient<IAdviseRepository, AdviseRepository>();
builder.Services.AddTransient<IPermissionRepository, PermissionRepository>();
builder.Services.AddTransient<IDashBoardRepository, DashBoardRepository>();
builder.Services.AddScoped<Filter_CheckToken>();
builder.Services.AddTransient<ILoggerManager, LoggerManager>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = configuration["RedisCacheUrl"]; });
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/NLog.config"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowNextJs", builder =>
	{
		builder.AllowAnyOrigin()
			   .AllowAnyMethod()
			   .AllowAnyHeader();
	});
});

//builder.Services.AddAuthentication(options =>
//{
//	options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//	options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
//{
//	options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
//	options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
//});

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
//.AddJwtBearer(options =>
//{
//	options.TokenValidationParameters = new TokenValidationParameters
//	{
//		ValidateIssuer = false,
//		ValidateAudience = false,
//		ValidateLifetime = false,
//		ValidateIssuerSigningKey = false,
//		ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
//		ValidAudience = builder.Configuration["Jwt:ValidAudience"],
//		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
//	};
//})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
	options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
	options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseCors(builder => builder
	.AllowAnyOrigin() 
	.AllowAnyMethod() 
	.AllowAnyHeader() 
	.WithExposedHeaders("New-AccessToken", "New-RefreshToken"));
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
		ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers",
		  "Origin, X-Requested-With, Content-Type, Accept");
	},
	FileProvider = new PhysicalFileProvider(
		   Path.Combine(builder.Environment.ContentRootPath, "FilesImages/Servicess")),
	RequestPath = "/FilesImages/Servicess"
});
app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
		ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers",
		  "Origin, X-Requested-With, Content-Type, Accept");
	},
	FileProvider = new PhysicalFileProvider(
		   Path.Combine(builder.Environment.ContentRootPath, "FilesImages/Products")),
	RequestPath = "/FilesImages/Products"
});
app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
		ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers",
		  "Origin, X-Requested-With, Content-Type, Accept");
	},
	FileProvider = new PhysicalFileProvider(
		   Path.Combine(builder.Environment.ContentRootPath, "FilesImages/Vouchers")),
	RequestPath = "/FilesImages/Vouchers"
});
app.MapControllers();

app.UseRouting();
app.Run();
