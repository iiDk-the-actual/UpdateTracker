using System;

namespace GorillaTag
{
	public abstract class DelegateListProcessorPlusMinus<T1, T2> : ListProcessorAbstract<T2> where T1 : DelegateListProcessorPlusMinus<T1, T2>, new() where T2 : Delegate
	{
		protected DelegateListProcessorPlusMinus()
		{
		}

		protected DelegateListProcessorPlusMinus(int capacity)
			: base(capacity)
		{
		}

		public static T1 operator +(DelegateListProcessorPlusMinus<T1, T2> left, T2 right)
		{
			if (left == null)
			{
				left = new T1();
			}
			if (right == null)
			{
				return (T1)((object)left);
			}
			left.Add(in right);
			return (T1)((object)left);
		}

		public static T1 operator -(DelegateListProcessorPlusMinus<T1, T2> left, T2 right)
		{
			if (left == null)
			{
				return default(T1);
			}
			if (right == null)
			{
				return (T1)((object)left);
			}
			left.Remove(in right);
			return (T1)((object)left);
		}
	}
}
