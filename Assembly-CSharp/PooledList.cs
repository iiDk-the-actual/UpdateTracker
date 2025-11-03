using System;
using System.Collections.Generic;
using GorillaTag;

public class PooledList<T> : ObjectPoolEvents
{
	void ObjectPoolEvents.OnTaken()
	{
	}

	void ObjectPoolEvents.OnReturned()
	{
		this.List.Clear();
	}

	public List<T> List = new List<T>();
}
