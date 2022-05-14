﻿using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser
{
    private readonly TokenType[] _comparisonOperators =
    {
        TokenType.OperatorLess,
        TokenType.OperatorLessEquals,
        TokenType.OperatorGreater,
        TokenType.OperatorGreaterEquals,
        TokenType.OperatorEqualsEquals,
        TokenType.OperatorBangEquals
    };

    private readonly TokenType[] _additiveOperator =
    {
        TokenType.OperatorMinus,
        TokenType.OperatorPlus
    };

    private readonly TokenType[] _multiplicativeOperators =
    {
        TokenType.OperatorAsterisk,
        TokenType.OperatorSlash,
        TokenType.OperatorPercent
    };

    private readonly TokenType[] _unaryOperators =
    {
        TokenType.OperatorPlus,
        TokenType.OperatorMinus,
        TokenType.OperatorBang
    };

    private readonly TokenType[] _literalTokenTypes =
    {
        TokenType.LiteralInteger,
        TokenType.LiteralFloat,
        TokenType.LiteralString,
        TokenType.KeywordTrue,
        TokenType.KeywordFalse,
        TokenType.KeywordNull
    };

    private readonly TokenType[] _typeTokenTypes =
    {
        TokenType.KeywordFloat,
        TokenType.KeywordInt,
        TokenType.KeywordBool,
        TokenType.KeywordString,
        TokenType.KeywordNull
    };

    private readonly TokenType[] _assignmentTokenTypes =
    {
        TokenType.OperatorEquals,
        TokenType.OperatorPlusEquals,
        TokenType.OperatorMinusEquals,
        TokenType.OperatorAsteriskEquals,
        TokenType.OperatorSlashEquals,
        TokenType.OperatorPercentEquals
    };

    // expression
    //     = block
    //     | conditional_expression
    //     | for_loop_expression
    //     | while_loop_expression
    //     | function_definition
    //     | pattern_matching
    //     | assignment;
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
    }

    private Expression ParseExpression()
    {
        if (TryParseExpression(out var parsedExpression))
            return parsedExpression!;
        throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
    }

    // block
    //     = LEFT_BRACE, { statement }, [ unterminated_statement ], RIGHT_BRACE;
    private Expression? ParseBlockExpression()
    {
        if (!TryConsumeToken(out _, TokenType.LeftBrace))
            return null;

        var statementList = new List<Statement>();
        var unterminatedStatement = (Statement?)null;
        while (TryParseStatement(out var parsedStatement))
        {
            if (!parsedStatement!.IsTerminated)
            {
                unterminatedStatement = parsedStatement;
                break;
            }
            statementList.Add(parsedStatement);
        }

        ConsumeToken(TokenType.RightBrace);

        return new BlockExpression(statementList, unterminatedStatement);
    }

    // conditional_expression
    //     = conditional_if_part, { conditional_elif_part }, [ conditional_else_part ];
    private Expression? ParseConditionalExpression()
    {
        if (!TryMatchConditionalIfPart(false, out var ifPart))
            return null;

        var elifPartList = new List<ConditionalElement>();
        while (TryMatchConditionalIfPart(true, out var elifPart))
            elifPartList.Add(elifPart!);

        return TryMatchConditionalElsePart(out var elsePart)
            ? new ConditionalExpression(ifPart!, elifPartList, elsePart)
            : new ConditionalExpression(ifPart!, elifPartList);
    }

    // conditional_if_part
    //     = KW_IF, parenthesized_expression, unterminated_statement;
    // conditional_elif_part
    //     = KW_ELIF, parenthesized_expression, unterminated_statement;
    private bool TryMatchConditionalIfPart(bool shouldMatchElifInstead, out ConditionalElement? ifPart)
    {
        ifPart = null;
        var expectedTokenType = shouldMatchElifInstead ? TokenType.KeywordElif : TokenType.KeywordIf;
        if (!TryConsumeToken(out _, expectedTokenType))
            return false;

        var condition = ParseParenthesizedExpression();
        var consequent = ParseExpression();
        ifPart = new ConditionalElement(condition, consequent);
        return true;
    }

    // conditional_else_part
    //     = KW_ELSE, unterminated_statement;
    private bool TryMatchConditionalElsePart(out Expression? elsePart)
    {
        elsePart = null;
        if (!TryConsumeToken(out _, TokenType.KeywordElse))
            return false;

        elsePart = ParseExpression();
        return true;
    }

    // parenthesized_expression
    //     = LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
    private Expression ParseParenthesizedExpression()
    {
        ConsumeToken(TokenType.LeftParenthesis);
        var expression = ParseExpression();
        ConsumeToken(TokenType.RightParenthesis);
        return expression;
    }

    // for_loop_expression
    //     = KW_FOR, for_loop_specification, unterminated_statement;
    private Expression? ParseForLoopExpression()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordFor))
            return null;

        var (counterName, loopRange) = ParseForLoopSpecification();
        return new ForLoopExpression(loopRange, ParseExpression(), counterName);
    }

    // for_loop_specification
    //     = LEFT_PARENTHESIS, [ IDENTIFIER, COMMA ], for_loop_range, RIGHT_PARENTHESIS;
    /// <returns>A tuple consisting of:
    /// <list type="bullet">
    /// <item>optional counter name,</item>
    /// <item>looping range.</item>
    /// </list>
    /// </returns>
    private (string?, ForLoopRange) ParseForLoopSpecification()
    {
        ConsumeToken(TokenType.LeftParenthesis);

        var hasCounter = TryConsumeToken(out var identifier, TokenType.Identifier);
        if (hasCounter)
            ConsumeToken(TokenType.Comma);

        var range = ParseForLoopRange();

        ConsumeToken(TokenType.RightParenthesis);

        return (hasCounter ? (string)identifier.Content! : null, range);
    }

    // for_loop_range
    //     = expression, [ COLON, expression, [ COLON, expression ] ];
    private ForLoopRange ParseForLoopRange()
    {
        var first = ParseExpression();
        if (!TryConsumeToken(out _, TokenType.Comma))
            return new ForLoopRange(first);

        var second = ParseExpression();
        if (!TryConsumeToken(out _, TokenType.Comma))
            return new ForLoopRange(second, first);

        var third = ParseExpression();
        return new ForLoopRange(second, first, third);
    }

    // while_loop_expression
    //     = KW_WHILE, parenthesized_expression, unterminated_statement;
    private Expression? ParseWhileLoopExpression()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordWhile))
            return null;

        var condition = ParseParenthesizedExpression();
        return new WhileLoopExpression(condition, ParseExpression());
    }

    // function_definition
    //     = KW_FUNCTI, LEFT_PARENTHESIS, parameter_list, RIGHT_PARENTHESIS, block;
    private Expression? ParseFunctionDefinitionExpression()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordFuncti))
            return null;

        ConsumeToken(TokenType.LeftParenthesis);
        var parameterList = ParseParameterList();
        ConsumeToken(TokenType.RightParenthesis);

        var body = ParseBlockExpression();
        if (body is null)
            throw new ParserException(new ExpectedBlockExpression(_lexer.CurrentToken));

        return new FunctionDefinitionExpression(parameterList, (BlockExpression)body);
    }

    // parameter_list
    //     = [ parameter, { COMMA, parameter } ];
    private List<FunctionParameter> ParseParameterList()
    {
        var list = new List<FunctionParameter>();
        if (!TryParseParameter(out var firstParameter))
            return list;
        list.Add(firstParameter!);
        while (TryConsumeToken(out _, TokenType.Comma))
            list.Add(ParseParameter());
        return list;
    }

    // parameter
    //     = [ KW_CONST ], IDENTIFIER, [ OP_BANG ];
    private bool TryParseParameter(out FunctionParameter? parameter)
    {
        parameter = null;
        var isConst = TryConsumeToken(out _, TokenType.KeywordConst);
        if (!TryConsumeToken(out var identifier, TokenType.Identifier))
            return !isConst
                ? false
                : throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, TokenType.Identifier));
        var isNullable = !TryConsumeToken(out _, TokenType.OperatorBang);

        parameter = new FunctionParameter((string)identifier.Content!, isConst, isNullable);
        return true;
    }

    private FunctionParameter ParseParameter()
    {
        if (TryParseParameter(out var parameter))
            return parameter!;
        throw new ParserException(
            new UnexpectedToken(_lexer.CurrentToken, TokenType.KeywordConst, TokenType.Identifier));
    }

    // pattern_matching
    //     = KW_MATCH, LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS, LEFT_BRACE, { pattern_specification }, [ default_pattern_specification ], RIGHT_BRACE;
    private Expression? ParsePatternMatchingExpression()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordMatch))
            return null;

        var argument = ParseParenthesizedExpression();
        ConsumeToken(TokenType.LeftBrace);

        var patternSpecificationList = new List<PatternMatchingBranch>();
        while (TryParsePatternSpecification(out var specification))
            patternSpecificationList.Add(specification!);

        ConsumeToken(TokenType.RightBrace);
        return new PatternMatchingExpression(argument, patternSpecificationList);
    }

    // pattern_specification
    //     = pattern_expression, COLON, expression, SEMICOLON;
    // default_pattern_specification
    //     = KW_DEFAULT, COLON, expression, SEMICOLON;
    private bool TryParsePatternSpecification(out PatternMatchingBranch? specification)
    {
        specification = null;

        var isDefault = TryConsumeToken(out _, TokenType.KeywordDefault);
        var condition = (Expression?)null;
        if (!isDefault && (condition = ParseDisjunctionPatternExpression()) is null)
            return false;

        ConsumeToken(TokenType.Colon);
        var consequent = ParseExpression();
        ConsumeToken(TokenType.Semicolon);
        specification = new PatternMatchingBranch(condition, consequent);
        return true;
    }

    // pattern_expression
    //     = pattern_expression_disjunction;
    // pattern_expression_disjunction
    //     = pattern_expression_conjunction, { KW_OR, pattern_expression_conjunction };
    private Expression ParseDisjunctionPatternExpression()
    {
        var expression = ParseConjunctionPatternExpression();
        while (TryConsumeToken(out _, TokenType.KeywordOr))
        {
            var right = ParseConjunctionPatternExpression();
            expression = new BinaryExpression(expression, Operator.PatternMatchingDisjunction, right);
        }
        return expression;
    }

    // pattern_expression_conjunction
    //     = pattern_expression_non_associative, { KW_AND, pattern_expression_non_associative };
    private Expression ParseConjunctionPatternExpression()
    {
        var expression = ParseNonAssociativePatternExpression();
        while (TryConsumeToken(out _, TokenType.KeywordAnd))
        {
            var right = ParseNonAssociativePatternExpression();
            expression = new BinaryExpression(expression, Operator.PatternMatchingConjunction, right);
        }
        return expression;
    }

    // pattern_expression_non_associative
    //     = OP_COMPARISON, LITERAL
    //     | OP_TYPE_CHECK, TYPE
    //     | expression
    //     | LEFT_PARENTHESIS, pattern_expression_disjunction, RIGHT_PARENTHESIS;
    private Expression ParseNonAssociativePatternExpression()
    {
        if (TryConsumeToken(out var comparisonOperator, _comparisonOperators))
            if (TryConsumeToken(out var literal, _literalTokenTypes))
                return new UnaryExpression(LiteralMapper.MapToLiteralExpression(literal),
                    OperatorMapper.MapPatternMatchingComparisonOperator(comparisonOperator.Type));
            else
                throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, _literalTokenTypes));

        if (TryConsumeToken(out _, TokenType.KeywordIs))
        {
            var typeCheckOperator = TryConsumeToken(out _, TokenType.KeywordNot)
                ? Operator.PatternMatchingNotEqualTypeCheck
                : Operator.PatternMatchingEqualTypeCheck;
            if (TryConsumeToken(out var type, _typeTokenTypes))
                return new UnaryExpression(TypeMapper.MapToTypeExpression(type.Type), typeCheckOperator);
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, _typeTokenTypes));
        }

        if (TryConsumeToken(out _, TokenType.LeftParenthesis))
        {
            var patternExpression = ParseDisjunctionPatternExpression();
            ConsumeToken(TokenType.RightParenthesis);
            return patternExpression;
        }

        var parsedExpression = ParseAssignmentExpression();
        if (parsedExpression is null)
            throw new ParserException(new ExpectedPatternExpression(_lexer.CurrentToken));
        return parsedExpression;
    }

    // assignment
    //     = null_coalescing, [ OP_ASSIGNMENT, assignment ];
    private Expression? ParseAssignmentExpression()
    {
        var left = ParseNullCoalescingExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out var @operator, _assignmentTokenTypes))
            return left;
        var right = ParseAssignmentExpression();
        if (right is null)
            throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));

        return new BinaryExpression(left, OperatorMapper.MapAssignmentOperator(@operator.Type), right);
    }

    // null_coalescing
    //     = nullsafe_pipe, { OP_QUERY_QUERY, nullsafe_pipe };
    private Expression? ParseNullCoalescingExpression()
    {
        var expression = ParseNullsafePipeExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.OperatorQueryQuery))
        {
            var right = ParseNullsafePipeExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.NullCoalescing, right);
        }
        return expression;
    }

    // nullsafe_pipe
    //     = disjunction, { OP_QUERY_GREATER, disjunction };
    private Expression? ParseNullsafePipeExpression()
    {
        var expression = ParseDisjunctionExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.OperatorQueryGreater))
        {
            var right = ParseDisjunctionExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.NullSafePipe, right);
        }
        return expression;
    }

    // disjunction
    //     = conjunction, { OP_OR_OR, conjunction };
    private Expression? ParseDisjunctionExpression()
    {
        var expression = ParseConjunctionExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.OperatorOrOr))
        {
            var right = ParseConjunctionExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.Disjunction, right);
        }
        return expression;
    }

    // conjunction
    //     = type_check, { OP_AND_AND, type_check };
    private Expression? ParseConjunctionExpression()
    {
        var expression = ParseTypeCheckExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.OperatorAndAnd))
        {
            var right = ParseTypeCheckExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.Conjunction, right);
        }
        return expression;
    }

    // type_check
    //     = comparison, { OP_TYPE_CHECK, TYPE };
    private Expression? ParseTypeCheckExpression()
    {
        var expression = ParseComparisonExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.KeywordIs))
        {
            var typeCheckOperator = TryConsumeToken(out _, TokenType.KeywordNot)
                ? Operator.NotEqualTypeCheck
                : Operator.EqualTypeCheck;
            var right = TypeMapper.MapToTypeExpression(ConsumeToken(_typeTokenTypes).Type);
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, typeCheckOperator, right);
        }
        return expression;
    }

    // comparison
    //     = concatenation, { OP_COMPARISON, concatenation };
    private Expression? ParseComparisonExpression()
    {
        var expression = ParseConcatenationExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out var comparisonOperator, _comparisonOperators))
        {
            var right = ParseConcatenationExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression,
                OperatorMapper.MapComparisonOperator(comparisonOperator.Type),
                right);
        }
        return expression;
    }

    // concatenation
    //     = term, { OP_DOT_DOT, term };
    private Expression? ParseConcatenationExpression()
    {
        var expression = ParseTermExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.OperatorDotDot))
        {
            var right = ParseTermExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.Concatenation, right);
        }
        return expression;
    }

    // term
    //     = factor, { OP_ADDITIVE, factor };
    private Expression? ParseTermExpression()
    {
        var expression = ParseFactorExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out var additiveOperator, _additiveOperator))
        {
            var right = ParseFactorExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression,
                OperatorMapper.MapAdditiveOperator(additiveOperator.Type),
                right);
        }
        return expression;
    }

    // factor
    //     = unary_prefixed, { OP_MULTIPLICATIVE, unary_prefixed };
    private Expression? ParseFactorExpression()
    {
        var expression = ParseUnaryPrefixedExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out var multiplicativeOperator, _multiplicativeOperators))
        {
            var right = ParseUnaryPrefixedExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression,
                OperatorMapper.MapMultiplicativeOperator(multiplicativeOperator.Type),
                right);
        }
        return expression;
    }

    // unary_prefixed
    //     = OP_UNARY_PREFIX, unary_prefixed
    //     | exponentiation;
    private Expression? ParseUnaryPrefixedExpression()
    {
        if (!TryConsumeToken(out var unaryOperator, _unaryOperators))
            return ParseExponentiationExpression();
        var expression = ParseUnaryPrefixedExpression();
        if (expression is null)
            throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));

        return new UnaryExpression(expression, OperatorMapper.MapUnaryOperator(unaryOperator.Type));
    }

    // exponentiation
    //     = namespace_access_or_function_call, { OP_CARET, namespace_access_or_function_call };
    private Expression? ParseExponentiationExpression()
    {
        var expression = ParseNamespaceAccessOrFunctionCallExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.OperatorCaret))
        {
            var right = ParseNamespaceAccessOrFunctionCallExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.Exponentiation, right);
        }
        return expression;
    }

    // namespace_access_or_function_call
    //     = namespace_access, [ function_call_part ] { OP_DOT, namespace_access, [ function_call_part ] };
    private Expression? ParseNamespaceAccessOrFunctionCallExpression()
    {
        var expression = ParsePrimaryExpression();
        if (expression is null)
            return null;

        if (TryParseFunctionCallPart(out var arguments))
            expression = new FunctionCallExpression(expression, arguments!);

        while (TryConsumeToken(out _, TokenType.OperatorDot))
        {
            var right = ParsePrimaryExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.NamespaceAccess, right);
            if (TryParseFunctionCallPart(out arguments))
                expression = new FunctionCallExpression(expression, arguments!);
        }
        return expression;
    }

    // function_call_part
    //     = LEFT_PARENTHESIS, arguments_list, RIGHT_PARENTHESIS;
    private bool TryParseFunctionCallPart(out List<Expression>? functionArguments)
    {
        functionArguments = new List<Expression>();
        if (!TryConsumeToken(out _, TokenType.LeftParenthesis))
            return false;
        functionArguments = ParseArgumentsList();
        ConsumeToken(TokenType.RightParenthesis);
        return true;
    }

    // arguments_list
    //     = [ argument, { COMMA, argument } ];
    private List<Expression> ParseArgumentsList()
    {
        var list = new List<Expression>();
        if (!TryParseExpression(out var firstArgument))
            return list;
        list.Add(firstArgument!);
        while (TryConsumeToken(out _, TokenType.Comma))
        {
            var nextArgument = ParseExpression();
            list.Add(nextArgument);
        }
        return list;
    }

    // primary_expression
    //     = LITERAL
    //     | IDENTIFIER
    //     | LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
    private Expression? ParsePrimaryExpression()
    {
        if (TryConsumeToken(out var literal, _literalTokenTypes))
            return LiteralMapper.MapToLiteralExpression(literal);
        if (TryConsumeToken(out var identifier, TokenType.Identifier))
            return new IdentifierExpression((string)identifier.Content!);
        if (!TryConsumeToken(out _, TokenType.LeftParenthesis))
            return null;

        var expression = ParseExpression();
        ConsumeToken(TokenType.RightParenthesis);
        return expression;
    }
}
