using System;
using System.Runtime.CompilerServices;

namespace Utilities
{
	public class FloatAverages : AverageCalculator<float>
	{
		public FloatAverages(int sampleCount)
			: base(sampleCount)
		{
			this.Reset();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override float PlusEquals(float value, float sample)
		{
			return value + sample;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override float MinusEquals(float value, float sample)
		{
			return value - sample;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override float Divide(float value, int sampleCount)
		{
			return value / (float)sampleCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override float Multiply(float value, int sampleCount)
		{
			return value * (float)sampleCount;
		}
	}
}
