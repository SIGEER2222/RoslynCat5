using RoslynCat.Roslyn;
using RoslynCat.Controllers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Autofac.Core;
using RoslynCat;
using RoslynCat.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddTransient<Compiler>();
builder.Services.AddTransient<IWorkSpaceService,WorkSpaceService>();
builder.Services.AddTransient<ICompleteProvider,CompleteProvider>();
//builder.Services.AddScoped<ISignatureProvider,SignatureProvider>();
builder.Services.AddTransient<IHoverProvider,HoverProvider>();
builder.Services.AddTransient<ICodeCheckProvider,CodeCheckProvider>();
builder.Services.AddTransient<IGistService,CodeSharing>();
builder.Services.AddTransient<CompletionProvider>();
// 注释
//builder.Services.AddSwaggerGen(swagger =>
//{
//    swagger.SwaggerDoc("v1",new OpenApiInfo {
//        Title = "CompletionApi"
//    });
//});

//builder.Services.AddTransient<CompletionDocument>();
//builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
//builder.Host.ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new MyApplicationModule()));
var app = builder.Build();
//builder.Services.AddControllersWithViews()

if (!app.Environment.IsDevelopment()) {
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//����������ʹ��swagger
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//app.MapGroup("/completion").MapTodosApi();
app.MapControllers();

app.Run();
