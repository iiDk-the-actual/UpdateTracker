using System;
using System.Collections.Generic;

namespace PerformanceSystems
{
	public static class FastRemoveExtensions
	{
		public static bool FastRemove<T>(this List<T> list, T itemToRemove)
		{
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			int count = list.Count;
			if (count == 0)
			{
				return false;
			}
			int num = count - 1;
			for (int i = 0; i < count; i++)
			{
				if (@default.Equals(list[i], itemToRemove))
				{
					list[i] = list[num];
					list.RemoveAt(num);
					return true;
				}
			}
			return false;
		}

		public static bool FastRemove<T>(this List<T> list, HashSet<T> setToRemove)
		{
			if (setToRemove == null || setToRemove.Count == 0 || list.Count == 0)
			{
				return false;
			}
			bool flag = false;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				T t = list[i];
				if (setToRemove.Contains(t))
				{
					int num = list.Count - 1;
					list[i] = list[num];
					list.RemoveAt(num);
					flag = true;
				}
			}
			return flag;
		}
	}
}
