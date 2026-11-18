namespace Exploration.Contracts;

public interface IExplorationApi
{
    Task<TourForSaleDto?> GetTourForSaleAsync(Guid tourId);
}
