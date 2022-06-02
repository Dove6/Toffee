using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser
{
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
        var expressionParsers = new List<ParseExpressionDelegate>
        {
            ParseBlockExpression,
            ParseConditionalExpression,
            ParseForLoopExpression,
            ParseWhileLoopExpression,
            ParseFunctionDefinitionExpression,
            ParsePatternMatchingExpression,
            ParseAssignmentExpression
        };

        parsedExpression = expressionParsers
            .Select(parser => parser())
            .FirstOrDefault(result => result is not null);
        return parsedExpression is not null;
    }

    private Expression ParseExpression()
    {
        if (TryParseExpression(out var parsedExpression))
            return parsedExpression!;
        throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
    }

    // block
    //     = LEFT_BRACE, unterminated_statement, { SEMICOLON, [ unterminated_statement ] }, RIGHT_BRACE;
    private Expression? ParseBlockExpression() => SupplyPosition(() =>
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
            SkipSemicolons();
        }

        InterceptParserError(() => ConsumeToken(TokenType.RightBrace));

        return new BlockExpression(statementList, unterminatedStatement);
    });

    // conditional_expression
    //     = conditional_if_part, { conditional_elif_part }, [ conditional_else_part ];
    private Expression? ParseConditionalExpression() => SupplyPosition(() =>
    {
        if (!TryMatchConditionalIfPart(false, out var ifPart))
            return null;

        var elifPartList = new List<ConditionalElement>();
        while (TryMatchConditionalIfPart(true, out var elifPart))
            elifPartList.Add(elifPart!);

        return TryMatchConditionalElsePart(out var elsePart)
            ? new ConditionalExpression(ifPart!, elifPartList, elsePart)
            : new ConditionalExpression(ifPart!, elifPartList);
    });

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
    private Expression ParseParenthesizedExpression() => SupplyPosition(() =>
    {
        InterceptParserError(() => ConsumeToken(TokenType.LeftParenthesis));
        var expression = ParseExpression();
        InterceptParserError(() => ConsumeToken(TokenType.RightParenthesis));
        return expression;
    })!;

    // for_loop_expression
    //     = KW_FOR, for_loop_specification, unterminated_statement;
    private Expression? ParseForLoopExpression() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordFor))
            return null;

        var (counterName, loopRange) = ParseForLoopSpecification();
        return new ForLoopExpression(loopRange, ParseExpression(), counterName);
    });

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
        InterceptParserError(() => ConsumeToken(TokenType.LeftParenthesis));

        var firstElement = ParseExpression();
        var hasCounter = firstElement is IdentifierExpression && TryConsumeToken(out _, TokenType.Comma);

        var range = ParseForLoopRange(hasCounter ? null : firstElement);

        InterceptParserError(() => ConsumeToken(TokenType.RightParenthesis));

        return (hasCounter ? (firstElement as IdentifierExpression)!.Name : null, range);
    }

    // for_loop_range
    //     = expression, [ COLON, expression, [ COLON, expression ] ];
    private ForLoopRange ParseForLoopRange(Expression? first)
    {
        first ??= ParseExpression();
        if (!TryConsumeToken(out _, TokenType.Colon))
            return new ForLoopRange(first);

        var second = ParseExpression();
        if (!TryConsumeToken(out _, TokenType.Colon))
            return new ForLoopRange(second, first);

        var third = ParseExpression();
        return new ForLoopRange(second, first, third);
    }

    // while_loop_expression
    //     = KW_WHILE, parenthesized_expression, unterminated_statement;
    private Expression? ParseWhileLoopExpression() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordWhile))
            return null;

        var condition = ParseParenthesizedExpression();
        return new WhileLoopExpression(condition, ParseExpression());
    });

    // function_definition
    //     = KW_FUNCTI, LEFT_PARENTHESIS, parameter_list, RIGHT_PARENTHESIS, block;
    private Expression? ParseFunctionDefinitionExpression() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordFuncti))
            return null;

        InterceptParserError(() => ConsumeToken(TokenType.LeftParenthesis));
        var parameterList = ParseParameterList();
        InterceptParserError(() => ConsumeToken(TokenType.RightParenthesis));

        var body = ParseBlockExpression();
        if (body is null)
            throw new ParserException(new ExpectedBlockExpression(_lexer.CurrentToken));

        return new FunctionDefinitionExpression(parameterList, (BlockExpression)body);
    });

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
    private Expression? ParsePatternMatchingExpression() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordMatch))
            return null;

        var argument = ParseParenthesizedExpression();
        InterceptParserError(() => ConsumeToken(TokenType.LeftBrace));

        var patternSpecificationList = new List<PatternMatchingBranch>();
        var defaultConsequent = (Expression?)null;
        while (TryParsePatternSpecification(out var specification))
        {
            if (specification!.Pattern is null)
            {
                defaultConsequent = specification.Consequent;
                break;
            }
            patternSpecificationList.Add(specification);
        }

        InterceptParserError(() => ConsumeToken(TokenType.RightBrace));
        return new PatternMatchingExpression(argument, patternSpecificationList, defaultConsequent);
    });

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
private Expression? ParseDisjunctionPatternExpression() => SupplyPosition(() =>
    {
        var expression = ParseConjunctionPatternExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.KeywordOr))
        {
            var right = ParseConjunctionPatternExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.PatternMatchingDisjunction, right);
        }
        return expression;
    });

    // pattern_expression_conjunction
    //     = pattern_expression_non_associative, { KW_AND, pattern_expression_non_associative };
    private Expression? ParseConjunctionPatternExpression() => SupplyPosition(() =>
    {
        var expression = ParseNonAssociativePatternExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.KeywordAnd))
        {
            var right = ParseNonAssociativePatternExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.PatternMatchingConjunction, right);
        }
        return expression;
    });

    // pattern_expression_non_associative
    //     = OP_COMPARISON, LITERAL
    //     | OP_TYPE_CHECK, TYPE
    //     | expression
    //     | LEFT_PARENTHESIS, pattern_expression_disjunction, RIGHT_PARENTHESIS;
    private Expression? ParseNonAssociativePatternExpression() => SupplyPosition(() =>
    {
        if (TryConsumeToken(out var comparisonOperator, OperatorMapper.PatternMatchingComparisonTokenTypes))
            if (TryConsumeToken(out var literal, LiteralMapper.LiteralTokenTypes))
                return new UnaryExpression(OperatorMapper.MapPatternMatchingComparisonOperator(comparisonOperator.Type),
                    LiteralMapper.MapToLiteralExpression(literal));
            else
                throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, LiteralMapper.LiteralTokenTypes));

        if (TryConsumeToken(out _, TokenType.KeywordIs))
        {
            var typeCheckOperator = TryConsumeToken(out _, TokenType.KeywordNot)
                ? Operator.PatternMatchingNotEqualTypeCheck
                : Operator.PatternMatchingEqualTypeCheck;
            if (TryConsumeToken(out var type, TypeMapper.TypeTokenTypes))
                return new UnaryExpression(typeCheckOperator, new TypeExpression(TypeMapper.MapToType(type.Type)));
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, TypeMapper.TypeTokenTypes));
        }

        if (!TryConsumeToken(out _, TokenType.LeftParenthesis))
            return ParseAssignmentExpression();

        var patternExpression = ParseDisjunctionPatternExpression();
        if (patternExpression is null)
            throw new ParserException(new ExpectedPatternExpression(_lexer.CurrentToken));
        InterceptParserError(() => ConsumeToken(TokenType.RightParenthesis));
        return new GroupingExpression(patternExpression);
    });

    // assignment
    //     = null_coalescing, [ OP_ASSIGNMENT, expression ];
    private Expression? ParseAssignmentExpression() => SupplyPosition(() =>
    {
        var left = ParseNullCoalescingExpression();
        if (left is null)
            return null;

        return TryConsumeToken(out var @operator, OperatorMapper.AssignmentTokenTypes)
            ? new BinaryExpression(left, OperatorMapper.MapAssignmentOperator(@operator.Type), ParseExpression())
            : left;
    });

    // null_coalescing
    //     = nullsafe_pipe, { OP_QUERY_QUERY, nullsafe_pipe };
    private Expression? ParseNullCoalescingExpression() => SupplyPosition(() =>
    {
        var expression = ParseNullSafePipeExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.OperatorQueryQuery))
        {
            var right = ParseNullSafePipeExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.NullCoalescing, right);
        }
        return expression;
    });

    // nullsafe_pipe
    //     = disjunction, { OP_QUERY_GREATER, disjunction };
    private Expression? ParseNullSafePipeExpression() => SupplyPosition(() =>
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
    });

    // disjunction
    //     = conjunction, { OP_OR_OR, conjunction };
    private Expression? ParseDisjunctionExpression() => SupplyPosition(() =>
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
    });

    // conjunction
    //     = type_check, { OP_AND_AND, type_check };
    private Expression? ParseConjunctionExpression() => SupplyPosition(() =>
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
    });

    // type_check
    //     = comparison, { OP_TYPE_CHECK, TYPE };
    private Expression? ParseTypeCheckExpression() => SupplyPosition(() =>
    {
        var expression = ParseComparisonExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.KeywordIs))
        {
            var typeCheckOperator = TryConsumeToken(out _, TokenType.KeywordNot)
                ? Operator.NotEqualTypeCheck
                : Operator.EqualTypeCheck;
            var right = new TypeExpression(TypeMapper.MapToType(ConsumeToken(TypeMapper.TypeTokenTypes).Type));
            expression = new BinaryExpression(expression, typeCheckOperator, right);
        }
        return expression;
    });

    // comparison
    //     = concatenation, { OP_COMPARISON, concatenation };
    private Expression? ParseComparisonExpression() => SupplyPosition(() =>
    {
        var expression = ParseConcatenationExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out var comparisonOperator, OperatorMapper.ComparisonTokenTypes))
        {
            var right = ParseConcatenationExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression,
                OperatorMapper.MapComparisonOperator(comparisonOperator.Type),
                right);
        }
        return expression;
    });

    // concatenation
    //     = term, { OP_DOT_DOT, term };
    private Expression? ParseConcatenationExpression() => SupplyPosition(() =>
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
    });

    // term
    //     = factor, { OP_ADDITIVE, factor };
    private Expression? ParseTermExpression() => SupplyPosition(() =>
    {
        var expression = ParseFactorExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out var additiveOperator, OperatorMapper.AdditiveTokenTypes))
        {
            var right = ParseFactorExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression,
                OperatorMapper.MapAdditiveOperator(additiveOperator.Type),
                right);
        }
        return expression;
    });

    // factor
    //     = unary_prefixed, { OP_MULTIPLICATIVE, unary_prefixed };
    private Expression? ParseFactorExpression() => SupplyPosition(() =>
    {
        var expression = ParseUnaryPrefixedExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out var multiplicativeOperator, OperatorMapper.MultiplicativeTokenTypes))
        {
            var right = ParseUnaryPrefixedExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression,
                OperatorMapper.MapMultiplicativeOperator(multiplicativeOperator.Type),
                right);
        }
        return expression;
    });

    // unary_prefixed
    //     = { OP_UNARY_PREFIX }, exponentiation;
    private Expression? ParseUnaryPrefixedExpression() => SupplyPosition(() =>
    {
        var unaryOperators = new Stack<Token>();
        while (TryConsumeToken(out var unaryOperator, OperatorMapper.UnaryTokenTypes))
            unaryOperators.Push(unaryOperator);

        var expression = ParseExponentiationExpression();
        if (expression is null)
        {
            if (unaryOperators.Count == 0)
                return null;
            throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
        }

        while (unaryOperators.TryPop(out var unaryOperator))
            expression = new UnaryExpression(OperatorMapper.MapUnaryOperator(unaryOperator.Type), expression);
        return expression;
    });

    // exponentiation
    //     = namespace_access_or_function_call, { OP_CARET, exponentiation };
    private Expression? ParseExponentiationExpression() => SupplyPosition(() =>
    {
        var expression = ParseNamespaceAccessOrFunctionCallExpression();
        if (expression is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorCaret))
            return expression;
        var right = ParseExponentiationExpression();
        if (right is null)
            throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
        return new BinaryExpression(expression, Operator.Exponentiation, right);
    });

    // namespace_access_or_function_call
    //     = primary_expression, { function_call_part } { OP_DOT, primary_expression, { function_call_part } };
    private Expression? ParseNamespaceAccessOrFunctionCallExpression() => SupplyPosition(() =>
    {
        var expression = ParsePrimaryExpression();
        if (expression is null)
            return null;

        while (TryParseFunctionCallPart(out var arguments))
            expression = new FunctionCallExpression(expression, arguments!);

        while (TryConsumeToken(out _, TokenType.OperatorDot))
        {
            var right = ParsePrimaryExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.NamespaceAccess, right);
            while (TryParseFunctionCallPart(out var arguments))
                expression = new FunctionCallExpression(expression, arguments!);
        }
        return expression;
    });

    // function_call_part
    //     = LEFT_PARENTHESIS, arguments_list, RIGHT_PARENTHESIS;
    private bool TryParseFunctionCallPart(out List<Expression>? functionArguments)
    {
        functionArguments = new List<Expression>();
        if (!TryConsumeToken(out _, TokenType.LeftParenthesis))
            return false;
        functionArguments = ParseArgumentsList();
        InterceptParserError(() => ConsumeToken(TokenType.RightParenthesis));
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
    //     | [ CASTING_TYPE ], LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
    private Expression? ParsePrimaryExpression() => SupplyPosition(() =>
    {
        if (TryConsumeToken(out var literal, LiteralMapper.LiteralTokenTypes))
            return LiteralMapper.MapToLiteralExpression(literal);
        if (TryConsumeToken(out var identifier, TokenType.Identifier))
            return new IdentifierExpression((string)identifier.Content!);
        if (TryConsumeToken(out var castingType, TypeMapper.CastingTypeTokenTypes))
            return new TypeCastExpression(TypeMapper.MapToCastingType(castingType.Type), ParseParenthesizedExpression());
        if (!TryConsumeToken(out _, TokenType.LeftParenthesis))
            return null;

        var expression = ParseExpression();
        InterceptParserError(() => ConsumeToken(TokenType.RightParenthesis));
        return new GroupingExpression(expression);
    });

    private Expression? SupplyPosition(Func<Expression?> parseMethod)
    {
        var expressionPosition = _lexer.CurrentToken.StartPosition;
        var expression = parseMethod();
        return expression is null
            ? null
            : expression with { StartPosition = expressionPosition, EndPosition = _lastTokenEndPosition };
    }
}
