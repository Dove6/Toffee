using System.Text;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer
{
    private Token? MatchKeywordOrIdentifier()
    {
        static bool IsPartOfIdentifier(char? c) => c is not null && (char.IsLetterOrDigit(c.Value) || c is '_');
        if (_scanner.CurrentCharacter is null || !char.IsLetter(_scanner.CurrentCharacter.Value))
            return null;

        var nameBuilder = new StringBuilder($"{_scanner.Advance()}");
        var maxLengthExceeded = false;
        while (IsPartOfIdentifier(_scanner.CurrentCharacter))
            CollectCharConsideringLengthLimit(nameBuilder, ref maxLengthExceeded);
        return KeywordOrIdentifierMapper.MapToKeywordOrIdentifier(nameBuilder.ToString());
    }
}
