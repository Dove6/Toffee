using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser
{
    // expression
    //     = assignment
    //     | block
    //     | conditional_expression
    //     | for_loop_expression
    //     | while_loop_expression
    //     | function_definition
    //     | pattern_matching;
    private bool TryParseExpression(out Expression? parsedExpression)
    {
        parsedExpression = null;
        foreach (var parser in _expressionParsers)
        {
            var parserResult = parser();
            if (parserResult is null)
                continue;
            parsedExpression = parserResult;
            return true;
        }
        return false;
        throw new NotImplementedException(); // TODO: error
    }

    // block
    //     = LEFT_BRACE, { statement }, [ unterminated_statement ], RIGHT_BRACE;
    private Expression? ParseBlockExpression()
    {
        if (!TryConsumeToken(out _, TokenType.LeftBrace))
            return null;

        throw new NotImplementedException();
    }

    // conditional_expression
    //     = KW_IF, parenthesized_expression, unterminated_statement, { conditional_elif_part }, [ conditional_else_part ];
    private Expression? ParseConditionalExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordIf)
            return null;

        throw new NotImplementedException();
    }

    // parenthesized_expression
    //     = LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
    private Expression ParseParenthesizedExpression()
    {
        throw new NotImplementedException();
    }

    // for_loop_expression
    //     = KW_FOR, for_loop_specification, unterminated_statement;
    private Expression? ParseForLoopExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordFor)
            return null;

        throw new NotImplementedException();
    }

    // for_loop_specification
    //     = LEFT_PARENTHESIS, [ IDENTIFIER, COMMA ], for_loop_range, RIGHT_PARENTHESIS;
    private bool TryParseForLoopSpecification(out string? counterName, out ForLoopRange? range)
    {
        counterName = null;
        range = null;
        return false;
        throw new NotImplementedException();
    }

    // for_loop_range
    //     = NUMBER, [ COLON, NUMBER, [ COLON, NUMBER ] ];
    private bool TryParseForLoopRange(out ForLoopRange? range)
    {
        range = null;
        return false;
        throw new NotImplementedException();
    }

    // while_loop_expression
    //     = KW_WHILE, parenthesized_expression, unterminated_statement;
    private Expression? ParseWhileLoopExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordWhile)
            return null;

        throw new NotImplementedException();
    }

    // function_definition
    //     = KW_FUNCTI, LEFT_PARENTHESIS, parameter_list, RIGHT_PARENTHESIS, block;
    private Expression? ParseFunctionDefinitionExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordFuncti)
            return null;

        throw new NotImplementedException();
    }

    // parameter_list
    //     = [ parameter, { COMMA, parameter } ];
    private bool TryParseParameterList(out List<FunctionParameter>? parameterList)
    {
        parameterList = null;
        return false;
        throw new NotImplementedException();
    }

    // parameter
    //     = [ KW_CONST ], IDENTIFIER, [ OP_BANG ];
    private bool TryParseParameter(out FunctionParameter? parameter)
    {
        parameter = null;
        return false;
        throw new NotImplementedException();
    }

    // pattern_matching
    //     = KW_MATCH, LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS, LEFT_BRACE, { pattern_specification }, RIGHT_BRACE;
    private Expression? ParsePatternMatchingExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordMatch)
            return null;

        throw new NotImplementedException();
    }

    // pattern_specification
    //     = pattern_expression, COLON, expression, SEMICOLON;
    private bool TryParsePatternSpecification(out PatternMatchingBranch? specification)
    {
        specification = null;
        return false;
        throw new NotImplementedException();
    }

    // pattern_expression
    //     = pattern_expression_disjunction
    //     | KW_DEFAULT;
    private bool TryParsePatternExpression(out Expression? patternExpression)
    {
        patternExpression = null;
        return false;
        throw new NotImplementedException();
    }

    // pattern_expression_disjunction
    //     = pattern_expression_conjunction, { KW_OR, pattern_expression_conjunction };
    private Expression? ParseDisjunctionPatternExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // pattern_expression_conjunction
    //     = pattern_expression_non_associative, { KW_AND, pattern_expression_non_associative };
    private Expression? ParseConjunctionPatternExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // pattern_expression_non_associative
    //     = OP_COMPARISON, LITERAL
    //     | OP_TYPE_CHECK, TYPE
    //     | expression
    //     | LEFT_PARENTHESIS, pattern_expression_disjunction, RIGHT_PARENTHESIS;
    private Expression? ParseNonAssociativePatternExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // assignment
    //     = null_coalescing, [ OP_ASSIGNMENT, assignment ];
    private Expression? ParseAssignmentExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // null_coalescing
    //     = nullsafe_pipe, { OP_QUERY_QUERY, nullsafe_pipe };
    private Expression? ParseNullCoalescingExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // nullsafe_pipe
    //     = disjunction, { OP_QUERY_GREATER, disjunction };
    private Expression? ParseNullsafePipeExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // disjunction
    //     = conjunction, { OP_OR_OR, conjunction };
    private Expression? ParseDisjunctionExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // conjunction
    //     = type_check, { OP_AND_AND, type_check };
    private Expression? ParseConjunctionExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // type_check
    //     = comparison, { OP_TYPE_CHECK, TYPE };
    private Expression? ParseTypeCheckExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // comparison
    //     = concatenation, { OP_COMPARISON, concatenation };
    private Expression? ParseComparisonExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // concatenation
    //     = term, { OP_DOT_DOT, term };
    private Expression? ParseNullConcatenationExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // term
    //     = factor, { OP_ADDITIVE, factor };
    private Expression? ParseTermExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // factor
    //     = unary_prefixed, { OP_MULTIPLICATIVE, unary_prefixed };
    private Expression? ParseFactorExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // unary_prefixed
    //     = OP_UNARY_PREFIX, unary_prefixed
    //     | exponentiation;
    private Expression? ParseUnaryPrefixedExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // exponentiation
    //     = suffixed_expression, { OP_CARET, suffixed_expression };
    private Expression? ParseExponentiationExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // suffixed_expression
    //     = primary_expression, [ function_call | namespace_access ];
    private Expression? ParseSuffixedExpressionExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // function_call
    //     = LEFT_PARENTHESIS, arguments_list, RIGHT_PARENTHESIS;
    private Expression? ParseFunctionCall()
    {
        return null;

        throw new NotImplementedException();
    }

    // arguments_list
    //     = [ argument, { COMMA, argument } ];
    private Expression? ParseArgumentsList()
    {
        return null;

        throw new NotImplementedException();
    }

    // argument
    //     = expression;
    private Expression? ParseArgument()
    {
        return null;

        throw new NotImplementedException();
    }

    // namespace_access
    //     = OP_NAMESPACE_ACCESS, primary_expression;
    private Expression? ParseNamespaceAccessExpression()
    {
        return null;

        throw new NotImplementedException();
    }

    // primary_expression
    //     = LITERAL
    //     | IDENTIFIER
    //     | LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
    private Expression? ParsePrimaryExpression()
    {
        return null;

        throw new NotImplementedException();
    }
}
