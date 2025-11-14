using System;

internal struct PlayIDWrappedData<T>
{
	public PlayIDWrappedData(T initialValue)
	{
		this.currentValue = initialValue;
		this.initialValue = initialValue;
		this.id = EnterPlayID.GetCurrent();
	}

	public T Value
	{
		get
		{
			if (!this.id.IsCurrent)
			{
				return this.initialValue;
			}
			return this.currentValue;
		}
		set
		{
			this.currentValue = value;
			this.id = EnterPlayID.GetCurrent();
		}
	}

	private T currentValue;

	private T initialValue;

	private EnterPlayID id;
}
