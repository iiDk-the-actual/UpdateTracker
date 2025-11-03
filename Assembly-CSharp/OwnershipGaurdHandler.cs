using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

internal class OwnershipGaurdHandler : IPunOwnershipCallbacks
{
	static OwnershipGaurdHandler()
	{
		PhotonNetwork.AddCallbackTarget(OwnershipGaurdHandler.callbackInstance);
	}

	internal static void RegisterView(PhotonView view)
	{
		if (view == null || OwnershipGaurdHandler.gaurdedViews.Contains(view))
		{
			return;
		}
		OwnershipGaurdHandler.gaurdedViews.Add(view);
	}

	internal static void RegisterViews(PhotonView[] photonViews)
	{
		for (int i = 0; i < photonViews.Length; i++)
		{
			OwnershipGaurdHandler.RegisterView(photonViews[i]);
		}
	}

	internal static void RemoveView(PhotonView view)
	{
		if (view == null)
		{
			return;
		}
		OwnershipGaurdHandler.gaurdedViews.Remove(view);
	}

	internal static void RemoveViews(PhotonView[] photonViews)
	{
		for (int i = 0; i < photonViews.Length; i++)
		{
			OwnershipGaurdHandler.RemoveView(photonViews[i]);
		}
	}

	void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
	{
		if (!OwnershipGaurdHandler.gaurdedViews.Contains(targetView))
		{
			return;
		}
		if (targetView.IsRoomView)
		{
			if (targetView.Owner != PhotonNetwork.MasterClient)
			{
				targetView.OwnerActorNr = 0;
				targetView.ControllerActorNr = 0;
				return;
			}
		}
		else if (targetView.OwnerActorNr != targetView.CreatorActorNr || targetView.ControllerActorNr != targetView.CreatorActorNr)
		{
			targetView.OwnerActorNr = targetView.CreatorActorNr;
			targetView.ControllerActorNr = targetView.CreatorActorNr;
		}
	}

	void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
	{
	}

	void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
	{
	}

	private static HashSet<PhotonView> gaurdedViews = new HashSet<PhotonView>();

	private static readonly OwnershipGaurdHandler callbackInstance = new OwnershipGaurdHandler();
}
