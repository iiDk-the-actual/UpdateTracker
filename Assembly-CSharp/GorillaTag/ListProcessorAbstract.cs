using System;

namespace GorillaTag
{
	public abstract class ListProcessorAbstract<T> : ListProcessor<T>
	{
		protected ListProcessorAbstract()
		{
			this.m_itemProcessorDelegate = new InAction<T>(this.ProcessItem);
		}

		protected ListProcessorAbstract(int capacity)
			: base(capacity, null)
		{
			this.m_itemProcessorDelegate = new InAction<T>(this.ProcessItem);
		}

		protected abstract void ProcessItem(in T item);
	}
}
