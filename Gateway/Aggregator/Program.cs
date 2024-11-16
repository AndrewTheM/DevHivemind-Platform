using Aggregator.Mapping;
using Aggregator.Services;
using Aggregator.Services.Contracts;
using BlogPlatform.Shared.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using CommentGrpcClient = BlogPlatform.Shared.GRPC.Protos.CommentGrpc.CommentGrpcClient;
using PostGrpcClient = BlogPlatform.Shared.GRPC.Protos.PostGrpc.PostGrpcClient;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SerilogHelpers.Configure);

builder.Services.AddScoped<LoggingDelegatingHandler>();

builder.Services.AddGrpcClient<PostGrpcClient>(opts =>
{
    string apiUrl = builder.Configuration["GrpcSettings:PostsUrl"];
    opts.Address = new Uri(apiUrl);
})
.AddHttpMessageHandler<LoggingDelegatingHandler>()
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());

builder.Services.AddGrpcClient<CommentGrpcClient>(opts =>
{
    string apiUrl = builder.Configuration["GrpcSettings:CommentsUrl"];
    opts.Address = new Uri(apiUrl);
})
.AddHttpMessageHandler<LoggingDelegatingHandler>()
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());

builder.Services.AddTransient<IPostService, PostService>();
builder.Services.AddTransient<ICommentService, CommentService>();

builder.Services.AddHttpClient<IRecommenderService, RecommenderService>(
    client => client.BaseAddress = new Uri(builder.Configuration["RecommenderUrl"] + "/recommend")
);

builder.Services.AddAutoMapper(typeof(GrpcMappingProfile));

const string scheme = JwtBearerDefaults.AuthenticationScheme;
builder.Services.AddAuthentication(scheme)
    .AddJwtBearer(scheme, options =>
    {
        options.Authority = builder.Configuration["IdentityUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();


static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: retry => TimeSpan.FromSeconds(Math.Pow(retry, 2)),
            onRetry: (exception, retryCount, context) =>
            {
                Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
            });
}

static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}
