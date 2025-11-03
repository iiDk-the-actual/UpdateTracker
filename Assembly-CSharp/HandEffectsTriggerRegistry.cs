using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaTag.Shared.Scripts.Utilities;
using TagEffects;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class HandEffectsTriggerRegistry : MonoBehaviour, ITickSystemTick, ITickSystemPost
{
	public bool TickRunning { get; set; }

	public bool PostTickRunning { get; set; }

	public static HandEffectsTriggerRegistry Instance { get; private set; }

	public static bool HasInstance { get; private set; }

	public static void FindInstance()
	{
		HandEffectsTriggerRegistry.Instance = Object.FindAnyObjectByType<HandEffectsTriggerRegistry>();
		HandEffectsTriggerRegistry.HasInstance = true;
	}

	private void Awake()
	{
		HandEffectsTriggerRegistry.Instance = this;
		HandEffectsTriggerRegistry.HasInstance = true;
		this.job = new HandEffectsTriggerRegistry.HandEffectsJob
		{
			positionInput = new NativeArray<Vector3>(30, Allocator.Persistent, NativeArrayOptions.ClearMemory),
			closeOutput = new NativeArray<bool>(900, Allocator.Persistent, NativeArrayOptions.ClearMemory),
			actualListSize = this.actualListSz
		};
	}

	private void OnEnable()
	{
		TickSystem<object>.AddCallbackTarget(this);
		TickSystem<object>.AddPostTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveCallbackTarget(this);
		TickSystem<object>.RemovePostTickCallback(this);
	}

	public void Register(IHandEffectsTrigger trigger)
	{
		if (this.triggers.Count < 30)
		{
			this.actualListSz++;
			this.triggers.Add(trigger);
		}
	}

	public void Unregister(IHandEffectsTrigger trigger)
	{
		int num = this.triggers.IndexOf(trigger);
		if (num >= 0)
		{
			this.actualListSz--;
			this.triggers.RemoveAt(num);
		}
	}

	private void OnDestroy()
	{
		if (!this.jobHandle.IsCompleted)
		{
			this.jobHandle.Complete();
		}
		this.job.Dispose();
	}

	public void Tick()
	{
		this.CopyInput();
		this.jobHandle = this.job.Schedule(this.actualListSz, 20, default(JobHandle));
	}

	public void PostTick()
	{
		this.jobHandle.Complete();
		this.CheckForHandEffectOnProcessedOutput();
	}

	public void CheckForHandEffectOnProcessedOutput()
	{
		this.newCollisionBits.Clear();
		for (int i = 0; i < this.triggers.Count; i++)
		{
			IHandEffectsTrigger handEffectsTrigger = this.triggers[i];
			int num = i * 30;
			for (int j = i + 1; j < this.triggers.Count; j++)
			{
				if (this.job.closeOutput[i * 30 + j])
				{
					IHandEffectsTrigger handEffectsTrigger2 = this.triggers[j];
					if (handEffectsTrigger.InTriggerZone(handEffectsTrigger2) || handEffectsTrigger2.InTriggerZone(handEffectsTrigger))
					{
						int num2 = num + j;
						this.newCollisionBits[num2] = true;
						if (!this.existingCollisionBits[num2] && Time.time - this.triggerTimes[i] > 0.5f && Time.time - this.triggerTimes[j] > 0.5f)
						{
							handEffectsTrigger.OnTriggerEntered(handEffectsTrigger2);
							handEffectsTrigger2.OnTriggerEntered(handEffectsTrigger);
							this.triggerTimes[i] = (this.triggerTimes[j] = Time.time);
						}
					}
				}
			}
		}
		this.existingCollisionBits.CopyFrom(this.newCollisionBits);
	}

	private void CopyInput()
	{
		for (int i = 0; i < this.actualListSz; i++)
		{
			this.job.positionInput[i] = this.triggers[i].Transform.position;
		}
		if (this.job.actualListSize != this.actualListSz)
		{
			this.job.actualListSize = this.actualListSz;
		}
	}

	private const int MAX_TRIGGERS = 30;

	private const int BIT_ARRAY_SIZE = 900;

	private const float COOLDOWN_TIME = 0.5f;

	private const float DEFAULT_RADIUS = 0.5f;

	private readonly List<IHandEffectsTrigger> triggers = new List<IHandEffectsTrigger>();

	private readonly float[] triggerTimes = new float[30];

	private readonly GTBitArray existingCollisionBits = new GTBitArray(900);

	private readonly GTBitArray newCollisionBits = new GTBitArray(900);

	private int actualListSz;

	private JobHandle jobHandle;

	private HandEffectsTriggerRegistry.HandEffectsJob job;

	[BurstCompile]
	private struct HandEffectsJob : IJobParallelFor, IDisposable
	{
		public void Execute(int i)
		{
			for (int j = i + 1; j < this.actualListSize; j++)
			{
				this.closeOutput[i * 30 + j] = (this.positionInput[i] - this.positionInput[j]).IsShorterThan(0.5f);
			}
		}

		public void Dispose()
		{
			this.positionInput.Dispose();
			this.closeOutput.Dispose();
		}

		[NativeDisableParallelForRestriction]
		public NativeArray<Vector3> positionInput;

		[NativeDisableParallelForRestriction]
		public NativeArray<bool> closeOutput;

		public int actualListSize;
	}
}
