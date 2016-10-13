using System;
using Microsoft.Z3;
using BDDToZ3Wrap;

namespace TestBDDToZ3Wrap
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			BuDDySharp.BuDDySharp.cpp_init (100, 100);
			BuDDySharp.BuDDySharp.setvarnum (10);

			var ctx = new Context ();
			Converter.Init(ctx);
			var x_gt_1 = ctx.MkGt (ctx.MkIntConst ("x"), ctx.MkInt (1));
			var a1 = x_gt_1.ToBDD ();
			var x_gt_2 = ctx.MkGt (ctx.MkIntConst ("x"), ctx.MkInt (1));
			var a2 = x_gt_2.ToBDD();
			BuDDySharp.BuDDySharp.printdot(a1);
			BuDDySharp.BuDDySharp.printdot(a2);
		}
	}
}
