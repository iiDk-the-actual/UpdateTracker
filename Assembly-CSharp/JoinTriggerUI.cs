using System;
using GorillaExtensions;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class JoinTriggerUI : MonoBehaviour
{
	private void Awake()
	{
		this.joinTrigger_isRefResolved = this.joinTriggerRef.TryResolve<GorillaNetworkJoinTrigger>(out this.joinTrigger) && this.joinTrigger != null;
	}

	private void Start()
	{
		this.didStart = true;
		this.OnEnable();
	}

	private void OnEnable()
	{
		if (this.didStart && this._IsValid())
		{
			this.joinTrigger.RegisterUI(this);
		}
	}

	private void OnDisable()
	{
		if (this._IsValid())
		{
			this.joinTrigger.UnregisterUI(this);
		}
	}

	public void SetState(JoinTriggerVisualState state, Func<string> oldZone, Func<string> newZone, Func<string> oldGameMode, Func<string> newGameMode)
	{
		switch (state)
		{
		case JoinTriggerVisualState.ConnectionError:
			this.milestoneRenderer.sharedMaterial = this.template.Milestone_Error;
			this.screenBGRenderer.sharedMaterial = this.template.ScreenBG_Error;
			this.screenText.text = (this.template.showFullErrorMessages ? GorillaScoreboardTotalUpdater.instance.offlineTextErrorString : this.template.ScreenText_Error);
			return;
		case JoinTriggerVisualState.AlreadyInRoom:
			this.milestoneRenderer.sharedMaterial = this.template.Milestone_AlreadyInRoom;
			this.screenBGRenderer.sharedMaterial = this.template.ScreenBG_AlreadyInRoom;
			this.screenText.text = this.template.ScreenText_AlreadyInRoom.GetText(oldZone, newZone, oldGameMode, newGameMode);
			return;
		case JoinTriggerVisualState.InPrivateRoom:
			this.milestoneRenderer.sharedMaterial = this.template.Milestone_InPrivateRoom;
			this.screenBGRenderer.sharedMaterial = this.template.ScreenBG_InPrivateRoom;
			this.screenText.text = this.template.ScreenText_InPrivateRoom.GetText(oldZone, newZone, oldGameMode, newGameMode);
			return;
		case JoinTriggerVisualState.NotConnectedSoloJoin:
			this.milestoneRenderer.sharedMaterial = this.template.Milestone_NotConnectedSoloJoin;
			this.screenBGRenderer.sharedMaterial = this.template.ScreenBG_NotConnectedSoloJoin;
			this.screenText.text = this.template.ScreenText_NotConnectedSoloJoin.GetText(oldZone, newZone, oldGameMode, newGameMode);
			return;
		case JoinTriggerVisualState.LeaveRoomAndSoloJoin:
			this.milestoneRenderer.sharedMaterial = this.template.Milestone_LeaveRoomAndSoloJoin;
			this.screenBGRenderer.sharedMaterial = this.template.ScreenBG_LeaveRoomAndSoloJoin;
			this.screenText.text = this.template.ScreenText_LeaveRoomAndSoloJoin.GetText(oldZone, newZone, oldGameMode, newGameMode);
			return;
		case JoinTriggerVisualState.LeaveRoomAndPartyJoin:
			this.milestoneRenderer.sharedMaterial = this.template.Milestone_LeaveRoomAndGroupJoin;
			this.screenBGRenderer.sharedMaterial = this.template.ScreenBG_LeaveRoomAndGroupJoin;
			this.screenText.text = this.template.ScreenText_LeaveRoomAndGroupJoin.GetText(oldZone, newZone, oldGameMode, newGameMode);
			return;
		case JoinTriggerVisualState.AbandonPartyAndSoloJoin:
			this.milestoneRenderer.sharedMaterial = this.template.Milestone_AbandonPartyAndSoloJoin;
			this.screenBGRenderer.sharedMaterial = this.template.ScreenBG_AbandonPartyAndSoloJoin;
			this.screenText.text = this.template.ScreenText_AbandonPartyAndSoloJoin.GetText(oldZone, newZone, oldGameMode, newGameMode);
			return;
		case JoinTriggerVisualState.ChangingGameModeSoloJoin:
			this.milestoneRenderer.sharedMaterial = this.template.Milestone_ChangingGameModeSoloJoin;
			this.screenBGRenderer.sharedMaterial = this.template.ScreenBG_ChangingGameModeSoloJoin;
			this.screenText.text = this.template.ScreenText_ChangingGameModeSoloJoin.GetText(oldZone, newZone, oldGameMode, newGameMode);
			return;
		default:
			return;
		}
	}

	private bool _IsValid()
	{
		if (!this.joinTrigger_isRefResolved)
		{
			if (this.joinTriggerRef.TargetID == 0)
			{
				Debug.LogError("ERROR!!!  JoinTriggerUI: XSceneRef `joinTriggerRef` is not assigned so could not resolve. Path=" + base.transform.GetPathQ(), this);
			}
			else
			{
				Debug.LogError("ERROR!!!  JoinTriggerUI: XSceneRef `joinTriggerRef` could not be resolved. Path=" + base.transform.GetPathQ(), this);
			}
		}
		return this.joinTrigger_isRefResolved;
	}

	[SerializeField]
	private XSceneRef joinTriggerRef;

	private GorillaNetworkJoinTrigger joinTrigger;

	private bool joinTrigger_isRefResolved;

	[SerializeField]
	private MeshRenderer milestoneRenderer;

	[SerializeField]
	private MeshRenderer screenBGRenderer;

	[SerializeField]
	private TextMeshPro screenText;

	[SerializeField]
	private JoinTriggerUITemplate template;

	private new bool didStart;
}
