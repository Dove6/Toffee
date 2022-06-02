using System.Collections.Generic;
using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;

namespace Toffee.Tests.LexicalAnalysis;

public class LexerErrorHandlerMock : ILexerErrorHandler
{
    public List<LexerError> HandledErrors = new();
    public List<LexerWarning> HandledWarnings = new();

    public bool HadErrors => HandledErrors.Count > 0;
    public bool HadWarnings => HandledWarnings.Count > 0;

    public void Handle(LexerError lexerError) => HandledErrors.Add(lexerError);
    public void Handle(LexerWarning lexerWarning) => HandledWarnings.Add(lexerWarning);
}
