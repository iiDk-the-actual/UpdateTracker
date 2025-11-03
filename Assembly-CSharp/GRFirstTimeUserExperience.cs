using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaNetworking;
using UnityEngine;

public class GRFirstTimeUserExperience : MonoBehaviour
{
	[ContextMenu("Set Player Pref")]
	private void RemovePlayerPref()
	{
		PlayerPrefs.SetString("spawnInWrongStump", "flagged");
		PlayerPrefs.Save();
	}

	private void OnEnable()
	{
		this.audioSource = base.GetComponent<AudioSource>();
		this.flickerSphere.SetActive(false);
		this.logoQuad.SetActive(false);
		this.flickerSphereOrigParent = this.flickerSphere.transform.parent;
		GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
		this.playerLight = GorillaTagger.Instance.mainCamera.GetComponentInChildren<GameLight>(true);
		this.playerLight.gameObject.SetActive(true);
		this.ChangeState(GRFirstTimeUserExperience.TransitionState.Waiting);
	}

	public void ChangeState(GRFirstTimeUserExperience.TransitionState state)
	{
		this.transitionState = state;
		switch (state)
		{
		case GRFirstTimeUserExperience.TransitionState.Waiting:
			this.stateStartTime = Time.time;
			return;
		case GRFirstTimeUserExperience.TransitionState.Flicker:
			this.transitionState = GRFirstTimeUserExperience.TransitionState.Flicker;
			this.flickerSphere.transform.SetParent(GTPlayer.Instance.headCollider.transform, false);
			this.flickerSphere.SetActive(true);
			this.logoQuad.SetActive(false);
			this.stateStartTime = Time.time;
			return;
		case GRFirstTimeUserExperience.TransitionState.Logo:
			this.stateStartTime = Time.time;
			this.flickerSphere.SetActive(true);
			this.logoQuad.SetActive(true);
			return;
		case GRFirstTimeUserExperience.TransitionState.ZoneLoad:
		{
			ZoneManagement instance = ZoneManagement.instance;
			instance.OnSceneLoadsCompleted = (Action)Delegate.Combine(instance.OnSceneLoadsCompleted, new Action(this.OnZoneLoadComplete));
			ZoneManagement.SetActiveZone(this.teleportZone);
			return;
		}
		case GRFirstTimeUserExperience.TransitionState.Teleport:
			PhotonNetworkController.Instance.AttemptToJoinPublicRoom(this.joinRoomTrigger, JoinType.Solo, null);
			GTPlayer.Instance.TeleportTo(this.teleportLocation.position, this.teleportLocation.rotation, false, false);
			GTPlayer.Instance.InitializeValues();
			this.stateStartTime = Time.time;
			return;
		case GRFirstTimeUserExperience.TransitionState.Exit:
			this.flickerSphere.transform.SetParent(this.flickerSphereOrigParent, false);
			this.flickerSphere.SetActive(false);
			this.logoQuad.SetActive(false);
			this.rootObject.SetActive(false);
			GorillaTagger.Instance.mainCamera.GetComponentInChildren<GameLight>(true).gameObject.SetActive(false);
			return;
		default:
			return;
		}
	}

	private void OnZoneLoadComplete()
	{
		ZoneManagement instance = ZoneManagement.instance;
		instance.OnSceneLoadsCompleted = (Action)Delegate.Remove(instance.OnSceneLoadsCompleted, new Action(this.OnZoneLoadComplete));
		this.ChangeState(GRFirstTimeUserExperience.TransitionState.Teleport);
	}

	public void InterruptWaitingTimer()
	{
		this.stateStartTime = -1f;
		for (int i = 0; i < this.delayObjects.Count; i++)
		{
			this.delayObjects[i].enabledTime = this.stateStartTime;
		}
	}

	private void Update()
	{
		switch (this.transitionState)
		{
		case GRFirstTimeUserExperience.TransitionState.Waiting:
			if (PrivateUIRoom.GetInOverlay())
			{
				if (this.stateStartTime >= 0f)
				{
					this.InterruptWaitingTimer();
				}
			}
			else if (this.stateStartTime < 0f)
			{
				this.stateStartTime = Time.time;
			}
			if (this.stateStartTime >= 0f && Time.time - this.stateStartTime >= this.transitionDelay)
			{
				this.ChangeState(GRFirstTimeUserExperience.TransitionState.Flicker);
				return;
			}
			break;
		case GRFirstTimeUserExperience.TransitionState.Flicker:
		{
			float num = Time.time - this.stateStartTime;
			if (this.stateStartTime >= 0f && num >= this.flickerDuration)
			{
				this.ChangeState(GRFirstTimeUserExperience.TransitionState.Logo);
				return;
			}
			bool flag = this.flickerTimeline.Evaluate(num / this.flickerDuration) < 0f;
			this.flickerSphere.SetActive(flag);
			if (flag && !this.flickerLightWasOff)
			{
				if (this.audioSource != null && this.flickerAudioCount < this.flickerAudio.Count && this.flickerAudio[this.flickerAudioCount] != null)
				{
					this.audioSource.PlayOneShot(this.flickerAudio[this.flickerAudioCount]);
				}
				this.flickerAudioCount++;
			}
			this.flickerLightWasOff = flag;
			return;
		}
		case GRFirstTimeUserExperience.TransitionState.Logo:
			if (this.stateStartTime >= 0f && Time.time - this.stateStartTime >= this.logoDisplayTime)
			{
				this.ChangeState(GRFirstTimeUserExperience.TransitionState.ZoneLoad);
				return;
			}
			break;
		case GRFirstTimeUserExperience.TransitionState.ZoneLoad:
			break;
		case GRFirstTimeUserExperience.TransitionState.Teleport:
			if (this.stateStartTime >= 0f && Time.time - this.stateStartTime >= this.teleportSettleTime)
			{
				this.ChangeState(GRFirstTimeUserExperience.TransitionState.Exit);
			}
			break;
		default:
			return;
		}
	}

	public Transform spawnPoint;

	public GameObject rootObject;

	public GameObject flickerSphere;

	public GameObject logoQuad;

	public AnimationCurve flickerTimeline;

	public float flickerDuration = 3f;

	public GTZone teleportZone = GTZone.none;

	public Transform teleportLocation;

	public float transitionDelay = 60f;

	public float logoDisplayTime = 4f;

	public float teleportSettleTime = 1f;

	public GorillaNetworkJoinTrigger joinRoomTrigger;

	public List<AudioClip> flickerAudio = new List<AudioClip>();

	public List<DisableGameObjectDelayed> delayObjects;

	private Transform flickerSphereOrigParent;

	private float stateStartTime = -1f;

	private bool flickerLightWasOff;

	private int flickerAudioCount;

	private AudioSource audioSource;

	private GRFirstTimeUserExperience.TransitionState transitionState;

	public GameLight playerLight;

	public enum TransitionState
	{
		Waiting,
		Flicker,
		Logo,
		ZoneLoad,
		Teleport,
		Exit
	}
}
