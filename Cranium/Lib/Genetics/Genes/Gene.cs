// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;

namespace Cranium.Lib.Genetics.Genes
{
	public abstract class Gene
	{
		public String Name { get; internal set; }
		public abstract void Cross(Gene otherGene);
		public abstract void Mutate();
		public abstract Gene Copy();
	}

	public abstract class Gene<T> : Gene
	{
		public T CurrentValue { get; internal set; }
		public T LowerBounds { get; internal set; }
		public T UpperBounds { get; internal set; }

		protected Gene(String name, T value, T lowerBounds, T upperBounds)
		{
			Name = name;
			CurrentValue = value;
			LowerBounds = lowerBounds;
			UpperBounds = upperBounds;
		}

		public override String ToString()
		{
			return CurrentValue.ToString();
		}
	}
}