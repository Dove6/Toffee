using Toffee.SyntacticAnalysis;

namespace Toffee.ErrorHandling;

public interface IParserErrorHandler
{
    void Handle(ParserError lexerError);
    void Handle(ParserWarning lexerWarning);
}
