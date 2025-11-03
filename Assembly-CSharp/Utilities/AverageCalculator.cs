using System;
using System.Runtime.CompilerServices;

namespace Utilities
{
	public abstract class AverageCalculator<T> where T : struct
	{
		public T Average
		{
			get
			{
				return this.m_average;
			}
		}

		public AverageCalculator(int sampleCount)
		{
			this.m_samples = new T[sampleCount];
		}

		public virtual void AddSample(T sample)
		{
			T t = this.m_samples[this.m_index];
			this.m_total = this.MinusEquals(this.m_total, t);
			this.m_total = this.PlusEquals(this.m_total, sample);
			this.m_average = this.Divide(this.m_total, this.m_samples.Length);
			this.m_samples[this.m_index] = sample;
			int num = this.m_index + 1;
			this.m_index = num;
			this.m_index = num % this.m_samples.Length;
		}

		public virtual void Reset()
		{
			T t = this.DefaultTypeValue();
			for (int i = 0; i < this.m_samples.Length; i++)
			{
				this.m_samples[i] = t;
			}
			this.m_index = 0;
			this.m_average = t;
			this.m_total = this.Multiply(t, this.m_samples.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual T DefaultTypeValue()
		{
			return default(T);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract T PlusEquals(T value, T sample);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract T MinusEquals(T value, T sample);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract T Divide(T value, int sampleCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract T Multiply(T value, int sampleCount);

		private T[] m_samples;

		private T m_average;

		private T m_total;

		private int m_index;
	}
}
