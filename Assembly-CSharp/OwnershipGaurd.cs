using System;
using Photon.Pun;
using UnityEngine;

internal class OwnershipGaurd : MonoBehaviour
{
	private void Start()
	{
		if (this.autoRegisterAll)
		{
			this.NetViews = base.GetComponents<PhotonView>();
		}
		if (this.NetViews == null)
		{
			return;
		}
		OwnershipGaurdHandler.RegisterViews(this.NetViews);
	}

	private void OnDestroy()
	{
		if (this.NetViews == null)
		{
			return;
		}
		OwnershipGaurdHandler.RemoveViews(this.NetViews);
	}

	[SerializeField]
	private PhotonView[] NetViews;

	[SerializeField]
	private bool autoRegisterAll = true;
}
