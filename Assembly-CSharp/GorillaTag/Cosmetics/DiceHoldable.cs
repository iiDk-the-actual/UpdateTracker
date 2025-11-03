using System;
using GorillaExtensions;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class DiceHoldable : TransferrableObject
	{
		internal override void OnEnable()
		{
			base.OnEnable();
			if (this._events == null)
			{
				this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
				NetPlayer netPlayer = ((base.myOnlineRig != null) ? base.myOnlineRig.creator : ((base.myRig != null) ? ((base.myRig.creator != null) ? base.myRig.creator : NetworkSystem.Instance.LocalPlayer) : null));
				if (netPlayer != null)
				{
					this._events.Init(netPlayer);
				}
				else
				{
					Debug.LogError("Failed to get a reference to the Photon Player needed to hook up the cosmetic event");
				}
			}
			if (this._events != null)
			{
				this._events.Activate += this.OnDiceEvent;
			}
		}

		internal override void OnDisable()
		{
			base.OnDisable();
			if (this._events != null)
			{
				this._events.Activate -= this.OnDiceEvent;
				Object.Destroy(this._events);
				this._events = null;
			}
		}

		private void OnDiceEvent(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			GorillaNot.IncrementRPCCall(info, "OnDiceEvent");
			if (sender != target)
			{
				return;
			}
			if (info.senderID != this.ownerRig.creator.ActorNumber)
			{
				return;
			}
			if ((bool)args[0])
			{
				Vector3 position = base.transform.position;
				Vector3 forward = base.transform.forward;
				Vector3 vector = (Vector3)args[1];
				(ref position).SetValueSafe(in vector);
				vector = (Vector3)args[2];
				(ref forward).SetValueSafe(in vector);
				float num = ((float)args[3]).ClampSafe(0.01f, 1f);
				int num2 = Mathf.Clamp((int)args[4], 1, 20);
				double finite = ((double)args[5]).GetFinite();
				this.ThrowDiceLocal(position, forward, num, num2, finite);
				return;
			}
			this.dicePhysics.EndThrow();
		}

		public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
		{
			if (this.dicePhysics.enabled)
			{
				if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
				{
					object[] array = new object[] { false };
					this._events.Activate.RaiseOthers(array);
				}
				base.transform.position = this.dicePhysics.transform.position;
				base.transform.rotation = this.dicePhysics.transform.rotation;
				this.dicePhysics.EndThrow();
				if (grabbingHand == EquipmentInteractor.instance.leftHand && this.currentState == TransferrableObject.PositionState.OnLeftArm)
				{
					this.canAutoGrabLeft = false;
					this.interpState = TransferrableObject.InterpolateState.None;
					this.currentState = TransferrableObject.PositionState.InLeftHand;
				}
				else if (grabbingHand == EquipmentInteractor.instance.rightHand && this.currentState == TransferrableObject.PositionState.OnRightArm)
				{
					this.canAutoGrabRight = false;
					this.interpState = TransferrableObject.InterpolateState.None;
					this.currentState = TransferrableObject.PositionState.InLeftHand;
				}
			}
			base.OnGrab(pointGrabbed, grabbingHand);
		}

		public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
		{
			if (!base.OnRelease(zoneReleased, releasingHand))
			{
				return false;
			}
			if (zoneReleased == null)
			{
				Vector3 position = base.transform.position;
				bool flag = releasingHand == EquipmentInteractor.instance.leftHand;
				Vector3 averageVelocity = GTPlayer.Instance.GetInteractPointVelocityTracker(flag).GetAverageVelocity(true, 0.15f, false);
				int randomSide = this.dicePhysics.GetRandomSide();
				double num = (PhotonNetwork.InRoom ? PhotonNetwork.Time : (-1.0));
				float scale = GTPlayer.Instance.scale;
				if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
				{
					object[] array = new object[] { true, position, averageVelocity, scale, randomSide, num };
					this._events.Activate.RaiseOthers(array);
				}
				this.ThrowDiceLocal(position, averageVelocity, scale, randomSide, num);
			}
			return true;
		}

		private void ThrowDiceLocal(Vector3 startPosition, Vector3 throwVelocity, float playerScale, int landingSide, double startTime)
		{
			this.dicePhysics.StartThrow(this, startPosition, throwVelocity, playerScale, landingSide, startTime);
		}

		[SerializeField]
		private DicePhysics dicePhysics;

		private RubberDuckEvents _events;
	}
}
