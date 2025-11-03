using System;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MaterialCyclerNetworked : MonoBehaviour
{
	public float SyncTimeOut
	{
		get
		{
			return this.syncTimeOut;
		}
	}

	public event Action<int, int3> OnSynchronize;

	private void Awake()
	{
		this.photonView = base.GetComponent<PhotonView>();
	}

	public void Synchronize(int materialIndex, Color c)
	{
		if (!this.masterClientOnly || PhotonNetwork.IsMasterClient)
		{
			int num = Mathf.CeilToInt(c.r * 9f);
			int num2 = Mathf.CeilToInt(c.g * 9f);
			int num3 = Mathf.CeilToInt(c.b * 9f);
			int num4 = num | (num2 << 8) | (num3 << 16);
			this.photonView.RPC("RPC_SynchronizePacked", RpcTarget.Others, new object[] { materialIndex, num4 });
		}
	}

	[PunRPC]
	public void RPC_SynchronizePacked(int index, int colourPacked, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RPC_SynchronizePacked");
		RigContainer rigContainer;
		if (this.OnSynchronize == null || (this.masterClientOnly && !info.Sender.IsMasterClient) || !VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer) || !rigContainer.Rig.IsPositionInRange(base.transform.position, 5f) || !FXSystem.CheckCallSpam(rigContainer.Rig.fxSettings, 21, info.SentServerTime))
		{
			return;
		}
		int num = colourPacked & 255;
		int num2 = (colourPacked >> 8) & 255;
		int num3 = (colourPacked >> 16) & 255;
		num = Mathf.Clamp(num, 0, 9);
		num2 = Mathf.Clamp(num2, 0, 9);
		num3 = Mathf.Clamp(num3, 0, 9);
		this.OnSynchronize(index, new int3(num, num2, num3));
	}

	[SerializeField]
	private float syncTimeOut = 1f;

	private PhotonView photonView;

	[SerializeField]
	private bool masterClientOnly;
}
