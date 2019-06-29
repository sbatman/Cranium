// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;

namespace Cranium.Lib.Genetics.Genes
{
	public class SingleGene : Gene<Single>
	{
		private static readonly Random _RND = new Random();

		public SingleGene(String name, Single value, Single lowerBounds, Single upperBounds) : base(name, value, lowerBounds, upperBounds)
		{
		}

		public override void Cross(Gene otherGene)
		{
			CurrentValue += (Single) ((((Gene<Single>) otherGene).CurrentValue - CurrentValue) * _RND.NextDouble());
			CurrentValue = Math.Min(CurrentValue, UpperBounds);
			CurrentValue = Math.Max(CurrentValue, LowerBounds);
		}

		public override void Mutate()
		{
			CurrentValue = LowerBounds + (Single) (_RND.NextDouble() * (UpperBounds - LowerBounds));
		}

		public override Gene Copy()
		{
			return new SingleGene(Name, CurrentValue, LowerBounds, UpperBounds);
		}
	}
}