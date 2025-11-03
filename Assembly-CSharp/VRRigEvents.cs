using System;
using GorillaTag;
using UnityEngine;

[RequireComponent(typeof(RigContainer))]
public class VRRigEvents : MonoBehaviour, IPreDisable
{
	public void PreDisable()
	{
		DelegateListProcessor<RigContainer> delegateListProcessor = this.disableEvent;
		if (delegateListProcessor == null)
		{
			return;
		}
		delegateListProcessor.InvokeSafe(in this.rigRef);
	}

	public void SendPostEnableEvent()
	{
		DelegateListProcessor<RigContainer> delegateListProcessor = this.enableEvent;
		if (delegateListProcessor == null)
		{
			return;
		}
		delegateListProcessor.InvokeSafe(in this.rigRef);
	}

	[SerializeField]
	private RigContainer rigRef;

	public DelegateListProcessor<RigContainer> disableEvent = new DelegateListProcessor<RigContainer>(5);

	public DelegateListProcessor<RigContainer> enableEvent = new DelegateListProcessor<RigContainer>(5);
}
