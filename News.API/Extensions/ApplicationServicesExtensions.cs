namespace News.API.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationsService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ImageUploader>();
            services.AddControllers();
            services.AddSingleton<AggregateTranslator>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IFavoriteTwoService, FavoriteTwoService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISocialMediaService, SocialMediaService>();
            //services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationService, NotificationTwoService>();
            services.AddScoped<INewsTwoService, NewsTwoService>();
            services.AddScoped<ISpeechService, SpeechService>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddTransient<IMailSettings, EmailSettings>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.AddMemoryCache();
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddSingleton<IHostedService, ArticleNotificationService>(); 
            services.AddSingleton(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new News.Core.Profilers.MappingProfile());
            }).CreateMapper());
           services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policyOptioons =>
                {
                    policyOptioons.AllowAnyHeader().AllowAnyMethod().WithOrigins(configuration["FrontBaseUrl"]);
                });
            }
                );
            return services;
        }
        public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
             options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")).EnableSensitiveDataLogging());
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
             {
                 options.Password.RequireDigit = true;
                 options.Password.RequiredLength = 6;
                 options.Password.RequireNonAlphanumeric = false;
                 options.Password.RequireUppercase = true;
                 options.Password.RequireLowercase = false;
             })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
            services.AddHttpClient();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = configuration["Jwt:Issuer"],
                                ValidAudience = configuration["Jwt:Audience"],
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                            };
                        });
            return services;
        }
    }
}
