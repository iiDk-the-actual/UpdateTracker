using System;
using GorillaExtensions;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

namespace GorillaTagScripts
{
	public class BuilderItemReliableState : MonoBehaviour, IPunObservable
	{
		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.IsWriting)
			{
				stream.SendNext(this.rightHandAttachPos);
				stream.SendNext(this.rightHandAttachRot);
				stream.SendNext(this.leftHandAttachPos);
				stream.SendNext(this.leftHandAttachRot);
				return;
			}
			this.rightHandAttachPos = (Vector3)stream.ReceiveNext();
			this.rightHandAttachRot = (Quaternion)stream.ReceiveNext();
			this.leftHandAttachPos = (Vector3)stream.ReceiveNext();
			this.leftHandAttachRot = (Quaternion)stream.ReceiveNext();
			float num = 10000f;
			if (!(in this.rightHandAttachPos).IsValid(in num))
			{
				this.rightHandAttachPos = Vector3.zero;
			}
			if (!(in this.rightHandAttachRot).IsValid())
			{
				this.rightHandAttachRot = quaternion.identity;
			}
			num = 10000f;
			if (!(in this.leftHandAttachPos).IsValid(in num))
			{
				this.leftHandAttachPos = Vector3.zero;
			}
			if (!(in this.leftHandAttachRot).IsValid())
			{
				this.leftHandAttachRot = quaternion.identity;
			}
			this.dirty = true;
		}

		public Vector3 rightHandAttachPos = Vector3.zero;

		public Quaternion rightHandAttachRot = Quaternion.identity;

		public Vector3 leftHandAttachPos = Vector3.zero;

		public Quaternion leftHandAttachRot = Quaternion.identity;

		public bool dirty;
	}
}
