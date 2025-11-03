using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class RCCosmeticNetworkSync : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
	{
		public void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			if (info.Sender == null)
			{
				this.DestroyThis();
				return;
			}
			if (info.Sender != base.photonView.Owner || base.photonView.IsRoomView)
			{
				GorillaNot.instance.SendReport("spoofed rc instantiate", info.Sender.UserId, info.Sender.NickName);
				this.DestroyThis();
				return;
			}
			object[] instantiationData = info.photonView.InstantiationData;
			if (instantiationData != null && instantiationData.Length >= 1)
			{
				object obj = instantiationData[0];
				if (obj is int)
				{
					int num = (int)obj;
					RigContainer rigContainer;
					if (VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender.ActorNumber), out rigContainer) && num > -1 && num < rigContainer.Rig.myBodyDockPositions.allObjects.Length)
					{
						this.rcRemote = rigContainer.Rig.myBodyDockPositions.allObjects[num] as RCRemoteHoldable;
						if (this.rcRemote != null)
						{
							this.rcRemote.networkSync = this;
							this.rcRemote.WakeUpRemoteVehicle();
						}
					}
					if (this.rcRemote == null)
					{
						this.DestroyThis();
					}
					return;
				}
			}
			this.DestroyThis();
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (info.Sender != base.photonView.Owner)
			{
				return;
			}
			if (stream.IsWriting)
			{
				stream.SendNext(this.syncedState.state);
				stream.SendNext(this.syncedState.position);
				stream.SendNext((int)BitPackUtils.PackRotation(this.syncedState.rotation, true));
				stream.SendNext(this.syncedState.dataA);
				stream.SendNext(this.syncedState.dataB);
				stream.SendNext(this.syncedState.dataC);
				return;
			}
			if (stream.IsReading)
			{
				byte state = this.syncedState.state;
				this.syncedState.state = (byte)stream.ReceiveNext();
				Vector3 vector = (Vector3)stream.ReceiveNext();
				(ref this.syncedState.position).SetValueSafe(in vector);
				Quaternion quaternion = BitPackUtils.UnpackRotation((uint)((int)stream.ReceiveNext()));
				(ref this.syncedState.rotation).SetValueSafe(in quaternion);
				this.syncedState.dataA = (byte)stream.ReceiveNext();
				this.syncedState.dataB = (byte)stream.ReceiveNext();
				this.syncedState.dataC = (byte)stream.ReceiveNext();
				if (state != this.syncedState.state && this.rcRemote != null && this.rcRemote.Vehicle != null && !this.rcRemote.Vehicle.enabled)
				{
					this.rcRemote.WakeUpRemoteVehicle();
				}
			}
		}

		[PunRPC]
		public void HitRCVehicleRPC(Vector3 hitVelocity, bool isProjectile, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "HitRCVehicleRPC");
			float num = 10000f;
			if (!(in hitVelocity).IsValid(in num))
			{
				GorillaNot.instance.SendReport("nan rc hit", info.Sender.UserId, info.Sender.NickName);
				return;
			}
			if (this.rcRemote != null && this.rcRemote.Vehicle != null)
			{
				this.rcRemote.Vehicle.AuthorityApplyImpact(hitVelocity, isProjectile);
			}
		}

		private void DestroyThis()
		{
			if (base.photonView.IsMine)
			{
				PhotonNetwork.Destroy(base.gameObject);
				return;
			}
			base.gameObject.SetActive(false);
		}

		public RCCosmeticNetworkSync.SyncedState syncedState;

		private RCRemoteHoldable rcRemote;

		public struct SyncedState
		{
			public byte state;

			public Vector3 position;

			public Quaternion rotation;

			public byte dataA;

			public byte dataB;

			public byte dataC;
		}
	}
}
