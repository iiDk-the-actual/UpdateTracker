using System;
using Unity.Collections;
using UnityEngine.Jobs;

public struct BuilderTableMeshInstances
{
	public TransformAccessArray transforms;

	public NativeList<int> texIndex;

	public NativeList<float> tint;
}
