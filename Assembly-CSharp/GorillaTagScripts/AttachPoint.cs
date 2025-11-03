using System;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts
{
	public class AttachPoint : MonoBehaviour
	{
		private void Start()
		{
			base.transform.parent.parent = null;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (this.attachPoint.childCount == 0)
			{
				this.UpdateHookState(false);
			}
			DecorativeItem componentInParent = other.GetComponentInParent<DecorativeItem>();
			if (componentInParent == null || componentInParent.InHand())
			{
				return;
			}
			if (this.IsHooked())
			{
				return;
			}
			this.UpdateHookState(true);
			componentInParent.SnapItem(true, this.attachPoint.position);
		}

		private void OnTriggerExit(Collider other)
		{
			DecorativeItem componentInParent = other.GetComponentInParent<DecorativeItem>();
			if (componentInParent == null || !componentInParent.InHand())
			{
				return;
			}
			this.UpdateHookState(false);
			componentInParent.SnapItem(false, Vector3.zero);
		}

		private void UpdateHookState(bool isHooked)
		{
			this.SetIsHook(isHooked);
		}

		internal void SetIsHook(bool isHooked)
		{
			this.isHooked = isHooked;
			UnityAction unityAction = this.onHookedChanged;
			if (unityAction == null)
			{
				return;
			}
			unityAction();
		}

		public bool IsHooked()
		{
			return this.isHooked || this.attachPoint.childCount != 0;
		}

		public Transform attachPoint;

		public UnityAction onHookedChanged;

		private bool isHooked;

		private bool wasHooked;

		public bool inForest;
	}
}
