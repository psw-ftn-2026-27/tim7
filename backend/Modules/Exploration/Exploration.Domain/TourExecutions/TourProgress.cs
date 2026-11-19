using Exploration.Domain.Tours;

namespace Exploration.Domain.TourExecutions;

// Checking a position compares the tour's key points with the session's completions, so the rule lives here and not in one TourExecution.
public sealed class TourProgress
{
    private const double ReachDistanceInKilometers = 0.05;

    // Records the position and completes at most one key point: the first by order the session has not completed, when it is near.
    public void CheckPosition(TourExecution execution, Tour tour, Location position)
    {
        execution.RecordPosition(position);

        var keyPoints = tour.KeyPoints;
        var nextKeyPoint = keyPoints.FirstOrDefault(keyPoint => !execution.HasCompleted(keyPoint.Id));
        if (nextKeyPoint is not null && nextKeyPoint.Location.DistanceTo(position) <= ReachDistanceInKilometers)
        {
            execution.CompleteKeyPoint(nextKeyPoint.Id, keyPoints.Count);
        }
    }
}
