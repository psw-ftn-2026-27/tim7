using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.Quizzes;

internal static class QuizConfiguration
{
    public static void ConfigureQuizzes(this ModelBuilder modelBuilder)
    {
        ConfigureQuiz(modelBuilder.Entity<Quiz>());
        ConfigureQuestion(modelBuilder.Entity<Question>());
    }

    private static void ConfigureQuiz(EntityTypeBuilder<Quiz> builder)
    {
        builder.Property(quiz => quiz.Name).HasMaxLength(200);
        builder.Property(quiz => quiz.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasMany(quiz => quiz.Questions).WithOne().IsRequired().OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureQuestion(EntityTypeBuilder<Question> builder)
    {
        builder.HasMany(question => question.AnswerOptions).WithOne().IsRequired().OnDelete(DeleteBehavior.Cascade);
    }
}
