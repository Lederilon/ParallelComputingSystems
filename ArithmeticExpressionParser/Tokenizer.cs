using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArithmeticExpressionParser
{
    // Expression := [ "-" ] Term { ("+" | "-") Term }
    // Term       := Factor> { ( "*" | "/" ) Factor }
    // Factor     := RealNumber | "(" Expression ")"
    // RealNumber := Digit{Digit} | [Digit] "." {Digit}
    // Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

    public class Tokenizer
    {
        private StringReader _reader;

        public IEnumerable<Token> Scan(string expression)
        {
            _reader = new StringReader(expression);
			try
			{
				var tokens = new List<Token>();
				while (_reader.Peek() != -1)
				{
					var c = (char)_reader.Peek();
					if (Char.IsWhiteSpace(c))
					{
						_reader.Read();
						continue;
					}

					if (Char.IsDigit(c) || c == '.')
					{
						var nr = ParseNumber();
						tokens.Add(new NumberConstantToken(nr));
					}
					else if (c == '-')
					{
						tokens.Add(new MinusToken(IdGenerator.GetIdGenerator()));
						_reader.Read();
					}
					else if (c == '+')
					{
						tokens.Add(new PlusToken(IdGenerator.GetIdGenerator()));
						_reader.Read();
					}
					else if (c == '*')
					{
						tokens.Add(new MultiplyToken(IdGenerator.GetIdGenerator()));
						_reader.Read();
					}
					else if (c == '/')
					{
						tokens.Add(new DivideToken(IdGenerator.GetIdGenerator()));
						_reader.Read();
					}
					else if (c == '(')
					{
						tokens.Add(new OpenParenthesisToken(IdGenerator.GetIdGenerator()));
						_reader.Read();
					}
					else if (c == ')')
					{
						tokens.Add(new ClosedParenthesisToken(IdGenerator.GetIdGenerator()));
						_reader.Read();
					}
					else if (char.IsLetter(c))
					{
						var variable = ParseVariable();
						tokens.Add(new NumberConstantToken(variable));
					}
					else
					{
						throw new Exception("Unknown character in expression: " + c);
					}
				}

				return tokens;
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
				Console.ReadLine();

				return new List<Token>();
			}
        }

		private string ParseVariable()
		{
			var sb = new StringBuilder();
			while (Char.IsLetterOrDigit((char)_reader.Peek()))
			{
				var c = (char)_reader.Read();
				sb.Append(c);
			}
			return sb.ToString();
		}

        private double ParseNumber()
        {
            var sb = new StringBuilder();
            var decimalExists = false;
            while (Char.IsDigit((char)_reader.Peek()) || ((char) _reader.Peek() == '.'))
            {
                var digit = (char)_reader.Read();
                if (digit == '.')
                {
                    if (decimalExists) throw new Exception("Multiple dots in decimal number");
                    decimalExists = true;
                }
                sb.Append(digit);
            }

            double res;
            if (!double.TryParse(sb.ToString(), out res))
                throw new Exception("Could not parse number: " + sb);

           return res;
        }
    }
}
