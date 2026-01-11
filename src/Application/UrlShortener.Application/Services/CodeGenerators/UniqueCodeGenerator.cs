using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace UrlShortener.Application.Services.CodeGenerators;

public class UniqueCodeGenerator : IUniqueCodeGenerator
{
    private readonly int _length;
    private readonly string _alphabet;
    private readonly int _alphabetLength;

    public UniqueCodeGenerator(IOptions<CodeGeneratorSettings> codeGeneratorSettings)
    {
        var settings = codeGeneratorSettings.Value;
        _length = settings.Length;
        _alphabet = settings.Alphabet;
        _alphabetLength = settings.Alphabet.Length;
    }

    public ValueTask<string> GenerateAsync(CancellationToken cancellationToken)
    {
        Span<char> buffer = stackalloc char[_length];

        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = _alphabet[RandomNumberGenerator.GetInt32(_alphabetLength)];
        }

        return new ValueTask<string>(new string(buffer));
    }
}