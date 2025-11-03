using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[DefaultExecutionOrder(0)]
public class VRRigJobManager : MonoBehaviour
{
	public static VRRigJobManager Instance
	{
		get
		{
			return VRRigJobManager._instance;
		}
	}

	private void Awake()
	{
		VRRigJobManager._instance = this;
		this.cachedInput = new NativeArray<VRRigJobManager.VRRigTransformInput>(9, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		this.tAA = new TransformAccessArray(9, 2);
		this.job = default(VRRigJobManager.VRRigTransformJob);
	}

	private void OnDestroy()
	{
		this.jobHandle.Complete();
		this.cachedInput.Dispose();
		this.tAA.Dispose();
	}

	public void RegisterVRRig(VRRig rig)
	{
		this.rigList.Add(rig);
		this.tAA.Add(rig.transform);
		this.actualListSz++;
	}

	public void DeregisterVRRig(VRRig rig)
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.rigList.Remove(rig);
		for (int i = this.actualListSz - 1; i >= 0; i--)
		{
			if (this.tAA[i] == rig.transform)
			{
				this.tAA.RemoveAtSwapBack(i);
				break;
			}
		}
		this.actualListSz--;
	}

	private void CopyInput()
	{
		for (int i = 0; i < this.actualListSz; i++)
		{
			this.cachedInput[i] = new VRRigJobManager.VRRigTransformInput
			{
				rigPosition = this.rigList[i].jobPos,
				rigRotaton = this.rigList[i].jobRotation
			};
			this.tAA[i] = this.rigList[i].transform;
		}
	}

	public void Update()
	{
		this.jobHandle.Complete();
		for (int i = 0; i < this.rigList.Count; i++)
		{
			this.rigList[i].RemoteRigUpdate();
		}
		this.CopyInput();
		this.job.input = this.cachedInput;
		this.jobHandle = this.job.Schedule(this.tAA, default(JobHandle));
	}

	[OnEnterPlay_SetNull]
	private static VRRigJobManager _instance;

	private const int MaxSize = 9;

	private const int questJobThreads = 2;

	private List<VRRig> rigList = new List<VRRig>(9);

	private NativeArray<VRRigJobManager.VRRigTransformInput> cachedInput;

	private TransformAccessArray tAA;

	private int actualListSz;

	private JobHandle jobHandle;

	private VRRigJobManager.VRRigTransformJob job;

	private struct VRRigTransformInput
	{
		public Vector3 rigPosition;

		public Quaternion rigRotaton;
	}

	[BurstCompile]
	private struct VRRigTransformJob : IJobParallelForTransform
	{
		public void Execute(int i, TransformAccess tA)
		{
			if (i < this.input.Length)
			{
				tA.position = this.input[i].rigPosition;
				tA.rotation = this.input[i].rigRotaton;
			}
		}

		[ReadOnly]
		public NativeArray<VRRigJobManager.VRRigTransformInput> input;
	}
}
