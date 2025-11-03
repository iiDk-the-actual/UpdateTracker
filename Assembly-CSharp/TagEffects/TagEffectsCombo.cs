using System;

namespace TagEffects
{
	[Serializable]
	public class TagEffectsCombo : IEquatable<TagEffectsCombo>
	{
		bool IEquatable<TagEffectsCombo>.Equals(TagEffectsCombo other)
		{
			return (other.inputA == this.inputA && other.inputB == this.inputB) || (other.inputA == this.inputB && other.inputB == this.inputA);
		}

		public override bool Equals(object obj)
		{
			return this.Equals((TagEffectsCombo)obj);
		}

		public override int GetHashCode()
		{
			return this.inputA.GetHashCode() * this.inputB.GetHashCode();
		}

		public TagEffectPack inputA;

		public TagEffectPack inputB;
	}
}
