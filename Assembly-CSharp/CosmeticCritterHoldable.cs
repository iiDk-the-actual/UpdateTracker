using System;
using GorillaNetworking;
using UnityEngine;

public abstract class CosmeticCritterHoldable : MonoBehaviour
{
	public int OwnerID { get; private set; }

	public bool IsLocal
	{
		get
		{
			return this.transferrableObject.IsLocalObject();
		}
	}

	public bool OwningPlayerMatches(PhotonMessageInfoWrapped info)
	{
		return this.transferrableObject.targetRig.creator == info.Sender;
	}

	protected virtual CallLimiter CreateCallLimiter()
	{
		return new CallLimiter(10, 2f, 0.5f);
	}

	public void ResetCallLimiter()
	{
		this.callLimiter.Reset();
	}

	private void TrySetID()
	{
		if (this.IsLocal)
		{
			PlayFabAuthenticator instance = PlayFabAuthenticator.instance;
			if (instance != null)
			{
				string playFabPlayerId = instance.GetPlayFabPlayerId();
				Type type = base.GetType();
				this.OwnerID = (playFabPlayerId + ((type != null) ? type.ToString() : null)).GetStaticHash();
				return;
			}
		}
		else if (this.transferrableObject.targetRig != null && this.transferrableObject.targetRig.creator != null)
		{
			string userId = this.transferrableObject.targetRig.creator.UserId;
			Type type2 = base.GetType();
			this.OwnerID = (userId + ((type2 != null) ? type2.ToString() : null)).GetStaticHash();
		}
	}

	protected virtual void Awake()
	{
		this.transferrableObject = base.GetComponentInParent<TransferrableObject>();
		this.callLimiter = this.CreateCallLimiter();
		if (this.IsLocal)
		{
			CosmeticCritterManager.Instance.RegisterLocalHoldable(this);
		}
	}

	protected virtual void OnEnable()
	{
		this.TrySetID();
	}

	protected virtual void OnDisable()
	{
	}

	protected TransferrableObject transferrableObject;

	protected CallLimiter callLimiter;
}
