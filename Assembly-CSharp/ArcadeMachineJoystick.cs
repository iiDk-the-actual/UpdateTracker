using System;
using Photon.Pun;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ArcadeMachineJoystick : HandHold, ISnapTurnOverride, IRequestableOwnershipGuardCallbacks
{
	public bool heldByLocalPlayer { get; private set; }

	public bool IsHeldLeftHanded
	{
		get
		{
			return this.heldByLocalPlayer && this.xrNode == XRNode.LeftHand;
		}
	}

	public ArcadeButtons currentButtonState { get; private set; }

	public int player { get; private set; }

	public void Init(ArcadeMachine machine, int player)
	{
		this.machine = machine;
		this.player = player;
		this.guard = base.GetComponent<RequestableOwnershipGuard>();
		this.guard.AddCallbackTarget(this);
	}

	public void BindController(bool leftHand)
	{
		this.xrNode = (leftHand ? XRNode.LeftHand : XRNode.RightHand);
		this.heldByLocalPlayer = true;
		if (!leftHand)
		{
			if (!this.snapTurn)
			{
				this.snapTurn = GorillaTagger.Instance.GetComponent<GorillaSnapTurn>();
			}
			if (this.snapTurn != null)
			{
				this.snapTurnOverride = true;
				this.snapTurn.SetTurningOverride(this);
			}
		}
		if (PhotonNetwork.IsMasterClient)
		{
			this.guard.TransferOwnership(PhotonNetwork.LocalPlayer, "");
		}
		else if (!this.guard.isMine)
		{
			this.guard.RequestOwnership(new Action(this.OnOwnershipSuccess), new Action(this.OnOwnershipFail));
		}
		ControllerInputPoller.AddUpdateCallback(new Action(this.OnInputUpdate));
		PlayerGameEvents.MiscEvent("PlayArcadeGame", 1);
	}

	private void OnOwnershipSuccess()
	{
	}

	private void OnOwnershipFail()
	{
		this.ForceRelease();
	}

	public void UnbindController()
	{
		this.heldByLocalPlayer = false;
		if (this.snapTurnOverride)
		{
			this.snapTurnOverride = false;
			this.snapTurn.UnsetTurningOverride(this);
		}
		this.OnInputUpdate();
		ControllerInputPoller.RemoveUpdateCallback(new Action(this.OnInputUpdate));
	}

	private void OnInputUpdate()
	{
		ArcadeButtons arcadeButtons = (ArcadeButtons)0;
		if (this.heldByLocalPlayer)
		{
			arcadeButtons |= ArcadeButtons.GRAB;
			if (ControllerInputPoller.Primary2DAxis(this.xrNode).y > 0.5f)
			{
				arcadeButtons |= ArcadeButtons.UP;
			}
			if (ControllerInputPoller.Primary2DAxis(this.xrNode).y < -0.5f)
			{
				arcadeButtons |= ArcadeButtons.DOWN;
			}
			if (ControllerInputPoller.Primary2DAxis(this.xrNode).x < -0.5f)
			{
				arcadeButtons |= ArcadeButtons.LEFT;
			}
			if (ControllerInputPoller.Primary2DAxis(this.xrNode).x > 0.5f)
			{
				arcadeButtons |= ArcadeButtons.RIGHT;
			}
			if (ControllerInputPoller.PrimaryButtonPress(this.xrNode))
			{
				arcadeButtons |= ArcadeButtons.B0;
			}
			if (ControllerInputPoller.SecondaryButtonPress(this.xrNode))
			{
				arcadeButtons |= ArcadeButtons.B1;
			}
			if (ControllerInputPoller.TriggerFloat(this.xrNode) > 0.5f)
			{
				arcadeButtons |= ArcadeButtons.TRIGGER;
			}
		}
		if (arcadeButtons != this.currentButtonState)
		{
			this.machine.OnJoystickStateChange(this.player, arcadeButtons);
		}
		this.currentButtonState = arcadeButtons;
	}

	public void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender != info.photonView.Owner)
		{
			return;
		}
		ArcadeButtons arcadeButtons = (ArcadeButtons)((int)stream.ReceiveNext());
		if (arcadeButtons != this.currentButtonState && this.machine != null)
		{
			this.machine.OnJoystickStateChange(this.player, arcadeButtons);
		}
		this.currentButtonState = arcadeButtons;
		this.machine.ReadPlayerDataPUN(this.player, stream, info);
	}

	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext((int)this.currentButtonState);
		this.machine.WritePlayerDataPUN(this.player, stream, info);
	}

	public void ReceiveRemoteState(ArcadeButtons newState)
	{
	}

	public bool TurnOverrideActive()
	{
		return this.snapTurnOverride;
	}

	public override bool CanBeGrabbed(GorillaGrabber grabber)
	{
		return !this.machine.IsControllerInUse(this.player);
	}

	public void ForceRelease()
	{
		this.heldByLocalPlayer = false;
		this.currentButtonState = (ArcadeButtons)0;
	}

	public void OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer)
	{
		if (this.heldByLocalPlayer && (toPlayer == null || !toPlayer.IsLocal))
		{
			this.ForceRelease();
		}
	}

	public bool OnOwnershipRequest(NetPlayer fromPlayer)
	{
		return !this.heldByLocalPlayer;
	}

	public bool OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer)
	{
		return !this.heldByLocalPlayer;
	}

	public void OnMyOwnerLeft()
	{
	}

	public void OnMyCreatorLeft()
	{
	}

	private XRNode xrNode;

	private ArcadeMachine machine;

	private RequestableOwnershipGuard guard;

	private GorillaSnapTurn snapTurn;

	private bool snapTurnOverride;
}
