grammar Skell;

expression
    : equality
    ;

equality
    : comparison ( ( NEQ | EQ ) comparison )*
    ;

comparison
    : addition ( ( LT | LE | GT | GE ) addition )*
    ;

addition
    : multiplication ( ( ADD | SUB ) multiplication )*
    ;

multiplication
    : unary ( ( DIV | MUL ) unary )*
    ;

unary
    : ( NOT | SUB ) unary
    | primary
    ;

primary
    : NUMBER
    | STRING
    | BOOL
    | NULL
    | LPAREN expression RPAREN
    ;

// Symbols and operators
LPAREN : '(' ;
RPAREN : ')' ;

EQ : '==' ;
NEQ : '!=' ;
LT : '<' ;
LE : '<=' ;
GT : '>' ;
GE : '>=' ;
ADD : '+' ;
SUB : '-' ;
DIV : '/' ;
MUL : '*' ;
NOT : '!' ;

// Datatypes
BOOL
    : FALSE
    | TRUE
    ;
FALSE : 'false' ;
TRUE : 'true' ;

NULL : 'null' ;

STRING : '"' ( '\\"' | . )*? '"' ;
NUMBER
    : INT
    ;

INT : [0-9]+ ;

WS: [ \n\t\r]+ -> skip; // skip all the whitespace