using System;
using System.Collections.Generic;
using GorillaLocomotion.Climbing;
using UnityEngine;
using UnityEngine.XR;

public class EquipmentInteractor : MonoBehaviour
{
	public GorillaHandClimber BodyClimber
	{
		get
		{
			return this.bodyClimber;
		}
	}

	public GorillaHandClimber LeftClimber
	{
		get
		{
			return this.leftClimber;
		}
	}

	public GorillaHandClimber RightClimber
	{
		get
		{
			return this.rightClimber;
		}
	}

	private void Awake()
	{
		if (EquipmentInteractor.instance == null)
		{
			EquipmentInteractor.instance = this;
			EquipmentInteractor.hasInstance = true;
		}
		else if (EquipmentInteractor.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		this.autoGrabLeft = true;
		this.autoGrabRight = true;
	}

	private void OnDestroy()
	{
		if (EquipmentInteractor.instance == this)
		{
			EquipmentInteractor.hasInstance = false;
			EquipmentInteractor.instance = null;
		}
	}

	public void ReleaseRightHand()
	{
		if (this.rightHandHeldEquipment != null)
		{
			this.rightHandHeldEquipment.OnRelease(null, this.rightHand);
		}
		if (this.leftHandHeldEquipment != null)
		{
			this.leftHandHeldEquipment.OnRelease(null, this.rightHand);
		}
		this.autoGrabRight = true;
	}

	public void ReleaseLeftHand()
	{
		if (this.rightHandHeldEquipment != null)
		{
			this.rightHandHeldEquipment.OnRelease(null, this.leftHand);
		}
		if (this.leftHandHeldEquipment != null)
		{
			this.leftHandHeldEquipment.OnRelease(null, this.leftHand);
		}
		this.autoGrabLeft = true;
	}

	public void ForceStopClimbing()
	{
		this.bodyClimber.ForceStopClimbing(false, false);
		this.leftClimber.ForceStopClimbing(false, false);
		this.rightClimber.ForceStopClimbing(false, false);
	}

	public bool GetIsHolding(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return this.leftHandHeldEquipment != null;
		}
		return this.rightHandHeldEquipment != null;
	}

	public bool IsGrabDisabled(XRNode node)
	{
		if (node == XRNode.LeftHand)
		{
			return this.disableLeftGrab;
		}
		return this.disableRightGrab;
	}

	public void InteractionPointDisabled(InteractionPoint interactionPoint)
	{
		if (this.iteratingInteractionPoints)
		{
			this.interactionPointsToRemove.Add(interactionPoint);
			return;
		}
		if (this.overlapInteractionPointsLeft != null)
		{
			this.overlapInteractionPointsLeft.Remove(interactionPoint);
		}
		if (this.overlapInteractionPointsRight != null)
		{
			this.overlapInteractionPointsRight.Remove(interactionPoint);
		}
	}

	public bool CanGrabLeft()
	{
		return !this.disableLeftGrab && this.leftHandHeldEquipment == null && this.builderPieceInteractor.heldPiece[0] == null;
	}

	public bool CanGrabRight()
	{
		return !this.disableRightGrab && this.rightHandHeldEquipment == null && this.builderPieceInteractor.heldPiece[1] == null;
	}

	private void LateUpdate()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.leftClimber.CheckHandClimber();
		this.rightClimber.CheckHandClimber();
		this.CheckInputValue(true);
		this.isLeftGrabbing = (this.wasLeftGrabPressed && this.grabValue > this.grabThreshold - this.grabHysteresis) || (!this.wasLeftGrabPressed && this.grabValue > this.grabThreshold + this.grabHysteresis);
		if (this.leftClimber && this.leftClimber.isClimbingOrGrabbing)
		{
			this.isLeftGrabbing = false;
		}
		this.CheckInputValue(false);
		this.isRightGrabbing = (this.wasRightGrabPressed && this.grabValue > this.grabThreshold - this.grabHysteresis) || (!this.wasRightGrabPressed && this.grabValue > this.grabThreshold + this.grabHysteresis);
		if (this.rightClimber && this.rightClimber.isClimbingOrGrabbing)
		{
			this.isRightGrabbing = false;
		}
		BuilderPiece builderPiece = this.builderPieceInteractor.heldPiece[0];
		BuilderPiece builderPiece2 = this.builderPieceInteractor.heldPiece[1];
		this.FireHandInteractions(this.leftHand, true, builderPiece);
		this.FireHandInteractions(this.rightHand, false, builderPiece2);
		if (!this.isRightGrabbing && this.wasRightGrabPressed)
		{
			this.ReleaseRightHand();
		}
		if (!this.isLeftGrabbing && this.wasLeftGrabPressed)
		{
			this.ReleaseLeftHand();
		}
		this.builderPieceInteractor.OnLateUpdate();
		if (GameBallPlayerLocal.instance != null)
		{
			GameBallPlayerLocal.instance.OnUpdateInteract();
		}
		if (GamePlayerLocal.instance != null)
		{
			GamePlayerLocal.instance.OnUpdateInteract();
		}
		this.wasLeftGrabPressed = this.isLeftGrabbing;
		this.wasRightGrabPressed = this.isRightGrabbing;
	}

	private void FireHandInteractions(GameObject interactingHand, bool isLeftHand, BuilderPiece pieceInHand)
	{
		if (isLeftHand)
		{
			this.justGrabbed = (this.isLeftGrabbing && !this.wasLeftGrabPressed) || (this.isLeftGrabbing && this.autoGrabLeft);
			this.justReleased = this.leftHandHeldEquipment != null && !this.isLeftGrabbing && this.wasLeftGrabPressed;
		}
		else
		{
			this.justGrabbed = (this.isRightGrabbing && !this.wasRightGrabPressed) || (this.isRightGrabbing && this.autoGrabRight);
			this.justReleased = this.rightHandHeldEquipment != null && !this.isRightGrabbing && this.wasRightGrabPressed;
		}
		List<InteractionPoint> list = (isLeftHand ? this.overlapInteractionPointsLeft : this.overlapInteractionPointsRight);
		bool flag = (isLeftHand ? (this.leftHandHeldEquipment != null) : (this.rightHandHeldEquipment != null));
		bool flag2 = pieceInHand != null;
		bool flag3 = (isLeftHand ? this.disableLeftGrab : this.disableRightGrab);
		bool flag4 = !flag && !flag2 && !flag3;
		this.iteratingInteractionPoints = true;
		foreach (InteractionPoint interactionPoint in list)
		{
			if (flag4 && interactionPoint != null)
			{
				if (this.justGrabbed)
				{
					interactionPoint.Holdable.OnGrab(interactionPoint, interactingHand);
				}
				else
				{
					interactionPoint.Holdable.OnHover(interactionPoint, interactingHand);
				}
			}
			if (this.justReleased)
			{
				this.tempZone = interactionPoint.GetComponent<DropZone>();
				if (this.tempZone != null)
				{
					if (interactingHand == this.leftHand)
					{
						if (this.leftHandHeldEquipment != null)
						{
							this.leftHandHeldEquipment.OnRelease(this.tempZone, interactingHand);
						}
					}
					else if (this.rightHandHeldEquipment != null)
					{
						this.rightHandHeldEquipment.OnRelease(this.tempZone, interactingHand);
					}
				}
			}
		}
		this.iteratingInteractionPoints = false;
		foreach (InteractionPoint interactionPoint2 in this.interactionPointsToRemove)
		{
			if (this.overlapInteractionPointsLeft != null)
			{
				this.overlapInteractionPointsLeft.Remove(interactionPoint2);
			}
			if (this.overlapInteractionPointsRight != null)
			{
				this.overlapInteractionPointsRight.Remove(interactionPoint2);
			}
		}
		this.interactionPointsToRemove.Clear();
	}

	public void UpdateHandEquipment(IHoldableObject newEquipment, bool forLeftHand)
	{
		if (forLeftHand)
		{
			if (newEquipment != null && newEquipment == this.rightHandHeldEquipment && !newEquipment.TwoHanded)
			{
				this.rightHandHeldEquipment = null;
			}
			if (this.leftHandHeldEquipment != null)
			{
				this.leftHandHeldEquipment.DropItemCleanup();
			}
			this.leftHandHeldEquipment = newEquipment;
			this.autoGrabLeft = false;
			return;
		}
		if (newEquipment != null && newEquipment == this.leftHandHeldEquipment && !newEquipment.TwoHanded)
		{
			this.leftHandHeldEquipment = null;
		}
		if (this.rightHandHeldEquipment != null)
		{
			this.rightHandHeldEquipment.DropItemCleanup();
		}
		this.rightHandHeldEquipment = newEquipment;
		this.autoGrabRight = false;
	}

	public void CheckInputValue(bool isLeftHand)
	{
		if (isLeftHand)
		{
			this.grabValue = ControllerInputPoller.GripFloat(XRNode.LeftHand);
			this.tempValue = ControllerInputPoller.TriggerFloat(XRNode.LeftHand);
		}
		else
		{
			this.grabValue = ControllerInputPoller.GripFloat(XRNode.RightHand);
			this.tempValue = ControllerInputPoller.TriggerFloat(XRNode.RightHand);
		}
		this.grabValue = Mathf.Max(this.grabValue, this.tempValue);
	}

	public void ForceDropEquipment(IHoldableObject equipment)
	{
		if (this.rightHandHeldEquipment == equipment)
		{
			this.rightHandHeldEquipment = null;
		}
		if (this.leftHandHeldEquipment == equipment)
		{
			this.leftHandHeldEquipment = null;
		}
	}

	public void ForceDropAnyEquipment()
	{
		this.rightHandHeldEquipment = null;
		this.leftHandHeldEquipment = null;
	}

	public void ForceDropManipulatableObject(HoldableObject manipulatableObject)
	{
		if ((HoldableObject)this.rightHandHeldEquipment == manipulatableObject)
		{
			this.rightHandHeldEquipment.OnRelease(null, this.rightHand);
			this.rightHandHeldEquipment = null;
			this.autoGrabRight = false;
		}
		if ((HoldableObject)this.leftHandHeldEquipment == manipulatableObject)
		{
			this.leftHandHeldEquipment.OnRelease(null, this.leftHand);
			this.leftHandHeldEquipment = null;
			this.autoGrabLeft = false;
		}
	}

	[OnEnterPlay_SetNull]
	public static volatile EquipmentInteractor instance;

	[OnEnterPlay_Set(false)]
	public static bool hasInstance;

	public IHoldableObject leftHandHeldEquipment;

	public IHoldableObject rightHandHeldEquipment;

	public BuilderPieceInteractor builderPieceInteractor;

	public GameObject rightHand;

	public GameObject leftHand;

	public InputDevice leftHandDevice;

	public InputDevice rightHandDevice;

	public List<InteractionPoint> overlapInteractionPointsLeft = new List<InteractionPoint>();

	public List<InteractionPoint> overlapInteractionPointsRight = new List<InteractionPoint>();

	public float grabRadius;

	public float grabThreshold = 0.7f;

	public float grabHysteresis = 0.05f;

	public bool wasLeftGrabPressed;

	public bool wasRightGrabPressed;

	public bool isLeftGrabbing;

	public bool isRightGrabbing;

	public bool justReleased;

	public bool justGrabbed;

	public bool disableLeftGrab;

	public bool disableRightGrab;

	public bool autoGrabLeft;

	public bool autoGrabRight;

	private float grabValue;

	private float tempValue;

	private DropZone tempZone;

	private bool iteratingInteractionPoints;

	private List<InteractionPoint> interactionPointsToRemove = new List<InteractionPoint>();

	[SerializeField]
	private GorillaHandClimber bodyClimber;

	[SerializeField]
	private GorillaHandClimber leftClimber;

	[SerializeField]
	private GorillaHandClimber rightClimber;
}
