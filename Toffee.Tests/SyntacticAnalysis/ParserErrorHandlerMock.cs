using System.Collections.Generic;
using Toffee.ErrorHandling;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis;

public class ParserErrorHandlerMock : IParserErrorHandler
{
    public List<ParserError> HandledErrors = new();
    public List<ParserWarning> HandledWarnings = new();

    public bool HadErrors => HandledErrors.Count > 0;
    public bool HadWarnings => HandledWarnings.Count > 0;

    public void Handle(ParserError lexerError) => HandledErrors.Add(lexerError);
    public void Handle(ParserWarning lexerWarning) => HandledWarnings.Add(lexerWarning);
}
