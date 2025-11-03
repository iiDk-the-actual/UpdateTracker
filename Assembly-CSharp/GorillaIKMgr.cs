using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GorillaIKMgr : MonoBehaviour
{
	public static GorillaIKMgr Instance
	{
		get
		{
			return GorillaIKMgr._instance;
		}
	}

	private void Awake()
	{
		GorillaIKMgr._instance = this;
		this.firstFrame = true;
		this.tAA = new TransformAccessArray(0, -1);
		this.transformList = new List<Transform>();
		this.job = new GorillaIKMgr.IKJob
		{
			constantInput = new NativeArray<GorillaIKMgr.IKConstantInput>(40, Allocator.Persistent, NativeArrayOptions.ClearMemory),
			input = new NativeArray<GorillaIKMgr.IKInput>(40, Allocator.Persistent, NativeArrayOptions.ClearMemory),
			output = new NativeArray<GorillaIKMgr.IKOutput>(40, Allocator.Persistent, NativeArrayOptions.ClearMemory)
		};
		this.jobXform = new GorillaIKMgr.IKTransformJob
		{
			transformRotations = new NativeArray<Quaternion>(140, Allocator.Persistent, NativeArrayOptions.ClearMemory)
		};
	}

	private void OnDestroy()
	{
		this.jobHandle.Complete();
		this.jobXformHandle.Complete();
		this.jobXform.transformRotations.Dispose();
		this.tAA.Dispose();
		this.job.input.Dispose();
		this.job.constantInput.Dispose();
		this.job.output.Dispose();
	}

	public void RegisterIK(GorillaIK ik)
	{
		this.ikList.Add(ik);
		this.actualListSz += 2;
		this.updatedSinceLastRun = true;
		if (this.job.constantInput.IsCreated)
		{
			this.SetConstantData(ik, this.actualListSz - 2);
		}
	}

	public void DeregisterIK(GorillaIK ik)
	{
		int num = this.ikList.FindIndex((GorillaIK curr) => curr == ik);
		this.updatedSinceLastRun = true;
		this.ikList.RemoveAt(num);
		this.actualListSz -= 2;
		if (this.job.constantInput.IsCreated)
		{
			for (int i = num; i < this.actualListSz; i++)
			{
				this.job.constantInput[i] = this.job.constantInput[i + 2];
			}
		}
	}

	private void SetConstantData(GorillaIK ik, int index)
	{
		this.job.constantInput[index] = new GorillaIKMgr.IKConstantInput
		{
			initRotLower = ik.initialLowerLeft,
			initRotUpper = ik.initialUpperLeft
		};
		this.job.constantInput[index + 1] = new GorillaIKMgr.IKConstantInput
		{
			initRotLower = ik.initialLowerRight,
			initRotUpper = ik.initialUpperRight
		};
	}

	private void CopyInput()
	{
		int num = 0;
		int i = 0;
		while (i < this.actualListSz)
		{
			GorillaIK gorillaIK = this.ikList[i / 2];
			this.job.input[i] = new GorillaIKMgr.IKInput
			{
				targetPos = gorillaIK.GetShoulderLocalTargetPos_Left()
			};
			this.job.input[i + 1] = new GorillaIKMgr.IKInput
			{
				targetPos = gorillaIK.GetShoulderLocalTargetPos_Right()
			};
			gorillaIK.ClearOverrides();
			i += 2;
			num++;
		}
	}

	private void CopyOutput()
	{
		bool flag = false;
		if (this.updatedSinceLastRun || this.tAA.length != this.ikList.Count * 7)
		{
			flag = true;
			this.tAA.Dispose();
			this.transformList.Clear();
		}
		for (int i = 0; i < this.ikList.Count; i++)
		{
			GorillaIK gorillaIK = this.ikList[i];
			if (flag || this.updatedSinceLastRun)
			{
				this.transformList.Add(gorillaIK.leftUpperArm);
				this.transformList.Add(gorillaIK.leftLowerArm);
				this.transformList.Add(gorillaIK.rightUpperArm);
				this.transformList.Add(gorillaIK.rightLowerArm);
				this.transformList.Add(gorillaIK.headBone);
				this.transformList.Add(gorillaIK.leftHand);
				this.transformList.Add(gorillaIK.rightHand);
			}
			this.jobXform.transformRotations[this.tFormCount * i] = this.job.output[i * 2].upperArmLocalRot;
			this.jobXform.transformRotations[this.tFormCount * i + 1] = this.job.output[i * 2].lowerArmLocalRot;
			this.jobXform.transformRotations[this.tFormCount * i + 2] = this.job.output[i * 2 + 1].upperArmLocalRot;
			this.jobXform.transformRotations[this.tFormCount * i + 3] = this.job.output[i * 2 + 1].lowerArmLocalRot;
			this.jobXform.transformRotations[this.tFormCount * i + 4] = gorillaIK.targetHead.rotation;
			this.jobXform.transformRotations[this.tFormCount * i + 5] = gorillaIK.targetLeft.rotation;
			this.jobXform.transformRotations[this.tFormCount * i + 6] = gorillaIK.targetRight.rotation;
		}
		if (flag)
		{
			this.tAA = new TransformAccessArray(this.transformList.ToArray(), -1);
		}
		this.updatedSinceLastRun = false;
	}

	public void LateUpdate()
	{
		if (!this.firstFrame)
		{
			this.jobXformHandle.Complete();
		}
		this.CopyInput();
		this.jobHandle = this.job.Schedule(this.actualListSz, 20, default(JobHandle));
		this.jobHandle.Complete();
		this.CopyOutput();
		this.jobXformHandle = this.jobXform.Schedule(this.tAA, default(JobHandle));
		this.firstFrame = false;
	}

	[OnEnterPlay_SetNull]
	private static GorillaIKMgr _instance;

	private const int MaxSize = 20;

	private List<GorillaIK> ikList = new List<GorillaIK>(20);

	private int actualListSz;

	private JobHandle jobHandle;

	private JobHandle jobXformHandle;

	private bool firstFrame = true;

	private TransformAccessArray tAA;

	private List<Transform> transformList;

	private bool updatedSinceLastRun;

	private int tFormCount = 7;

	private GorillaIKMgr.IKJob job;

	private GorillaIKMgr.IKTransformJob jobXform;

	private struct IKConstantInput
	{
		public Quaternion initRotLower;

		public Quaternion initRotUpper;
	}

	private struct IKInput
	{
		public Vector3 targetPos;
	}

	private struct IKOutput
	{
		public IKOutput(Quaternion upperArmLocalRot_, Quaternion lowerArmLocalRot_)
		{
			this.upperArmLocalRot = upperArmLocalRot_;
			this.lowerArmLocalRot = lowerArmLocalRot_;
		}

		public Quaternion upperArmLocalRot;

		public Quaternion lowerArmLocalRot;
	}

	[BurstCompile]
	private struct IKJob : IJobParallelFor
	{
		public void Execute(int i)
		{
			Quaternion initRotUpper = this.constantInput[i].initRotUpper;
			Vector3 vector = GorillaIKMgr.IKJob.upperArmLocalPos;
			Quaternion quaternion = initRotUpper * this.constantInput[i].initRotLower;
			Vector3 vector2 = vector + initRotUpper * GorillaIKMgr.IKJob.forearmLocalPos;
			Vector3 vector3 = vector2 + quaternion * GorillaIKMgr.IKJob.handLocalPos;
			float num = 0f;
			float magnitude = (vector - vector2).magnitude;
			float magnitude2 = (vector2 - vector3).magnitude;
			float num2 = magnitude + magnitude2 - num;
			Vector3 normalized = (vector3 - vector).normalized;
			Vector3 normalized2 = (vector2 - vector).normalized;
			Vector3 normalized3 = (vector3 - vector2).normalized;
			Vector3 normalized4 = (this.input[i].targetPos - vector).normalized;
			float num3 = Mathf.Clamp((this.input[i].targetPos - vector).magnitude, num, num2);
			float num4 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(normalized, normalized2), -1f, 1f));
			float num5 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(-normalized2, normalized3), -1f, 1f));
			float num6 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(normalized, normalized4), -1f, 1f));
			float num7 = Mathf.Acos(Mathf.Clamp((magnitude2 * magnitude2 - magnitude * magnitude - num3 * num3) / (-2f * magnitude * num3), -1f, 1f));
			float num8 = Mathf.Acos(Mathf.Clamp((num3 * num3 - magnitude * magnitude - magnitude2 * magnitude2) / (-2f * magnitude * magnitude2), -1f, 1f));
			Vector3 normalized5 = Vector3.Cross(normalized, normalized2).normalized;
			Vector3 normalized6 = Vector3.Cross(normalized, normalized4).normalized;
			Quaternion quaternion2 = Quaternion.AngleAxis((num7 - num4) * 57.29578f, Quaternion.Inverse(initRotUpper) * normalized5);
			Quaternion quaternion3 = Quaternion.AngleAxis((num8 - num5) * 57.29578f, Quaternion.Inverse(quaternion) * normalized5);
			Quaternion quaternion4 = Quaternion.AngleAxis(num6 * 57.29578f, Quaternion.Inverse(initRotUpper) * normalized6);
			Quaternion quaternion5 = this.constantInput[i].initRotUpper * quaternion4 * quaternion2;
			Quaternion quaternion6 = this.constantInput[i].initRotLower * quaternion3;
			this.output[i] = new GorillaIKMgr.IKOutput(quaternion5, quaternion6);
		}

		public NativeArray<GorillaIKMgr.IKConstantInput> constantInput;

		public NativeArray<GorillaIKMgr.IKInput> input;

		public NativeArray<GorillaIKMgr.IKOutput> output;

		private static readonly Vector3 upperArmLocalPos = new Vector3(-0.0002577677f, 0.1454885f, -0.02598158f);

		private static readonly Vector3 forearmLocalPos = new Vector3(4.204223E-06f, 0.4061671f, -1.043081E-06f);

		private static readonly Vector3 handLocalPos = new Vector3(3.073364E-08f, 0.3816895f, 1.117587E-08f);
	}

	[BurstCompile]
	private struct IKTransformJob : IJobParallelForTransform
	{
		public void Execute(int index, TransformAccess xform)
		{
			if (index % 7 <= 3)
			{
				xform.localRotation = this.transformRotations[index];
				return;
			}
			xform.rotation = this.transformRotations[index];
		}

		public NativeArray<Quaternion> transformRotations;
	}
}
