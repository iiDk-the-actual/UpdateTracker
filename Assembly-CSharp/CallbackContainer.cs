using System;
using GorillaTag;

internal class CallbackContainer<T> : ListProcessorAbstract<T> where T : ICallBack
{
	public CallbackContainer()
		: base(100)
	{
	}

	public CallbackContainer(int capacity)
		: base(capacity)
	{
	}

	public void TryRunCallbacks()
	{
		this.ProcessListSafe();
	}

	protected override void ProcessItem(in T item)
	{
		T t = item;
		t.CallBack();
	}
}
