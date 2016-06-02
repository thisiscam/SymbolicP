class Inst {
	public static T Get_Prop<T>(T prop) {
		return prop;
	}
	public static T Set_Prop<T>(T prop, T val) {
		return val;
	}
	public static object[] New_Array(int init_size) {
		return new object[init_size];
	}
	public static object Array_Get(object[] arr, int idx) {
		return arr[idx];
	}
	public static void Array_Set(object[] arr, int idx, object val) {
		arr[idx] = val;
	}

	public static int Int(int v) {
		return v;
	}
	public static int Add(int a, int b) {
		return a + b;
	}
	public static int Mul(int a, int b) {
		return a * b;
	}

}