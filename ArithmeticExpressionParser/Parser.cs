using System;
using System.Collections.Generic;

namespace ArithmeticExpressionParser
	{

	class NodeInfo
	{
		public Node Node;
		public string Text;
		public int StartPos;
		public int Size { get { return Text.Length; } }
		public int EndPos { get { return StartPos + Size; } set { StartPos = value - Size; } }
		public NodeInfo Parent, Left, Right;
	}

	public class Node
	{
		public long Start { get; private set; }

		public long Finish { get; private set; }

		public void Allocate(long start, long finish)
		{
			Start = start;
			Finish = finish;
		}

		public long EarlyStart
		{
			get
			{
				var leftConstraint = LeftChild == null || LeftChild.Token is NumberConstantToken ? 0 : (LeftChild.Finish + 1);
				var rightConstraint = RightChild == null || RightChild.Token is NumberConstantToken? 0 : (RightChild.Finish + 1);
				return Math.Max(leftConstraint, rightConstraint) ;
			}
		}

		public double Result { get; set; }

		public Token Token { get; set; }

		private Node _leftChild;

		public Node Parent;

		private Node _rightChild;

		public Node LeftChild
		{
			get
			{
				return _leftChild;
			}
			set
			{
				_leftChild = value;
				_leftChild.Parent = this;
			}
		}

		public Node RightChild
		{
			get
			{
				return _rightChild;
			}
			set
			{
				_rightChild = value;
				_rightChild.Parent = this;
			}
		}

		public long Depth
		{
			get
			{
				var rightDepth = RightChild != null ? RightChild.Depth : 0;
				var leftDepth = LeftChild != null ? LeftChild.Depth : 0;
				return Math.Max(rightDepth , leftDepth) + ((Token is OperatorToken)? 1 : 0);
			}
		}

		public bool InBraced { get; set; }

		private bool optimize()
		{
			if (LeftChild != null
				&& LeftChild.Depth > RightChild.Depth + 1)
			{
				if (Token is PlusToken || Token is MultiplyToken)
				{
					reduce(this, LeftChild);
					return true;
				}
				if (LeftChild.Token is DivideToken)
				{
					revert(this, LeftChild, new MultiplyToken(IdGenerator.GetIdGenerator()));
					return true;
				}
				if (LeftChild.Token is MinusToken)
				{
					revert(this, LeftChild, new MultiplyToken(IdGenerator.GetIdGenerator()));
					return true;
				}
			}
			return false;
		}

		public void ExtractBraces()
		{
			if (LeftChild != null)
			{
				LeftChild.ExtractBraces();
			}

			if (Token is MinusToken || Token is PlusToken
				&& LeftChild != null
				&& RightChild != null
				&& (LeftChild.Token.Key == RightChild.Token.Key))
			{
				var leftToken = LeftChild.LeftChild.Token as NumberConstantToken;
				var rightToke = RightChild.LeftChild.Token as NumberConstantToken;

				if (leftToken != null && rightToke != null && leftToken.Value == rightToke.Value)
				{
					var token = Token;
					Token = LeftChild.Token;

					RightChild.LeftChild = LeftChild.RightChild;
					LeftChild = LeftChild.LeftChild;
					RightChild.Token = token;
					return;
				}

				leftToken = LeftChild.RightChild.Token as NumberConstantToken;
				rightToke = RightChild.RightChild.Token as NumberConstantToken;

				if (leftToken != null && rightToke != null && leftToken.Value == rightToke.Value)
				{
					var token = Token;
					Token = LeftChild.Token;

					LeftChild.RightChild = RightChild.LeftChild;

					RightChild = RightChild.RightChild;
					LeftChild.Token = token;
					return;
				}
			}
		}

		public void Optimize(Node root, bool debug = false)
		{
			while (optimize());

			if (debug)
			{
				root.Print(root);
			}

			if (LeftChild != null)
			{
				LeftChild.Optimize(root);
			}
			

			if (RightChild != null)
			{
				RightChild.Optimize(root);
			}
		}

		private void reduce(Node currentNode, Node leftChild)
		{
			currentNode.LeftChild = leftChild.LeftChild;
			var rightChild = currentNode.RightChild;
			rightChild.Token.Id = IdGenerator.GetIdGenerator();

			var node = new Node
			{
				Token = (Token)currentNode.Token.Clone(),
				LeftChild = leftChild.RightChild,
				RightChild = RightChild
			};
			node.Token.Id = IdGenerator.GetIdGenerator();

			currentNode.RightChild = node;
		}

		private void revert(Node currentNode, Node leftChild, Token leftToken)
		{
			currentNode.LeftChild = leftChild.LeftChild;
			var rightChild = currentNode.RightChild;
			rightChild.Token.Id = IdGenerator.GetIdGenerator();

			var node = new Node
			{
				Token = (Token)leftToken.Clone(),
				LeftChild = leftChild.RightChild,
				RightChild = RightChild
			};

			node.Token.Id = IdGenerator.GetIdGenerator();
			currentNode.RightChild = node;
		}

		public void Print(Node root, string textFormat = "0", int spacing = 1, int topMargin = 2, int leftMargin = 2)
		{
			if (root == null) return;
			int rootTop = Console.CursorTop + topMargin;
			var last = new List<NodeInfo>();
			var next = root;
			for (int level = 0; next != null; level++)
			{
				var item = new NodeInfo { Node = next, Text = next.Token.ToString() };
				if (level < last.Count)
				{
					item.StartPos = last[level].EndPos + spacing;
					last[level] = item;
				}
				else
				{
					item.StartPos = leftMargin;
					last.Add(item);
				}
				if (level > 0)
				{
					item.Parent = last[level - 1];
					if (next == item.Parent.Node.LeftChild)
					{
						item.Parent.Left = item;
						item.EndPos = Math.Max(item.EndPos, item.Parent.StartPos - 1);
					}
					else
					{
						item.Parent.Right = item;
						item.StartPos = Math.Max(item.StartPos, item.Parent.EndPos + 1);
					}
				}
				next = next.LeftChild ?? next.RightChild;
				for (; next == null; item = item.Parent)
				{
					int top = rootTop + 2 * level;
					Print(item.Text, top, item.StartPos);
					if (item.Left != null)
					{
						Print("/", top + 1, item.Left.EndPos);
						Print("_", top, item.Left.EndPos + 1, item.StartPos);
					}
					if (item.Right != null)
					{
						Print("_", top, item.EndPos, item.Right.StartPos - 1);
						Print("\\", top + 1, item.Right.StartPos - 1);
					}
					if (--level < 0) break;
					if (item == item.Parent.Left)
					{
						item.Parent.StartPos = item.EndPos + 1;
						next = item.Parent.Node.RightChild;
					}
					else
					{
						if (item.Parent.Left == null)
							item.Parent.EndPos = item.StartPos - 1;
						else
							item.Parent.StartPos += (item.StartPos - 1 - item.Parent.EndPos) / 2;
					}
				}
			}
			Console.SetCursorPosition(0, rootTop + 2 * last.Count - 1);
		}

		public void Print(string s, int top, int left, int right = -1)
		{
			Console.SetCursorPosition(left, top);
			if (right < 0) right = left + s.Length;
			while (Console.CursorLeft < right) Console.Write(s);
		}
	}
			
		public class Parser
		{
			private readonly string _expression;

			private readonly TokensWalker _walker;

			private long _lastId;

			private long getId()
			{
				_lastId++;
				return (_lastId);
			}

			public Parser(string expression)
			{
				_expression = expression;
				var tokens = new Tokenizer().Scan(_expression);
				_walker = new TokensWalker(tokens);
			}

			// EBNF Grammar:
			// Expression := [ "-" ] Term { ("+" | "-") Term }
			// Term       := Factor { ( "*" | "/" ) Factor }
			// Factor     := RealNumber | "(" Expression ")"
			// RealNumber := Digit{Digit} | [Digit] "." {Digit}
			// Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

			// Expression := [ "-" ] Term { ("+" | "-") Term }
			public Node Parse()
			{
				var isNegative = NextIsMinus();

				if (isNegative)
					GetNext();

				var leftNode = TermValue();
				var valueOfExpression = leftNode.Result;

				if (isNegative)
				{
					var newNode = new Node
					{
						RightChild = leftNode,
						Result = -leftNode.Result,
						Token = new MinusToken(getId())
					};
					valueOfExpression = -valueOfExpression;
					leftNode = newNode;
				}

				while (NextIsMinusOrPlus())
				{
					var op = GetTermOperand();
					var rightNode = TermValue();
					var nextTermValue = rightNode.Result;
					if (op is PlusToken)
						valueOfExpression += nextTermValue;
					else
						valueOfExpression -= nextTermValue;
					var newNode = new Node
					{
						LeftChild = leftNode,
						RightChild = rightNode,
						Result = valueOfExpression,
						Token = op
					};
					leftNode = newNode;
				}
				return leftNode;
			}

			// Term       := Factor { ( "*" | "/" ) Factor }
			private Node TermValue()
			{
				var currentNode = FactorValue();
				var totalVal = currentNode.Result;

				while (NextIsMultiplicationOrDivision())
				{
					var newNode = new Node();
					var op = GetFactorOperand();
					newNode.Token = op;
					var rightNode = FactorValue();
					newNode.RightChild = rightNode;
					newNode.LeftChild = currentNode;
					var nextFactor = rightNode.Result;

					if (op is DivideToken)
						totalVal /= nextFactor;
					else
						totalVal *= nextFactor;
					currentNode = newNode;
					currentNode.Result = totalVal;
				}

				return currentNode;
			}

			// Factor     := RealNumber | "(" Expression ")"
			private Node FactorValue()
			{
				if (NextIsDigit())
				{
					var nr = GetNumber();
					return nr;
				}
				if (!NextIsOpeningBracket())
					throw new Exception("Expecting Real number or variable or '(' in expression, instead got : " + (PeekNext() != null ? PeekNext().ToString()  : "End of expression"));          
				GetNext();

				var val = Parse();
			
				if (!(NextIs(typeof(ClosedParenthesisToken))))
					throw new Exception("Expecting ')' in expression, instead got: " + (PeekNext() != null ? PeekNext().ToString() : "End of expression"));           
				GetNext();
				val.InBraced = true;
				return val;
			}

			private bool NextIsMinus()
			{
				return _walker.ThereAreMoreTokens && _walker.IsNextOfType(typeof(MinusToken));
			}

			private bool NextIsOpeningBracket()
			{
				return NextIs(typeof(OpenParenthesisToken));
			}

			private Token GetTermOperand()
			{
				var c = GetNext();
				if (c is PlusToken)
					return c;
				if (c is MinusToken)
					return c;

				throw new Exception("Expected term operand '+' or '-' but found" + c);
			}

			private Token GetFactorOperand()
			{
				var c = GetNext();
				if (c is DivideToken)
					return c;
				if (c is MultiplyToken)
					return c;

				throw new Exception("Expected factor operand '/' or '*' but found" + c);
			}

			private Token GetNext()
			{
				return _walker.GetNext();
			}

			private Token PeekNext()
			{
				return _walker.ThereAreMoreTokens ? _walker.PeekNext() : null;
			}

			private Node GetNumber()
			{
				var next = _walker.GetNext();

				var nr = next as NumberConstantToken;
				if (nr == null)
					throw new Exception("Expecting Real number but got " + next);

				var node = new Node()
				{
					Result = nr.Value,
					Token = nr
				};
				return node;
			}

			private bool NextIsDigit()
			{
				if (!_walker.ThereAreMoreTokens)
					return false;
				return _walker.PeekNext() is NumberConstantToken;
			}

			private bool NextIs(Type type)
			{
				return _walker.ThereAreMoreTokens && _walker.IsNextOfType(type);
			}

			private bool NextIsMinusOrPlus()
			{
				return _walker.ThereAreMoreTokens && (NextIs(typeof(MinusToken)) || NextIs(typeof(PlusToken)));
			}

			private bool NextIsMultiplicationOrDivision()
			{
				return _walker.ThereAreMoreTokens && (NextIs(typeof(MultiplyToken)) || NextIs(typeof(DivideToken)));
			}
		}
	}