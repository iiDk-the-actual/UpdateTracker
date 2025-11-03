using System;
using GorillaExtensions;
using UnityEngine;

namespace Photon.Pun
{
	[HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
	public class RigOwnedTransformView : MonoBehaviourPun, IPunObservable
	{
		public bool IsMine { get; private set; }

		public void SetIsMine(bool isMine)
		{
			this.IsMine = isMine;
		}

		public void Awake()
		{
			this.m_StoredPosition = base.transform.localPosition;
			this.m_NetworkPosition = Vector3.zero;
			this.m_networkScale = Vector3.one;
			this.m_NetworkRotation = Quaternion.identity;
		}

		private void Reset()
		{
			this.m_UseLocal = true;
		}

		private void OnEnable()
		{
			this.m_firstTake = true;
		}

		public void Update()
		{
			Transform transform = base.transform;
			if (!this.IsMine && this.IsValid(this.m_NetworkPosition) && this.IsValid(this.m_NetworkRotation))
			{
				if (this.m_UseLocal)
				{
					transform.localPosition = Vector3.MoveTowards(transform.localPosition, this.m_NetworkPosition, this.m_Distance * Time.deltaTime * (float)PhotonNetwork.SerializationRate);
					transform.localRotation = Quaternion.RotateTowards(transform.localRotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime * (float)PhotonNetwork.SerializationRate);
					return;
				}
				transform.position = Vector3.MoveTowards(transform.position, this.m_NetworkPosition, this.m_Distance * Time.deltaTime * (float)PhotonNetwork.SerializationRate);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime * (float)PhotonNetwork.SerializationRate);
			}
		}

		private bool IsValid(Vector3 v)
		{
			return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) && !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
		}

		private bool IsValid(Quaternion q)
		{
			return !float.IsNaN(q.x) && !float.IsNaN(q.y) && !float.IsNaN(q.z) && !float.IsNaN(q.w) && !float.IsInfinity(q.x) && !float.IsInfinity(q.y) && !float.IsInfinity(q.z) && !float.IsInfinity(q.w);
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (info.Sender != info.photonView.Owner)
			{
				return;
			}
			try
			{
				Transform transform = base.transform;
				if (stream.IsWriting)
				{
					if (this.m_SynchronizePosition)
					{
						if (this.m_UseLocal)
						{
							this.m_Direction = transform.localPosition - this.m_StoredPosition;
							this.m_StoredPosition = transform.localPosition;
							stream.SendNext(transform.localPosition);
							stream.SendNext(this.m_Direction);
						}
						else
						{
							this.m_Direction = transform.position - this.m_StoredPosition;
							this.m_StoredPosition = transform.position;
							stream.SendNext(transform.position);
							stream.SendNext(this.m_Direction);
						}
					}
					if (this.m_SynchronizeRotation)
					{
						if (this.m_UseLocal)
						{
							stream.SendNext(transform.localRotation);
						}
						else
						{
							stream.SendNext(transform.rotation);
						}
					}
					if (this.m_SynchronizeScale)
					{
						stream.SendNext(transform.localScale);
					}
				}
				else
				{
					if (this.m_SynchronizePosition)
					{
						Vector3 vector = (Vector3)stream.ReceiveNext();
						(ref this.m_NetworkPosition).SetValueSafe(in vector);
						vector = (Vector3)stream.ReceiveNext();
						(ref this.m_Direction).SetValueSafe(in vector);
						if (this.m_firstTake)
						{
							if (this.m_UseLocal)
							{
								transform.localPosition = this.m_NetworkPosition;
							}
							else
							{
								transform.position = this.m_NetworkPosition;
							}
							this.m_Distance = 0f;
						}
						else
						{
							float num = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
							this.m_NetworkPosition += this.m_Direction * num;
							if (this.m_UseLocal)
							{
								this.m_Distance = Vector3.Distance(transform.localPosition, this.m_NetworkPosition);
							}
							else
							{
								this.m_Distance = Vector3.Distance(transform.position, this.m_NetworkPosition);
							}
						}
					}
					if (this.m_SynchronizeRotation)
					{
						Quaternion quaternion = (Quaternion)stream.ReceiveNext();
						(ref this.m_NetworkRotation).SetValueSafe(in quaternion);
						if (this.m_firstTake)
						{
							this.m_Angle = 0f;
							if (this.m_UseLocal)
							{
								transform.localRotation = this.m_NetworkRotation;
							}
							else
							{
								transform.rotation = this.m_NetworkRotation;
							}
						}
						else if (this.m_UseLocal)
						{
							this.m_Angle = Quaternion.Angle(transform.localRotation, this.m_NetworkRotation);
						}
						else
						{
							this.m_Angle = Quaternion.Angle(transform.rotation, this.m_NetworkRotation);
						}
					}
					if (this.m_SynchronizeScale)
					{
						Vector3 vector = (Vector3)stream.ReceiveNext();
						(ref this.m_networkScale).SetValueSafe(in vector);
						transform.localScale = this.m_networkScale;
					}
					if (this.m_firstTake)
					{
						this.m_firstTake = false;
					}
				}
			}
			catch
			{
			}
		}

		public void GTAddition_DoTeleport()
		{
			this.m_firstTake = true;
		}

		private float m_Distance;

		private float m_Angle;

		private Vector3 m_Direction;

		private Vector3 m_NetworkPosition;

		private Vector3 m_StoredPosition;

		private Vector3 m_networkScale;

		private Quaternion m_NetworkRotation;

		public bool m_SynchronizePosition = true;

		public bool m_SynchronizeRotation = true;

		public bool m_SynchronizeScale;

		[Tooltip("Indicates if localPosition and localRotation should be used. Scale ignores this setting, and always uses localScale to avoid issues with lossyScale.")]
		public bool m_UseLocal;

		private bool m_firstTake;
	}
}
