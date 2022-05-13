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

        TryMatchConditionalElsePart(out var elsePart);

        return new ConditionalExpression(ifPart!, elifPartList, elsePart);
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
        if (!TryParseStatement(out var consequent))
            throw new NotImplementedException();
        ifPart = new ConditionalElement(condition, consequent!);
        return true;
    }

    // conditional_else_part
    //     = KW_ELSE, unterminated_statement;
    private bool TryMatchConditionalElsePart(out Statement? elsePart)
    {
        elsePart = null;
        if (!TryConsumeToken(out _, TokenType.KeywordElse))
            return false;

        if (!TryParseStatement(out elsePart))
            throw new NotImplementedException();
        return true;
    }

    // parenthesized_expression
    //     = LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
    private Expression ParseParenthesizedExpression()
    {
        ConsumeToken(TokenType.LeftBrace);

        if (!TryParseExpression(out var expression))
            throw new NotImplementedException();

        ConsumeToken(TokenType.RightBrace);

        return expression!;
    }

    // for_loop_expression
    //     = KW_FOR, for_loop_specification, unterminated_statement;
    private Expression? ParseForLoopExpression()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordFor))
            return null;

        var (counterName, loopRange) = ParseForLoopSpecification();

        if (!TryParseStatement(out var body))
            throw new NotImplementedException();

        return new ForLoopExpression(loopRange, body!, counterName);
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
        if (!TryParseExpression(out var first))
            throw new NotImplementedException();

        if (!TryConsumeToken(out _, TokenType.Comma))
            return new ForLoopRange(first!);
        if (!TryParseExpression(out var second))
            throw new NotImplementedException();

        if (!TryConsumeToken(out _, TokenType.Comma))
            return new ForLoopRange(second!, first!);
        if (!TryParseExpression(out var third))
            throw new NotImplementedException();

        return new ForLoopRange(second!, first!, third!);
    }

    // while_loop_expression
    //     = KW_WHILE, parenthesized_expression, unterminated_statement;
    private Expression? ParseWhileLoopExpression()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordWhile))
            return null;

        var condition = ParseParenthesizedExpression();

        if (!TryParseStatement(out var body))
            throw new NotImplementedException();

        return new WhileLoopExpression(condition, body!);
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
            throw new NotImplementedException();

        return new FunctionDefinitionExpression(parameterList, (BlockExpression)body);
    }

    // parameter_list
    //     = [ parameter, { COMMA, parameter } ];
    private List<FunctionParameter> ParseParameterList()
    {
        var list = new List<FunctionParameter>();
        while (TryParseParameter(out var parameter))
            list.Add(parameter!);
        return list;
    }

    // parameter
    //     = [ KW_CONST ], IDENTIFIER, [ OP_BANG ];
    private bool TryParseParameter(out FunctionParameter? parameter)
    {
        parameter = null;
        var isConst = TryConsumeToken(out _, TokenType.KeywordConst);
        if (!TryConsumeToken(out var identifier, TokenType.Identifier))
            return isConst ? false : throw new NotImplementedException();
        var isNullable = !TryConsumeToken(out _, TokenType.OperatorBang);

        parameter = new FunctionParameter((string)identifier.Content!, isConst, isNullable);
        return true;
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
        if (!TryParseExpression(out var consequent))
            throw new NotImplementedException();
        ConsumeToken(TokenType.Semicolon);

        specification = new PatternMatchingBranch(condition, consequent!);
        return true;
    }

    // pattern_expression
    //     = pattern_expression_disjunction;
    // pattern_expression_disjunction
    //     = pattern_expression_conjunction, { KW_OR, pattern_expression_conjunction };
    private Expression? ParseDisjunctionPatternExpression()
    {
        var left = ParseConjunctionPatternExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.KeywordOr))
            return left;
        var right = ParseConjunctionExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Disjunction, right);
    }

    // pattern_expression_conjunction
    //     = pattern_expression_non_associative, { KW_AND, pattern_expression_non_associative };
    private Expression? ParseConjunctionPatternExpression()
    {
        var left = ParseNonAssociativePatternExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.KeywordOr))
            return left;
        var right = ParseNonAssociativePatternExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Conjunction, right);
    }

    // pattern_expression_non_associative
    //     = OP_COMPARISON, LITERAL
    //     | OP_TYPE_CHECK, TYPE
    //     | expression
    //     | LEFT_PARENTHESIS, pattern_expression_disjunction, RIGHT_PARENTHESIS;
    private Expression? ParseNonAssociativePatternExpression()
    {
        if (TryConsumeToken(out var comparisonOperator,
                TokenType.OperatorLess,
                TokenType.OperatorLessEquals,
                TokenType.OperatorGreater,
                TokenType.OperatorGreaterEquals,
                TokenType.OperatorEqualsEquals,
                TokenType.OperatorBangEquals))
            if (TryConsumeToken(out var literal,
                    TokenType.LiteralFloat,
                    TokenType.LiteralInteger,
                    TokenType.LiteralString,
                    TokenType.KeywordTrue,
                    TokenType.KeywordFalse,
                    TokenType.KeywordNull))
                return new UnaryExpression(new LiteralExpression(literal.Content!),
                    Operator.EqualComparison); // TODO: literal and operator mapping
            else
                throw new NotImplementedException();

        if (TryConsumeToken(out _, TokenType.KeywordIs))
        {
            var typeCheckOperator = TryConsumeToken(out _, TokenType.KeywordNot)
                ? Operator.NotEqualTypeCheck
                : Operator.EqualTypeCheck;
            if (TryConsumeToken(out var type,
                    TokenType.KeywordFloat,
                    TokenType.KeywordInt,
                    TokenType.KeywordBool,
                    TokenType.KeywordString,
                    TokenType.KeywordNull))
                return new UnaryExpression(new LiteralExpression(type.Content!), typeCheckOperator);  // TODO: TypeExpression
            throw new NotImplementedException();
        }

        if (TryConsumeToken(out _, TokenType.LeftParenthesis))
        {
            var patternExpression = ParsePatternMatchingExpression();
            if (patternExpression is null)
                throw new NotImplementedException();
            ConsumeToken(TokenType.RightParenthesis);
            return patternExpression;
        }

        return ParseAssignmentExpression();
    }

    // assignment
    //     = null_coalescing, [ OP_ASSIGNMENT, assignment ];
    private Expression? ParseAssignmentExpression()  // TODO: introduce indirect ParseExpression
    {
        var left = ParseNullCoalescingExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out var @operator,
                TokenType.OperatorEquals,
                TokenType.OperatorPlusEquals,
                TokenType.OperatorMinusEquals,
                TokenType.OperatorAsteriskEquals,
                TokenType.OperatorSlashEquals,
                TokenType.OperatorPercentEquals))
            return left;
        var right = ParseNullCoalescingExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Assignment, right);  // TODO: operator mapping
    }

    // null_coalescing
    //     = nullsafe_pipe, { OP_QUERY_QUERY, nullsafe_pipe };
    private Expression? ParseNullCoalescingExpression()
    {
        var left = ParseNullsafePipeExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorQueryQuery))
            return left;
        var right = ParseNullsafePipeExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.NullCoalescing, right);
    }

    // nullsafe_pipe
    //     = disjunction, { OP_QUERY_GREATER, disjunction };
    private Expression? ParseNullsafePipeExpression()
    {
        var left = ParseDisjunctionExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorQueryGreater))
            return left;
        var right = ParseDisjunctionExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.NullSafePipe, right);
    }

    // disjunction
    //     = conjunction, { OP_OR_OR, conjunction };
    private Expression? ParseDisjunctionExpression()
    {
        var left = ParseConjunctionExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorOrOr))
            return left;
        var right = ParseConjunctionExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Disjunction, right);
    }

    // conjunction
    //     = type_check, { OP_AND_AND, type_check };
    private Expression? ParseConjunctionExpression()
    {
        var left = ParseTypeCheckExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorAndAnd))
            return left;
        var right = ParseTypeCheckExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Conjunction, right);
    }

    // type_check
    //     = comparison, { OP_TYPE_CHECK, TYPE };
    private Expression? ParseTypeCheckExpression()
    {
        var left = ParseComparisonExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.KeywordIs))
            return left;
        var typeCheckOperator = TryConsumeToken(out _, TokenType.KeywordNot)
            ? Operator.NotEqualTypeCheck
            : Operator.EqualTypeCheck;
        var right = ParsePrimaryExpression();  // TODO: TypeExpression
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, typeCheckOperator, right);
    }

    // comparison
    //     = concatenation, { OP_COMPARISON, concatenation };
    private Expression? ParseComparisonExpression()
    {
        var left = ParseConcatenationExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _,
                TokenType.OperatorLess,
                TokenType.OperatorLessEquals,
                TokenType.OperatorGreater,
                TokenType.OperatorGreaterEquals,
                TokenType.OperatorEqualsEquals,
                TokenType.OperatorBangEquals))  // TODO: extract common token groups
            return left;
        var right = ParseConcatenationExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.EqualComparison, right);  // TODO: operator mapping
    }

    // concatenation
    //     = term, { OP_DOT_DOT, term };
    private Expression? ParseConcatenationExpression()
    {
        var left = ParseTermExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorDotDot))
            return left;
        var right = ParseTermExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Concatenation, right);
    }

    // term
    //     = factor, { OP_ADDITIVE, factor };
    private Expression? ParseTermExpression()
    {
        var left = ParseFactorExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorMinus, TokenType.OperatorPlus))  // TODO: extract common token groups
            return left;
        var right = ParseFactorExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Addition, right);  // TODO: operator mapping
    }

    // factor
    //     = unary_prefixed, { OP_MULTIPLICATIVE, unary_prefixed };
    private Expression? ParseFactorExpression()
    {
        var left = ParseUnaryPrefixedExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorAsterisk, TokenType.OperatorSlash, TokenType.OperatorPercent))  // TODO: extract common token groups
            return left;
        var right = ParseUnaryPrefixedExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Multiplication, right);  // TODO: operator mapping
    }

    // unary_prefixed
    //     = OP_UNARY_PREFIX, unary_prefixed
    //     | exponentiation;
    private Expression? ParseUnaryPrefixedExpression()
    {
        if (!TryConsumeToken(out _, TokenType.OperatorPlus, TokenType.OperatorMinus, TokenType.OperatorBang))  // TODO: extract common token groups
            return ParseExponentiationExpression();
        var expression = ParseUnaryPrefixedExpression();
        if (expression is null)
            throw new NotImplementedException();

        return new UnaryExpression(expression, Operator.NumberPromotion);  // TODO: operator mapping
    }

    // exponentiation
    //     = suffixed_expression, { OP_CARET, suffixed_expression };
    private Expression? ParseExponentiationExpression()
    {
        var left = ParseSuffixedExpressionExpression();
        if (left is null)
            return null;

        if (!TryConsumeToken(out _, TokenType.OperatorCaret))
            return left;
        var right = ParseSuffixedExpressionExpression();
        if (right is null)
            throw new NotImplementedException();

        return new BinaryExpression(left, Operator.Exponentiation, right);
    }

    // suffixed_expression
    //     = primary_expression, [ function_call | namespace_access ];
    private Expression? ParseSuffixedExpressionExpression()
    {
        var left = ParsePrimaryExpression();
        if (left is null)
            return null;

        if (TryParseFunctionCall(out var arguments))
            return new FunctionCallExpression(left, arguments!);
        if (TryParseNamespaceAccessExpression(out var right, out var @operator))
            return new BinaryExpression(left, @operator!.Value, right!);
        return left;
    }

    // function_call
    //     = LEFT_PARENTHESIS, arguments_list, RIGHT_PARENTHESIS;
    private bool TryParseFunctionCall(out List<Expression>? functionArguments)
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
            if (!TryParseExpression(out var nextArgument))
                throw new NotImplementedException();
            list.Add(nextArgument!);
        }
        return list;
    }

    // namespace_access
    //     = OP_NAMESPACE_ACCESS, primary_expression;
    private bool TryParseNamespaceAccessExpression(out Expression? parsedExpression, out Operator? @operator)
    {
        parsedExpression = null;
        @operator = Operator.NamespaceAccess;  // TODO: operator mapping
        if (!TryConsumeToken(out _, TokenType.OperatorDot, TokenType.OperatorQueryDot))
            return false;

        parsedExpression = ParsePrimaryExpression();
        if (parsedExpression is null)
            throw new NotImplementedException();
        return true;
    }

    // primary_expression
    //     = LITERAL
    //     | IDENTIFIER
    //     | LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
    private Expression? ParsePrimaryExpression()
    {
        if (TryConsumeToken(out var literal,
                TokenType.LiteralFloat,
                TokenType.LiteralInteger,
                TokenType.LiteralString,
                TokenType.KeywordTrue,
                TokenType.KeywordFalse,
                TokenType.KeywordNull)) // TODO: extract
            return new LiteralExpression(literal.Content!);  // TODO: mapping
        if (TryConsumeToken(out var identifier, TokenType.Identifier))
            return new LiteralExpression(identifier.Content!);
        if (TryConsumeToken(out _, TokenType.LeftParenthesis))
        {
            if (!TryParseExpression(out var expression))
                throw new NotImplementedException();
            ConsumeToken(TokenType.RightParenthesis);
            return expression;
        }
        return null;
    }
}
