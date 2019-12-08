using System;

namespace ArithmeticExpressionParser
{
	public abstract class Token : ICloneable
	{
		public long Id { get; set; }

		public string Key;

		public Token(long id)
		{
			Id = id;
		}

		public abstract object Clone();
	}

    public class OperatorToken : Token
    {
		public OperatorToken(long id) : base(id)
		{
		}

		public override object Clone()
		{
			return new OperatorToken(IdGenerator.GetIdGenerator());
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}
    public class PlusToken : OperatorToken
    {
		public PlusToken(long id) : base(id)
		{
			Key = "'+'";
		}

		public override string ToString()
		{
			return string.Format(Key + "({0})", Id);
		}
		
		public override object Clone()
		{
			return new PlusToken(IdGenerator.GetIdGenerator());
		}
	}

    public class MinusToken : OperatorToken
    {
		public MinusToken(long id) : base(id)
		{
			Key = "'-'";
		}

		public override string ToString()
		{
			return string.Format(Key + "({0})", Id); 
		}

		public override object Clone()
		{
			return new MinusToken(IdGenerator.GetIdGenerator());
		}
	}

    public class MultiplyToken : OperatorToken
    {
		public MultiplyToken(long id) : base(id)
		{
			Key = "'*'";
		}

		public override string ToString()
		{
			return string.Format(Key + "({0})", Id);
		}

		public override object Clone()
		{
			return new MultiplyToken(IdGenerator.GetIdGenerator());
		}
	}

    public class DivideToken : OperatorToken
    {
		public DivideToken(long id) : base(id)
		{
			Key = "'/'";
		}

		public override string ToString()
		{
			return string.Format(Key + "({0})", Id); 
		}

		public override object Clone()
		{
			return new MultiplyToken(IdGenerator.GetIdGenerator());
		}
	}

	public class ParenthesisToken : Token
	{
		public ParenthesisToken(long id) : base(id)
		{
		}

		public override object Clone()
		{
			return new ParenthesisToken(IdGenerator.GetIdGenerator());
		}
	}

	public class OpenParenthesisToken : ParenthesisToken
	{
		public OpenParenthesisToken(long id) : base(id)
		{
		}

		public override string ToString()
		{
			return "'('";
		}

		public override object Clone()
		{
			return new OpenParenthesisToken(IdGenerator.GetIdGenerator());
		}
	}

	public class ClosedParenthesisToken : ParenthesisToken
	{
		public ClosedParenthesisToken(long id) : base(id)
		{
		}

		public override string ToString()
		{
			return "')'";
		}

		public override object Clone()
		{
			return new ClosedParenthesisToken(IdGenerator.GetIdGenerator());
		}
	}


	public class NumberConstantToken : Token
	{
		private readonly double _value;

		public NumberConstantToken(double value) : base(0)
		{
			_value = value;
		}

		private readonly string _name;

		public NumberConstantToken(string name) : base(0)
		{
			_name = name;
		}

		public double Value
		{
			get { return _value; }
		}

		public override string ToString()
		{
			if (_name != null)
			{
				return _name;
			}
			return Value.ToString();
		}

		public override object Clone()
		{
			return new NumberConstantToken(Value);
		}
	}
}