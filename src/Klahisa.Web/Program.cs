using Klahisa.Web.Shared.Extensions;
using Klahisa.Web.Shared.Middlewares;
using Microsoft.AspNetCore.Mvc;

var cultura = new CultureInfo("pt-PT");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddFeatureViews();
builder.Services.AddFeatureClients(builder.Configuration);
builder.Services.AddAppAuthentication();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
