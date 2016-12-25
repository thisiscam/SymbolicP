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
			
			var not_a = new MDD(0, new bool[]{false, true});
			if(a.Not() != not_a) 
			{
				Console.WriteLine("Failed");
			}
			
			
		}
	}
}
