namespace UrlShortener.Application.Services.CodeGenerators;

public class CodeGeneratorSettings
{
    private const string DefaultAlphabet =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public int Length { get; set; } = 7;
    public string Alphabet { get; set; } = DefaultAlphabet;
}