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
			var x = ctx.MkIntConst ("x");
			var x_gt_1 = ctx.MkGt (x, ctx.MkInt (1));
			var a = x_gt_1.ToBDD ();
			var a2 = x_gt_1.ToBDD ();
			var x_gt_1_prime = a.ToZ3Expr ();
			Console.WriteLine (x_gt_1_prime);
		}
	}
}
