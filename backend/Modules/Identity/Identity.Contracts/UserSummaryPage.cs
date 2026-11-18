namespace Identity.Contracts;

public sealed record UserSummaryPage(List<UserSummary> Items, int TotalCount);
