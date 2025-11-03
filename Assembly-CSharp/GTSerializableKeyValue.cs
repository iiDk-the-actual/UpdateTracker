using System;

[Serializable]
public struct GTSerializableKeyValue<T1, T2>
{
	public GTSerializableKeyValue(T1 k, T2 v)
	{
		this.k = k;
		this.v = v;
	}

	public T1 k;

	public T2 v;
}
