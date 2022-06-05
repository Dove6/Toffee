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
    = [ KW_CONST ], IDENTIFIER, [ OP_EQUALS, expression ];
break
    = KW_BREAK;
break_if
    = KW_BREAK_IF, parenthesized_expression;
return
    = KW_RETURN, expression;
expression
    = block
    | conditional_expression
    | for_loop_expression
    | while_loop_expression
    | function_definition
    | pattern_matching;
    | assignment;
block
    = LEFT_BRACE, [ unterminated_statement, { SEMICOLON, [ unterminated_statement ] } ], RIGHT_BRACE;
conditional_expression
    = conditional_if_part, { conditional_elif_part }, [ conditional_else_part ];
conditional_if_part
    = KW_IF, parenthesized_expression, expression;
conditional_elif_part
    = KW_ELIF, parenthesized_expression, expression;
conditional_else_part
    = KW_ELSE, expression;
for_loop_expression
    = KW_FOR, for_loop_specification, expression;
for_loop_specification
    = LEFT_PARENTHESIS, [ IDENTIFIER, COMMA ], for_loop_range, RIGHT_PARENTHESIS;
for_loop_range
    = expression, [ COLON, expression, [ COLON, expression ] ];
while_loop_expression
    = KW_WHILE, parenthesized_expression, expression;
function_definition
    = KW_FUNCTI, LEFT_PARENTHESIS, parameter_list, RIGHT_PARENTHESIS, block;
parameter_list
    = [ parameter, { COMMA, parameter } ];
parameter
    = [ KW_CONST ], IDENTIFIER, [ OP_BANG ];
pattern_matching
    = KW_MATCH, parenthesized_expression, LEFT_BRACE, { pattern_specification }, [ default_pattern_specification ], RIGHT_BRACE;
parenthesized_expression
    = LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
pattern_specification
    = pattern_expression, COLON, expression, SEMICOLON;
default_pattern_specification
    = KW_DEFAULT, COLON, expression, SEMICOLON;
pattern_expression
    = pattern_expression_disjunction;
pattern_expression_disjunction
    = pattern_expression_conjunction, { KW_OR, pattern_expression_conjunction };
pattern_expression_conjunction
    = pattern_expression_non_associative, { KW_AND, pattern_expression_non_associative };
pattern_expression_non_associative
    = OP_COMPARISON, LITERAL
    | OP_TYPE_CHECK, TYPE
    | expression
    | LEFT_PARENTHESIS, pattern_expression_disjunction, RIGHT_PARENTHESIS;
assignment
    = null_coalescing, [ OP_ASSIGNMENT, expression ];
null_coalescing
    = nullsafe_pipe, { OP_QUERY_QUERY, nullsafe_pipe };
nullsafe_pipe
    = disjunction, { OP_QUERY_GREATER, disjunction };
disjunction
    = conjunction, { OP_OR_OR, conjunction };
conjunction
    = type_check, { OP_AND_AND, type_check };
type_check
    = comparison, { OP_TYPE_CHECK, TYPE };
comparison
    = concatenation, { OP_COMPARISON, concatenation };
concatenation
    = term, { OP_DOT_DOT, term };
term
    = factor, { OP_ADDITIVE, factor };
factor
    = unary_prefixed, { OP_MULTIPLICATIVE, unary_prefixed };
unary_prefixed
    = OP_UNARY_PREFIX, unary_prefixed
    | exponentiation;
exponentiation
    = namespace_access_or_function_call, { OP_CARET, exponentiation };
function_call
    = primary_expression, { function_call_part };
function_call_part
    = LEFT_PARENTHESIS, arguments_list, RIGHT_PARENTHESIS;
arguments_list
    = [ argument, { COMMA, argument } ];
argument
    = expression;
primary_expression
    = LITERAL
    | IDENTIFIER, { OP_DOT, IDENTIFIER }
    | [ CASTING_TYPE ], LEFT_PARENTHESIS, expression, RIGHT_PARENTHESIS;
```

## Leksemy

```ebnf
LITERAL
    = STRING
    | NUMBER
    | BOOL
    | KW_NULL;
NUMBER
    = INTEGER
    | FLOAT;
BOOL
    = KW_FALSE
    | KW_TRUE;
COMMENT
    = LINE_COMMENT
    | MULTILINE_COMMENT;
KEYWORD
    = TYPE
    | KW_INIT
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
    | KW_DEFAULT
    | KW_FALSE
    | KW_TRUE;
TYPE
    = CASTING_TYPE
    | KW_FUNCTION
    | KW_NULL;
CASTING_TYPE
    = KW_INT
    | KW_FLOAT
    | KW_STRING
    | KW_BOOL;
OPERATOR
    = OP_DOT
    | OP_CARET
    | OP_UNARY_PREFIX
    | OP_MULTIPLICATIVE
    | OP_DOT_DOT
    | OP_COMPARISON
    | OP_TYPE_CHECK
    | OP_AND_AND
    | OP_OR_OR
    | OP_QUERY_QUERY
    | OP_QUERY_GREATER
    | OP_ASSIGNMENT;
PARENTHESES
    = LEFT_PARENTHESIS
    | RIGHT_PARENTHESIS
    | LEFT_BRACE
    | RIGHT_BRACE;
OP_UNARY_PREFIX
    = OP_ADDITIVE
    | OP_BANG;
OP_ADDITIVE
    = OP_PLUS
    | OP_MINUS;
OP_MULTIPLICATIVE
    = OP_ASTERISK
    | OP_SLASH
    | OP_PERCENT;
OP_COMPARISON
    = OP_LESS
    | OP_LESS_EQUALS
    | OP_GREATER
    | OP_GREATER_EQUALS
    | OP_EQUALS_EQUALS
    | OP_BANG_EQUALS;
OP_TYPE_CHECK
    = KW_IS, [ KW_NOT ];
OP_ASSIGNMENT
    = OP_EQUALS
    | OP_PLUS_EQUALS
    | OP_MINUS_EQUALS
    | OP_ASTERISK_EQUALS
    | OP_SLASH_EQUALS
    | OP_PERCENT_EQUALS;
```

Definicje z użyciem wyrażeń regularnych:
```js
STRING                 = /"(\\\\|\\"|[\s\S])*?"/;
INTEGER                = /0x[0-9a-fA-F]+|0c[0-7]+|0b[01]+|[0-9]+/;
FLOAT                  = /[0-9]+\.[0-9]*(e[-+]?[0-9]+)?/i;
LINE_COMMENT           = /\/\/[^\r\n\x1e]*/;
MULTILINE_COMMENT      = /\/\*[\s\S]*?\*\//;
KW_INT                 = /int/;
KW_FLOAT               = /float/;
KW_STRING              = /string/;
KW_BOOL                = /bool/;
KW_FUNCTION            = /function/;
KW_NULL                = /null/;
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
KW_FALSE               = /false/;
KW_TRUE                = /true/;
COMMA                  = /,/;
COLON                  = /:/;
SEMICOLON              = /;/;
OP_DOT                 = /\./;
OP_QUERY_DOT           = /\?\./;
LEFT_PARENTHESIS       = /\(/;
RIGHT_PARENTHESIS      = /\)/;
LEFT_BRACKET           = /\[/;
RIGHT_BRACKET          = /\]/;
LEFT_BRACE             = /{/;
RIGHT_BRACE            = /}/;
OP_CARET               = /\^/;
OP_PLUS                = /\+/;
OP_MINUS               = /-/;
OP_BANG                = /!/;
OP_ASTERISK            = /\*/;
OP_SLASH               = /\//;
OP_PERCENT             = /%/;
OP_DOT_DOT             = /\.\./;
OP_LESS                = /</;
OP_LESS_EQUALS         = /<=/;
OP_GREATER             = />/;
OP_GREATER_EQUALS      = />=/;
OP_EQUALS_EQUALS       = /==/;
OP_BANG_EQUALS         = /!=/;
OP_AND_AND             = /&&/;
OP_OR_OR               = /\|\|/;
OP_QUERY_QUERY         = /\?\?/;
OP_QUERY_GREATER       = /\?>/;
OP_EQUALS              = /=/;
OP_PLUS_EQUALS         = /+=/;
OP_MINUS_EQUALS        = /-=/;
OP_ASTERISK_EQUALS     = /\*=/;
OP_SLASH_EQUALS        = /\/=/;
OP_PERCENT_EQUALS      = /%=/;
IDENTIFIER             = /[\pL_][\pL_\pN]*/;
```
