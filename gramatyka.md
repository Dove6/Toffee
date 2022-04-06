# Gramatyka 

```ebnf
program               = { statement }, EOF;
statement             = namespace_import_stmt
                      | expr_statement;
namespace_import_stmt = KW_PULL, namespace, SEMICOLON;
namespace             = IDENTIFIER, { OP_DOT, IDENTIFIER };
expr_statement        = expression, SEMICOLON;
expression            = assignment
                      | variable_init_expr
                      | block
                      | conditional_expr
                      | for_loop_expr
                      | while_loop_expr
                      | function_definition
                      | pattern_matching;
variable_init_expr    = KW_INIT, variable_init, SEMICOLON;
variable_init_list    = variable_init, { COMMA, variable_init };
variable_init         = [ KW_CONST ], IDENTIFIER, OP_ASSIGNMENT, expression;
block                 = OP_LEFT_BRACE, { expr_statement }, OP_RIGHT_BRACE;
paren_condition       = OP_LEFT_PAREN, expression, OP_RIGHT_PAREN;
conditional_expr      = KW_IF, paren_condition, expression, { cond_elif_part }, [ cond_else_part ];
cond_elif_part        = KW_ELIF, paren_condition, expression;
cond_else_part        = KW_ELSE, paren_condition, expression;
while_loop_expr       = KW_WHILE, paren_condition, expression;
for_loop_expr         = KW_FOR, OP_LEFT_PAREN, range_specification, OP_RIGHT_PAREN, expression;
range_specification   = NUMBER, [ COLON, NUMBER, [ COLON, NUMBER ] ];
function_definition   = KW_FUNCTI, OP_LEFT_PAREN, parameter_list, OP_RIGHT_PAREN, expression;
parameter_list        = [ parameter, { COMMA, parameter } ];
parameter             = IDENTIFIER;
pattern_matching      = KW_MATCH, OP_LEFT_PAREN, expression, OP_RIGHT_PAREN, OP_LEFT_BRACE, { pattern_spec }, OP_RIGHT_BRACE;
pattern_spec          = pattern_expr, COLON, expression, SEMICOLON;
pattern_expr          = ptrn_expr_disjunction
                      | KW_DEFAULT;
ptrn_expr_disjunction = ptrn_expr_conjunction, { KW_OR, ptrn_expr_conjunction };
ptrn_expr_conjunction = ptrn_expr_non_assoc, { KW_AND, ptrn_expr_non_assoc };
ptrn_expr_non_assoc   = ( OP_REL_COMPARISON | KW_IS, [ KW_NOT ] ), ptrn_expr_primary
                      | OP_LEFT_PAREN, ptrn_expr_disjunction, OP_RIGHT_PAREN;
ptrn_expr_primary     = LITERAL
                      | TYPE;
assignment            = IDENTIFIER, OP_ASSIGNMENTS, assignment
                      | null_coalescing;
null_coalescing       = disjunction, { OP_NULL_COALESCING, disjunction };
disjunction           = conjunction, { OP_LOGICAL_OR, conjunction };
conjunction           = equality_comparison, { OP_LOGICAL_AND, equality_comparison };
equality_comparison   = relational_comparison, { OP_EQ_COMPARISON, relational_comparison };
relational_comparison = concatenation, { OP_REL_COMPARISON, concatenation };
concatenation         = term, { OP_CONCATENATION, term };
term                  = factor, { OP_ADDITIVE, factor };
factor                = unary_prefixed, { OP_MULTIPLICATIVE, unary_prefixed };
unary_prefixed        = OP_UNARY_PREFIX, unary_prefixed
                      | exponentiation;
exponentiation        = suffixed_expr, { OP_EXPONENTIATION, suffixed_expr };
suffixed_expr         = primary_expr, [ function_call | namespace_access ];
function_call         = OP_LEFT_PAREN, arguments_list, OP_RIGHT_PAREN;
arguments_list        = [ argument, { COMMA, argument } ];
argument              = expression;
namespace_access      = OP_NAMESPACE, primary_expr;
primary_expr          = LITERAL
                      | IDENTIFIER
                      | OP_LEFT_PAREN, expression, OP_RIGHT_PAREN;
```

## Leksemy

```ebnf
TYPE                   = "int" | "float" | "string" | "bool";
LITERAL                = STRING | NUMBER | BOOL | NULL;
STRING                 = /"(\\\\|\\"|[\s\S])*?"/;
NUMBER                 = INTEGER | FLOAT;
INTEGER                = /0x[0-9a-fA-F]+|0c[0-7]+|0b[01]+|[0-9]+/;
FLOAT                  = /((\.[0-9]+)|[0-9]+(\.[0-9]*)?)(e[-+]?[0-9]+)?/i;
BOOL                   = KW_FALSE | KW_TRUE;
NULL                   = KW_NULL;
COMMENT                = LINE_COMMENT | MULTILINE_COMMENT;
LINE_COMMENT           = /\/\/[^\r\n\x1e]*/;
MULTILINE_COMMENT      = /\/\*[\s\S]*?\*\//;
KEYWORD                = KW_INIT | KW_CONST | KW_PULL | KW_IF | KW_ELIF | KW_ELSE | KW_WHILE | KW_FOR | KW_FUNCTI
                       | KW_MATCH | KW_AND | KW_OR | KW_IS | KW_NOT | KW_DEFAULT;
KW_INIT                = "init";
KW_CONST               = "const";
KW_PULL                = "pull"
KW_IF                  = "if";
KW_ELIF                = "elif"
KW_ELSE                = "else";
KW_WHILE               = "while";
KW_FOR                 = "for";
KW_FUNCTI              = "functi";
KW_MATCH               = "match";
KW_AND                 = "and";
KW_OR                  = "or";
KW_IS                  = "is";
KW_NOT                 = "not";
KW_DEFAULT             = "default";
COMMA                  = ",";
COLON                  = ":";
SEMICOLON              = ";";
OPERATOR               = OP_NAMESPACE | OP_PARENTHESES | OP_EXPONENTIATION | OP_UNARY_PREFIX | OP_MULTIPLICATIVE | OP_ADDITIVE
                       | OP_CONCATENATION | OP_REL_COMPARISON | OP_LOGICAL_AND | OP_LOGICAL_OR | OP_NULL_COALESCING | OP_ASSIGNMENTS;
OP_NAMESPACE           = OP_DOT | OP_QUESTION_DOT;
OP_PARENTHESES         = OP_LEFT_PAREN | OP_RIGHT_PAREN | OP_LEFT_BRACKET | OP_RIGHT_BRACKET | OP_LEFT_BRACE | OP_RIGHT_BRACE;
OP_UNARY_PREFIX        = OP_PLUS | OP_MINUS | OP_BANG;
OP_MULTIPLICATIVE      = OP_ASTERISK | OP_SLASH | OP_PERCENT;
OP_ADDITIVE            = OP_PLUS | OP_MINUS;
OP_REL_COMPARISON      = OP_LESSER | OP_LESSER_EQUAL | OP_GREATER | OP_GREATER_EQUAL;
OP_EQ_COMPARISON       = OP_EQUAL | OP_NOT_EQUAL;
OP_ASSIGNMENTS         = OP_ASSIGNMENT | OP_PLUS_ASSIGNMENT | OP_MINUS_ASSIGNMENT | OP_ASTERISK_ASSIGNMENT | OP_SLASH_ASSIGNMENT
                       | OP_PERCENT_ASSIGNMENT;
OP_DOT                 = ".";
OP_QUESTION_DOT        = "?.";
OP_LEFT_PAREN          = "(";
OP_RIGHT_PAREN         = ")";
OP_LEFT_BRACKET        = "[";
OP_RIGHT_BRACKET       = "]";
OP_LEFT_BRACE          = "{";
OP_RIGHT_BRACE         = "}";
OP_EXPONENTIATION      = "^";
OP_PLUS                = "+";
OP_MINUS               = "-";
OP_BANG                = "!";
OP_ASTERISK            = "*";
OP_SLASH               = "/";
OP_PERCENT             = "%";
OP_PLUS                = "+";
OP_MINUS               = "-";
OP_CONCATENATION       = "..";
OP_LESSER              = "<";
OP_LESSER_EQUAL        = "<=";
OP_GREATER             = ">";
OP_GREATER_EQUAL       = ">=";
OP_EQUAL               = "==";
OP_NOT_EQUAL           = "!=";
OP_LOGICAL_AND         = "&&";
OP_LOGICAL_OR          = "||";
OP_NULL_COALESCING     = "??";
OP_ASSIGNMENT          = "=";
OP_PLUS_ASSIGNMENT     = "+=";
OP_MINUS_ASSIGNMENT    = "-=";
OP_ASTERISK_ASSIGNMENT = "*=";
OP_SLASH_ASSIGNMENT    = "/=";
OP_PERCENT_ASSIGNMENT  = "%=";
IDENTIFIER             = /[\pL_][\pL_\pN]*/;
```
