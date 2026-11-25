using Games.Application;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence;

internal sealed class GamesDbContext : DbContext, IUnitOfWork
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<SocialChallenge> SocialChallenges => Set<SocialChallenge>();
    public DbSet<SocialChallengeRun> SocialChallengeRuns => Set<SocialChallengeRun>();
    public DbSet<LocationChallenge> LocationChallenges => Set<LocationChallenge>();
    public DbSet<LocationChallengeProgress> LocationChallengeProgresses => Set<LocationChallengeProgress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("games");
        builder.ConfigureQuizzes();
        builder.ConfigureQuizAttempts();
        builder.ConfigureSocialChallenges();
        builder.ConfigureSocialChallengeRuns();
        builder.ConfigureLocationChallenges();
    }
}
