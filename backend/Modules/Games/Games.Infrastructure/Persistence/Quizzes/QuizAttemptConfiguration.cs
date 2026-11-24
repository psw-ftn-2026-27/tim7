using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.Quizzes;

internal static class QuizAttemptConfiguration
{
    public static void ConfigureQuizAttempts(this ModelBuilder modelBuilder)
    {
        ConfigureQuizAttempt(modelBuilder.Entity<QuizAttempt>());
    }

    private static void ConfigureQuizAttempt(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("QuizAttempts");
        builder.Property(attempt => attempt.Id).ValueGeneratedNever();

        builder.OwnsMany(attempt => attempt.GivenAnswers, givenAnswers =>
        {
            givenAnswers.ToTable("GivenAnswers");
            givenAnswers.WithOwner().HasForeignKey("QuizAttemptId");
        });

        builder.HasIndex(attempt => new { attempt.UserId, attempt.QuizId }).IsUnique();
    }
}
