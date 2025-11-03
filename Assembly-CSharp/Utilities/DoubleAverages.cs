using System;
using System.Runtime.CompilerServices;

namespace Utilities
{
	public class DoubleAverages : AverageCalculator<double>
	{
		public DoubleAverages(int sampleCount)
			: base(sampleCount)
		{
			this.Reset();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override double PlusEquals(double value, double sample)
		{
			return value + sample;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override double MinusEquals(double value, double sample)
		{
			return value - sample;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override double Divide(double value, int sampleCount)
		{
			return value / (double)sampleCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override double Multiply(double value, int sampleCount)
		{
			return value * (double)sampleCount;
		}
	}
}
