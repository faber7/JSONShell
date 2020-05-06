grammar Skell;

// Language rules
program : statement+ ; // start rule
statement : EOL | programExec EOL | declaration EOL | expression EOL | control ;

programExec : SYM_DOLLAR ~EOL* ;

statementBlock : LCURL statement* RCURL ;

declaration : KW_LET IDENTIFIER OP_ASSGN (expression | function);

function : KW_FUN LPAREN (functionArg (SYM_COMMA functionArg)*)? RPAREN statementBlock ;
functionArg : typeName IDENTIFIER ;

control : ifControl | forControl | returnControl ;
ifControl : ifThenControl | ifThenElseControl ;
ifThenControl : KW_IF expression KW_THEN statementBlock ;
ifThenElseControl : ifThenControl KW_ELSE (statementBlock | ifControl) ;
forControl : KW_FOR IDENTIFIER KW_IN expression statementBlock ;
returnControl : KW_RETURN expression? ;

expression : eqExpr ;
eqExpr : relExpr ((OP_NE | OP_EQ) relExpr)? ;
relExpr : addExpr ((OP_GT | OP_GE | OP_LT | OP_LE) addExpr)? ;
addExpr : mulExpr ((OP_SUB | OP_ADD) mulExpr)* ;
mulExpr : unary ((OP_DIV | OP_MUL) unary)* ;
unary : (OP_NOT | OP_SUB) unary
      | primary
      ;
primary : term
        | primary LSQR (STRING | NUMBER | IDENTIFIER) RSQR
        | fnCall
        | LPAREN expression RPAREN
        ;

// If there is no argument, the function is identified as a term instead and executed directly
fnCall : IDENTIFIER expression+ ;

term : value
     | IDENTIFIER
     ;

value : object | array | STRING | NUMBER | bool ;
bool : KW_TRUE | KW_FALSE ;
array : LSQR EOL? (term (SYM_COMMA EOL? term)*)? RSQR ;
pair : STRING SYM_COLON term ;
object : LCURL EOL? (pair (SYM_COMMA EOL? pair)*)? RCURL ;

// Datatypes
typeName : TYPE_OBJECT | TYPE_ARRAY | TYPE_NUMBER | TYPE_STRING | TYPE_BOOL ;

// Lexer Rules
EOL: '\n' ;
WS: [ \t\r]+ -> skip; // skip all the whitespace

// Keywords
KW_TRUE : 'true' ;
KW_FALSE : 'false' ;
KW_IF : 'if' ;
KW_THEN : 'then' ;
KW_ELSE : 'else' ;
KW_FOR : 'for' ;
KW_IN : 'in' ;
KW_RETURN : 'return' ;
KW_LET : 'let' ;
KW_FUN : 'fun' ;
// Typenames
TYPE_OBJECT : 'object' ;
TYPE_ARRAY : 'array' ;
TYPE_NUMBER : 'number' ;
TYPE_STRING : 'string' ;
TYPE_BOOL : 'bool' ;

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
STRING : SYM_QUOTE (ESC | SAFECODEPOINT)* SYM_QUOTE ;
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