using System;
using TagEffects;
using UnityEngine;

public class HandTapReactor : MonoBehaviour
{
	private void LeftDown(HandEffectContext ctx)
	{
		this.handTapEvents.InvokeAll(HandTapReactor.TapType.LeftDown);
	}

	private void LeftUp(HandEffectContext ctx)
	{
		this.handTapEvents.InvokeAll(HandTapReactor.TapType.LeftUp);
	}

	private void LeftGesture(IHandEffectsTrigger.Mode mode)
	{
		FlagEvents<HandTapReactor.TapType> flagEvents = this.handTapEvents;
		HandTapReactor.TapType tapType;
		switch (mode)
		{
		case IHandEffectsTrigger.Mode.HighFive:
			tapType = HandTapReactor.TapType.LeftHighFive;
			break;
		case IHandEffectsTrigger.Mode.FistBump:
			tapType = HandTapReactor.TapType.LeftFistBump;
			break;
		case IHandEffectsTrigger.Mode.Tag3P:
			tapType = HandTapReactor.TapType.LeftTagThirdPerson;
			break;
		case IHandEffectsTrigger.Mode.Tag1P:
			tapType = HandTapReactor.TapType.LeftTagFirstPerson;
			break;
		default:
			<PrivateImplementationDetails>.ThrowSwitchExpressionException(mode);
			break;
		}
		flagEvents.InvokeAll(tapType);
	}

	private void RightDown(HandEffectContext ctx)
	{
		this.handTapEvents.InvokeAll(HandTapReactor.TapType.RightDown);
	}

	private void RightUp(HandEffectContext ctx)
	{
		this.handTapEvents.InvokeAll(HandTapReactor.TapType.RightUp);
	}

	private void RightGesture(IHandEffectsTrigger.Mode mode)
	{
		FlagEvents<HandTapReactor.TapType> flagEvents = this.handTapEvents;
		HandTapReactor.TapType tapType;
		switch (mode)
		{
		case IHandEffectsTrigger.Mode.HighFive:
			tapType = HandTapReactor.TapType.RightHighFive;
			break;
		case IHandEffectsTrigger.Mode.FistBump:
			tapType = HandTapReactor.TapType.RightFistBump;
			break;
		case IHandEffectsTrigger.Mode.Tag3P:
			tapType = HandTapReactor.TapType.RightTagThirdPerson;
			break;
		case IHandEffectsTrigger.Mode.Tag1P:
			tapType = HandTapReactor.TapType.RightTagFirstPerson;
			break;
		default:
			<PrivateImplementationDetails>.ThrowSwitchExpressionException(mode);
			break;
		}
		flagEvents.InvokeAll(tapType);
	}

	private void OnEnable()
	{
		if (this.myRig == null)
		{
			this.myRig = base.GetComponentInParent<VRRig>();
			IHandEffectsTrigger[] componentsInChildren = this.myRig.GetComponentsInChildren<IHandEffectsTrigger>();
			if (componentsInChildren[0].RightHand)
			{
				this.rightHandTrigger = componentsInChildren[0];
				this.leftHandTrigger = componentsInChildren[1];
			}
			else
			{
				this.rightHandTrigger = componentsInChildren[1];
				this.leftHandTrigger = componentsInChildren[0];
			}
		}
		if (this.myRig != null)
		{
			this.myRig.LeftHandEffect.handTapDown += this.LeftDown;
			this.myRig.LeftHandEffect.handTapUp += this.LeftUp;
			IHandEffectsTrigger handEffectsTrigger = this.leftHandTrigger;
			handEffectsTrigger.OnTrigger = (Action<IHandEffectsTrigger.Mode>)Delegate.Combine(handEffectsTrigger.OnTrigger, new Action<IHandEffectsTrigger.Mode>(this.LeftGesture));
			this.myRig.RightHandEffect.handTapDown += this.RightDown;
			this.myRig.RightHandEffect.handTapUp += this.RightUp;
			IHandEffectsTrigger handEffectsTrigger2 = this.rightHandTrigger;
			handEffectsTrigger2.OnTrigger = (Action<IHandEffectsTrigger.Mode>)Delegate.Combine(handEffectsTrigger2.OnTrigger, new Action<IHandEffectsTrigger.Mode>(this.RightGesture));
		}
	}

	private void OnDisable()
	{
		if (this.myRig != null)
		{
			this.myRig.LeftHandEffect.handTapDown -= this.LeftDown;
			this.myRig.LeftHandEffect.handTapUp -= this.LeftUp;
			IHandEffectsTrigger handEffectsTrigger = this.leftHandTrigger;
			handEffectsTrigger.OnTrigger = (Action<IHandEffectsTrigger.Mode>)Delegate.Remove(handEffectsTrigger.OnTrigger, new Action<IHandEffectsTrigger.Mode>(this.LeftGesture));
			this.myRig.RightHandEffect.handTapDown -= this.RightDown;
			this.myRig.RightHandEffect.handTapUp -= this.RightUp;
			IHandEffectsTrigger handEffectsTrigger2 = this.rightHandTrigger;
			handEffectsTrigger2.OnTrigger = (Action<IHandEffectsTrigger.Mode>)Delegate.Remove(handEffectsTrigger2.OnTrigger, new Action<IHandEffectsTrigger.Mode>(this.RightGesture));
		}
	}

	[SerializeField]
	private FlagEvents<HandTapReactor.TapType> handTapEvents;

	private VRRig myRig;

	private IHandEffectsTrigger leftHandTrigger;

	private IHandEffectsTrigger rightHandTrigger;

	[Flags]
	private enum TapType
	{
		None = 0,
		LeftDown = 1,
		LeftUp = 2,
		LeftHighFive = 4,
		LeftFistBump = 8,
		LeftTagFirstPerson = 16,
		LeftTagThirdPerson = 32,
		AllLeft = 63,
		RightDown = 64,
		RightUp = 128,
		RightHighFive = 256,
		RightFistBump = 512,
		RightTagFirstPerson = 1024,
		RightTagThirdPerson = 2048,
		AllRight = 4032,
		All = -1
	}
}
