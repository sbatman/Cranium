// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project Cranium
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cranium.Lib.Genetics.Genes;

namespace Cranium.Lib.Genetics
{
	public class DNA
	{
		private static readonly Random _RND = new Random();
		private readonly Dictionary<String, Gene> _Genes = new Dictionary<String, Gene>();

		public Double Score { get; set; }

		public DNA(IEnumerable<Gene> genes, Double score = 0)
		{
			Score = score;
			foreach (Gene gene in genes)
			{
				_Genes.Add(gene.Name, gene);
			}
		}

		public Gene GetGene(String name)
		{
			return _Genes[name];
		}

		public T GetGene<T>(String name) where T : Gene
		{
			return (T) _Genes[name];
		}

		public void Mutate(Single strength)
		{
			foreach (KeyValuePair<String, Gene> keyValuePair in _Genes)
			{
				if (_RND.NextDouble() < strength) keyValuePair.Value.Mutate();
			}
		}

		public void Cross(DNA otherDNA)
		{
			foreach (KeyValuePair<String, Gene> keyValuePair in _Genes)
			{
				keyValuePair.Value.Cross(otherDNA.GetGene(keyValuePair.Key));
			}
		}

		public DNA Copy()
		{
			return new DNA(_Genes.Select(gene => gene.Value.Copy()), Score);
		}

		public override String ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("DNA ");
			foreach (Gene gene in _Genes.Values)
			{
				stringBuilder.Append($"{gene.Name}:{gene},");
			}

			return stringBuilder.ToString();
		}
	}
}