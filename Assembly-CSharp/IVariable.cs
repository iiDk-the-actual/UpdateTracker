using System;

public interface IVariable<T> : IVariable
{
	T Value
	{
		get
		{
			return this.Get();
		}
		set
		{
			this.Set(value);
		}
	}

	T Get();

	void Set(T value);

	Type IVariable.ValueType
	{
		get
		{
			return typeof(T);
		}
	}
}
