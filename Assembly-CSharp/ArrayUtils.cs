using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class ArrayUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearch<T>(this T[] array, T value) where T : IComparable<T>
	{
		return Array.BinarySearch<T>(array, 0, array.Length, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty<T>(this T[] array)
	{
		return array == null || array.Length == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty<T>(this List<T> list)
	{
		return list == null || list.Count == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Swap<T>(this T[] array, int from, int to)
	{
		T t = array[from];
		T t2 = array[to];
		array[to] = t;
		array[from] = t2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Swap<T>(this List<T> list, int from, int to)
	{
		T t = list[from];
		T t2 = list[to];
		list[to] = t;
		list[from] = t2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Clone<T>(T[] source)
	{
		if (source == null)
		{
			return null;
		}
		if (source.Length == 0)
		{
			return Array.Empty<T>();
		}
		T[] array = new T[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			array[i] = source[i];
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<T> Clone<T>(List<T> source)
	{
		if (source == null)
		{
			return null;
		}
		if (source.Count == 0)
		{
			return new List<T>();
		}
		return new List<T>(source);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOfRef<T>(this T[] array, T value) where T : class
	{
		if (array == null || array.Length == 0)
		{
			return -1;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOfRef<T>(this List<T> list, T value) where T : class
	{
		if (list == null || list.Count == 0)
		{
			return -1;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool GTEnsureNoNulls<T>(ref T[] unityObjs) where T : Object
	{
		if (unityObjs == null)
		{
			unityObjs = Array.Empty<T>();
		}
		int num = 0;
		for (int i = 0; i < unityObjs.Length; i++)
		{
			if (!(unityObjs[i] == null))
			{
				unityObjs[num] = unityObjs[i];
				num++;
			}
		}
		bool flag = num != unityObjs.Length;
		if (flag)
		{
			Array.Resize<T>(ref unityObjs, num);
		}
		return flag;
	}
}
