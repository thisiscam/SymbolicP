using System;
using System.Runtime.InteropServices;
using System.Security;


namespace MeddlyWrap
{
	using System.Diagnostics;
	using MDD_Ptr = IntPtr;

	public sealed class MDD
	{
		private MDD_Ptr inner;
		
		static MDD _bddtrue = null;
		static MDD _bddfalse = null;
		
		public static MDD bddtrue { get { return _bddtrue; }}
		public static MDD bddfalse { get { return _bddfalse; }}
		
		static MDD()
		{
			PInvoke.meddly_init(1000);
			_bddtrue = new MDD(PInvoke.get_mdd_true());
			_bddfalse = new MDD(PInvoke.get_mdd_false());
		}
		
		public MDD(int var, bool[] terminals)
		{
			inner = PInvoke.create_edge_for_var(var, terminals);
		}
		
		public MDD(MDD_Ptr inner)
		{
			this.inner = inner;
		}
		
		~MDD()
		{
			PInvoke.mdd_free(inner);
		}
		
		public MDD And(MDD other)
		{
			return new MDD(PInvoke.mdd_and(this.inner, other.inner));
		}
		
		public MDD Or(MDD other)
		{
			return new MDD(PInvoke.mdd_or(this.inner, other.inner));
		}
		
		public MDD Not()
		{
			return new MDD(PInvoke.mdd_not(this.inner));
		}
		
		public bool EqualEqual(MDD other)
		{
			return PInvoke.mdd_equalequal(this.inner, other.inner);
		}
		
		public static void AllocateVar(int bound, string name)
		{
			PInvoke.allocate_variable(bound, name);
		}
		
		public static int GetNumVars()
		{
			return PInvoke.get_num_vars();
		}
		
		/* For compats */
		public MDD_Ptr Id { get { return inner; }}
		
		public MDD(MDD_Ptr inner, bool memoryOwn)
		{
			this.inner = inner;
		}
		
		public bool not_pure_bool()
		{
			return false;
		}
		
		public bool Biimp(MDD other)
		{
			throw new Exception("not implemented");
		}
		
		public void PrintDot()
		{
			Console.WriteLine("MDD print dot not implemented");
		}
		
		public override string ToString()
		{
			var p = PInvoke.edge_to_string(inner);
			string ret = Marshal.PtrToStringAuto(p);
			PInvoke.delete_string(p);
			return ret;
		}
		
		static class PInvoke
		{
			const string DLLNAME = "Meddly_Wrap";
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public extern static void meddly_init(int num_vars);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public extern static void meddly_close();
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern void allocate_variable(int bound, string name);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern MDD_Ptr create_edge_for_var(int var, bool[] terms);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern MDD_Ptr mdd_and(MDD_Ptr a, MDD_Ptr b);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern MDD_Ptr mdd_or(MDD_Ptr a, MDD_Ptr b);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern MDD_Ptr mdd_not(MDD_Ptr a);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern bool mdd_equalequal(MDD_Ptr a, MDD_Ptr b);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern MDD_Ptr get_mdd_true();
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern MDD_Ptr get_mdd_false();
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern void mdd_free(MDD_Ptr a);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern int get_num_vars();

			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern IntPtr edge_to_string(MDD_Ptr d);
			
			[SuppressUnmanagedCodeSecurity]
			[DllImport(DLLNAME)]
			public static extern void delete_string(IntPtr s);
		}	
	}
}
