using System;
using UnityEngine;
using UnityEngine.Events;

public class OnSqueezeTrigger : MonoBehaviour
{
	private void Start()
	{
		this.myRig = base.GetComponentInParent<VRRig>();
	}

	private void Update()
	{
		bool flag;
		if (this.myHoldable.InLeftHand())
		{
			flag = (this.indexFinger ? this.myRig.leftIndex.calcT : this.myRig.leftMiddle.calcT) > 0.5f;
		}
		else
		{
			flag = this.myHoldable.InRightHand() && (this.indexFinger ? this.myRig.rightIndex.calcT : this.myRig.rightMiddle.calcT) > 0.5f;
		}
		if (flag != this.triggerWasDown)
		{
			if (flag)
			{
				this.onPress.Invoke();
				this.updateWhilePressed.Invoke();
			}
			else
			{
				this.onRelease.Invoke();
			}
		}
		else if (flag)
		{
			this.updateWhilePressed.Invoke();
		}
		this.triggerWasDown = flag;
	}

	[SerializeField]
	private TransferrableObject myHoldable;

	[SerializeField]
	private UnityEvent onPress;

	[SerializeField]
	private UnityEvent onRelease;

	[SerializeField]
	private UnityEvent updateWhilePressed;

	private VRRig myRig;

	private bool indexFinger = true;

	private bool triggerWasDown;
}
