using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.EasyMeshCombiner
{
	[AddComponentMenu("")]
	public class MTAssetsMathematics : MonoBehaviour
	{
		public static List<T> RandomizeThisList<T>(List<T> list)
		{
			int count = list.Count;
			int num = count - 1;
			for (int i = 0; i < num; i++)
			{
				int num2 = Random.Range(i, count);
				T t = list[i];
				list[i] = list[num2];
				list[num2] = t;
			}
			return list;
		}

		public static Vector3 GetHalfPositionBetweenTwoPoints(Vector3 pointA, Vector3 pointB)
		{
			return Vector3.Lerp(pointA, pointB, 0.5f);
		}
	}
}
