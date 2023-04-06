using RoslynCat.Roslyn;
using RoslynCat.Controllers;
using RoslynCat.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddTransient<Compiler>();

builder.Services.AddControllers();

builder.Services.AddTransient<ICompleteProvider,CompletionProvider>();
//builder.Services.AddTransient<ISignatureProvider,SignatureProvider>();
builder.Services.AddTransient<IHoverProvider,HoverProvider>();
builder.Services.AddTransient<ICodeCheckProvider,CodeCheckProvider>();
builder.Services.AddTransient<CompletionDocument>();

Console.WriteLine(builder.Services.Any(d => d.ServiceType == typeof(ICompleteProvider)));
var app = builder.Build();                              

if (!app.Environment.IsDevelopment()) {
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGroup("/completion").MapTodosApi();
//app.MapGet("/",() => "Hello World!");

app.Run();

//static IHostBuilder CreateHostBuilder(string[] args) =>
//    Host.CreateDefaultBuilder(args)
//        .ConfigureServices((_,services) =>
//            services.AddTransient<ICompleteProvider,CompletionProvider>()
//                    .AddTransient<IHoverProvider,HoverProvider>()
//                    .AddTransient<ICodeCheckProvider,CodeCheckProvider>());
