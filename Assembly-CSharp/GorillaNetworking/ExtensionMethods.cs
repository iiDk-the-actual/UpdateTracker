using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaNetworking
{
	public static class ExtensionMethods
	{
		public static void SafeInvoke<T>(this Action<T> action, T data)
		{
			try
			{
				if (action != null)
				{
					action(data);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("[PlayFabTitleDataCache::SafeInvoke] Failure invoking action: {0}", ex));
			}
		}

		public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if (dict.ContainsKey(key))
			{
				dict[key] = value;
				return;
			}
			dict.Add(key, value);
		}
	}
}
