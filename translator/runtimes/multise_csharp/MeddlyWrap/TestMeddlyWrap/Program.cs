using System;
using MeddlyWrap;

namespace TestMeddlyWrap
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			MDD.AllocateVar(2, "a");
			//MDD.AllocateVar(3, "b");
			
			var a = new MDD(0, 1);
			var na = new MDD(0, 0);
			Console.WriteLine(a.And(na));
			//var b = new MDD(1, new bool[]{true, false});
			
			//var na = new MDD(0, new bool[]{false, true});
			
			//if(a.EqualEqual(na)) {
			//	Console.WriteLine("?");
			//}
			
			// Console.WriteLine(a);
			//Console.WriteLine(na);
			//Console.WriteLine(a.And(b));
			
			//if(!a.Not().Or(b.Not()).EqualEqual(a.And(b).Not())) 
			//{
			//	Console.WriteLine("Failed");
			//}
		}
	}
}
