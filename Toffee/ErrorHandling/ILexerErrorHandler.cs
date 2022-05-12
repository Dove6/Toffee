using Toffee.LexicalAnalysis;

namespace Toffee.ErrorHandling;

public interface ILexerErrorHandler
{
    void Handle(LexerError lexerError);
    void Handle(LexerWarning lexerWarning);
}
