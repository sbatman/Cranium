using System;

namespace Cranium.Lib.Genetics.Genes
{
    public class BooleanGene : Gene<Boolean>
    {
        private static readonly Random _RND = new Random();
        public BooleanGene(String name, Boolean value) : base(name, value, false, true)
        {
        }

        public override void Cross(Gene otherGene)
        {
            CurrentValue = ((BooleanGene)otherGene).CurrentValue != CurrentValue ? _RND.NextDouble() > 0.5f : CurrentValue;
        }

        public override void Mutate()
        {
            CurrentValue = _RND.NextDouble() > 0.5f;
        }

        public override Gene Copy()
        {
            return new BooleanGene(Name, CurrentValue);
        }
    }
}