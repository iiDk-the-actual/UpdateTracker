using System;
using System.Collections.Generic;

namespace GorillaExtensions
{
	public static class CollectionExtensions
	{
		public static void AddAll<T>(this ICollection<T> collection, IEnumerable<T> ts)
		{
			foreach (T t in ts)
			{
				collection.Add(t);
			}
		}
	}
}
