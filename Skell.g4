grammar Skell;

// Language rules
program : statement+ ; // start rule
statement : expression | control;

statementBlock : LCURL statement* RCURL ;

control : ifControl ;
ifControl : ifThenControl | ifThenElseControl ;
ifThenControl : KW_IF expression KW_THEN statementBlock ;
ifThenElseControl : ifThenControl KW_ELSE (statementBlock | ifControl) ;

expression : eqExpr ;
eqExpr : relExpr ((OP_NE | OP_EQ) relExpr)? ;
relExpr : addExpr ((OP_GT | OP_GE | OP_LT | OP_LE) addExpr)? ;
addExpr : mulExpr ((OP_SUB | OP_ADD) mulExpr)* ;
mulExpr : unary ((OP_DIV | OP_MUL) unary)* ;
unary : (OP_NOT | OP_SUB) unary
      | primary
      ;
primary : term
        | LPAREN expression RPAREN
        | primary LSQR (STRING | NUMBER | IDENTIFIER) RSQR
        ;

term : value
     | IDENTIFIER
     ;

value : object | array | STRING | NUMBER | bool ;
bool : KW_TRUE | KW_FALSE ;
array : LSQR value (SYM_COMMA value)* RSQR
      | LSQR RSQR
      ;
pair : STRING SYM_COLON value ;
object : LCURL pair (SYM_COMMA pair)* RCURL
       | LCURL RCURL
       ;

// Datatypes
typeSpecifier : KW_OBJECT | KW_ARRAY | KW_NUMBER | KW_STRING | KW_BOOL | KW_NULL ;

// Lexer Rules
WS: [ \n\t\r]+ -> skip; // skip all the whitespace

// Keywords
KW_OBJECT : 'object' ;
KW_ARRAY : 'array' ;
KW_NUMBER : 'number' ;
KW_STRING : 'string' ;
KW_BOOL : 'bool' ;
KW_NULL : 'null' ;
KW_TRUE : 'true' ;
KW_FALSE : 'false' ;
KW_IF : 'if' ;
KW_THEN : 'then' ;
KW_ELSE : 'else' ;
KW_FOR : 'for' ;
KW_IN : 'in' ;
KW_RETURN : 'return' ;

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
INT : ZERO | NONZERO_DIGIT DIGIT* ;
FRAC : SYM_PERIOD DIGIT+ ;
EXP : ('e' | 'E') SIGN? INT? ;
HEX : [0-9a-fA-F] ;
UNICODE : 'u' HEX HEX HEX HEX ;
ESC : '\\' (["\\/bfnrt] | UNICODE) ;
SAFECODEPOINT : ~ ["\\\u0000-\u001F];