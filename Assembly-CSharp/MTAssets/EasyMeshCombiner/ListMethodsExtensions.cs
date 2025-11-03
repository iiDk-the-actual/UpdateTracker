using System;
using System.Collections.Generic;

namespace MTAssets.EasyMeshCombiner
{
	public static class ListMethodsExtensions
	{
		public static void RemoveAllNullItems<T>(this List<T> list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i] == null)
				{
					list.RemoveAt(i);
				}
			}
		}
	}
}
