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
        var resultStatement = (Statement?)null;
        while (TryParseStatement(out var parsedStatement))
        {
            if (resultStatement is not null)
            {
                if (!resultStatement.IsTerminated)
                    EmitError(new ExpectedSemicolon(parsedStatement!));
                statementList.Add(resultStatement);
            }
            resultStatement = parsedStatement;
            SkipSemicolons();
        }
        if (resultStatement is not null && resultStatement.IsTerminated)
        {
            statementList.Add(resultStatement);
            resultStatement = null;
        }
        Expression? resultExpression = null;
        if (resultStatement is ExpressionStatement resultExpressionStatement)
            resultExpression = resultExpressionStatement.Expression;
        else if (resultStatement is not null)
            statementList.Add(resultStatement);

        InterceptParserError(() => ConsumeToken(TokenType.RightBrace));

        return new BlockExpression(statementList, resultExpression);
    });

    // conditional_expression
    //     = conditional_if_part, { conditional_elif_part }, [ conditional_else_part ];
    private Expression? ParseConditionalExpression() => SupplyPosition(() =>
    {
        if (!TryMatchConditionalIfPart(false, out var ifPart))
            return null;

        var branchList = new List<ConditionalElement> { ifPart! };
        while (TryMatchConditionalIfPart(true, out var elifPart))
            branchList.Add(elifPart!);

        return TryMatchConditionalElsePart(out var elsePart)
            ? new ConditionalExpression(branchList, elsePart)
            : new ConditionalExpression(branchList);
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
        var consequent = ParseStatement();
        BlockExpression blockConsequent;
        if (consequent is ExpressionStatement expressionStatement)
            blockConsequent = expressionStatement.Expression as BlockExpression
                ?? new BlockExpression(new List<Statement>(), expressionStatement.Expression);
        else
            blockConsequent = new BlockExpression(new List<Statement> { consequent });
        ifPart = new ConditionalElement(condition, blockConsequent);
        return true;
    }

    // conditional_else_part
    //     = KW_ELSE, unterminated_statement;
    private bool TryMatchConditionalElsePart(out BlockExpression? elsePart)
    {
        elsePart = null;
        if (!TryConsumeToken(out _, TokenType.KeywordElse))
            return false;

        var consequent = ParseStatement();
        BlockExpression blockConsequent;
        if (consequent is ExpressionStatement expressionStatement)
            blockConsequent = expressionStatement.Expression as BlockExpression
                              ?? new BlockExpression(new List<Statement>(), expressionStatement.Expression);
        else
            blockConsequent = new BlockExpression(new List<Statement> { consequent });
        elsePart = blockConsequent;
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
        var body = ParseStatement();
        BlockExpression blockBody;
        if (body is ExpressionStatement expressionStatement)
            blockBody = expressionStatement.Expression as BlockExpression
                ?? new BlockExpression(new List<Statement>(), expressionStatement.Expression);
        else
            blockBody = new BlockExpression(new List<Statement> { body });
        // TODO: warn about ignored expression
        return new ForLoopExpression(loopRange, blockBody, counterName);
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
        var body = ParseStatement();
        BlockExpression blockBody;
        if (body is ExpressionStatement expressionStatement)
            blockBody = expressionStatement.Expression as BlockExpression
                ?? new BlockExpression(new List<Statement>(), expressionStatement.Expression);
        else
            blockBody = new BlockExpression(new List<Statement> { body });
        // TODO: warn about ignored expression
        return new WhileLoopExpression(condition, blockBody);
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

        var body = ParseStatement();
        BlockExpression blockBody;
        if (body is ExpressionStatement expressionStatement)
            blockBody = expressionStatement.Expression as BlockExpression
                        ?? new BlockExpression(new List<Statement>(), expressionStatement.Expression);
        else
            blockBody = new BlockExpression(new List<Statement> { body });

        return new FunctionDefinitionExpression(parameterList, blockBody);
    });

    // parameter_list
    //     = [ parameter, { COMMA, parameter } ];
    private List<FunctionParameter> ParseParameterList()
    {
        var list = new List<FunctionParameter>();
        if (!TryParseParameter(out var firstParameter))
            return !TryConsumeToken(out var commaToken, TokenType.Comma)
                ? list
                : throw new ParserException(new ExpectedParameter(commaToken));
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

        // TODO: come up with a less hacky solution
        var internalArgument = new IdentifierExpression("match");

        var patternSpecificationList = new List<ConditionalElement>();
        var defaultConsequent = (Expression?)null;
        while (TryParsePatternSpecification(internalArgument, out var specification))
        {
            if (defaultConsequent is not null)
                if (specification!.Condition is null)
                    EmitError(new DuplicatedDefaultPattern(specification.Consequent));
                else
                    EmitError(new BranchAfterDefaultPattern(specification.Condition));
            if (specification!.Condition is null)
                defaultConsequent = specification.Consequent;
            else
                patternSpecificationList.Add(specification);
        }
        if (defaultConsequent is null)
            EmitWarning(new DefaultBranchMissing(_lexer.CurrentToken.StartPosition));

        InterceptParserError(() => ConsumeToken(TokenType.RightBrace));
        var blockDefaultConsequent = defaultConsequent as BlockExpression;
        if (blockDefaultConsequent is null && defaultConsequent is not null)
            blockDefaultConsequent = new BlockExpression(new List<Statement>(), defaultConsequent);

        return new BlockExpression(new List<Statement>
            {
                new VariableInitializationListStatement(new List<VariableInitialization>
                {
                    new(internalArgument.Name, argument, true)
                })
            },
            new ConditionalExpression(patternSpecificationList, blockDefaultConsequent));
    });

    // pattern_specification
    //     = pattern_expression, COLON, expression, SEMICOLON;
    // default_pattern_specification
    //     = KW_DEFAULT, COLON, expression, SEMICOLON;
    private bool TryParsePatternSpecification(Expression argument, out ConditionalElement? specification)
    {
        specification = null;

        var isDefault = TryConsumeToken(out _, TokenType.KeywordDefault);
        var condition = (Expression?)null;
        if (!isDefault && (condition = ParseDisjunctionPatternExpression(argument)) is null)
            return false;

        InterceptParserError(() => ConsumeToken(TokenType.Colon));
        var consequent = ParseStatement();
        InterceptParserError(() => ConsumeToken(TokenType.Semicolon));

        BlockExpression blockConsequent;
        if (consequent is ExpressionStatement expressionStatement)
            blockConsequent = expressionStatement.Expression as BlockExpression
                              ?? new BlockExpression(new List<Statement>(), expressionStatement.Expression);
        else
            blockConsequent = new BlockExpression(new List<Statement> { consequent });
        specification = new ConditionalElement(condition, blockConsequent);
        return true;
    }

    // pattern_expression
    //     = pattern_expression_disjunction;
    // pattern_expression_disjunction
    //     = pattern_expression_conjunction, { KW_OR, pattern_expression_conjunction };
private Expression? ParseDisjunctionPatternExpression(Expression argument) => SupplyPosition(() =>
    {
        var expression = ParseConjunctionPatternExpression(argument);
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.KeywordOr))
        {
            var right = ParseConjunctionPatternExpression(argument);
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.Disjunction, right);
        }
        return expression;
    });

    // pattern_expression_conjunction
    //     = pattern_expression_non_associative, { KW_AND, pattern_expression_non_associative };
    private Expression? ParseConjunctionPatternExpression(Expression argument) => SupplyPosition(() =>
    {
        var expression = ParseNonAssociativePatternExpression(argument);
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.KeywordAnd))
        {
            var right = ParseNonAssociativePatternExpression(argument);
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            expression = new BinaryExpression(expression, Operator.Conjunction, right);
        }
        return expression;
    });

    // pattern_expression_non_associative
    //     = OP_COMPARISON, LITERAL
    //     | OP_TYPE_CHECK, TYPE
    //     | expression
    //     | LEFT_PARENTHESIS, pattern_expression_disjunction, RIGHT_PARENTHESIS;
    private Expression? ParseNonAssociativePatternExpression(Expression argument) => SupplyPosition(() =>
    {
        if (TryConsumeToken(out var comparisonOperator, OperatorMapper.PatternMatchingComparisonTokenTypes))
            if (TryConsumeToken(out var literal, LiteralMapper.LiteralTokenTypes))
                return new ComparisonExpression(argument,
                    new List<ComparisonElement>
                    {
                        new(OperatorMapper.MapPatternMatchingComparisonOperator(comparisonOperator.Type),
                            LiteralMapper.MapToLiteralExpression(literal))
                    });
            else
                throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, LiteralMapper.LiteralTokenTypes));

        if (TryConsumeToken(out _, TokenType.KeywordIs))
        {
            var isInequalityCheck = TryConsumeToken(out _, TokenType.KeywordNot);
            if (TryConsumeToken(out var type, TypeMapper.TypeTokenTypes))
                return new TypeCheckExpression(argument, TypeMapper.MapToType(type.Type), isInequalityCheck);
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, TypeMapper.TypeTokenTypes));
        }

        if (!TryConsumeToken(out _, TokenType.LeftParenthesis))
        {
            var assignmentExpression = ParseAssignmentExpression();
            return assignmentExpression is not null
                ? new FunctionCallExpression(assignmentExpression, new List<Expression> { argument })
                : null;
        }

        var patternExpression = ParseDisjunctionPatternExpression(argument);
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
            var isInequalityCheck = TryConsumeToken(out _, TokenType.KeywordNot);
            var typeToken = ConsumeToken(TypeMapper.TypeTokenTypes);
            expression = new TypeCheckExpression(expression, TypeMapper.MapToType(typeToken.Type), isInequalityCheck);
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

        var comparisonList = new List<ComparisonElement>();
        while (TryConsumeToken(out var comparisonOperator, OperatorMapper.ComparisonTokenTypes))
        {
            var right = ParseConcatenationExpression();
            if (right is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
            comparisonList.Add(new ComparisonElement(OperatorMapper.MapComparisonOperator(comparisonOperator.Type),
                right));
        }
        return comparisonList.Count > 0
            ? new ComparisonExpression(expression, comparisonList)
            : expression;
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
    //     = OP_UNARY_PREFIX, unary_prefixed
    //     | exponentiation;
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

        if (expression is LiteralExpression { Type: DataType.Integer, Value: > 9223372036854775807ul } badLiteral)
        {
            var isNegative = false;
            var errorPosition = badLiteral.StartPosition;
            if (unaryOperators.TryPeek(out var nearestOperator))
            {
                errorPosition = new UnaryExpression(OperatorMapper.MapUnaryOperator(nearestOperator.Type), expression)
                    .StartPosition;
                isNegative = nearestOperator.Type == TokenType.OperatorMinus;
            }
            if (badLiteral.Value is not 9223372036854775808ul || !isNegative)
                EmitError(new IntegerOutOfRange(errorPosition, (ulong)badLiteral.Value, isNegative));
        }

        while (unaryOperators.TryPop(out var unaryOperator))
            expression = new UnaryExpression(OperatorMapper.MapUnaryOperator(unaryOperator.Type), expression);
        return expression;
    });

    // exponentiation
    //     = namespace_access_or_function_call, { OP_CARET, exponentiation };
    private Expression? ParseExponentiationExpression() => SupplyPosition(() =>
    {
        var components = new Stack<Expression>();
        var expression = ParseFunctionCallExpression();
        if (expression is null)
            return null;

        while (TryConsumeToken(out _, TokenType.OperatorCaret))
        {
            if (expression is LiteralExpression { Type: DataType.Integer, Value: > 9223372036854775807ul } badLiteral)
                EmitError(new IntegerOutOfRange(badLiteral));
            components.Push(expression);
            expression = ParseFunctionCallExpression();
            if (expression is null)
                throw new ParserException(new ExpectedExpression(_lexer.CurrentToken));
        }

        while (components.TryPop(out var left))
            expression = new BinaryExpression(left, Operator.Exponentiation, expression);
        return expression;
    });

    // function_call
    //     = primary_expression, { function_call_part };
    private Expression? ParseFunctionCallExpression() => SupplyPosition(() =>
    {
        var expression = ParsePrimaryExpression();
        if (expression is null)
            return null;

        while (TryParseFunctionCallPart(out var arguments))
            expression = new FunctionCallExpression(expression, arguments);
        return expression;
    });

    // function_call_part
    //     = LEFT_PARENTHESIS, arguments_list, RIGHT_PARENTHESIS;
    private bool TryParseFunctionCallPart(out List<Expression> functionArguments)
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
            return !TryConsumeToken(out var commaToken, TokenType.Comma)
                ? list
                : throw new ParserException(new ExpectedExpression(commaToken));
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
    //     | IDENTIFIER, { OP_DOT, IDENTIFIER }
    //     | [ CASTING_TYPE ], LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
    private Expression? ParsePrimaryExpression() => SupplyPosition(() =>
    {
        if (TryConsumeToken(out var literal, LiteralMapper.LiteralTokenTypes))
            return LiteralMapper.MapToLiteralExpression(literal);
        if (TryConsumeToken(out var identifier, TokenType.Identifier))
        {
            var namespaceLevels = new List<string>();
            while (TryConsumeToken(out _, TokenType.OperatorDot))
            {
                namespaceLevels.Add((string)identifier.Content!);
                identifier = ConsumeToken(TokenType.Identifier);
            }
            return new IdentifierExpression(namespaceLevels, (string)identifier.Content!);
        }
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
