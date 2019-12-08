using System;
using System.Collections.Generic;
using System.Linq;

namespace ArithmeticExpressionParser
{
	public class DynamicPipeLine
	{
		public class Allocation
		{
			public Node Node;

			public bool IsLocked;
		}

		public class AllocationStats
		{
			public long TimeSpentInParallelSystem { get; set; }

			public long TimeSpentWithNoParalelism { get; set; }

			public decimal Effectivness { get { return (decimal)SeedUp / (decimal)Layers; } }

			public decimal SeedUp { get { return (decimal)TimeSpentWithNoParalelism / (decimal)TimeSpentInParallelSystem; } }

			public long Layers {get; set;}

			public override string ToString()
			{
				return string.Format(
								"\nSequentialTime {1}"+
								"\nParallel Time {0}"+
								"\nLayers {2}" +
								"\nSpeed up {3}" +
								"\nEffectivness {4}"
								, TimeSpentInParallelSystem, TimeSpentWithNoParalelism,Layers, SeedUp, Effectivness);
			}
		}

		public Dictionary<string, long> Weights{ get; private set; }

		public long Leyers { get; private set; }

		private long lastTackt { get; set; }

		private long tacktDuration;

		private Dictionary<long, string> mem = new Dictionary<long, string>();

		private Dictionary<long, long> _taktDuration = new Dictionary<long, long>();

		private Allocation[,] layersAllocation;

		private void shiftNextLayers(long currentTakt, long newTackt, long taktFrom)
		{
			for (var i = 1; i < Leyers; i++)
			{
				var taktIncrease = (i * newTackt) - currentTakt;

				for (var j = 0; j < currentTakt; j++)
				{
					layersAllocation[i, j + taktIncrease] = layersAllocation[i, j];
					layersAllocation[i, j] = null;
				}
			}
		}

		public DynamicPipeLine(Dictionary<string, long> weights, long leyers)
		{
			Leyers = leyers;
			Weights = weights;
			layersAllocation = new Allocation[Leyers,1000];
		}

		public List<Node> GetLeafs(Node node)
		{
			if (node == null || node.Token is NumberConstantToken)
			{
				return new List<Node>();
			}
			if (node.LeftChild != null && node.RightChild !=null 
				&& node.LeftChild.Token is NumberConstantToken && node.RightChild.Token is NumberConstantToken)
			{
				return new List<Node> { node };
			}
			var leafs = new List<Node>();
			leafs.AddRange(GetLeafs(node.LeftChild));
			leafs.AddRange(GetLeafs(node.RightChild));
			return leafs;
		}

		private void allocate(Node node)
		{
			var tastStart = lastTackt;

			if (!_taktDuration.TryGetValue(node.EarlyStart + 1, out long taktD) || taktD == 0)
			{
				if (node.EarlyStart > tastStart)
				{
					tastStart = node.EarlyStart;
				}
			}
			else
			{

				while (node.EarlyStart > tastStart)
				{
					if (!_taktDuration.TryGetValue(tastStart, out long taktDuration))
					{
						tastStart = node.EarlyStart;
					}
					else
					{
						tastStart += taktDuration;
					}
				}
			}

			var currentTackt = tastStart;
			mem[currentTackt] = "R";
			currentTackt++;

			var duration = getDuration(node);

			if (_taktDuration.TryGetValue(currentTackt, out long takt))
			{
			}

			var ceiledDuration = Math.Max(duration, takt);

			if (takt < duration && takt != 0)
			{
				shiftNextLayers(takt, duration, currentTackt);
			}
			else
			{

			}

			var unused = ceiledDuration - duration;

			for (var i = 0; i < Leyers; i++)
			{
				for (var j = 0; j < duration; j++)
				{
					_taktDuration[currentTackt] = ceiledDuration;
					layersAllocation[i, currentTackt] = new Allocation()
					{
						Node = node
					};
					currentTackt++;
				}

				for (var j = 0; j < unused; j++)
				{
					//_taktDuration[currentTackt] = ceiledDuration;
					//layersAllocation[i, currentTackt] = new Allocation()
					//{
					//	IsLocked = true
					//};
					currentTackt++;
				}
			}

			var finish = currentTackt - unused;
			node.Allocate(tastStart, finish);
			lastTackt = tastStart + duration;
			mem[finish] = "W";
		}

		private Dictionary<long, bool> _allocated = new Dictionary<long, bool>();

		private void allocate(List<Node> openNodes)
		{
			var nextStepOpened = new List<Node>();
			var orderedNodes = openNodes.OrderBy(n=>n.EarlyStart).ThenByDescending(n => getDuration(n));

			var firtsAllocated = false;
			foreach (var node in orderedNodes)
			{
				if (firtsAllocated)
				{
					nextStepOpened.Add(node);
					continue;
				}

				allocate(node);
				_allocated[node.Token.Id] = true;

				if (node.Parent != null)
				{
					var parent = node.Parent;

					if (parent.LeftChild.Token is NumberConstantToken ||
						(_allocated.TryGetValue(node.Parent.LeftChild.Token.Id, out bool allocatedL) && allocatedL))
					{
						if (parent.RightChild.Token is NumberConstantToken ||
						(_allocated.TryGetValue(parent.RightChild.Token.Id, out bool allocatedR) && allocatedR))
						{
							nextStepOpened.Add(node.Parent);
						}
					}
					
				}

				firtsAllocated = true;
			}

			if (nextStepOpened.Any())
			{
				allocate(nextStepOpened);
			}
		}

		public long getCommulativeDuration(Node top)
		{
			if (top == null || top.Token is NumberConstantToken)
			{
				return 0;
			}

			var childeWeight = getCommulativeDuration(top.LeftChild);
			var parentWeight = getCommulativeDuration(top.RightChild);
			var currentWeight = getDuration(top) * Leyers;
			return childeWeight + parentWeight + currentWeight;
		}

		public AllocationStats Allocate(Node top)
		{
			_allocated.Clear();
			var leafs = GetLeafs(top);
			allocate(leafs);
			var timeResultIsWrittenInMemory = top.Finish + 1;
			var seqentionalDuration = getCommulativeDuration(top) + 2;
			var stats = new AllocationStats
			{
				TimeSpentInParallelSystem = timeResultIsWrittenInMemory,
				TimeSpentWithNoParalelism = seqentionalDuration,
				Layers = Leyers
			};
			return stats;
		}

		public void PrintPipeAllocation()
		{
			Console.Write("".PadRight(5));
			Console.Write("M".PadRight(5));

			for (var i = 0; i < Leyers; i++)
			{
				var allocationVisualisation = string.Format("T{0}",i + 1).PadRight(7);
				Console.Write(allocationVisualisation);
			}
			Console.WriteLine();
			
			for (var i = 0; i < 120; i++)
			{
				Console.Write((i+1).ToString().PadRight(5));

				if (mem.TryGetValue(i, out string act))
				{
					Console.Write(act.PadRight(7));
				}
				else
				{
					Console.Write("#".PadRight(7));
				}

				for (var j = 0; j < Leyers; j++)
				{
					var allocation = layersAllocation[j, i];
					var allocationVisualisation =
						(allocation == null || allocation.IsLocked || allocation.Node == null || allocation.Node.Token == null
							? "#" 
							: allocation.Node.Token.ToString()).PadRight(7);
					Console.Write(allocationVisualisation);
				}
				Console.WriteLine();
			}
		}

		long getDuration(Node node)
		{
			return Weights[node.Token.Key];
		}

	}
}
