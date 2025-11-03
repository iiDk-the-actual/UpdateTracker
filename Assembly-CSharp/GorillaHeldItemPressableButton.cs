using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class GorillaHeldItemPressableButton : MonoBehaviour, IDelayedExecListener
{
	public event Action<GorillaHeldItemPressableButton, TransferrableObject, bool> onPressed;

	public event Action<GorillaHeldItemPressableButton, TransferrableObject, bool> onReleased;

	private void Start()
	{
		if (this.acceptAnyHoldableThatMatchesType)
		{
			this.acceptedTypes = new List<Type>();
			foreach (TransferrableObject transferrableObject in this.acceptedHoldables)
			{
				this.acceptedTypes.Add(transferrableObject.GetType());
			}
		}
	}

	protected void OnTriggerEnter(Collider collider)
	{
		if (!base.enabled)
		{
			return;
		}
		if (this.touchTime + this.delayBetweenSuccessfulPresses >= Time.time)
		{
			return;
		}
		TransferrableObject transferrableObject = collider.GetComponentInParent<TransferrableObject>();
		if (transferrableObject == null)
		{
			transferrableObject = collider.transform.parent.GetComponentInParent<TransferrableObject>();
		}
		if (transferrableObject == null || !transferrableObject.InHand())
		{
			return;
		}
		if (this.acceptAnyHoldableThatMatchesType)
		{
			if (!this.acceptedTypes.Contains(transferrableObject.GetType()))
			{
				return;
			}
		}
		else if (!this.acceptedHoldables.Contains(transferrableObject))
		{
			return;
		}
		this.touchTime = Time.time;
		switch (this.mode)
		{
		case HeldItemButtonMode.OneShot:
		{
			UnityEvent<TransferrableObject> unityEvent = this.onPressButton;
			if (unityEvent != null)
			{
				unityEvent.Invoke(transferrableObject);
			}
			Action<GorillaHeldItemPressableButton, TransferrableObject, bool> action = this.onPressed;
			if (action != null)
			{
				action(this, transferrableObject, transferrableObject.InLeftHand());
			}
			this.ButtonActivation(transferrableObject);
			this.ButtonActivationWithHand(transferrableObject, transferrableObject.InLeftHand());
			break;
		}
		case HeldItemButtonMode.ResetAfterDelay:
		{
			this.isOn = true;
			UnityEvent<TransferrableObject> unityEvent2 = this.onPressButton;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke(transferrableObject);
			}
			Action<GorillaHeldItemPressableButton, TransferrableObject, bool> action2 = this.onPressed;
			if (action2 != null)
			{
				action2(this, transferrableObject, transferrableObject.InLeftHand());
			}
			this.ButtonActivation(transferrableObject);
			this.ButtonActivationWithHand(transferrableObject, transferrableObject.InLeftHand());
			GTDelayedExec.Add(this, this.delayBetweenSuccessfulPresses, 0);
			break;
		}
		case HeldItemButtonMode.Toggle:
			this.isOn = !this.isOn;
			if (this.isOn)
			{
				UnityEvent<TransferrableObject> unityEvent3 = this.onPressButton;
				if (unityEvent3 != null)
				{
					unityEvent3.Invoke(transferrableObject);
				}
				Action<GorillaHeldItemPressableButton, TransferrableObject, bool> action3 = this.onPressed;
				if (action3 != null)
				{
					action3(this, transferrableObject, transferrableObject.InLeftHand());
				}
				this.ButtonActivation(transferrableObject);
				this.ButtonActivationWithHand(transferrableObject, transferrableObject.InLeftHand());
			}
			else
			{
				UnityEvent<TransferrableObject> unityEvent4 = this.onReleaseButton;
				if (unityEvent4 != null)
				{
					unityEvent4.Invoke(transferrableObject);
				}
				Action<GorillaHeldItemPressableButton, TransferrableObject, bool> action4 = this.onReleased;
				if (action4 != null)
				{
					action4(this, transferrableObject, transferrableObject.InLeftHand());
				}
			}
			break;
		}
		GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(this.pressButtonSoundIndex, transferrableObject.InLeftHand(), 0.05f);
		GorillaTagger.Instance.StartVibration(transferrableObject.InLeftHand(), GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
		if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, new object[]
			{
				67,
				transferrableObject.InLeftHand(),
				0.05f
			});
		}
		switch (this.consumeItem)
		{
		case HeldItemButtonConsumeMode.None:
			break;
		case HeldItemButtonConsumeMode.Destroy:
			transferrableObject.OnMyCreatorLeft();
			return;
		case HeldItemButtonConsumeMode.Disable:
			transferrableObject.gameObject.SetActive(false);
			break;
		default:
			return;
		}
	}

	public virtual void ButtonActivation(TransferrableObject holdable)
	{
	}

	public virtual void ButtonActivationWithHand(TransferrableObject holdable, bool isLeftHand)
	{
	}

	public virtual void ResetState()
	{
		this.isOn = false;
		UnityEvent<TransferrableObject> unityEvent = this.onReleaseButton;
		if (unityEvent != null)
		{
			unityEvent.Invoke(null);
		}
		Action<GorillaHeldItemPressableButton, TransferrableObject, bool> action = this.onReleased;
		if (action == null)
		{
			return;
		}
		action(this, null, false);
	}

	public void OnDelayedAction(int contextIndex)
	{
		this.ResetState();
	}

	public int pressButtonSoundIndex = 67;

	public bool isOn;

	public float delayBetweenSuccessfulPresses = 0.25f;

	private float touchTime;

	public HeldItemButtonMode mode;

	public List<TransferrableObject> acceptedHoldables;

	private List<Type> acceptedTypes;

	public bool acceptAnyHoldableThatMatchesType = true;

	public HeldItemButtonConsumeMode consumeItem;

	[Space]
	public UnityEvent<TransferrableObject> onPressButton;

	[Space]
	public UnityEvent<TransferrableObject> onReleaseButton;
}
