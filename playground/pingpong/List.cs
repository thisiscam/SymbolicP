#include "pingpong_Macros.h"

class List {
	public object[] data;
	public int Count;

	public List() {
		object[] L0 = Inst.New_Array(8);
		this.data = Inst.Set_Prop<object[]>(this.data, L0);
	}
	public void Add(object o) {
		int L0 = -1, L2 = -1, L3 = -1, L4 = -1, L6 = -1, L7 = -1;
		object[] L1 = null, L5 = null;
		int pc = 0;
		switch(pc) {
		case 0:		L0 = Inst.Get_Prop<int>(this.Count);
					L1 = Inst.Get_Prop<object[]>(this.data);
					L2 = Inst.Get_Prop<int>(L1.Length);
					if(L0 != L2) { pc = 7; break; }
					L3 = Inst.Int(2);
					L4 = Inst.Mul(L2, L3);
					L5 = Inst.New_Array(L4);
					this.data = Inst.Set_Prop<object[]>(this.data, L5);
		CASE(7):	Inst.Array_Set(L1, L0, o);
					L6 = Inst.Int(1);
					L7 = Inst.Add(L0, L6);
					this.Count = Inst.Set_Prop<int>(this.Count, L7);
		CASE(-1):	return;
		}

	}
}