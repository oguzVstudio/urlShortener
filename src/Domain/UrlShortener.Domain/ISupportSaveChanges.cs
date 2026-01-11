namespace UrlShortener.Domain;

public interface ISupportSaveChanges
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}