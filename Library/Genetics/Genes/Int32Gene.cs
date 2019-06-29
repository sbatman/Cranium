// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;

namespace Cranium.Lib.Genetics.Genes
{
	public class Int32Gene : Gene<Int32>
	{
		private static readonly Random _RND = new Random();

		public Int32Gene(String name, Int32 value, Int32 lowerBounds, Int32 upperBounds) : base(name, value, lowerBounds, upperBounds)
		{
		}

		public override void Cross(Gene otherGene)
		{
			CurrentValue += (Int32) ((((Gene<Int32>) otherGene).CurrentValue - CurrentValue) * _RND.NextDouble());
			CurrentValue = Math.Min(CurrentValue, UpperBounds);
			CurrentValue = Math.Max(CurrentValue, LowerBounds);
		}

		public override void Mutate()
		{
			CurrentValue = LowerBounds + (Int32) (_RND.NextDouble() * (UpperBounds - LowerBounds));
		}

		public override Gene Copy()
		{
			return new Int32Gene(Name, CurrentValue, LowerBounds, UpperBounds);
		}
	}
}