using System.Text;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer
{
    private Token? MatchKeywordOrIdentifier()
    {
        // TODO: OoM exception
        static bool IsPartOfIdentifier(char? c) => c is not null && (char.IsLetterOrDigit(c.Value) || c is '_');
        if (_scanner.CurrentCharacter is null || !char.IsLetter(_scanner.CurrentCharacter.Value))
            return null;

        var nameBuilder = new StringBuilder($"{_scanner.CurrentCharacter.Value}");
        var maxLengthExceeded = false;
        _scanner.Advance();
        while (IsPartOfIdentifier(_scanner.CurrentCharacter))
        {
            AppendCharConsideringLengthLimit(nameBuilder, _scanner.CurrentCharacter, ref maxLengthExceeded);
            _scanner.Advance();
        }
        return KeywordOrIdentifierMapper.MapToKeywordOrIdentifier(nameBuilder.ToString());
    }
}
