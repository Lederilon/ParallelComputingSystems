using System;
using System.Collections.Generic;

namespace ArithmeticExpressionParser
{
	class Pars
	{
		private static Dictionary<string, long> config = new Dictionary<string, long>
				{
					{ "'+'",2 },
					{ "'-'",2},
					{ "'*'",4 },
					{ "'/'",8}
				};

		private static long layers = 2;

		static void testAllocation(Node top)
		{
			top.Print(top);
			var pipeLine = new DynamicPipeLine(config, layers);
			var stats = pipeLine.Allocate(top);
			Console.WriteLine(stats);
			pipeLine.PrintPipeAllocation();
		}

		public static void Main()
		{
			var expression = Console.ReadLine();
			var parser = new Parser(expression);

			var pipeLine = new DynamicPipeLine(config, layers);

			try
			{
				var top = parser.Parse();
				//testAllocation(top);
				top.Optimize(top);
				//testAllocation(top);

				top.ExtractBraces();
				testAllocation(top);

				//top.Optimize(top);
				//testAllocation(top);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.ReadLine();
				return;
			}
			System.Console.ReadLine();

		}
	}
}
