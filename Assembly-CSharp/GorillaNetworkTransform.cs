using System;
using System.Runtime.InteropServices;
using Fusion;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

[NetworkBehaviourWeaved(15)]
internal class GorillaNetworkTransform : NetworkComponent, ITickSystemTick
{
	public bool RespectOwnership
	{
		get
		{
			return this.respectOwnership;
		}
	}

	public bool TickRunning { get; set; }

	[Networked]
	[NetworkedWeaved(0, 15)]
	private unsafe GorillaNetworkTransform.NetTransformData data
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GorillaNetworkTransform.data. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(GorillaNetworkTransform.NetTransformData*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GorillaNetworkTransform.data. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(GorillaNetworkTransform.NetTransformData*)(this.Ptr + 0) = value;
		}
	}

	public new void Awake()
	{
		this.m_StoredPosition = base.transform.localPosition;
		this.m_NetworkPosition = Vector3.zero;
		this.m_NetworkScale = Vector3.zero;
		this.m_NetworkRotation = Quaternion.identity;
		this.maxDistanceSquare = this.maxDistance * this.maxDistance;
	}

	private new void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		this.m_firstTake = true;
		if (this.clampToSpawn)
		{
			this.clampOriginPoint = (this.m_UseLocal ? base.transform.localPosition : base.transform.position);
		}
		TickSystem<object>.AddTickCallback(this);
	}

	private new void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		TickSystem<object>.RemoveTickCallback(this);
	}

	public void Tick()
	{
		if (!base.IsLocallyOwned)
		{
			if (this.m_UseLocal)
			{
				base.transform.SetLocalPositionAndRotation(Vector3.MoveTowards(base.transform.localPosition, this.m_NetworkPosition, this.m_Distance * Time.deltaTime * (float)NetworkSystem.Instance.TickRate), Quaternion.RotateTowards(base.transform.localRotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime * (float)NetworkSystem.Instance.TickRate));
				return;
			}
			base.transform.SetPositionAndRotation(Vector3.MoveTowards(base.transform.position, this.m_NetworkPosition, this.m_Distance * Time.deltaTime * (float)NetworkSystem.Instance.TickRate), Quaternion.RotateTowards(base.transform.rotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime * (float)NetworkSystem.Instance.TickRate));
		}
	}

	public override void WriteDataFusion()
	{
		GorillaNetworkTransform.NetTransformData netTransformData = this.SharedWrite();
		double num = NetworkSystem.Instance.SimTick / 1000.0;
		netTransformData.SentTime = num;
		this.data = netTransformData;
	}

	public override void ReadDataFusion()
	{
		this.SharedRead(this.data);
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		if (this.respectOwnership && player != base.Owner)
		{
			return;
		}
		GorillaNetworkTransform.NetTransformData netTransformData = this.SharedWrite();
		if (this.m_SynchronizePosition)
		{
			stream.SendNext(netTransformData.position);
			stream.SendNext(netTransformData.velocity);
		}
		if (this.m_SynchronizeRotation)
		{
			stream.SendNext(netTransformData.rotation);
		}
		if (this.m_SynchronizeScale)
		{
			stream.SendNext(netTransformData.scale);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		if (this.respectOwnership && player != base.Owner)
		{
			return;
		}
		GorillaNetworkTransform.NetTransformData netTransformData = default(GorillaNetworkTransform.NetTransformData);
		if (this.m_SynchronizePosition)
		{
			netTransformData.position = (Vector3)stream.ReceiveNext();
			netTransformData.velocity = (Vector3)stream.ReceiveNext();
		}
		if (this.m_SynchronizeRotation)
		{
			netTransformData.rotation = (Quaternion)stream.ReceiveNext();
		}
		if (this.m_SynchronizeScale)
		{
			netTransformData.scale = (Vector3)stream.ReceiveNext();
		}
		netTransformData.SentTime = (double)((float)info.SentServerTime);
		this.SharedRead(netTransformData);
	}

	private void SharedRead(GorillaNetworkTransform.NetTransformData data)
	{
		if (this.m_SynchronizePosition)
		{
			(ref this.m_NetworkPosition).SetValueSafe(in data.position);
			(ref this.m_Velocity).SetValueSafe(in data.velocity);
			if (this.clampDistanceFromSpawn && Vector3.SqrMagnitude(this.clampOriginPoint - this.m_NetworkPosition) > this.maxDistanceSquare)
			{
				this.m_NetworkPosition = this.clampOriginPoint + this.m_Velocity.normalized * this.maxDistance;
				this.m_Velocity = Vector3.zero;
			}
			if (this.m_firstTake)
			{
				if (this.m_UseLocal)
				{
					base.transform.localPosition = this.m_NetworkPosition;
				}
				else
				{
					base.transform.position = this.m_NetworkPosition;
				}
				this.m_Distance = 0f;
			}
			else
			{
				float num = Mathf.Abs((float)(NetworkSystem.Instance.SimTime - data.SentTime));
				this.m_NetworkPosition += this.m_Velocity * num;
				if (this.m_UseLocal)
				{
					this.m_Distance = Vector3.Distance(base.transform.localPosition, this.m_NetworkPosition);
				}
				else
				{
					this.m_Distance = Vector3.Distance(base.transform.position, this.m_NetworkPosition);
				}
			}
		}
		if (this.m_SynchronizeRotation)
		{
			(ref this.m_NetworkRotation).SetValueSafe(in data.rotation);
			if (this.m_firstTake)
			{
				this.m_Angle = 0f;
				if (this.m_UseLocal)
				{
					base.transform.localRotation = this.m_NetworkRotation;
				}
				else
				{
					base.transform.rotation = this.m_NetworkRotation;
				}
			}
			else if (this.m_UseLocal)
			{
				this.m_Angle = Quaternion.Angle(base.transform.localRotation, this.m_NetworkRotation);
			}
			else
			{
				this.m_Angle = Quaternion.Angle(base.transform.rotation, this.m_NetworkRotation);
			}
		}
		if (this.m_SynchronizeScale)
		{
			(ref this.m_NetworkScale).SetValueSafe(in data.scale);
			base.transform.localScale = this.m_NetworkScale;
		}
		if (this.m_firstTake)
		{
			this.m_firstTake = false;
		}
	}

	private GorillaNetworkTransform.NetTransformData SharedWrite()
	{
		GorillaNetworkTransform.NetTransformData netTransformData = default(GorillaNetworkTransform.NetTransformData);
		if (this.m_SynchronizePosition)
		{
			if (this.m_UseLocal)
			{
				this.m_Velocity = base.transform.localPosition - this.m_StoredPosition;
				this.m_StoredPosition = base.transform.localPosition;
				netTransformData.position = base.transform.localPosition;
				netTransformData.velocity = this.m_Velocity;
			}
			else
			{
				this.m_Velocity = base.transform.position - this.m_StoredPosition;
				this.m_StoredPosition = base.transform.position;
				netTransformData.position = base.transform.position;
				netTransformData.velocity = this.m_Velocity;
			}
		}
		if (this.m_SynchronizeRotation)
		{
			if (this.m_UseLocal)
			{
				netTransformData.rotation = base.transform.localRotation;
			}
			else
			{
				netTransformData.rotation = base.transform.rotation;
			}
		}
		if (this.m_SynchronizeScale)
		{
			netTransformData.scale = base.transform.localScale;
		}
		return netTransformData;
	}

	public void GTAddition_DoTeleport()
	{
		this.m_firstTake = true;
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.data = this._data;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._data = this.data;
	}

	[Tooltip("Indicates if localPosition and localRotation should be used. Scale ignores this setting, and always uses localScale to avoid issues with lossyScale.")]
	public bool m_UseLocal;

	[SerializeField]
	private bool respectOwnership;

	[SerializeField]
	private bool clampDistanceFromSpawn = true;

	[SerializeField]
	private float maxDistance = 100f;

	private float maxDistanceSquare;

	[SerializeField]
	private bool clampToSpawn = true;

	[Tooltip("Use this if clampToSpawn is false, to set the center point to check the synced position against")]
	[SerializeField]
	private Vector3 clampOriginPoint;

	public bool m_SynchronizePosition = true;

	public bool m_SynchronizeRotation = true;

	public bool m_SynchronizeScale;

	private float m_Distance;

	private float m_Angle;

	private Vector3 m_Velocity;

	private Vector3 m_NetworkPosition;

	private Vector3 m_StoredPosition;

	private Vector3 m_NetworkScale;

	private Quaternion m_NetworkRotation;

	private bool m_firstTake;

	[WeaverGenerated]
	[DefaultForProperty("data", 0, 15)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private GorillaNetworkTransform.NetTransformData _data;

	[NetworkStructWeaved(15)]
	[StructLayout(LayoutKind.Explicit, Size = 60)]
	private struct NetTransformData : INetworkStruct
	{
		[FieldOffset(0)]
		public Vector3 position;

		[FieldOffset(12)]
		public Vector3 velocity;

		[FieldOffset(24)]
		public Quaternion rotation;

		[FieldOffset(40)]
		public Vector3 scale;

		[FieldOffset(52)]
		public double SentTime;
	}
}
