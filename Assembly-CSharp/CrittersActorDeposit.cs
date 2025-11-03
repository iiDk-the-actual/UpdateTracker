using System;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;

public class CrittersActorDeposit : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		if (other.attachedRigidbody.IsNotNull())
		{
			CrittersActor component = other.attachedRigidbody.GetComponent<CrittersActor>();
			if (CrittersManager.instance.LocalAuthority() && component.IsNotNull() && this.CanDeposit(component) && this.IsAttachAvailable())
			{
				this.HandleDeposit(component);
			}
		}
	}

	protected virtual bool CanDeposit(CrittersActor depositActor)
	{
		if (depositActor.crittersActorType != this.actorType)
		{
			return false;
		}
		CrittersActor crittersActor;
		if (CrittersManager.instance.actorById.TryGetValue(depositActor.parentActorId, out crittersActor))
		{
			return crittersActor.crittersActorType == CrittersActor.CrittersActorType.Grabber;
		}
		return depositActor.parentActorId == -1;
	}

	private bool IsAttachAvailable()
	{
		return this.allowMultiAttach || this.currentAttach == null;
	}

	protected virtual void HandleDeposit(CrittersActor depositedActor)
	{
		this.currentAttach = depositedActor;
		depositedActor.ReleasedEvent.AddListener(new UnityAction<CrittersActor>(this.HandleDetach));
		CrittersActor crittersActor = this.attachPoint;
		bool flag = this.snapOnAttach;
		bool flag2 = this.disableGrabOnAttach;
		depositedActor.GrabbedBy(crittersActor, flag, default(Quaternion), default(Vector3), flag2);
	}

	protected virtual void HandleDetach(CrittersActor detachingActor)
	{
		this.currentAttach = null;
	}

	public CrittersActor attachPoint;

	public CrittersActor.CrittersActorType actorType;

	public bool disableGrabOnAttach;

	public bool allowMultiAttach;

	public bool snapOnAttach;

	private CrittersActor currentAttach;
}
