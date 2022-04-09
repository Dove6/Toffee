# Gramatyka 

```ebnf
program
    = { statement };
statement
    = unterminated_statement, SEMICOLON, { SEMICOLON };
unterminated_statement
    = namespace_import
    | variable_initialization_list
    | break
    | break_if
    | return
    | expression;
namespace_import
    = KW_PULL, namespace;
namespace
    = IDENTIFIER, { OP_DOT, IDENTIFIER };
variable_initialization_list
    = KW_INIT, variable_initialization, { COMMA, variable_initialization };
variable_initialization
    = [ KW_CONST ], IDENTIFIER, [ OP_ASSIGNMENT, expression ];
break
    = KW_BREAK;
break_if
    = KW_BREAK_IF, parenthesized_expression;
return
    = KW_RETURN, expression;
expression
    = assignment
    | block
    | conditional_expression
    | for_loop_expression
    | while_loop_expression
    | function_definition
    | pattern_matching;
block
    = OP_LEFT_BRACE, { statement }, [ unterminated_statement ], OP_RIGHT_BRACE;
conditional_expression
    = KW_IF, parenthesized_expression, unterminated_statement, { conditional_elif_part }, [ conditional_else_part ];
conditional_elif_part
    = KW_ELIF, parenthesized_expression, unterminated_statement;
conditional_else_part
    = KW_ELSE, unterminated_statement;
for_loop_expression
    = KW_FOR, for_loop_specification, unterminated_statement;
for_loop_specification
    = OP_LEFT_PARENTHESIS, [ IDENTIFIER, COMMA ], for_loop_range, OP_RIGHT_PARENTHESIS;
for_loop_range
    = NUMBER, [ COLON, NUMBER, [ COLON, NUMBER ] ];
while_loop_expression
    = KW_WHILE, parenthesized_expression, unterminated_statement;
parenthesized_expression
    = OP_LEFT_PARENTHESIS, expression, OP_RIGHT_PARENTHESIS;
function_definition
    = KW_FUNCTI, OP_LEFT_PARENTHESIS, parameter_list, OP_RIGHT_PARENTHESIS, expression;
parameter_list
    = [ parameter, { COMMA, parameter } ];
parameter
    = [ KW_CONST ], IDENTIFIER;
pattern_matching
    = KW_MATCH, OP_LEFT_PARENTHESIS, expression, OP_RIGHT_PARENTHESIS, OP_LEFT_BRACE, { pattern_specification }, OP_RIGHT_BRACE;
pattern_specification
    = pattern_expression, COLON, expression, SEMICOLON;
pattern_expression
    = pattern_expression_disjunction
    | KW_DEFAULT;
pattern_expression_disjunction
    = pattern_expression_conjunction, { KW_OR, pattern_expression_conjunction };
pattern_expression_conjunction
    = pattern_expression_non_associative, { KW_AND, pattern_expression_non_associative };
pattern_expression_non_associative
    = OP_RELATION_COMPARISON, LITERAL
    | KW_IS, [ KW_NOT ], ( TYPE | LITERAL )
    | OP_LEFT_PARENTHESIS, pattern_expression_disjunction, OP_RIGHT_PARENTHESIS;
assignment
    = null_coalescing, [ OP_ASSIGNMENTS, assignment ];
null_coalescing
    = disjunction, { OP_NULL_COALESCING, disjunction };
disjunction
    = conjunction, { OP_LOGICAL_OR, conjunction };
conjunction
    = comparison, { OP_LOGICAL_AND, comparison };
comparison
    = concatenation, { OP_COMPARISON, concatenation };
concatenation
    = term, { OP_DOUBLE_DOT, term };
term
    = factor, { OP_ADDITIVE, factor };
factor
    = unary_prefixed, { OP_MULTIPLICATIVE, unary_prefixed };
unary_prefixed
    = OP_UNARY_PREFIX, unary_prefixed
    | exponentiation;
exponentiation
    = suffixed_expression, { OP_EXPONENTIATION, suffixed_expression };
suffixed_expression
    = primary_expression, [ function_call | namespace_access ];
function_call
    = OP_LEFT_PARENTHESIS, arguments_list, OP_RIGHT_PARENTHESIS;
arguments_list
    = [ argument, { COMMA, argument } ];
argument
    = expression;
namespace_access
    = OP_NAMESPACE_ACCESS, primary_expression;
primary_expression
    = LITERAL
    | IDENTIFIER
    | OP_LEFT_PARENTHESIS, expression, OP_RIGHT_PARENTHESIS;
```

## Leksemy

```ebnf
LITERAL
    = STRING
    | NUMBER
    | BOOL
    | NULL;
NUMBER
    = INTEGER
    | FLOAT;
BOOL
    = KW_FALSE
    | KW_TRUE;
NULL
    = KW_NULL;
COMMENT
    = LINE_COMMENT
    | MULTILINE_COMMENT;
KEYWORD
    = KW_INIT
    | KW_CONST
    | KW_PULL
    | KW_IF
    | KW_ELIF
    | KW_ELSE
    | KW_WHILE
    | KW_FOR
    | KW_FUNCTI
    | KW_MATCH
    | KW_AND
    | KW_OR
    | KW_IS
    | KW_NOT
    | KW_DEFAULT;
OPERATOR
    = OP_NAMESPACE_ACCESS
    | OP_PARENTHESES
    | OP_EXPONENTIATION
    | OP_UNARY_PREFIX
    | OP_MULTIPLICATIVE
    | OP_ADDITIVE
    | OP_DOUBLE_DOT
    | OP_RELATION_COMPARISON
    | OP_LOGICAL_AND
    | OP_LOGICAL_OR
    | OP_NULL_COALESCING
    | OP_ASSIGNMENTS;
OP_NAMESPACE_ACCESS
    = OP_DOT
    | OP_QUESTION_DOT;
OP_PARENTHESES
    = OP_NAMESPACE_ACCESS
    | OP_RIGHT_PARENTHESIS
    | OP_LEFT_BRACKET
    | OP_RIGHT_BRACKET
    | OP_LEFT_BRACE
    | OP_RIGHT_BRACE;
OP_UNARY_PREFIX
    = OP_PLUS
    | OP_MINUS
    | OP_BANG;
OP_MULTIPLICATIVE
    = OP_ASTERISK
    | OP_SLASH
    | OP_PERCENT;
OP_ADDITIVE
    = OP_PLUS
    | OP_MINUS;
OP_COMPARISON
    = OP_RELATION_COMPARISON
    | OP_EQUALITY_COMPARISON;
OP_RELATION_COMPARISON
    = OP_LESSER
    | OP_LESSER_EQUAL
    | OP_GREATER
    | OP_GREATER_EQUAL;
OP_EQUALITY_COMPARISON
    = OP_EQUAL
    | OP_NOT_EQUAL;
OP_ASSIGNMENTS
    = OP_ASSIGNMENT
    | OP_PLUS_ASSIGNMENT
    | OP_MINUS_ASSIGNMENT
    | OP_ASTERISK_ASSIGNMENT
    | OP_SLASH_ASSIGNMENT
    | OP_PERCENT_ASSIGNMENT;
```

Definicje z użyciem wyrażeń regularnych:
```js
TYPE                   = /int|float|string|bool/;
STRING                 = /"(\\\\|\\"|[\s\S])*?"/;
INTEGER                = /0x[0-9a-fA-F]+|0c[0-7]+|0b[01]+|[0-9]+/;
FLOAT                  = /((\.[0-9]+)|[0-9]+\.[0-9]*)(e[-+]?[0-9]+)?/i;
LINE_COMMENT           = /\/\/[^\r\n\x1e]*/;
MULTILINE_COMMENT      = /\/\*[\s\S]*?\*\//;
KW_INIT                = /init/;
KW_CONST               = /const/;
KW_PULL                = /pull/
KW_IF                  = /if/;
KW_ELIF                = /elif/
KW_ELSE                = /else/;
KW_WHILE               = /while/;
KW_FOR                 = /for/;
KW_BREAK               = /break/;
KW_BREAK_IF            = /break_if/;
KW_FUNCTI              = /functi/;
KW_RETURN              = /return/;
KW_MATCH               = /match/;
KW_AND                 = /and/;
KW_OR                  = /or/;
KW_IS                  = /is/;
KW_NOT                 = /not/;
KW_DEFAULT             = /default/;
COMMA                  = /,/;
COLON                  = /:/;
SEMICOLON              = /;/;
OP_DOT                 = /\./;
OP_QUESTION_DOT        = /\?\./;
OP_NAMESPACE_ACCESS    = /\(/;
OP_RIGHT_PARENTHESIS   = /\)/;
OP_LEFT_BRACKET        = /\[/;
OP_RIGHT_BRACKET       = /\]/;
OP_LEFT_BRACE          = /{/;
OP_RIGHT_BRACE         = /}/;
OP_EXPONENTIATION      = /\^/;
OP_PLUS                = /\+/;
OP_MINUS               = /-/;
OP_BANG                = /!/;
OP_ASTERISK            = /\*/;
OP_SLASH               = /\//;
OP_PERCENT             = /%/;
OP_DOUBLE_DOT          = /\.\./;
OP_LESSER              = /</;
OP_LESSER_EQUAL        = /<=/;
OP_GREATER             = />/;
OP_GREATER_EQUAL       = />=/;
OP_EQUAL               = /==/;
OP_NOT_EQUAL           = /!=/;
OP_LOGICAL_AND         = /&&/;
OP_LOGICAL_OR          = /\|\|/;
OP_NULL_COALESCING     = /\?\?/;
OP_ASSIGNMENT          = /=/;
OP_PLUS_ASSIGNMENT     = /+=/;
OP_MINUS_ASSIGNMENT    = /-=/;
OP_ASTERISK_ASSIGNMENT = /\*=/;
OP_SLASH_ASSIGNMENT    = /\/=/;
OP_PERCENT_ASSIGNMENT  = /%=/;
IDENTIFIER             = /[\pL_][\pL_\pN]*/;
```
