using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Kredit;
public class Startup(IConfiguration Configuration)
{

	public void ConfigureServices(IServiceCollection services)
    {
		JwtOptions jwtOptions = Configuration.GetSection(JwtOptions.Jwt)
			.Get<JwtOptions>()!;

		services.AddSingleton(jwtOptions);

        services.AddSingleton( new UserRepository(jwtOptions) );
        services.AddSingleton<MediaRepository>();
        services.AddControllers();

		services.AddAuthentication(options =>
		{
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		})
			.AddJwtBearer(options => 
			{
				#if DEBUG
					options.RequireHttpsMetadata = false;
				#else
					options.RequireHttpsMetadata = true;
				#endif
				
				options.MapInboundClaims = false;
			
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ClockSkew = TimeSpan.Zero,
			
					ValidateAudience = true,
					ValidAudience = jwtOptions.Audience,
			
					ValidateIssuer = true,
					ValidIssuer = jwtOptions.Issuer,
			
					ValidateLifetime = true,
			
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = jwtOptions.GetSecurityKey()
				};
			});

		services.AddAuthorizationBuilder()
			.AddDefaultPolicy("Authenticated", policy =>
			{
				policy.RequireAuthenticatedUser();
				policy.RequireClaim(JwtRegisteredClaimNames.Name);
				policy.RequireClaim(JwtRegisteredClaimNames.Sub);
				policy.RequireClaim(JwtOptions.RoleClaim);
			});
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
            
		app.UseCors(builder => builder
			.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()
		);

        app.UseHttpsRedirection();
        app.UseStaticFiles(
            new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider( Path.Combine(Directory.GetCurrentDirectory(), "Resources") ),
                RequestPath = "/Resources"
            }
        );


        app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}