using System;
using GorillaExtensions;
using UnityEngine;

namespace Photon.Pun
{
	[RequireComponent(typeof(Rigidbody))]
	public class RigOwnedRigidbodyView : MonoBehaviourPun, IPunObservable
	{
		public bool IsMine { get; private set; }

		public void SetIsMine(bool isMine)
		{
			this.IsMine = isMine;
		}

		public void Awake()
		{
			this.m_Body = base.GetComponent<Rigidbody>();
			this.m_NetworkPosition = default(Vector3);
			this.m_NetworkRotation = default(Quaternion);
		}

		public void FixedUpdate()
		{
			if (!this.IsMine)
			{
				this.m_Body.position = Vector3.MoveTowards(this.m_Body.position, this.m_NetworkPosition, this.m_Distance * (1f / (float)PhotonNetwork.SerializationRate));
				this.m_Body.rotation = Quaternion.RotateTowards(this.m_Body.rotation, this.m_NetworkRotation, this.m_Angle * (1f / (float)PhotonNetwork.SerializationRate));
			}
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (info.Sender != info.photonView.Owner)
			{
				return;
			}
			try
			{
				if (stream.IsWriting)
				{
					stream.SendNext(this.m_Body.position);
					stream.SendNext(this.m_Body.rotation);
					if (this.m_SynchronizeVelocity)
					{
						stream.SendNext(this.m_Body.linearVelocity);
					}
					if (this.m_SynchronizeAngularVelocity)
					{
						stream.SendNext(this.m_Body.angularVelocity);
					}
					stream.SendNext(this.m_Body.IsSleeping());
				}
				else
				{
					Vector3 vector = (Vector3)stream.ReceiveNext();
					(ref this.m_NetworkPosition).SetValueSafe(in vector);
					Quaternion quaternion = (Quaternion)stream.ReceiveNext();
					(ref this.m_NetworkRotation).SetValueSafe(in quaternion);
					if (this.m_TeleportEnabled && Vector3.Distance(this.m_Body.position, this.m_NetworkPosition) > this.m_TeleportIfDistanceGreaterThan)
					{
						this.m_Body.position = this.m_NetworkPosition;
					}
					if (this.m_SynchronizeVelocity || this.m_SynchronizeAngularVelocity)
					{
						float num = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
						if (this.m_SynchronizeVelocity)
						{
							Vector3 vector2 = (Vector3)stream.ReceiveNext();
							float num2 = 10000f;
							if (!(in vector2).IsValid(in num2))
							{
								vector2 = Vector3.zero;
							}
							if (!this.m_Body.isKinematic)
							{
								this.m_Body.linearVelocity = vector2;
							}
							this.m_NetworkPosition += this.m_Body.linearVelocity * num;
							this.m_Distance = Vector3.Distance(this.m_Body.position, this.m_NetworkPosition);
						}
						if (this.m_SynchronizeAngularVelocity)
						{
							Vector3 vector3 = (Vector3)stream.ReceiveNext();
							float num2 = 10000f;
							if (!(in vector3).IsValid(in num2))
							{
								vector3 = Vector3.zero;
							}
							this.m_Body.angularVelocity = vector3;
							this.m_NetworkRotation = Quaternion.Euler(this.m_Body.angularVelocity * num) * this.m_NetworkRotation;
							this.m_Angle = Quaternion.Angle(this.m_Body.rotation, this.m_NetworkRotation);
						}
					}
					if ((bool)stream.ReceiveNext())
					{
						this.m_Body.Sleep();
					}
				}
			}
			catch
			{
			}
		}

		private float m_Distance;

		private float m_Angle;

		private Rigidbody m_Body;

		private Vector3 m_NetworkPosition;

		private Quaternion m_NetworkRotation;

		public bool m_SynchronizeVelocity = true;

		public bool m_SynchronizeAngularVelocity;

		public bool m_TeleportEnabled;

		public float m_TeleportIfDistanceGreaterThan = 3f;
	}
}
