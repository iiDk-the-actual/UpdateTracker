using System;

namespace GorillaTag
{
	public class DelegateListProcessor : DelegateListProcessorPlusMinus<DelegateListProcessor, Action>
	{
		public DelegateListProcessor()
		{
		}

		public DelegateListProcessor(int capacity)
			: base(capacity)
		{
		}

		public void Invoke()
		{
			this.ProcessList();
		}

		public void InvokeSafe()
		{
			this.ProcessListSafe();
		}

		protected override void ProcessItem(in Action del)
		{
			del();
		}
	}
}
