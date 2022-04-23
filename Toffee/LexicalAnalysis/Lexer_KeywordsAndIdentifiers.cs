using System.Text;

namespace Toffee.LexicalAnalysis;

public partial class Lexer
{
    private Token? MatchKeywordOrIdentifier()
    {
        // TODO: limit length
        static bool IsPartOfIdentifier(char? c) => c is not null && (char.IsLetterOrDigit(c.Value) || c is '_');
        if (_scanner.CurrentCharacter is null || !char.IsLetter(_scanner.CurrentCharacter.Value))
            return null;

        var nameBuilder = new StringBuilder($"{_scanner.CurrentCharacter.Value}");
        _scanner.Advance();
        while (IsPartOfIdentifier(_scanner.CurrentCharacter))
        {
            nameBuilder.Append(_scanner.CurrentCharacter.Value);
            _scanner.Advance();
        }
        return KeywordOrIdentifierMapper.MapToKeywordOrIdentifier(nameBuilder.ToString());
    }
}
