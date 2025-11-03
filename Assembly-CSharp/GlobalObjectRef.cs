using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
public struct GlobalObjectRef
{
	public static GlobalObjectRef ObjectToRefSlow(Object target)
	{
		return default(GlobalObjectRef);
	}

	public static Object RefToObjectSlow(GlobalObjectRef @ref)
	{
		return null;
	}

	[FieldOffset(0)]
	public ulong targetObjectId;

	[FieldOffset(8)]
	public ulong targetPrefabId;

	[FieldOffset(16)]
	public Guid assetGUID;

	[HideInInspector]
	[FieldOffset(32)]
	public int identifierType;

	[NonSerialized]
	[FieldOffset(32)]
	private GlobalObjectRefType refType;
}
