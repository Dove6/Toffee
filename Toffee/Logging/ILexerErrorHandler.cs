using Toffee.LexicalAnalysis;

namespace Toffee.Logging;

public interface ILexerErrorHandler
{
    void Handle(LexerError lexerError);
    void Handle(LexerWarning lexerWarning);
}
