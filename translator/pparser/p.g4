grammar p;

program
    : EOF
    | top_decl_list
    | annotation_set                
    | annotation_set top_decl_list    
    ;

top_decl_list
    : top_decl
    | top_decl_list top_decl 
    ;

top_decl
    : include_decl
    | type_def_decl
    | event_decl
    | machine_decl
    | fun_decl
    ;

/******************* Annotations *******************/ 
annotation_set
    : LBRACKET RBRACKET                  
    | LBRACKET annotation_list RBRACKET   
    ;

annotation_list
    : annotation
    | annotation_list COMMA annotation
    ;

annotation
    : ID ASSIGN NULL    
    | ID ASSIGN TRUE    
    | ID ASSIGN FALSE   
    | ID ASSIGN ID      
    | ID ASSIGN INT     
    ;

/******************* ptype Declarations **********************/
type_def_decl
    : TYPE ID ASSIGN ptype SEMICOLON        
    | MODEL TYPE ID ASSIGN ptype SEMICOLON   
    ;

/******************* Include Declarations *******************/ 
include_decl
    : INCLUDE STR 
    ;

/******************* Event Declarations *******************/ 
event_decl
    : EVENT ID ev_card_or_none ev_type_or_none event_annot_or_none SEMICOLON 
    ;

ev_card_or_none
    : ASSERT INT                           
    | ASSUME INT                           
    |                                   
    ;

ev_type_or_none
    : COLON ptype                           
    |                                   
    ;

event_annot_or_none
    : annotation_set                                 
    |
    ;

/******************* Machine Declarations *******************/
machine_decl
    : machine_name_decl mach_annot_or_none LCBRACE machine_body RCBRACE  
    ;

machine_name_decl
    : is_main MACHINE ID  mach_card_or_none  # machine_name_decl_regular
    | MODEL ID mach_card_or_none             # machine_name_decl_model
    | SPEC ID observes_list                  # machine_name_decl_spec
    ;
   
observes_list
    : OBSERVES event_list 
    ;

is_main
    : MAIN                                 
    |                                   
    ;

mach_card_or_none
    : ASSERT INT                           
    | ASSUME INT                           
    |                                   
    ;

mach_annot_or_none
    : annotation_set                                 
    |
    ;

/******************* Machine Bodies *******************/
machine_body
    : machine_body_item                                  
    | machine_body machine_body_item                
    ;

machine_body_item
    : var_decl
    | fun_decl
    | state_decl
    | group
    ;

/******************* Variable Declarations *******************/
var_decl
    : VAR var_list COLON ptype SEMICOLON               
    | VAR var_list COLON ptype annotation_set SEMICOLON 
    ;

var_list
    : ID                                             
    | ID COMMA var_list    
    ;

local_var_decl
    : VAR local_var_list COLON ptype SEMICOLON            
    ; 

local_var_decl_list
    : local_var_decl local_var_decl_list
    |
    ; 

local_var_list
    : ID                                          
    | local_var_list COMMA ID    
    ;

payload_var_decl_or_none
    : LPAREN ID COLON ptype RPAREN 
    |                             
    ;

payload_var_decl_or_none_ref
    : LPAREN ID COLON ptype RPAREN 
    |                             
    ;

payload_none
    :                             
    ;

/******************* Function Declarations *******************/
fun_decl
    : is_model fun_name_decl params_or_none ret_type_or_none fun_annot_or_none LCBRACE stmt_block RCBRACE 
    ;

fun_name_decl
    : FUN ID 
    ;

is_model
    : MODEL                                
    |                                   
    ;

fun_annot_or_none
    : annotation_set 
    |
    ;

params_or_none
    : LPAREN RPAREN
    | LPAREN nmd_tup_type_list RPAREN                  
    ;

ret_type_or_none
    : COLON ptype                                    
    | 
    ;

/*******************       group        *******************/
group
    : group_name LCBRACE RCBRACE                   
    | group_name LCBRACE group_body RCBRACE   
    ;

group_body
    : group_item
    | group_body group_item 
    ;

group_item
    : state_decl
    | group
    ;

group_name
    : GROUP ID 
    ;

/******************* State Declarations *******************/
state_decl
    : is_start_state_or_none is_hot_or_cold_or_none STATE ID state_annot_or_none LCBRACE RCBRACE                  
    | is_start_state_or_none is_hot_or_cold_or_none STATE ID state_annot_or_none LCBRACE state_body RCBRACE   
    ;

is_start_state_or_none
    : START
    |
    ;

is_hot_or_cold_or_none
    : HOT        
    | COLD       
    |
    ;

state_annot_or_none
    : annotation_set 
    |
    ;

state_body
    : state_body_item
    | state_body_item state_body 
    ;

state_body_item
    : ENTRY payload_var_decl_or_none LCBRACE stmt_block RCBRACE                                                         # state_body_item_entry_unnamed
    | ENTRY ID SEMICOLON                                                                                                # state_body_item_entry_fn_named
    | EXIT payload_none LCBRACE stmt_block RCBRACE                                                                      # state_body_item_exit_unnamed
    | EXIT ID SEMICOLON                                                                                                 # state_body_item_exit_fn_named
    | DEFER non_default_event_list trig_annot_or_none SEMICOLON                                                         # state_body_item_defer
    | IGNORE non_default_event_list trig_annot_or_none SEMICOLON                                                        # state_body_item_ignore
    | on_event_list DO ID trig_annot_or_none SEMICOLON                                                                  # state_body_item_on_e_do_fn_named
    | on_event_list DO trig_annot_or_none payload_var_decl_or_none LCBRACE stmt_block RCBRACE                           # state_body_item_on_e_do_unamed
    | on_event_list PUSH state_target trig_annot_or_none SEMICOLON                                                      # state_body_item_push
    | on_event_list GOTO state_target trig_annot_or_none SEMICOLON                                                      # state_body_item_on_e_goto
    | on_event_list GOTO state_target trig_annot_or_none WITH payload_var_decl_or_none_ref LCBRACE stmt_block RCBRACE   # state_body_item_on_e_goto_with_unnamed
    | on_event_list GOTO state_target trig_annot_or_none WITH ID SEMICOLON                                              # state_body_item_on_e_goto_with_fn_named
    ;

on_event_list
    : ON event_list          
    ;

non_default_event_list
    : non_default_event_id
    | non_default_event_list COMMA non_default_event_id 
    ;

event_list
    : event_id
    | event_list COMMA event_id
    ;

event_id
    : ID        
    | HALT      
    | NULL      
    ;

non_default_event_id
    : ID        
    | HALT      
    ;

trig_annot_or_none
    : annotation_set  
    |
    ;

/******************* ptype Expressions *******************/

ptype
    : NULL                                          # ptype_null                       
    | BOOL                                          # ptype_bool
    | INT_TYPE                                      # ptype_int_type
    | EVENT                                         # ptype_event
    | MACHINE                                       # ptype_machine
    | ANY                                           # ptype_any
    | ID                                            # ptype_typedef
    | SEQ LBRACKET ptype RBRACKET                   # ptype_seq
    | MAP LBRACKET ptype COMMA ptype RBRACKET       # ptype_map
    | LPAREN tup_type_list RPAREN                   # ptype_tuple
    | LPAREN nmd_tup_type_list RPAREN               # ptype_named_tuple
    ;

tup_type_list
    : ptype                  
    | ptype COMMA tup_type_list   
    ;

qualifier_or_none
    : REF    
    | XFER      
    |        
    ;

nmd_tup_type_list
    : ID qualifier_or_none COLON ptype                         
    | ID qualifier_or_none COLON ptype COMMA nmd_tup_type_list    
    ;

/******************* Statements *******************/

stmt
    : SEMICOLON                                                                     # stmt_semicolon
    | LCBRACE RCBRACE                                                               # stmt_rbrace
    | POP SEMICOLON                                                                 # stmt_pop
    | LCBRACE stmt_list RCBRACE                                                     # stmt_stmt_list
    | ASSERT exp SEMICOLON                                                          # stmt_assert
    | ASSERT exp COMMA STR SEMICOLON                                                # stmt_assert_str
    | PRINT STR SEMICOLON                                                           # stmt_print
    | RETURN SEMICOLON                                                              # stmt_return
    | RETURN exp SEMICOLON                                                          # stmt_return_exp
    | exp ASSIGN exp SEMICOLON                                                      # stmt_assign
    | exp REMOVE exp SEMICOLON                                                      # stmt_remove
    | exp INSERT exp SEMICOLON                                                      # stmt_insert
    | WHILE LPAREN exp RPAREN stmt                                                  # stmt_while
    | IF LPAREN exp RPAREN stmt ELSE stmt                                           # stmt_if_then_else
    | IF LPAREN exp RPAREN stmt                                                     # stmt_if_then
    | NEW ID LPAREN RPAREN SEMICOLON                                                # stmt_new
    | NEW ID LPAREN single_expr_arg_list RPAREN SEMICOLON                           # stmt_new_with_arguments
    | ID LPAREN RPAREN SEMICOLON                                                    # stmt_call
    | ID LPAREN expr_arg_list RPAREN SEMICOLON                                      # stmt_call_with_arguments
    | RAISE exp SEMICOLON                                                           # stmt_raise
    | RAISE exp COMMA single_expr_arg_list SEMICOLON                                # stmt_raise_with_arguments
    | qualifier_or_none SEND exp COMMA exp SEMICOLON                                # stmt_send
    | qualifier_or_none SEND exp COMMA exp COMMA single_expr_arg_list SEMICOLON     # stmt_send_with_arguments
    | ANNOUNCE exp SEMICOLON                                                         # stmt_announce
    | ANNOUNCE exp COMMA single_expr_arg_list SEMICOLON                              # stmt_announce_with_arguments
    | receive_stmt LCBRACE case_list RCBRACE                                        # stmt_recieve
    ;

receive_stmt
    : RECEIVE                                      
    ;

pcase 
    : case_event_list payload_var_decl_or_none LCBRACE stmt_block RCBRACE       
    ;

case_event_list
    : CASE event_list COLON
    ;

case_list
    : pcase   
    | case_list pcase 
    ;
    
stmt_block
    : local_var_decl_list                                             
    | local_var_decl_list stmt_list
    ;

stmt_list
    : stmt
    | stmt stmt_list                                           
    ;

state_target
    : ID                  
    | state_target DOT ID   
    ;

/******************* Value Expressions *******************/

exp 
    : exp LOR exp_7 
    | exp_7
    ;

exp_7
    : exp_7 LAND exp_6   
    | exp_6
    ;

exp_6 
    : exp_5 EQ exp_5 
    | exp_5 NE exp_5 
    | exp_5
    ;

exp_5 
    : exp_4 LT exp_4 
    | exp_4 LE exp_4 
    | exp_4 GT exp_4 
    | exp_4 GE exp_4 
    | exp_4 IN exp_4 
    | exp_4
    ;

exp_4 
    : exp_4 AS ptype  
    | exp_3
    ;

exp_3 
    : exp_3 PLUS exp_2    
    | exp_3 MINUS exp_2  
    | exp_2
    ;

exp_2 
    : exp_2 MUL exp_1     
    | exp_2 DIV exp_1  
    | exp_1
    ;

exp_1 
    : MINUS exp_0 
    | LNOT  exp_0 
    | exp_0
    ;

exp_0 
    : TRUE                                                      # exp_true                    
    | FALSE                                                     # exp_false
    | THIS                                                      # exp_this
    | NONDET                                                    # exp_nondet
    | FAIRNONDET                                                # exp_fairnondet
    | NULL                                                      # exp_null
    | HALT                                                      # exp_halt
    | INT                                                       # exp_int
    | ID                                                        # exp_id
    | exp_0 DOT ID                                              # exp_getattr
    | exp_0 DOT INT                                             # exp_getidx
    | exp_0 LBRACKET exp RBRACKET                               # exp_getitem
    | LPAREN exp RPAREN                                         # exp_grouped
    | KEYS LPAREN exp RPAREN                                    # exp_keys
    | VALUES  LPAREN exp RPAREN                                 # exp_values
    | SIZEOF  LPAREN exp RPAREN                                 # exp_sizeof
    | DEFAULT LPAREN ptype RPAREN                               # exp_default
    | NEW ID LPAREN RPAREN                                      # exp_new
    | NEW ID LPAREN single_expr_arg_list RPAREN                 # exp_new_with_arguments
    | LPAREN exp COMMA             RPAREN                       # exp_tuple_1_elem
    | LPAREN exp COMMA expr_arg_list RPAREN                     # exp_tuple_n_elems
    | ID LPAREN RPAREN                                          # exp_call
    | ID LPAREN expr_arg_list RPAREN                            # exp_call_with_arguments
    | LPAREN ID ASSIGN exp COMMA RPAREN                         # exp_named_tuple_1_elem
    | LPAREN ID ASSIGN exp COMMA nmd_expr_arg_list RPAREN       # exp_named_tuple_n_elems
    ;

// An arg list that can be a single expr, or an exprs
single_expr_arg_list
    : exp                              
    | exp qualifier_or_none COMMA single_expr_arg_list 
    ;

// An arg list that is always packed into an exprs.
expr_arg_list
    : exp qualifier_or_none               
    | exp qualifier_or_none COMMA expr_arg_list 
    ;

// A named arg list that is always packed into named exprs.
nmd_expr_arg_list
    : ID ASSIGN exp                       
    | ID ASSIGN exp COMMA nmd_expr_arg_list 
    ;

/* Begin lexer */

GOTO : 'goto';
WITH : 'with';
VALUES : 'values';
AS : 'as';
REF : 'ref';
PLUS : '+';
LT : '<';
KEYS : 'keys';
FUN : 'fun';
DEFER : 'defer';
INCLUDE : 'include';
LNOT : '!';
LPAREN : '(';
SEND : 'send';
ENTRY : 'entry';
RBRACKET : ']';
NEW : 'new';
ASSIGN : '=';
ANNOUNCE : 'announce';
START : 'start';
COLON : ':';
LE : '<=';
MODEL : 'model';
GE : '>=';
MACHINE : 'machine';
TRUE : 'true';
IF : 'if';
DEFAULT : 'default';
DIV : '/';
IN : 'in';
OBSERVES : 'observes';
MINUS : '-';
LAND : '&&';
CASE : 'case';
LCBRACE : '{';
HOT : 'hot';
MAIN : 'main';
REMOVE : '-=';
NULL : 'null';
NONDET : '$';
COMMA : ',';
SEQ : 'seq';
SPEC : 'spec';
DOT : '.';
SIZEOF : 'sizeof';
RECEIVE : 'receive';
BOOL : 'bool';
INT_TYPE: 'int';
TYPE : 'type';
ANY : 'any';
NE : '!=';
ASSERT : 'assert';
STATE : 'state';
GT : '>';
WHILE : 'while';
INSERT : '+=';
RETURN : 'return';
DO : 'do';
SEMICOLON : ';';
RCBRACE : '}';
GROUP : 'group';
FAIRNONDET : '$$';
THIS : 'this';
XFER : 'xfer';
PRINT : 'print';
HALT : 'halt';
VAR : 'var';
ON : 'on';
IGNORE : 'ignore';
COLD : 'cold';
RPAREN : ')';
ELSE : 'else';
EXIT : 'exit';
EVENT : 'event';
FALSE : 'false';
LOR : '||';
PUSH : 'push';
POP : 'pop';
ASSUME : 'assume';
RAISE : 'raise';
EQ : '==';
LBRACKET : '[';
MUL : '*';
MAP : 'map';

STR
   :  '"' ( ESC_SEQ | ~('\\'|'"') )* '"'
   ;

fragment
ESC_SEQ
   :  '\\' ('b'|'t'|'n'|'f'|'r'|'"'|'\''|'\\')
   |  UNICODE_ESC
   |  OCTAL_ESC
   ;

fragment
HEX_DIGIT : ('0'..'9'|'a'..'f'|'A'..'F') ;

fragment
OCTAL_ESC
   :  '\\' ('0'..'3') ('0'..'7') ('0'..'7')
   |  '\\' ('0'..'7') ('0'..'7')
   |  '\\' ('0'..'7')
   ;

fragment
UNICODE_ESC
   :  '\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
   ;

WS : (' ' | '\t' | '\n' | '\r' | '\f')+ -> skip;
MULTILINE_COMMENT : '/*' .*? '*/' -> skip;
COMMENT : '//' ~[\r\n]* -> skip;

fragment LOWER: 'a'..'z';
fragment UPPER: 'A'..'Z';
fragment LETTER : (LOWER | UPPER) ;
fragment DIGIT : '0'..'9';
INT : DIGIT+ ;
ID : ('a'..'z'|'A'..'Z'|'_')('a'..'z'|'A'..'Z'|'_'|'0'..'9')*;

