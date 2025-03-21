using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("MINITWIT_DB_CONNECTION_STRING");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<AppUser, IdentityRole<int>>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errorMessage = context
            .ModelState.Values.SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault();
        return new BadRequestObjectResult(new { status = 400, error_msg = errorMessage });
    };
});

var app = builder.Build();

using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    var keyNormalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>();
    var users = context.Users.ToList();
    foreach (var user in users)
    {
        // Fix the user so it matches a user created by UserManager.
        user.LockoutEnabled = true;
        user.NormalizedUserName ??= keyNormalizer.NormalizeName(user.UserName);
        user.NormalizedEmail ??= keyNormalizer.NormalizeEmail(user.Email);
        user.SecurityStamp ??= GenerateBase32();
    }
    context.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();

app.Run();

const string _base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

static string GenerateBase32()
{
    const int length = 20;
    // base32 takes 5 bytes and converts them into 8 characters, which would be (byte length / 5) * 8
    // except that it also pads ('=') for the last processed chunk if it's less than 5 bytes.
    // So in order to handle the padding we add 1 less than the chunk size to our byte length
    // which will either be removed due to integer division truncation if the length was already a multiple of 5
    // or it will increase the divided length by 1 meaning that a 1-4 byte length chunk will be 1 instead of 0
    // so the padding is now included in our string length calculation
    return string.Create(
        ((length + 4) / 5) * 8,
        0,
        static (buffer, _) =>
        {
            Span<byte> bytes = stackalloc byte[length];
            RandomNumberGenerator.Fill(bytes);

            var index = 0;
            for (int offset = 0; offset < bytes.Length; )
            {
                byte a,
                    b,
                    c,
                    d,
                    e,
                    f,
                    g,
                    h;
                int numCharsToOutput = GetNextGroup(
                    bytes,
                    ref offset,
                    out a,
                    out b,
                    out c,
                    out d,
                    out e,
                    out f,
                    out g,
                    out h
                );

                buffer[index + 7] = ((numCharsToOutput >= 8) ? _base32Chars[h] : '=');
                buffer[index + 6] = ((numCharsToOutput >= 7) ? _base32Chars[g] : '=');
                buffer[index + 5] = ((numCharsToOutput >= 6) ? _base32Chars[f] : '=');
                buffer[index + 4] = ((numCharsToOutput >= 5) ? _base32Chars[e] : '=');
                buffer[index + 3] = ((numCharsToOutput >= 4) ? _base32Chars[d] : '=');
                buffer[index + 2] = (numCharsToOutput >= 3) ? _base32Chars[c] : '=';
                buffer[index + 1] = (numCharsToOutput >= 2) ? _base32Chars[b] : '=';
                buffer[index] = (numCharsToOutput >= 1) ? _base32Chars[a] : '=';
                index += 8;
            }
        }
    );
}

// returns the number of bytes that were output
static int GetNextGroup(
    Span<byte> input,
    ref int offset,
    out byte a,
    out byte b,
    out byte c,
    out byte d,
    out byte e,
    out byte f,
    out byte g,
    out byte h
)
{
    uint b1,
        b2,
        b3,
        b4,
        b5;

    int retVal;
    switch (input.Length - offset)
    {
        case 1:
            retVal = 2;
            break;
        case 2:
            retVal = 4;
            break;
        case 3:
            retVal = 5;
            break;
        case 4:
            retVal = 7;
            break;
        default:
            retVal = 8;
            break;
    }

    b1 = (offset < input.Length) ? input[offset++] : 0U;
    b2 = (offset < input.Length) ? input[offset++] : 0U;
    b3 = (offset < input.Length) ? input[offset++] : 0U;
    b4 = (offset < input.Length) ? input[offset++] : 0U;
    b5 = (offset < input.Length) ? input[offset++] : 0U;

    a = (byte)(b1 >> 3);
    b = (byte)(((b1 & 0x07) << 2) | (b2 >> 6));
    c = (byte)((b2 >> 1) & 0x1f);
    d = (byte)(((b2 & 0x01) << 4) | (b3 >> 4));
    e = (byte)(((b3 & 0x0f) << 1) | (b4 >> 7));
    f = (byte)((b4 >> 2) & 0x1f);
    g = (byte)(((b4 & 0x3) << 3) | (b5 >> 5));
    h = (byte)(b5 & 0x1f);

    return retVal;
}
