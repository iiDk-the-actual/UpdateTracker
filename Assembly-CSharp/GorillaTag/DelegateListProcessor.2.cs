using System;

namespace GorillaTag
{
	public class DelegateListProcessor<T> : DelegateListProcessorPlusMinus<DelegateListProcessor<T>, Action<T>>
	{
		public DelegateListProcessor()
		{
		}

		public DelegateListProcessor(int capacity)
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

		protected override void ProcessItem(in Action<T> item)
		{
			item(this.m_data);
		}

		private T m_data;
	}
}
