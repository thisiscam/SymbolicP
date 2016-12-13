# Generated from p.g4 by ANTLR 4.5.3
from antlr4 import *

# This class defines a complete generic visitor for a parse tree produced by pParser.

class pVisitor(ParseTreeVisitor):

    # Visit a parse tree produced by pParser#program.
    def visitProgram(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#top_decl_list.
    def visitTop_decl_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#top_decl.
    def visitTop_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#annotation_set.
    def visitAnnotation_set(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#annotation_list.
    def visitAnnotation_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#annotation.
    def visitAnnotation(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#type_def_decl.
    def visitType_def_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#include_decl.
    def visitInclude_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#event_decl.
    def visitEvent_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ev_card_or_none.
    def visitEv_card_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ev_type_or_none.
    def visitEv_type_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#event_annot_or_none.
    def visitEvent_annot_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#machine_decl.
    def visitMachine_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#machine_name_decl_regular.
    def visitMachine_name_decl_regular(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#machine_name_decl_model.
    def visitMachine_name_decl_model(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#machine_name_decl_spec.
    def visitMachine_name_decl_spec(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#observes_list.
    def visitObserves_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#is_main.
    def visitIs_main(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#mach_card_or_none.
    def visitMach_card_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#mach_annot_or_none.
    def visitMach_annot_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#machine_body.
    def visitMachine_body(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#machine_body_item.
    def visitMachine_body_item(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#var_decl.
    def visitVar_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#var_list.
    def visitVar_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#local_var_decl.
    def visitLocal_var_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#local_var_decl_list.
    def visitLocal_var_decl_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#local_var_list.
    def visitLocal_var_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#payload_var_decl_or_none.
    def visitPayload_var_decl_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#payload_var_decl_or_none_ref.
    def visitPayload_var_decl_or_none_ref(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#payload_none.
    def visitPayload_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#fun_decl.
    def visitFun_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#fun_name_decl.
    def visitFun_name_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#is_model.
    def visitIs_model(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#fun_annot_or_none.
    def visitFun_annot_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#params_or_none.
    def visitParams_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ret_type_or_none.
    def visitRet_type_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#group.
    def visitGroup(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#group_body.
    def visitGroup_body(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#group_item.
    def visitGroup_item(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#group_name.
    def visitGroup_name(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_decl.
    def visitState_decl(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#is_start_state_or_none.
    def visitIs_start_state_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#is_hot_or_cold_or_none.
    def visitIs_hot_or_cold_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_annot_or_none.
    def visitState_annot_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body.
    def visitState_body(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_entry_unnamed.
    def visitState_body_item_entry_unnamed(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_entry_fn_named.
    def visitState_body_item_entry_fn_named(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_exit_unnamed.
    def visitState_body_item_exit_unnamed(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_exit_fn_named.
    def visitState_body_item_exit_fn_named(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_defer.
    def visitState_body_item_defer(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_ignore.
    def visitState_body_item_ignore(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_on_e_do_fn_named.
    def visitState_body_item_on_e_do_fn_named(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_on_e_do_unamed.
    def visitState_body_item_on_e_do_unamed(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_push.
    def visitState_body_item_push(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_on_e_goto.
    def visitState_body_item_on_e_goto(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_on_e_goto_with_unnamed.
    def visitState_body_item_on_e_goto_with_unnamed(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_on_e_goto_with_fn_named.
    def visitState_body_item_on_e_goto_with_fn_named(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#on_event_list.
    def visitOn_event_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#non_default_event_list.
    def visitNon_default_event_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#event_list.
    def visitEvent_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#event_id.
    def visitEvent_id(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#non_default_event_id.
    def visitNon_default_event_id(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#trig_annot_or_none.
    def visitTrig_annot_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_null.
    def visitPtype_null(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_bool.
    def visitPtype_bool(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_int_type.
    def visitPtype_int_type(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_event.
    def visitPtype_event(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_machine.
    def visitPtype_machine(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_any.
    def visitPtype_any(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_typedef.
    def visitPtype_typedef(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_seq.
    def visitPtype_seq(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_map.
    def visitPtype_map(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_tuple.
    def visitPtype_tuple(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ptype_named_tuple.
    def visitPtype_named_tuple(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#tup_type_list.
    def visitTup_type_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#qualifier_or_none.
    def visitQualifier_or_none(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#nmd_tup_type_list.
    def visitNmd_tup_type_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_semicolon.
    def visitStmt_semicolon(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_rbrace.
    def visitStmt_rbrace(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_pop.
    def visitStmt_pop(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_stmt_list.
    def visitStmt_stmt_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_assert.
    def visitStmt_assert(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_assert_str.
    def visitStmt_assert_str(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_print.
    def visitStmt_print(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_return.
    def visitStmt_return(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_return_exp.
    def visitStmt_return_exp(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_assign.
    def visitStmt_assign(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_remove.
    def visitStmt_remove(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_insert.
    def visitStmt_insert(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_while.
    def visitStmt_while(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_if_then_else.
    def visitStmt_if_then_else(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_if_then.
    def visitStmt_if_then(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_new.
    def visitStmt_new(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_new_with_arguments.
    def visitStmt_new_with_arguments(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_call.
    def visitStmt_call(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_call_with_arguments.
    def visitStmt_call_with_arguments(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_raise.
    def visitStmt_raise(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_raise_with_arguments.
    def visitStmt_raise_with_arguments(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_send.
    def visitStmt_send(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_send_with_arguments.
    def visitStmt_send_with_arguments(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_announce.
    def visitStmt_announce(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_announce_with_arguments.
    def visitStmt_announce_with_arguments(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_recieve.
    def visitStmt_recieve(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#receive_stmt.
    def visitReceive_stmt(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#pcase.
    def visitPcase(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#case_event_list.
    def visitCase_event_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#case_list.
    def visitCase_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_block.
    def visitStmt_block(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_list.
    def visitStmt_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_target.
    def visitState_target(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp.
    def visitExp(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_7.
    def visitExp_7(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_6.
    def visitExp_6(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_5.
    def visitExp_5(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_4.
    def visitExp_4(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_3.
    def visitExp_3(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_2.
    def visitExp_2(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_1.
    def visitExp_1(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_getidx.
    def visitExp_getidx(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_sizeof.
    def visitExp_sizeof(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_call.
    def visitExp_call(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_new.
    def visitExp_new(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_call_with_arguments.
    def visitExp_call_with_arguments(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_nondet.
    def visitExp_nondet(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_this.
    def visitExp_this(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_id.
    def visitExp_id(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_getattr.
    def visitExp_getattr(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_named_tuple_1_elem.
    def visitExp_named_tuple_1_elem(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_keys.
    def visitExp_keys(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_grouped.
    def visitExp_grouped(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_named_tuple_n_elems.
    def visitExp_named_tuple_n_elems(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_true.
    def visitExp_true(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_values.
    def visitExp_values(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_default.
    def visitExp_default(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_null.
    def visitExp_null(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_new_with_arguments.
    def visitExp_new_with_arguments(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_false.
    def visitExp_false(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_halt.
    def visitExp_halt(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_getitem.
    def visitExp_getitem(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_fairnondet.
    def visitExp_fairnondet(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_tuple_1_elem.
    def visitExp_tuple_1_elem(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_int.
    def visitExp_int(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#exp_tuple_n_elems.
    def visitExp_tuple_n_elems(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#single_expr_arg_list.
    def visitSingle_expr_arg_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#expr_arg_list.
    def visitExpr_arg_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#nmd_expr_arg_list.
    def visitNmd_expr_arg_list(self, ctx):
        return self.visitChildren(ctx)


