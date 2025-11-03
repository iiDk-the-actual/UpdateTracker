using System;
using GorillaExtensions;
using GorillaTag.GuidedRefs;
using UnityEngine;

public class GorillaGeoHideShowTrigger : GorillaTriggerBox, IGuidedRefReceiverMono, IGuidedRefMonoBehaviour, IGuidedRefObject
{
	protected void Awake()
	{
		((IGuidedRefObject)this).GuidedRefInitialize();
	}

	public override void OnBoxTriggered()
	{
		if (!this._guidedRefsAreFullyResolved)
		{
			return;
		}
		if (this.makeSureThisIsDisabled != null)
		{
			foreach (GameObject gameObject in this.makeSureThisIsDisabled)
			{
				if (gameObject == null)
				{
					Debug.LogError("GorillaGeoHideShowTrigger: null item in makeSureThisIsDisabled. \"" + base.transform.GetPath() + "\"", this);
					return;
				}
				gameObject.SetActive(false);
			}
		}
		if (this.makeSureThisIsEnabled != null)
		{
			foreach (GameObject gameObject2 in this.makeSureThisIsEnabled)
			{
				if (gameObject2 == null)
				{
					Debug.LogError("GorillaGeoHideShowTrigger: null item in makeSureThisIsDisabled. \"" + base.transform.GetPath() + "\"", this);
					return;
				}
				gameObject2.SetActive(true);
			}
		}
	}

	void IGuidedRefObject.GuidedRefInitialize()
	{
		GuidedRefHub.RegisterReceiverArray<GorillaGeoHideShowTrigger, GameObject>(this, "makeSureThisIsDisabled", ref this.makeSureThisIsDisabled, ref this.makeSureThisIsDisabled_gRefs);
		GuidedRefHub.RegisterReceiverArray<GorillaGeoHideShowTrigger, GameObject>(this, "makeSureThisIsEnabled", ref this.makeSureThisIsEnabled, ref this.makeSureThisIsEnabled_gRefs);
		GuidedRefHub.ReceiverFullyRegistered<GorillaGeoHideShowTrigger>(this);
	}

	bool IGuidedRefReceiverMono.GuidedRefTryResolveReference(GuidedRefTryResolveInfo target)
	{
		return GuidedRefHub.TryResolveArrayItem<GorillaGeoHideShowTrigger, GameObject>(this, this.makeSureThisIsDisabled, this.makeSureThisIsDisabled_gRefs, target) || GuidedRefHub.TryResolveArrayItem<GorillaGeoHideShowTrigger, GameObject>(this, this.makeSureThisIsDisabled, this.makeSureThisIsEnabled_gRefs, target);
	}

	void IGuidedRefReceiverMono.OnAllGuidedRefsResolved()
	{
		this._guidedRefsAreFullyResolved = true;
	}

	void IGuidedRefReceiverMono.OnGuidedRefTargetDestroyed(int fieldId)
	{
		this._guidedRefsAreFullyResolved = false;
	}

	int IGuidedRefReceiverMono.GuidedRefsWaitingToResolveCount { get; set; }

	Transform IGuidedRefMonoBehaviour.get_transform()
	{
		return base.transform;
	}

	int IGuidedRefObject.GetInstanceID()
	{
		return base.GetInstanceID();
	}

	[SerializeField]
	private GameObject[] makeSureThisIsDisabled;

	[SerializeField]
	private GuidedRefReceiverArrayInfo makeSureThisIsDisabled_gRefs = new GuidedRefReceiverArrayInfo(false);

	[SerializeField]
	private GameObject[] makeSureThisIsEnabled;

	[SerializeField]
	private GuidedRefReceiverArrayInfo makeSureThisIsEnabled_gRefs = new GuidedRefReceiverArrayInfo(false);

	private bool _guidedRefsAreFullyResolved;
}
