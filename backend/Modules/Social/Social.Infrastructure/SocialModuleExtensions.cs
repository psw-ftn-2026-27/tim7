using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Shared.Infrastructure.Images;
using Social.Application;
using Social.Application.BlogAuthoring;
using Social.Application.BlogCommenting;
using Social.Application.BlogReading;
using Social.Application.BlogVoting;
using Social.Application.Chats;
using Social.Application.ClubChat;
using Social.Application.ClubBrowsing;
using Social.Application.ClubInvitations;
using Social.Application.Blogs;
using Social.Application.ClubManagement;
using Social.Application.ClubMembership;
using Social.Application.ClubRoles;
using Social.Application.Clubs;
using Social.Infrastructure.Images;
using Social.Infrastructure.Persistence;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Chats;
using Social.Infrastructure.Persistence.Clubs;

namespace Social.Infrastructure;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SocialDbContext>(configuration, "social");
        services.AddHostedService<SocialModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<SocialMapperProfile>());

        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SocialDbContext>());
        services.AddScoped<IBlogReadRepository, BlogReadRepository>();
        services.AddScoped<IClubRepository, ClubRepository>();
        services.AddScoped<IClubReadRepository, ClubReadRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IChatReadRepository, ChatReadRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IMessageReadRepository, MessageReadRepository>();
        services.AddScoped<IImageStore, SocialImageStore>();
        services.AddScoped<IImageUsage, SocialImageUsage>();

        services.AddScoped<BlogAuthoringService>();
        services.AddScoped<BlogAuthoringQueries>();
        services.AddScoped<BlogReadingQueries>();
        services.AddScoped<BlogCommentingService>();
        services.AddScoped<BlogVotingService>();
        services.AddScoped<ClubManagementService>();
        services.AddScoped<ClubManagementQueries>();
        services.AddScoped<ClubBrowsingQueries>();
        services.AddScoped<ClubMembershipService>();
        services.AddScoped<ClubMembershipQueries>();
        services.AddScoped<ClubRolesService>();
        services.AddScoped<ClubInvitationsService>();
        services.AddScoped<ClubInvitationsQueries>();
        services.AddScoped<ClubChatService>();
        services.AddScoped<ClubChatQueries>();

        return services;
    }
}
