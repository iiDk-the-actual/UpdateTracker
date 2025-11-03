using System;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class TransferrableObjectHoldablePart_Pin : TransferrableObjectHoldablePart
	{
		protected void OnEnable()
		{
			UnityEvent onEnableHoldable = this.OnEnableHoldable;
			if (onEnableHoldable == null)
			{
				return;
			}
			onEnableHoldable.Invoke();
		}

		protected override void UpdateHeld(VRRig rig, bool isHeldLeftHand)
		{
			if (rig.isOfflineVRRig)
			{
				Transform controllerTransform = GTPlayer.Instance.GetControllerTransform(isHeldLeftHand);
				if (GTPlayer.Instance.GetInteractPointVelocityTracker(isHeldLeftHand).GetAverageVelocity(true, 0.15f, false).magnitude > this.breakStrengthThreshold || (controllerTransform.position - this.pin.transform.position).IsLongerThan(this.maxHandSnapDistance))
				{
					this.OnRelease(null, isHeldLeftHand ? EquipmentInteractor.instance.leftHand : EquipmentInteractor.instance.rightHand);
					UnityEvent onBreak = this.OnBreak;
					if (onBreak != null)
					{
						onBreak.Invoke();
					}
					if (this.transferrableParentObject && this.transferrableParentObject.IsMyItem())
					{
						UnityEvent onBreakLocal = this.OnBreakLocal;
						if (onBreakLocal == null)
						{
							return;
						}
						onBreakLocal.Invoke();
					}
					return;
				}
				controllerTransform.position = this.pin.position;
			}
		}

		[SerializeField]
		private float breakStrengthThreshold = 0.8f;

		[SerializeField]
		private float maxHandSnapDistance = 0.5f;

		[SerializeField]
		private Transform pin;

		public UnityEvent OnBreak;

		public UnityEvent OnBreakLocal;

		public UnityEvent OnEnableHoldable;
	}
}
