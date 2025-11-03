using System;
using System.Collections.Generic;

public static class DictRefTypeUtils
{
	public static void TryGetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value) where TValue : class, new()
	{
		if (dict.TryGetValue(key, out value) && value != null)
		{
			return;
		}
		value = new TValue();
		dict.Add(key, value);
	}
}
