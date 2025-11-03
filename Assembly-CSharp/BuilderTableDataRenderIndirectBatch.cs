using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

public class BuilderTableDataRenderIndirectBatch
{
	public int totalInstances;

	public TransformAccessArray instanceTransform;

	public NativeArray<int> instanceTransformIndexToDataIndex;

	public List<int> pieceIDPerTransform;

	public NativeArray<Matrix4x4> instanceObjectToWorld;

	public NativeArray<int> instanceTexIndex;

	public NativeArray<float> instanceTint;

	public NativeArray<int> instanceLodLevel;

	public NativeArray<int> instanceLodLevelDirty;

	public NativeList<BuilderTableMeshInstances> renderMeshes;

	public GraphicsBuffer commandBuf;

	public GraphicsBuffer matrixBuf;

	public GraphicsBuffer texIndexBuf;

	public GraphicsBuffer tintBuf;

	public NativeArray<GraphicsBuffer.IndirectDrawIndexedArgs> commandData;

	public int commandCount;

	public RenderParams rp;
}
