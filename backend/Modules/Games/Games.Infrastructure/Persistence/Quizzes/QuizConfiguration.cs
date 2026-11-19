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
        ConfigureAnswerOption(modelBuilder.Entity<AnswerOption>());
    }

    private static void ConfigureQuiz(EntityTypeBuilder<Quiz> builder)
    {
        builder.Property(quiz => quiz.Name).HasMaxLength(200);
        builder.Property(quiz => quiz.Status).HasConversion<string>().HasMaxLength(20);
        builder.ComplexProperty(quiz => quiz.Location);
        builder.HasMany(quiz => quiz.Questions).WithOne().IsRequired().OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureQuestion(EntityTypeBuilder<Question> builder)
    {
        // The ID is generated in the constructor; without this EF would take a new question for an existing row.
        builder.Property(question => question.Id).ValueGeneratedNever();
        builder.HasMany(question => question.AnswerOptions).WithOne().IsRequired().OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAnswerOption(EntityTypeBuilder<AnswerOption> builder)
    {
        // The ID is generated in the constructor; without this EF would take a new answer option for an existing row.
        builder.Property(answerOption => answerOption.Id).ValueGeneratedNever();
    }
}
