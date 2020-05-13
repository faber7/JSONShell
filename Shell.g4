grammar Shell;

// Language rules
program : program_statement+ ; // start rule
program_statement : namespace_declaration EOL
                  | statement
                  ;
statement : EOL
          | load_namespace EOL
          | program_shorthand EOL
          | declaration EOL
          | expression EOL
          | control
          ;

load_namespace : KW_USING STRING (KW_AS IDENTIFIER)? ;

namespace_declaration : KW_NAMESPACE IDENTIFIER LCURL EOL? namespaced_statement* RCURL ;
namespaced_statement : EOL | namespaced_declaration EOL | namespace_declaration EOL | load_namespace EOL ;
namespaced_declaration : IDENTIFIER OP_ASSGN (expression | function) ;

program_shorthand : SYM_DOLLAR ~EOL* ;

statement_block : LCURL statement* RCURL ;

declaration : KW_LET IDENTIFIER (LSQR expression RSQR)* OP_ASSGN (expression | function);

function : KW_FUN LPAREN (function_argument (SYM_COMMA function_argument)*)? RPAREN statement_block ;
function_argument : argument_type_specifier IDENTIFIER ;

control : control_if | control_for | control_return ;

control_if : if_then | if_then_else ;
if_then : KW_IF expression statement_block ;
if_then_else : if_then KW_ELSE (statement_block | control_if) ;

control_for : KW_FOR IDENTIFIER KW_IN expression statement_block ;

control_return : KW_RETURN expression? ;

expression : expression_or (KW_IS value_type_specifier)? | function_call ;
expression_or : expression_and (OP_OR expression_and)* ;
expression_and : expression_equality (OP_AND expression_equality)* ;
expression_equality : expression_relational ((OP_NE | OP_EQ) expression_relational)? ;
expression_relational : expression_addition ((OP_GT | OP_GE | OP_LT | OP_LE) expression_addition)? ;
expression_addition : expression_multiplication ((OP_SUB | OP_ADD) expression_multiplication)* ;
expression_multiplication : expression_unary ((OP_DIV | OP_MUL | OP_MOD) expression_unary)* ;
expression_unary : (OP_NOT | OP_SUB) expression_unary
                 | expression_primary
                 ;
expression_primary : term
                   | LPAREN expression RPAREN
                   ;

// If there is no argument, the function is identified as a term instead and executed directly
function_call : (identifier_namespaced | IDENTIFIER) expression* ;

term : value
     | IDENTIFIER
     | identifier_namespaced
     | term LSQR expression RSQR
     ;

identifier_namespaced : (IDENTIFIER SYM_PERIOD)+ IDENTIFIER ;

value : object | array | STRING | NUMBER | bool | KW_NULL ;
bool : KW_TRUE | KW_FALSE ;
array : LSQR EOL? (term (SYM_COMMA EOL? term)*)? RSQR ;
pair : STRING SYM_COLON term ;
object : LCURL EOL? (pair (SYM_COMMA EOL? pair)*)? RCURL ;

// Type specifiers
argument_type_specifier : value_type_specifier | TYPE_ANY ;
value_type_specifier : TYPE_OBJECT | TYPE_ARRAY | TYPE_NUMBER | TYPE_STRING | TYPE_BOOL ;

// Lexer Rules
EOL: '\n' ;
WS: [ \t\r]+ -> skip; // skip all the whitespace

// Keywords
KW_TRUE : 'true' ;
KW_FALSE : 'false' ;
KW_IF : 'if' ;
KW_ELSE : 'else' ;
KW_FOR : 'for' ;
KW_IN : 'in' ;
KW_RETURN : 'return' ;
KW_LET : 'let' ;
KW_FUN : 'fun' ;
KW_IS : 'is' ;
KW_NULL : 'null' ;
KW_NAMESPACE : 'namespace' ;
KW_USING : 'using' ;
KW_AS : 'as' ;
// Typenames
TYPE_OBJECT : 'object' ;
TYPE_ARRAY : 'array' ;
TYPE_NUMBER : 'number' ;
TYPE_STRING : 'string' ;
TYPE_BOOL : 'bool' ;
TYPE_ANY : 'any' ;

// Symbols and operators
LSQR: '[' ;
RSQR: ']' ;
LCURL: '{' ;
RCURL: '}' ;
LPAREN : '(' ;
RPAREN : ')' ;
SYM_PERIOD : '.' ;
SYM_COMMA : ',' ;
SYM_QUOTE : '"' ;
SYM_COLON : ':' ;
SYM_DOLLAR : '$' ;

OP_ASSGN : '=' ;

OP_EQ : '==' ;
OP_NE : '!=' ;
OP_LT : '<' ;
OP_LE : '<=' ;
OP_GT : '>' ;
OP_GE : '>=' ;
OP_NOT : '!' ;
OP_OR : '|' ;
OP_AND : '&' ;

OP_ADD : '+' ;
OP_SUB : '-' ;
OP_DIV : '/' ;
OP_MUL : '*' ;
OP_MOD : '%' ;

IDENTIFIER: NONDIGIT (NONDIGIT | DIGIT)* ;
STRING : SYM_QUOTE (ESC | SAFECODEPOINT | SYM_DOLLAR LCURL ~[}]* RCURL)* SYM_QUOTE ;
NUMBER : OP_SUB? INT FRAC? EXP? ;

NONDIGIT : [a-zA-Z_] ;
DIGIT : [0-9] ;
NONZERO_DIGIT : [1-9] ;
ZERO : '0' ;
SIGN : (OP_ADD | OP_SUB) ;
INT : DIGIT+ ;
FRAC : SYM_PERIOD DIGIT+ ;
EXP : ('e' | 'E') SIGN? INT? ;
HEX : [0-9a-fA-F] ;
UNICODE : 'u' HEX HEX HEX HEX ;
ESC : '\\' (["\\/bfnrt] | UNICODE) ;
SAFECODEPOINT : ~ ["\\\u0000-\u001F];