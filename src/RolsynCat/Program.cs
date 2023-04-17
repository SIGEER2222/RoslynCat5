using RoslynCat.Roslyn;
using RoslynCat.Controllers;
using RoslynCat.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddOptions();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddTransient<Compiler>();
builder.Services.AddTransient<IWorkSpaceService,WorkSpaceService>();
builder.Services.AddTransient<ICompleteProvider,CompleteProvider>();
//builder.Services.AddScoped<ISignatureProvider,SignatureProvider>();
builder.Services.AddTransient<IHoverProvider,HoverProvider>();
builder.Services.AddTransient<ICodeCheckProvider,CodeCheckProvider>();

builder.Services.AddHttpClient("GithubApi", client =>
{
    client.BaseAddress = new Uri("https://api.github.com");
});

builder.Services.AddTransient<IGistService,CodeSharing>();
builder.Services.AddTransient<CompletionProvider>();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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
