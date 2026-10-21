using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Core.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddTransient<IPostService, PostService>();
        services.AddTransient<IFriendshipService, FriendshipService>();

        return services;
    }
}
