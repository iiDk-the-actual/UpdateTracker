using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class CloserCosmetic : MonoBehaviour, ITickSystemTick
	{
		public bool TickRunning { get; set; }

		private void OnEnable()
		{
			TickSystem<object>.AddCallbackTarget(this);
			this.localRotA = this.sideA.transform.localRotation;
			this.localRotB = this.sideB.transform.localRotation;
			this.fingerValue = 0f;
			this.UpdateState(CloserCosmetic.State.Opening);
		}

		private void OnDisable()
		{
			TickSystem<object>.RemoveCallbackTarget(this);
		}

		public void Tick()
		{
			switch (this.currentState)
			{
			case CloserCosmetic.State.Closing:
				this.Closing();
				return;
			case CloserCosmetic.State.Opening:
				this.Opening();
				break;
			case CloserCosmetic.State.None:
				break;
			default:
				return;
			}
		}

		public void Close(bool leftHand, float fingerFlexValue)
		{
			this.UpdateState(CloserCosmetic.State.Closing);
			this.fingerValue = fingerFlexValue;
		}

		public void Open(bool leftHand, float fingerFlexValue)
		{
			this.UpdateState(CloserCosmetic.State.Opening);
			this.fingerValue = fingerFlexValue;
		}

		private void Closing()
		{
			float num = (this.useFingerFlexValueAsStrength ? Mathf.Clamp01(this.fingerValue) : 1f);
			Quaternion quaternion = Quaternion.Euler(this.maxRotationB);
			Quaternion quaternion2 = Quaternion.Slerp(this.localRotB, quaternion, num);
			this.sideB.transform.localRotation = quaternion2;
			Quaternion quaternion3 = Quaternion.Euler(this.maxRotationA);
			Quaternion quaternion4 = Quaternion.Slerp(this.localRotA, quaternion3, num);
			this.sideA.transform.localRotation = quaternion4;
			if (Quaternion.Angle(this.sideB.transform.localRotation, quaternion2) < 0.1f && Quaternion.Angle(this.sideA.transform.localRotation, quaternion4) < 0.1f)
			{
				this.UpdateState(CloserCosmetic.State.None);
			}
		}

		private void Opening()
		{
			float num = (this.useFingerFlexValueAsStrength ? Mathf.Clamp01(this.fingerValue) : 1f);
			Quaternion quaternion = Quaternion.Slerp(this.sideB.transform.localRotation, this.localRotB, num);
			this.sideB.transform.localRotation = quaternion;
			Quaternion quaternion2 = Quaternion.Slerp(this.sideA.transform.localRotation, this.localRotA, num);
			this.sideA.transform.localRotation = quaternion2;
			if (Quaternion.Angle(this.sideB.transform.localRotation, quaternion) < 0.1f && Quaternion.Angle(this.sideA.transform.localRotation, quaternion2) < 0.1f)
			{
				this.UpdateState(CloserCosmetic.State.None);
			}
		}

		private void UpdateState(CloserCosmetic.State newState)
		{
			this.currentState = newState;
		}

		[SerializeField]
		private GameObject sideA;

		[SerializeField]
		private GameObject sideB;

		[SerializeField]
		private Vector3 maxRotationA;

		[SerializeField]
		private Vector3 maxRotationB;

		[SerializeField]
		private bool useFingerFlexValueAsStrength;

		private Quaternion localRotA;

		private Quaternion localRotB;

		private CloserCosmetic.State currentState;

		private float fingerValue;

		private enum State
		{
			Closing,
			Opening,
			None
		}
	}
}
