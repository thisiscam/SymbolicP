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
			MDD.AllocateVar(2, "b");
			
			var a = new MDD(0, new bool[]{true, false});
			var b = new MDD(1, new bool[]{true, false});

			if(!a.Not().Or(b.Not()).EqualEqual(a.And(b).Not())) 
			{
				Console.WriteLine("Failed");
			}
		}
	}
}
