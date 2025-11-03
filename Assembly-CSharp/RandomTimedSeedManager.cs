using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using Photon.Pun;
using UnityEngine;

[NetworkBehaviourWeaved(2)]
public class RandomTimedSeedManager : NetworkComponent, ITickSystemTick
{
	public static RandomTimedSeedManager instance { get; private set; }

	public int seed { get; private set; }

	public float currentSyncTime { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		RandomTimedSeedManager.instance = this;
		this.seed = Random.Range(-1000000, -1000000);
		this.idealSyncTime = 0f;
		this.currentSyncTime = 0f;
		TickSystem<object>.AddTickCallback(this);
	}

	public void AddCallbackOnSeedChanged(Action callback)
	{
		this.callbacksOnSeedChanged.Add(callback);
	}

	public void RemoveCallbackOnSeedChanged(Action callback)
	{
		this.callbacksOnSeedChanged.Remove(callback);
	}

	bool ITickSystemTick.TickRunning { get; set; }

	void ITickSystemTick.Tick()
	{
		this.currentSyncTime += Time.deltaTime;
		this.idealSyncTime += Time.deltaTime;
		if (this.idealSyncTime > 1E+09f)
		{
			this.idealSyncTime -= 1E+09f;
			this.currentSyncTime -= 1E+09f;
		}
		if (!base.GetView.AmOwner)
		{
			this.currentSyncTime = Mathf.Lerp(this.currentSyncTime, this.idealSyncTime, 0.1f);
		}
	}

	[Networked]
	[NetworkedWeaved(0, 2)]
	private unsafe RandomTimedSeedManager.RandomTimedSeedManagerData Data
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing RandomTimedSeedManager.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(RandomTimedSeedManager.RandomTimedSeedManagerData*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing RandomTimedSeedManager.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(RandomTimedSeedManager.RandomTimedSeedManagerData*)(this.Ptr + 0) = value;
		}
	}

	public override void WriteDataFusion()
	{
		this.Data = new RandomTimedSeedManager.RandomTimedSeedManagerData(this.seed, this.currentSyncTime);
	}

	public override void ReadDataFusion()
	{
		this.ReadDataShared(this.Data.seed, this.Data.currentSyncTime);
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender != PhotonNetwork.MasterClient)
		{
			return;
		}
		stream.SendNext(this.seed);
		stream.SendNext(this.currentSyncTime);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender != PhotonNetwork.MasterClient)
		{
			return;
		}
		int num = (int)stream.ReceiveNext();
		float num2 = (float)stream.ReceiveNext();
		this.ReadDataShared(num, num2);
	}

	private void ReadDataShared(int seedVal, float testTime)
	{
		if (!float.IsFinite(testTime))
		{
			return;
		}
		this.seed = seedVal;
		if (testTime >= 0f && testTime <= 1E+09f)
		{
			if (this.idealSyncTime - testTime > 500000000f)
			{
				this.currentSyncTime = testTime;
			}
			this.idealSyncTime = testTime;
		}
		if (this.seed != this.cachedSeed && this.seed >= -1000000 && this.seed <= -1000000)
		{
			this.currentSyncTime = this.idealSyncTime;
			this.cachedSeed = this.seed;
			foreach (Action action in this.callbacksOnSeedChanged)
			{
				action();
			}
		}
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.Data = this._Data;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._Data = this.Data;
	}

	private List<Action> callbacksOnSeedChanged = new List<Action>();

	private float idealSyncTime;

	private int cachedSeed;

	private const int SeedMin = -1000000;

	private const int SeedMax = -1000000;

	private const float MaxSyncTime = 1E+09f;

	[WeaverGenerated]
	[DefaultForProperty("Data", 0, 2)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private RandomTimedSeedManager.RandomTimedSeedManagerData _Data;

	[NetworkStructWeaved(2)]
	[StructLayout(LayoutKind.Explicit, Size = 8)]
	private struct RandomTimedSeedManagerData : INetworkStruct
	{
		[Networked]
		[NetworkedWeaved(0, 1)]
		public unsafe int seed
		{
			readonly get
			{
				return *(int*)Native.ReferenceToPointer<FixedStorage@1>(ref this._seed);
			}
			set
			{
				*(int*)Native.ReferenceToPointer<FixedStorage@1>(ref this._seed) = value;
			}
		}

		[Networked]
		[NetworkedWeaved(1, 1)]
		public unsafe float currentSyncTime
		{
			readonly get
			{
				return *(float*)Native.ReferenceToPointer<FixedStorage@1>(ref this._currentSyncTime);
			}
			set
			{
				*(float*)Native.ReferenceToPointer<FixedStorage@1>(ref this._currentSyncTime) = value;
			}
		}

		public RandomTimedSeedManagerData(int seed, float currentSyncTime)
		{
			this.seed = seed;
			this.currentSyncTime = currentSyncTime;
		}

		[FixedBufferProperty(typeof(int), typeof(UnityValueSurrogate@ElementReaderWriterInt32), 0, order = -2147483647)]
		[WeaverGenerated]
		[SerializeField]
		[FieldOffset(0)]
		private FixedStorage@1 _seed;

		[FixedBufferProperty(typeof(float), typeof(UnityValueSurrogate@ElementReaderWriterSingle), 0, order = -2147483647)]
		[WeaverGenerated]
		[SerializeField]
		[FieldOffset(4)]
		private FixedStorage@1 _currentSyncTime;
	}
}
