using System;

namespace GorillaTag
{
	public class InDelegateListProcessor<T> : DelegateListProcessorPlusMinus<InDelegateListProcessor<T>, InAction<T>>
	{
		public InDelegateListProcessor()
		{
		}

		public InDelegateListProcessor(int capacity)
			: base(capacity)
		{
		}

		public void InvokeSafe(in T data)
		{
			this.m_data = data;
			this.ProcessListSafe();
			this.m_data = default(T);
		}

		public void Invoke(in T data)
		{
			this.m_data = data;
			this.ProcessList();
			this.m_data = default(T);
		}

		protected override void ProcessItem(in InAction<T> item)
		{
			item(in this.m_data);
		}

		private T m_data;
	}
}
