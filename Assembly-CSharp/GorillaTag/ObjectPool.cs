using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GorillaTag
{
	public class ObjectPool<T> where T : ObjectPoolEvents, new()
	{
		protected ObjectPool()
		{
		}

		public ObjectPool(int amount)
			: this(amount, amount)
		{
		}

		public ObjectPool(int initialAmount, int maxAmount)
		{
			this.InitializePool(initialAmount, maxAmount);
		}

		protected void InitializePool(int initialAmount, int maxAmount)
		{
			this.maxInstances = maxAmount;
			this.pool = new Stack<T>(initialAmount);
			for (int i = 0; i < initialAmount; i++)
			{
				this.pool.Push(this.CreateInstance());
			}
		}

		public T Take()
		{
			T t;
			if (this.pool.Count < 1)
			{
				t = this.CreateInstance();
			}
			else
			{
				t = this.pool.Pop();
			}
			t.OnTaken();
			return t;
		}

		public void Return(T instance)
		{
			instance.OnReturned();
			if (this.pool.Count == this.maxInstances)
			{
				return;
			}
			this.pool.Push(instance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual T CreateInstance()
		{
			return new T();
		}

		private Stack<T> pool;

		public int maxInstances = 500;
	}
}
