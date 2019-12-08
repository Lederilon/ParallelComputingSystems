using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArithmeticExpressionParser
{
	public static class IdGenerator
	{
		private static long _lastId;

		public static long GetIdGenerator()
		{
			_lastId++;
			return _lastId;
		}
	}
}
