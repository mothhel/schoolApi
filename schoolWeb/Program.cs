namespace schoolWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // ÞÑÇÁÉ ÑÇÈØ ÇáÜ API ãä appsettings.json
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

            // ÅÖÇÝÉ HttpClient ááÊÚÇãá ãÚ ÇáÜ API
            builder.Services.AddHttpClient("SchoolApi", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl!);
            });

            // ÊÝÚíá Session áÍÝÙ ÇáÊæßä æÈíÇäÇÊ ÇáãÓÊÎÏã
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(120);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ÊÝÚíá Session
            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
