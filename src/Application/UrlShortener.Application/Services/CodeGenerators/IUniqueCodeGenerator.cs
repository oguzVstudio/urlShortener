namespace UrlShortener.Application.Services.CodeGenerators;

public interface IUniqueCodeGenerator
{
    ValueTask<string> GenerateAsync(CancellationToken cancellationToken);
}