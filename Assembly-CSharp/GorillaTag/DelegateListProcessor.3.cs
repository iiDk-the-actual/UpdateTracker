using System;

namespace GorillaTag
{
	public class DelegateListProcessor<T1, T2> : DelegateListProcessorPlusMinus<DelegateListProcessor<T1, T2>, Action<T1, T2>>
	{
		public DelegateListProcessor()
		{
		}

		public DelegateListProcessor(int capacity)
			: base(capacity)
		{
		}

		public void InvokeSafe(in T1 data1, in T2 data2)
		{
			this.SetData(in data1, in data2);
			this.ProcessListSafe();
			this.ResetData();
		}

		public void Invoke(in T1 data1, in T2 data2)
		{
			this.SetData(in data1, in data2);
			this.ProcessList();
			this.ResetData();
		}

		protected override void ProcessItem(in Action<T1, T2> item)
		{
			item(this.m_data1, this.m_data2);
		}

		private void SetData(in T1 data1, in T2 data2)
		{
			this.m_data1 = data1;
			this.m_data2 = data2;
		}

		private void ResetData()
		{
			this.m_data1 = default(T1);
			this.m_data2 = default(T2);
		}

		private T1 m_data1;

		private T2 m_data2;
	}
}
