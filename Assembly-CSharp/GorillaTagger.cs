using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CjLib;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using GorillaNetworking;
using GorillaTag.Cosmetics;
using GorillaTag.GuidedRefs;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Voice.Unity;
using Steamworks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class GorillaTagger : MonoBehaviour, IGuidedRefReceiverMono, IGuidedRefMonoBehaviour, IGuidedRefObject
{
	public static GorillaTagger Instance
	{
		get
		{
			return GorillaTagger._instance;
		}
	}

	public void SetExtraHandPosition(StiltID stiltID, Vector3 position, bool canTag, bool canStun)
	{
		this.stiltTagData[(int)stiltID].currentPositionForTag = position;
		this.stiltTagData[(int)stiltID].hasCurrentPosition = true;
		this.stiltTagData[(int)stiltID].canTag = canTag;
		this.stiltTagData[(int)stiltID].canStun = canStun;
	}

	public NetworkView myVRRig
	{
		get
		{
			return this.offlineVRRig.netView;
		}
	}

	internal VRRigSerializer rigSerializer
	{
		get
		{
			return this.offlineVRRig.rigSerializer;
		}
	}

	public Rigidbody rigidbody { get; private set; }

	public float DefaultHandTapVolume
	{
		get
		{
			return this.cacheHandTapVolume;
		}
	}

	public Recorder myRecorder { get; private set; }

	public float sphereCastRadius
	{
		get
		{
			if (this.tagRadiusOverride == null)
			{
				return 0.03f;
			}
			return this.tagRadiusOverride.Value;
		}
	}

	public event Action<bool, Vector3, Vector3> OnHandTap;

	public bool hasTappedSurface { get; private set; }

	public void ResetTappedSurfaceCheck()
	{
		this.hasTappedSurface = false;
	}

	public void SetTagRadiusOverrideThisFrame(float radius)
	{
		this.tagRadiusOverride = new float?(radius);
		this.tagRadiusOverrideFrame = Time.frameCount;
	}

	protected void Awake()
	{
		this.GuidedRefInitialize();
		this.RecoverMissingRefs();
		this.MirrorCameraCullingMask = new Watchable<int>(this.BaseMirrorCameraCullingMask);
		this.stiltTagData[0].isLeftHand = true;
		this.stiltTagData[2].isLeftHand = true;
		if (GorillaTagger._instance != null && GorillaTagger._instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			GorillaTagger._instance = this;
			GorillaTagger.hasInstance = true;
			Action action = GorillaTagger.onPlayerSpawnedRootCallback;
			if (action != null)
			{
				action();
			}
		}
		GRFirstTimeUserExperience grfirstTimeUserExperience = Object.FindAnyObjectByType<GRFirstTimeUserExperience>(FindObjectsInactive.Include);
		GameObject gameObject = ((grfirstTimeUserExperience != null) ? grfirstTimeUserExperience.gameObject : null);
		if (!this.disableTutorial && (this.testTutorial || (PlayerPrefs.GetString("tutorial") != "done" && PlayerPrefs.GetString("didTutorial") != "done" && NetworkSystemConfig.AppVersion != "dev")))
		{
			base.transform.parent.position = new Vector3(-140f, 28f, -102f);
			base.transform.parent.eulerAngles = new Vector3(0f, 180f, 0f);
			GTPlayer.Instance.InitializeValues();
			PlayerPrefs.SetFloat("redValue", Random.value);
			PlayerPrefs.SetFloat("greenValue", Random.value);
			PlayerPrefs.SetFloat("blueValue", Random.value);
			PlayerPrefs.Save();
		}
		else
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("didTutorial", true);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
			PlayerPrefs.SetString("didTutorial", "done");
			PlayerPrefs.Save();
			bool flag = true;
			if (gameObject != null && PlayerPrefs.GetString("spawnInWrongStump") == "flagged" && flag)
			{
				gameObject.SetActive(true);
				GRFirstTimeUserExperience grfirstTimeUserExperience2;
				if (gameObject.TryGetComponent<GRFirstTimeUserExperience>(out grfirstTimeUserExperience2) && grfirstTimeUserExperience2.spawnPoint != null)
				{
					GTPlayer.Instance.TeleportTo(grfirstTimeUserExperience2.spawnPoint.position, grfirstTimeUserExperience2.spawnPoint.rotation, false, false);
					GTPlayer.Instance.InitializeValues();
					PlayerPrefs.DeleteKey("spawnInWrongStump");
					PlayerPrefs.Save();
				}
			}
		}
		this.thirdPersonCamera.SetActive(Application.platform != RuntimePlatform.Android);
		this.inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
		this.wasInOverlay = false;
		this.baseSlideControl = GTPlayer.Instance.slideControl;
		this.gorillaTagColliderLayerMask = UnityLayer.GorillaTagCollider.ToLayerMask();
		this.rigidbody = base.GetComponent<Rigidbody>();
		this.cacheHandTapVolume = this.handTapVolume;
		OVRManager.foveatedRenderingLevel = OVRManager.FoveatedRenderingLevel.Medium;
		this._leftHandDown = new GorillaTagger.DebouncedBool(this._framesForHandTrigger, false);
		this._rightHandDown = new GorillaTagger.DebouncedBool(this._framesForHandTrigger, false);
	}

	protected void OnDestroy()
	{
		if (GorillaTagger._instance == this)
		{
			GorillaTagger._instance = null;
			GorillaTagger.hasInstance = false;
		}
	}

	private void IsXRSubsystemActive()
	{
		this.loadedDeviceName = XRSettings.loadedDeviceName;
		List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>();
		SubsystemManager.GetSubsystems<XRDisplaySubsystem>(list);
		using (List<XRDisplaySubsystem>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.running)
				{
					this.xrSubsystemIsActive = true;
					return;
				}
			}
		}
		this.xrSubsystemIsActive = false;
	}

	protected void Start()
	{
		this.IsXRSubsystemActive();
		if (this.loadedDeviceName == "OpenVR Display")
		{
			Quaternion quaternion = Quaternion.Euler(new Vector3(-90f, 180f, -20f));
			Quaternion quaternion2 = Quaternion.Euler(new Vector3(-90f, 180f, 20f));
			Quaternion quaternion3 = Quaternion.Euler(new Vector3(-141f, 204f, -27f));
			Quaternion quaternion4 = Quaternion.Euler(new Vector3(-141f, 156f, 27f));
			GTPlayer.Instance.SetHandOffsets(true, new Vector3(-0.02f, 0f, -0.07f), quaternion3 * Quaternion.Inverse(quaternion));
			GTPlayer.Instance.SetHandOffsets(false, new Vector3(0.02f, 0f, -0.07f), quaternion4 * Quaternion.Inverse(quaternion2));
		}
		this.bodyVector = new Vector3(0f, this.bodyCollider.height / 2f - this.bodyCollider.radius, 0f);
		if (SteamManager.Initialized)
		{
			this.gameOverlayActivatedCb = Callback<GameOverlayActivated_t>.Create(new Callback<GameOverlayActivated_t>.DispatchDelegate(this.OnGameOverlayActivated));
		}
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		this.isGameOverlayActive = pCallback.m_bActive > 0;
	}

	protected void LateUpdate()
	{
		GorillaTagger.<>c__DisplayClass133_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		if (this.isGameOverlayActive)
		{
			if (this.leftHandTriggerCollider.activeSelf)
			{
				this.leftHandTriggerCollider.SetActive(false);
				this.rightHandTriggerCollider.SetActive(true);
			}
			GTPlayer.Instance.inOverlay = true;
		}
		else
		{
			if (!this.leftHandTriggerCollider.activeSelf)
			{
				this.leftHandTriggerCollider.SetActive(true);
				this.rightHandTriggerCollider.SetActive(true);
			}
			GTPlayer.Instance.inOverlay = false;
		}
		if (this.xrSubsystemIsActive && Application.platform != RuntimePlatform.Android)
		{
			if (Mathf.Abs(Time.fixedDeltaTime - 1f / XRDevice.refreshRate) > 0.0001f)
			{
				Debug.Log(" =========== adjusting refresh size =========");
				Debug.Log(" fixedDeltaTime before:\t" + Time.fixedDeltaTime.ToString());
				Debug.Log(" refresh rate         :\t" + XRDevice.refreshRate.ToString());
				Time.fixedDeltaTime = 1f / XRDevice.refreshRate;
				Debug.Log(" fixedDeltaTime after :\t" + Time.fixedDeltaTime.ToString());
				Debug.Log(" history size before  :\t" + GTPlayer.Instance.velocityHistorySize.ToString());
				GTPlayer.Instance.velocityHistorySize = Mathf.Max(Mathf.Min(Mathf.FloorToInt(XRDevice.refreshRate * 0.083333336f), 10), 6);
				if (GTPlayer.Instance.velocityHistorySize > 9)
				{
					GTPlayer.Instance.velocityHistorySize--;
				}
				Debug.Log("new history size: " + GTPlayer.Instance.velocityHistorySize.ToString());
				Debug.Log(" ============================================");
				GTPlayer.Instance.slideControl = 1f - this.CalcSlideControl(XRDevice.refreshRate);
				GTPlayer.Instance.InitializeValues();
			}
		}
		else if (Application.platform != RuntimePlatform.Android && OVRManager.instance != null && OVRManager.OVRManagerinitialized && OVRManager.instance.gameObject != null && OVRManager.instance.gameObject.activeSelf)
		{
			Object.Destroy(OVRManager.instance.gameObject);
		}
		if (!this.frameRateUpdated && Application.platform == RuntimePlatform.Android && OVRManager.instance.gameObject.activeSelf)
		{
			InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
			int num = OVRManager.display.displayFrequenciesAvailable.Length - 1;
			float num2 = OVRManager.display.displayFrequenciesAvailable[num];
			float systemDisplayFrequency = OVRPlugin.systemDisplayFrequency;
			if (systemDisplayFrequency != 60f)
			{
				if (systemDisplayFrequency == 71f)
				{
					num2 = 72f;
				}
			}
			else
			{
				num2 = 60f;
			}
			while (num2 > 90f)
			{
				num--;
				if (num < 0)
				{
					break;
				}
				num2 = OVRManager.display.displayFrequenciesAvailable[num];
			}
			float num3 = 1f;
			if (Mathf.Abs(Time.fixedDeltaTime - 1f / num2 * num3) > 0.0001f)
			{
				float num4 = Time.fixedDeltaTime - 1f / num2 * num3;
				Debug.Log(" =========== adjusting refresh size =========");
				Debug.Log("!!!!Time.fixedDeltaTime - (1f / newRefreshRate) * " + num3.ToString() + ")" + num4.ToString());
				Debug.Log("Old Refresh rate: " + systemDisplayFrequency.ToString());
				Debug.Log("New Refresh rate: " + num2.ToString());
				Debug.Log(" fixedDeltaTime before:\t" + Time.fixedDeltaTime.ToString());
				Debug.Log(" fixedDeltaTime after :\t" + (1f / num2).ToString());
				Time.fixedDeltaTime = 1f / num2 * num3;
				OVRPlugin.systemDisplayFrequency = num2;
				GTPlayer.Instance.velocityHistorySize = Mathf.FloorToInt(num2 * 0.083333336f);
				if (GTPlayer.Instance.velocityHistorySize > 9)
				{
					GTPlayer.Instance.velocityHistorySize--;
				}
				Debug.Log(" fixedDeltaTime after :\t" + Time.fixedDeltaTime.ToString());
				Debug.Log(" history size before  :\t" + GTPlayer.Instance.velocityHistorySize.ToString());
				Debug.Log("new history size: " + GTPlayer.Instance.velocityHistorySize.ToString());
				Debug.Log(" ============================================");
				GTPlayer.Instance.slideControl = 1f - this.CalcSlideControl(XRDevice.refreshRate);
				GTPlayer.Instance.InitializeValues();
				OVRManager.instance.gameObject.SetActive(false);
				this.frameRateUpdated = true;
			}
		}
		if (!this.xrSubsystemIsActive && Application.platform != RuntimePlatform.Android && Mathf.Abs(Time.fixedDeltaTime - 0.0069444445f) > 0.0001f)
		{
			Debug.Log("updating delta time. was: " + Time.fixedDeltaTime.ToString() + ". now it's " + 0.0069444445f.ToString());
			Application.targetFrameRate = 144;
			Time.fixedDeltaTime = 0.0069444445f;
			GTPlayer.Instance.velocityHistorySize = Mathf.Min(Mathf.FloorToInt(12f), 10);
			if (GTPlayer.Instance.velocityHistorySize > 9)
			{
				GTPlayer.Instance.velocityHistorySize--;
			}
			Debug.Log("new history size: " + GTPlayer.Instance.velocityHistorySize.ToString());
			GTPlayer.Instance.slideControl = 1f - this.CalcSlideControl(144f);
			GTPlayer.Instance.InitializeValues();
		}
		this.otherPlayer = null;
		this.touchedPlayer = null;
		CS$<>8__locals1.otherTouchedPlayer = null;
		if (this.tagRadiusOverrideFrame < Time.frameCount)
		{
			this.tagRadiusOverride = null;
		}
		Vector3 position = this.leftHandTransform.position;
		Vector3 position2 = this.rightHandTransform.position;
		Vector3 position3 = this.headCollider.transform.position;
		Vector3 position4 = this.bodyCollider.transform.position;
		float scale = GTPlayer.Instance.scale;
		float num5 = this.sphereCastRadius * scale;
		CS$<>8__locals1.bodyHit = false;
		CS$<>8__locals1.leftHandHit = false;
		CS$<>8__locals1.canTagHit = false;
		CS$<>8__locals1.canStunHit = false;
		if (!(GorillaGameManager.instance is CasualGameMode))
		{
			this.nonAllocHits = Physics.OverlapCapsuleNonAlloc(this.lastLeftHandPositionForTag, position, num5, this.colliderOverlaps, this.gorillaTagColliderLayerMask, QueryTriggerInteraction.Collide);
			this.<LateUpdate>g__TryTaggingAllHitsOverlap|133_0(true, this.maxTagDistance, true, false, ref CS$<>8__locals1);
			this.nonAllocHits = Physics.OverlapCapsuleNonAlloc(position3, position, num5, this.colliderOverlaps, this.gorillaTagColliderLayerMask, QueryTriggerInteraction.Collide);
			this.<LateUpdate>g__TryTaggingAllHitsOverlap|133_0(true, this.maxTagDistance, true, false, ref CS$<>8__locals1);
			this.nonAllocHits = Physics.OverlapCapsuleNonAlloc(this.lastRightHandPositionForTag, position2, num5, this.colliderOverlaps, this.gorillaTagColliderLayerMask, QueryTriggerInteraction.Collide);
			this.<LateUpdate>g__TryTaggingAllHitsOverlap|133_0(false, this.maxTagDistance, true, false, ref CS$<>8__locals1);
			this.nonAllocHits = Physics.OverlapCapsuleNonAlloc(position3, position2, num5, this.colliderOverlaps, this.gorillaTagColliderLayerMask, QueryTriggerInteraction.Collide);
			this.<LateUpdate>g__TryTaggingAllHitsOverlap|133_0(false, this.maxTagDistance, true, false, ref CS$<>8__locals1);
			for (int i = 0; i < 4; i++)
			{
				GorillaTagger.StiltTagData stiltTagData = this.stiltTagData[i];
				if (stiltTagData.hasLastPosition && stiltTagData.hasCurrentPosition && (stiltTagData.canTag || stiltTagData.canStun))
				{
					this.nonAllocHits = Physics.OverlapCapsuleNonAlloc(stiltTagData.currentPositionForTag, stiltTagData.lastPositionForTag, num5, this.colliderOverlaps, this.gorillaTagColliderLayerMask, QueryTriggerInteraction.Collide);
					this.<LateUpdate>g__TryTaggingAllHitsOverlap|133_0(i == 0 || i == 2, this.maxStiltTagDistance, stiltTagData.canTag, stiltTagData.canStun, ref CS$<>8__locals1);
				}
			}
			this.topVector = this.lastHeadPositionForTag;
			this.bottomVector = this.lastBodyPositionForTag - this.bodyVector;
			this.nonAllocHits = Physics.CapsuleCastNonAlloc(this.topVector, this.bottomVector, this.bodyCollider.radius * 2f * GTPlayer.Instance.scale, this.bodyRaycastSweep.normalized, this.nonAllocRaycastHits, Mathf.Max(this.bodyRaycastSweep.magnitude, num5), this.gorillaTagColliderLayerMask, QueryTriggerInteraction.Collide);
			this.<LateUpdate>g__TryTaggingAllHitsCapsulecast|133_1(this.maxTagDistance, true, false, ref CS$<>8__locals1);
		}
		if (this.otherPlayer != null)
		{
			if (CS$<>8__locals1.canTagHit && (!CS$<>8__locals1.canStunHit || GorillaGameManager.instance.LocalCanTag(NetworkSystem.Instance.LocalPlayer, this.otherPlayer)))
			{
				GameMode.ActiveGameMode.LocalTag(this.otherPlayer, NetworkSystem.Instance.LocalPlayer, CS$<>8__locals1.bodyHit, CS$<>8__locals1.leftHandHit);
				GameMode.ReportTag(this.otherPlayer);
			}
			if (CS$<>8__locals1.canStunHit)
			{
				RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.TaggedTime, this.otherPlayer);
			}
		}
		if (CS$<>8__locals1.otherTouchedPlayer != null && GorillaGameManager.instance != null)
		{
			CustomGameMode.TouchPlayer(CS$<>8__locals1.otherTouchedPlayer);
		}
		if (CS$<>8__locals1.otherTouchedPlayer != null)
		{
			this.HitWithKnockBack(CS$<>8__locals1.otherTouchedPlayer, NetworkSystem.Instance.LocalPlayer, CS$<>8__locals1.leftHandHit);
		}
		GTPlayer instance = GTPlayer.Instance;
		bool flag = true;
		StiltID stiltID = StiltID.None;
		this.ProcessHandTapping(in flag, in stiltID, ref this.lastLeftTap, ref this.lastLeftUpTap, ref this.leftHandWasTouching, in this.leftHandSlideSource);
		flag = false;
		stiltID = StiltID.None;
		this.ProcessHandTapping(in flag, in stiltID, ref this.lastRightTap, ref this.lastRightUpTap, ref this.rightHandWasTouching, in this.rightHandSlideSource);
		for (int j = 0; j < 4; j++)
		{
			GorillaTagger.StiltTagData stiltTagData2 = this.stiltTagData[j];
			if (stiltTagData2.hasLastPosition && stiltTagData2.hasCurrentPosition)
			{
				stiltID = (StiltID)j;
				this.ProcessHandTapping(in stiltTagData2.isLeftHand, in stiltID, ref stiltTagData2.lastTap, ref stiltTagData2.lastUpTap, ref stiltTagData2.wasTouching, in this.leftHandSlideSource);
				this.stiltTagData[j] = stiltTagData2;
			}
		}
		this.CheckEndStatusEffect();
		this.lastLeftHandPositionForTag = position;
		this.lastRightHandPositionForTag = position2;
		this.lastBodyPositionForTag = position4;
		this.lastHeadPositionForTag = position3;
		for (int k = 0; k < 4; k++)
		{
			GorillaTagger.StiltTagData stiltTagData3 = this.stiltTagData[k];
			if (stiltTagData3.hasLastPosition || stiltTagData3.hasCurrentPosition)
			{
				stiltTagData3.lastPositionForTag = stiltTagData3.currentPositionForTag;
				stiltTagData3.hasLastPosition = stiltTagData3.hasCurrentPosition;
				stiltTagData3.hasCurrentPosition = false;
				this.stiltTagData[k] = stiltTagData3;
			}
		}
		if (GTPlayer.Instance.IsBodySliding && (double)GTPlayer.Instance.RigidbodyVelocity.magnitude >= 0.15)
		{
			if (!this.bodySlideSource.isPlaying)
			{
				this.bodySlideSource.Play();
			}
		}
		else
		{
			this.bodySlideSource.Stop();
		}
		if (GorillaComputer.instance == null || NetworkSystem.Instance.LocalRecorder == null)
		{
			return;
		}
		if (float.IsFinite(GorillaTagger.moderationMutedTime) && GorillaTagger.moderationMutedTime >= 0f)
		{
			GorillaTagger.moderationMutedTime -= Time.deltaTime;
		}
		if (GorillaComputer.instance.voiceChatOn == "TRUE")
		{
			this.myRecorder = NetworkSystem.Instance.LocalRecorder;
			if (this.offlineVRRig.remoteUseReplacementVoice)
			{
				this.offlineVRRig.remoteUseReplacementVoice = false;
			}
			if (GorillaTagger.moderationMutedTime > 0f)
			{
				this.myRecorder.TransmitEnabled = false;
			}
			if (GorillaComputer.instance.pttType != "OPEN MIC")
			{
				this.primaryButtonPressRight = false;
				this.secondaryButtonPressRight = false;
				this.primaryButtonPressLeft = false;
				this.secondaryButtonPressLeft = false;
				this.primaryButtonPressRight = ControllerInputPoller.PrimaryButtonPress(XRNode.RightHand);
				this.secondaryButtonPressRight = ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
				this.primaryButtonPressLeft = ControllerInputPoller.PrimaryButtonPress(XRNode.LeftHand);
				this.secondaryButtonPressLeft = ControllerInputPoller.SecondaryButtonPress(XRNode.LeftHand);
				if (this.primaryButtonPressRight || this.secondaryButtonPressRight || this.primaryButtonPressLeft || this.secondaryButtonPressLeft)
				{
					if (GorillaComputer.instance.pttType == "PUSH TO MUTE")
					{
						this.offlineVRRig.shouldSendSpeakingLoudness = false;
						bool transmitEnabled = this.myRecorder.TransmitEnabled;
						this.myRecorder.TransmitEnabled = false;
						return;
					}
					if (GorillaComputer.instance.pttType == "PUSH TO TALK")
					{
						this.offlineVRRig.shouldSendSpeakingLoudness = true;
						if (GorillaTagger.moderationMutedTime <= 0f && !this.myRecorder.TransmitEnabled)
						{
							this.myRecorder.TransmitEnabled = true;
							return;
						}
					}
				}
				else if (GorillaComputer.instance.pttType == "PUSH TO MUTE")
				{
					this.offlineVRRig.shouldSendSpeakingLoudness = true;
					if (GorillaTagger.moderationMutedTime <= 0f && !this.myRecorder.TransmitEnabled)
					{
						this.myRecorder.TransmitEnabled = true;
						return;
					}
				}
				else if (GorillaComputer.instance.pttType == "PUSH TO TALK")
				{
					this.offlineVRRig.shouldSendSpeakingLoudness = false;
					bool transmitEnabled2 = this.myRecorder.TransmitEnabled;
					this.myRecorder.TransmitEnabled = false;
					return;
				}
			}
			else
			{
				if (GorillaTagger.moderationMutedTime <= 0f && !this.myRecorder.TransmitEnabled)
				{
					this.myRecorder.TransmitEnabled = true;
				}
				if (!this.offlineVRRig.shouldSendSpeakingLoudness)
				{
					this.offlineVRRig.shouldSendSpeakingLoudness = true;
					return;
				}
			}
		}
		else if (GorillaComputer.instance.voiceChatOn == "FALSE")
		{
			this.myRecorder = NetworkSystem.Instance.LocalRecorder;
			if (!this.offlineVRRig.remoteUseReplacementVoice)
			{
				this.offlineVRRig.remoteUseReplacementVoice = true;
			}
			if (this.myRecorder.TransmitEnabled)
			{
				this.myRecorder.TransmitEnabled = false;
			}
			if (GorillaComputer.instance.pttType != "OPEN MIC")
			{
				this.primaryButtonPressRight = false;
				this.secondaryButtonPressRight = false;
				this.primaryButtonPressLeft = false;
				this.secondaryButtonPressLeft = false;
				this.primaryButtonPressRight = ControllerInputPoller.PrimaryButtonPress(XRNode.RightHand);
				this.secondaryButtonPressRight = ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
				this.primaryButtonPressLeft = ControllerInputPoller.PrimaryButtonPress(XRNode.LeftHand);
				this.secondaryButtonPressLeft = ControllerInputPoller.SecondaryButtonPress(XRNode.LeftHand);
				if (this.primaryButtonPressRight || this.secondaryButtonPressRight || this.primaryButtonPressLeft || this.secondaryButtonPressLeft)
				{
					if (GorillaComputer.instance.pttType == "PUSH TO MUTE")
					{
						this.offlineVRRig.shouldSendSpeakingLoudness = false;
						return;
					}
					if (GorillaComputer.instance.pttType == "PUSH TO TALK")
					{
						this.offlineVRRig.shouldSendSpeakingLoudness = true;
						return;
					}
				}
				else
				{
					if (GorillaComputer.instance.pttType == "PUSH TO MUTE")
					{
						this.offlineVRRig.shouldSendSpeakingLoudness = true;
						return;
					}
					if (GorillaComputer.instance.pttType == "PUSH TO TALK")
					{
						this.offlineVRRig.shouldSendSpeakingLoudness = false;
						return;
					}
				}
			}
			else if (!this.offlineVRRig.shouldSendSpeakingLoudness)
			{
				this.offlineVRRig.shouldSendSpeakingLoudness = true;
				return;
			}
		}
		else
		{
			this.myRecorder = NetworkSystem.Instance.LocalRecorder;
			if (this.offlineVRRig.remoteUseReplacementVoice)
			{
				this.offlineVRRig.remoteUseReplacementVoice = false;
			}
			if (this.offlineVRRig.shouldSendSpeakingLoudness)
			{
				this.offlineVRRig.shouldSendSpeakingLoudness = false;
			}
			if (this.myRecorder.TransmitEnabled)
			{
				this.myRecorder.TransmitEnabled = false;
			}
		}
	}

	private bool TryToTag(VRRig rig, Vector3 hitObjectPos, bool isBodyTag, bool canStun, float maxTagDistance, out NetPlayer taggedPlayer, out NetPlayer touchedPlayer)
	{
		taggedPlayer = null;
		touchedPlayer = null;
		if (NetworkSystem.Instance.InRoom)
		{
			this.tempCreator = ((rig != null) ? rig.creator : null);
			if (this.tempCreator != null && NetworkSystem.Instance.LocalPlayer != this.tempCreator)
			{
				touchedPlayer = this.tempCreator;
				if (GorillaGameManager.instance != null && Time.time > this.taggedTime + this.tagCooldown && (canStun || GorillaGameManager.instance.LocalCanTag(NetworkSystem.Instance.LocalPlayer, this.tempCreator)) && (this.headCollider.transform.position - hitObjectPos).sqrMagnitude < maxTagDistance * maxTagDistance * GTPlayer.Instance.scale)
				{
					if (!isBodyTag)
					{
						this.StartVibration((this.leftHandTransform.position - hitObjectPos).magnitude < (this.rightHandTransform.position - hitObjectPos).magnitude, this.tagHapticStrength, this.tagHapticDuration);
					}
					else
					{
						this.StartVibration(true, this.tagHapticStrength, this.tagHapticDuration);
						this.StartVibration(false, this.tagHapticStrength, this.tagHapticDuration);
					}
					taggedPlayer = this.tempCreator;
					return true;
				}
			}
		}
		return false;
	}

	private bool TryToTag(Collider hitCollider, bool isBodyTag, bool canStun, float maxTagDistance, out NetPlayer taggedPlayer, out NetPlayer touchedNetPlayer)
	{
		VRRig vrrig;
		if (!this.tagRigDict.TryGetValue(hitCollider, out vrrig))
		{
			vrrig = hitCollider.GetComponentInParent<VRRig>();
			this.tagRigDict.Add(hitCollider, vrrig);
		}
		if (vrrig == null)
		{
			PropHuntTaggableProp componentInParent = hitCollider.GetComponentInParent<PropHuntTaggableProp>();
			if (!(componentInParent != null))
			{
				taggedPlayer = null;
				touchedNetPlayer = null;
				return false;
			}
			vrrig = componentInParent.ownerRig;
		}
		else if (GorillaGameManager.instance != null && GorillaGameManager.instance.GameType() == GameModeType.PropHunt)
		{
			taggedPlayer = null;
			touchedNetPlayer = null;
			return false;
		}
		return this.TryToTag(vrrig, hitCollider.transform.position, isBodyTag, canStun, maxTagDistance, out taggedPlayer, out touchedNetPlayer);
	}

	private void HitWithKnockBack(NetPlayer taggedPlayer, NetPlayer taggingPlayer, bool leftHand)
	{
		Vector3 averageVelocity = GTPlayer.Instance.GetHandVelocityTracker(leftHand).GetAverageVelocity(true, 0.15f, false);
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(taggingPlayer, out rigContainer))
		{
			return;
		}
		VRMap vrmap = (leftHand ? rigContainer.Rig.leftHand : rigContainer.Rig.rightHand);
		Vector3 vector = (leftHand ? (-vrmap.rigTarget.right) : vrmap.rigTarget.right);
		RigContainer rigContainer2;
		CosmeticEffectsOnPlayers.CosmeticEffect cosmeticEffect;
		if (VRRigCache.Instance.TryGetVrrig(taggedPlayer, out rigContainer2) && rigContainer2.Rig.TemporaryCosmeticEffects.TryGetValue(CosmeticEffectsOnPlayers.EFFECTTYPE.TagWithKnockback, out cosmeticEffect))
		{
			RoomSystem.HitPlayer(taggedPlayer, vector.normalized, averageVelocity.magnitude);
		}
	}

	public void StartVibration(bool forLeftController, float amplitude, float duration)
	{
		base.StartCoroutine(this.HapticPulses(forLeftController, amplitude, duration));
	}

	private IEnumerator HapticPulses(bool forLeftController, float amplitude, float duration)
	{
		float startTime = Time.time;
		uint channel = 0U;
		global::UnityEngine.XR.InputDevice device;
		if (forLeftController)
		{
			device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
		}
		else
		{
			device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
		}
		while (Time.time < startTime + duration)
		{
			device.SendHapticImpulse(channel, amplitude, this.hapticWaitSeconds);
			yield return new WaitForSeconds(this.hapticWaitSeconds * 0.9f);
		}
		yield break;
	}

	public void PlayHapticClip(bool forLeftController, AudioClip clip, float strength)
	{
		if (forLeftController)
		{
			if (this.leftHapticsRoutine != null)
			{
				base.StopCoroutine(this.leftHapticsRoutine);
			}
			this.leftHapticsRoutine = base.StartCoroutine(this.AudioClipHapticPulses(forLeftController, clip, strength));
			return;
		}
		if (this.rightHapticsRoutine != null)
		{
			base.StopCoroutine(this.rightHapticsRoutine);
		}
		this.rightHapticsRoutine = base.StartCoroutine(this.AudioClipHapticPulses(forLeftController, clip, strength));
	}

	public void StopHapticClip(bool forLeftController)
	{
		if (forLeftController)
		{
			if (this.leftHapticsRoutine != null)
			{
				base.StopCoroutine(this.leftHapticsRoutine);
				this.leftHapticsRoutine = null;
				return;
			}
		}
		else if (this.rightHapticsRoutine != null)
		{
			base.StopCoroutine(this.rightHapticsRoutine);
			this.rightHapticsRoutine = null;
		}
	}

	private IEnumerator AudioClipHapticPulses(bool forLeftController, AudioClip clip, float strength)
	{
		uint channel = 0U;
		int bufferSize = 8192;
		int sampleWindowSize = 256;
		float[] audioData;
		global::UnityEngine.XR.InputDevice device;
		if (forLeftController)
		{
			float[] array;
			if ((array = this.leftHapticsBuffer) == null)
			{
				array = (this.leftHapticsBuffer = new float[bufferSize]);
			}
			audioData = array;
			device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
		}
		else
		{
			float[] array2;
			if ((array2 = this.rightHapticsBuffer) == null)
			{
				array2 = (this.rightHapticsBuffer = new float[bufferSize]);
			}
			audioData = array2;
			device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
		}
		int sampleOffset = -bufferSize;
		float startTime = Time.time;
		float length = clip.length;
		float endTime = Time.time + length;
		float sampleRate = (float)clip.samples;
		while (Time.time <= endTime)
		{
			float num = (Time.time - startTime) / length;
			int num2 = (int)(sampleRate * num);
			if (Mathf.Max(num2 + sampleWindowSize - 1, audioData.Length - 1) >= sampleOffset + bufferSize)
			{
				clip.GetData(audioData, num2);
				sampleOffset = num2;
			}
			float num3 = 0f;
			int num4 = Mathf.Min(clip.samples - num2, sampleWindowSize);
			for (int i = 0; i < num4; i++)
			{
				float num5 = audioData[num2 - sampleOffset + i];
				num3 += num5 * num5;
			}
			float num6 = Mathf.Clamp01(((num4 > 0) ? Mathf.Sqrt(num3 / (float)num4) : 0f) * strength);
			device.SendHapticImpulse(channel, num6, Time.fixedDeltaTime);
			yield return null;
		}
		if (forLeftController)
		{
			this.leftHapticsRoutine = null;
		}
		else
		{
			this.rightHapticsRoutine = null;
		}
		yield break;
	}

	public void DoVibration(XRNode node, float amplitude, float duration)
	{
		global::UnityEngine.XR.InputDevice deviceAtXRNode = InputDevices.GetDeviceAtXRNode(node);
		if (deviceAtXRNode.isValid)
		{
			deviceAtXRNode.SendHapticImpulse(0U, amplitude, duration);
		}
	}

	public void UpdateColor(float red, float green, float blue)
	{
		this.offlineVRRig.InitializeNoobMaterialLocal(red, green, blue);
		if (NetworkSystem.Instance != null && !NetworkSystem.Instance.InRoom)
		{
			this.offlineVRRig.bodyRenderer.ResetBodyMaterial();
		}
	}

	protected void OnTriggerEnter(Collider other)
	{
		GorillaTriggerBox gorillaTriggerBox;
		if (other.TryGetComponent<GorillaTriggerBox>(out gorillaTriggerBox))
		{
			gorillaTriggerBox.OnBoxTriggered();
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		GorillaTriggerBox gorillaTriggerBox;
		if (other.TryGetComponent<GorillaTriggerBox>(out gorillaTriggerBox))
		{
			gorillaTriggerBox.OnBoxExited();
		}
	}

	public void ShowCosmeticParticles(bool showParticles)
	{
		if (showParticles)
		{
			this.mainCamera.GetComponent<Camera>().cullingMask |= UnityLayer.GorillaCosmeticParticle.ToLayerMask();
			this.MirrorCameraCullingMask.value |= UnityLayer.GorillaCosmeticParticle.ToLayerMask();
			return;
		}
		this.mainCamera.GetComponent<Camera>().cullingMask &= ~UnityLayer.GorillaCosmeticParticle.ToLayerMask();
		this.MirrorCameraCullingMask.value &= ~UnityLayer.GorillaCosmeticParticle.ToLayerMask();
	}

	public void ApplyStatusEffect(GorillaTagger.StatusEffect newStatus, float duration)
	{
		this.EndStatusEffect(this.currentStatus);
		this.currentStatus = newStatus;
		this.statusEndTime = Time.time + duration;
		switch (newStatus)
		{
		case GorillaTagger.StatusEffect.None:
		case GorillaTagger.StatusEffect.Slowed:
			break;
		case GorillaTagger.StatusEffect.Frozen:
			GTPlayer.Instance.disableMovement = true;
			break;
		default:
			return;
		}
	}

	private void CheckEndStatusEffect()
	{
		if (Time.time > this.statusEndTime)
		{
			this.EndStatusEffect(this.currentStatus);
		}
	}

	private void EndStatusEffect(GorillaTagger.StatusEffect effectToEnd)
	{
		switch (effectToEnd)
		{
		case GorillaTagger.StatusEffect.None:
			break;
		case GorillaTagger.StatusEffect.Frozen:
			GTPlayer.Instance.disableMovement = false;
			this.currentStatus = GorillaTagger.StatusEffect.None;
			return;
		case GorillaTagger.StatusEffect.Slowed:
			this.currentStatus = GorillaTagger.StatusEffect.None;
			break;
		default:
			return;
		}
	}

	private float CalcSlideControl(float fps)
	{
		return Mathf.Pow(Mathf.Pow(1f - this.baseSlideControl, 120f), 1f / fps);
	}

	public static void OnPlayerSpawned(Action action)
	{
		if (GorillaTagger._instance)
		{
			action();
			return;
		}
		GorillaTagger.onPlayerSpawnedRootCallback = (Action)Delegate.Combine(GorillaTagger.onPlayerSpawnedRootCallback, action);
	}

	private void ProcessHandTapping(in bool isLeftHand, in StiltID stiltID, ref float lastTapTime, ref float lastTapUpTime, ref bool wasHandTouching, in AudioSource handSlideSource)
	{
		bool flag;
		bool flag2;
		int num;
		GorillaSurfaceOverride gorillaSurfaceOverride;
		RaycastHit raycastHit;
		Vector3 vector;
		GorillaVelocityTracker gorillaVelocityTracker;
		GTPlayer.Instance.GetHandTapData(isLeftHand, stiltID, out flag, out flag2, out num, out gorillaSurfaceOverride, out raycastHit, out vector, out gorillaVelocityTracker);
		GorillaTagger.DebouncedBool debouncedBool = (isLeftHand ? this._leftHandDown : this._rightHandDown);
		if (GTPlayer.Instance.inOverlay)
		{
			handSlideSource.GTStop();
			return;
		}
		if (flag2)
		{
			this.StartVibration(isLeftHand, this.tapHapticStrength / 5f, Time.fixedDeltaTime);
			if (!handSlideSource.isPlaying)
			{
				handSlideSource.GTPlay();
			}
			return;
		}
		handSlideSource.GTStop();
		bool wasStablyEnabled = debouncedBool.WasStablyEnabled;
		debouncedBool.Set(flag);
		bool flag3 = !wasHandTouching && flag && debouncedBool.JustEnabled;
		bool flag4 = wasHandTouching && !flag && wasStablyEnabled;
		wasHandTouching = flag;
		if (!flag4 && !flag3)
		{
			return;
		}
		Tappable tappable = null;
		bool flag5 = gorillaSurfaceOverride != null && gorillaSurfaceOverride.TryGetComponent<Tappable>(out tappable);
		HandEffectContext handEffect = this.offlineVRRig.GetHandEffect(isLeftHand, stiltID);
		if ((!flag5 || !tappable.overrideTapCooldown) && (!handEffect.SeparateUpTapCooldown || !flag4 || Time.time <= lastTapUpTime + this.tapCoolDown) && Time.time <= lastTapTime + this.tapCoolDown)
		{
			return;
		}
		float sqrMagnitude = (gorillaVelocityTracker.GetAverageVelocity(true, 0.03f, false) / GTPlayer.Instance.scale).sqrMagnitude;
		float sqrMagnitude2 = gorillaVelocityTracker.GetAverageVelocity(false, 0.03f, false).sqrMagnitude;
		this.handTapSpeed = Mathf.Sqrt(Mathf.Max(sqrMagnitude, sqrMagnitude2));
		if (handEffect.SeparateUpTapCooldown && flag4)
		{
			lastTapUpTime = Time.time;
		}
		else
		{
			lastTapTime = Time.time;
		}
		this.dirFromHitToHand = Vector3.Normalize(raycastHit.point - vector);
		GorillaAmbushManager gorillaAmbushManager = GameMode.ActiveGameMode as GorillaAmbushManager;
		if (gorillaAmbushManager != null && gorillaAmbushManager.IsInfected(NetworkSystem.Instance.LocalPlayer))
		{
			this.handTapVolume = Mathf.Clamp(this.handTapSpeed, 0f, gorillaAmbushManager.crawlingSpeedForMaxVolume);
		}
		else
		{
			this.handTapVolume = this.cacheHandTapVolume;
		}
		GorillaFreezeTagManager gorillaFreezeTagManager = GameMode.ActiveGameMode as GorillaFreezeTagManager;
		if (gorillaFreezeTagManager != null && gorillaFreezeTagManager.IsFrozen(NetworkSystem.Instance.LocalPlayer))
		{
			this.audioClipIndex = gorillaFreezeTagManager.GetFrozenHandTapAudioIndex();
		}
		else if (gorillaSurfaceOverride != null)
		{
			this.audioClipIndex = gorillaSurfaceOverride.overrideIndex;
		}
		else
		{
			this.audioClipIndex = num;
		}
		if (gorillaSurfaceOverride != null)
		{
			if (gorillaSurfaceOverride.sendOnTapEvent)
			{
				IBuilderTappable builderTappable;
				if (flag5)
				{
					tappable.OnTap(this.handTapVolume);
				}
				else if (gorillaSurfaceOverride.TryGetComponent<IBuilderTappable>(out builderTappable))
				{
					builderTappable.OnTapLocal(this.handTapVolume);
				}
			}
			PlayerGameEvents.TapObject(gorillaSurfaceOverride.name);
		}
		Vector3 averageVelocity = gorillaVelocityTracker.GetAverageVelocity(true, 0.03f, false);
		if (GameMode.ActiveGameMode != null)
		{
			GameMode.ActiveGameMode.HandleHandTap(NetworkSystem.Instance.LocalPlayer, tappable, isLeftHand, averageVelocity, raycastHit.normal);
		}
		this.StartVibration(isLeftHand, this.tapHapticStrength, this.tapHapticDuration);
		this.offlineVRRig.SetHandEffectData(handEffect, this.audioClipIndex, flag3, isLeftHand, stiltID, this.handTapVolume, this.handTapSpeed, this.dirFromHitToHand);
		FXSystem.PlayFX(handEffect);
		Action<bool, Vector3, Vector3> onHandTap = this.OnHandTap;
		if (onHandTap != null)
		{
			onHandTap(isLeftHand, raycastHit.point, raycastHit.normal);
		}
		this.hasTappedSurface = true;
		if (CrittersManager.instance.IsNotNull() && CrittersManager.instance.LocalAuthority())
		{
			CrittersRigActorSetup crittersRigActorSetup = CrittersManager.instance.rigSetupByRig[this.offlineVRRig];
			if (crittersRigActorSetup.IsNotNull())
			{
				CrittersLoudNoise crittersLoudNoise = (CrittersLoudNoise)crittersRigActorSetup.rigActors[isLeftHand ? 0 : 2].actorSet;
				if (crittersLoudNoise.IsNotNull())
				{
					crittersLoudNoise.PlayHandTapLocal(isLeftHand);
				}
			}
		}
		GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this.offlineVRRig.zoneEntity.currentZone);
		if (managerForZone.IsNotNull() && managerForZone.ghostReactorManager.IsNotNull() && !averageVelocity.AlmostZero())
		{
			Transform handFollower = GTPlayer.Instance.GetHandFollower(isLeftHand);
			RaycastHit raycastHit2;
			if (Physics.Raycast(new Ray(handFollower.position, averageVelocity.normalized), out raycastHit2, 10f))
			{
				Vector3 vector2 = Vector3.ProjectOnPlane(-handFollower.forward, raycastHit2.normal);
				managerForZone.ghostReactorManager.OnTapLocal(isLeftHand, raycastHit2.point + raycastHit2.normal * 0.005f, Quaternion.LookRotation(vector2.normalized, isLeftHand ? (-raycastHit2.normal) : raycastHit2.normal), gorillaSurfaceOverride, averageVelocity);
			}
		}
		if (NetworkSystem.Instance.InRoom && this.myVRRig.IsNotNull() && this.myVRRig != null)
		{
			this.myVRRig.GetView.RPC("OnHandTapRPC", RpcTarget.Others, new object[]
			{
				this.audioClipIndex,
				flag3,
				isLeftHand,
				stiltID,
				this.handTapSpeed,
				Utils.PackVector3ToLong(this.dirFromHitToHand)
			});
		}
	}

	public void DebugDrawTagCasts(Color color)
	{
		float num = this.sphereCastRadius * GTPlayer.Instance.scale;
		this.DrawSphereCast(this.lastLeftHandPositionForTag, this.leftRaycastSweep.normalized, num, Mathf.Max(this.leftRaycastSweep.magnitude, num), color);
		this.DrawSphereCast(this.headCollider.transform.position, this.leftHeadRaycastSweep.normalized, num, Mathf.Max(this.leftHeadRaycastSweep.magnitude, num), color);
		this.DrawSphereCast(this.lastRightHandPositionForTag, this.rightRaycastSweep.normalized, num, Mathf.Max(this.rightRaycastSweep.magnitude, num), color);
		this.DrawSphereCast(this.headCollider.transform.position, this.rightHeadRaycastSweep.normalized, num, Mathf.Max(this.rightHeadRaycastSweep.magnitude, num), color);
	}

	private void DrawSphereCast(Vector3 start, Vector3 dir, float radius, float dist, Color color)
	{
		DebugUtil.DrawCapsule(start, start + dir * dist, radius, 16, 16, color, true, DebugUtil.Style.Wireframe);
	}

	private void RecoverMissingRefs()
	{
		if (!this.offlineVRRig)
		{
			this.RecoverMissingRefs_Asdf<AudioSource>(ref this.leftHandSlideSource, "leftHandSlideSource", "./**/Left Arm IK/SlideAudio");
			this.RecoverMissingRefs_Asdf<AudioSource>(ref this.rightHandSlideSource, "rightHandSlideSource", "./**/Right Arm IK/SlideAudio");
		}
	}

	private void RecoverMissingRefs_Asdf<T>(ref T objRef, string objFieldName, string recoveryPath) where T : Object
	{
		if (objRef)
		{
			return;
		}
		Transform transform;
		if (!this.offlineVRRig.transform.TryFindByPath(recoveryPath, out transform, false))
		{
			Debug.LogError(string.Concat(new string[] { "`", objFieldName, "` reference missing and could not find by path: \"", recoveryPath, "\"" }), this);
		}
		objRef = transform.GetComponentInChildren<T>();
		if (!objRef)
		{
			Debug.LogError(string.Concat(new string[] { "`", objFieldName, "` reference is missing. Found transform with recover path, but did not find the component. Recover path: \"", recoveryPath, "\"" }), this);
		}
	}

	public void GuidedRefInitialize()
	{
		GuidedRefHub.RegisterReceiverField<GorillaTagger>(this, "offlineVRRig", ref this.offlineVRRig_gRef);
		GuidedRefHub.ReceiverFullyRegistered<GorillaTagger>(this);
	}

	int IGuidedRefReceiverMono.GuidedRefsWaitingToResolveCount { get; set; }

	bool IGuidedRefReceiverMono.GuidedRefTryResolveReference(GuidedRefTryResolveInfo target)
	{
		if (this.offlineVRRig_gRef.fieldId == target.fieldId && this.offlineVRRig == null)
		{
			this.offlineVRRig = target.targetMono.GuidedRefTargetObject as VRRig;
			return this.offlineVRRig != null;
		}
		return false;
	}

	void IGuidedRefReceiverMono.OnAllGuidedRefsResolved()
	{
	}

	void IGuidedRefReceiverMono.OnGuidedRefTargetDestroyed(int fieldId)
	{
	}

	Transform IGuidedRefMonoBehaviour.get_transform()
	{
		return base.transform;
	}

	int IGuidedRefObject.GetInstanceID()
	{
		return base.GetInstanceID();
	}

	[CompilerGenerated]
	private void <LateUpdate>g__TryTaggingAllHitsOverlap|133_0(bool isLeftHand, float maxTagDistance, bool canTag = true, bool canStun = false, ref GorillaTagger.<>c__DisplayClass133_0 A_5)
	{
		for (int i = 0; i < this.nonAllocHits; i++)
		{
			VRRig vrrig;
			if (this.colliderOverlaps[i].gameObject.activeSelf && (!this.tagRigDict.TryGetValue(this.colliderOverlaps[i], out vrrig) || !(vrrig == VRRig.LocalRig)))
			{
				if (this.TryToTag(this.colliderOverlaps[i], true, canStun, maxTagDistance, out this.tryPlayer, out this.touchedPlayer))
				{
					this.otherPlayer = this.tryPlayer;
					A_5.bodyHit = false;
					A_5.leftHandHit = isLeftHand;
					A_5.canTagHit = canTag;
					A_5.canStunHit = canStun;
					return;
				}
				if (this.touchedPlayer != null)
				{
					A_5.otherTouchedPlayer = this.touchedPlayer;
				}
			}
		}
	}

	[CompilerGenerated]
	private void <LateUpdate>g__TryTaggingAllHitsCapsulecast|133_1(float maxTagDistance, bool canTag = true, bool canStun = false, ref GorillaTagger.<>c__DisplayClass133_0 A_4)
	{
		for (int i = 0; i < this.nonAllocHits; i++)
		{
			VRRig vrrig;
			if (this.nonAllocRaycastHits[i].collider.gameObject.activeSelf && (!this.tagRigDict.TryGetValue(this.nonAllocRaycastHits[i].collider, out vrrig) || !(vrrig == VRRig.LocalRig)))
			{
				if (this.TryToTag(this.nonAllocRaycastHits[i].collider, false, canStun, maxTagDistance, out this.tryPlayer, out this.touchedPlayer))
				{
					this.otherPlayer = this.tryPlayer;
					A_4.bodyHit = true;
					A_4.canTagHit = canTag;
					A_4.canStunHit = canStun;
					return;
				}
				if (this.touchedPlayer != null)
				{
					A_4.otherTouchedPlayer = this.touchedPlayer;
				}
			}
		}
	}

	[OnEnterPlay_SetNull]
	private static GorillaTagger _instance;

	[OnEnterPlay_Set(false)]
	public static bool hasInstance;

	public static float moderationMutedTime = -1f;

	public bool inCosmeticsRoom;

	public SphereCollider headCollider;

	public CapsuleCollider bodyCollider;

	private Vector3 lastLeftHandPositionForTag;

	private Vector3 lastRightHandPositionForTag;

	private Vector3 lastBodyPositionForTag;

	private Vector3 lastHeadPositionForTag;

	private GorillaTagger.StiltTagData[] stiltTagData = new GorillaTagger.StiltTagData[4];

	public Transform rightHandTransform;

	public Transform leftHandTransform;

	public float hapticWaitSeconds = 0.05f;

	public float handTapVolume = 0.1f;

	public float handTapSpeed;

	public float tapCoolDown = 0.15f;

	public float lastLeftTap;

	public float lastLeftUpTap;

	public float lastRightTap;

	public float lastRightUpTap;

	private bool leftHandWasTouching;

	private bool rightHandWasTouching;

	public float tapHapticDuration = 0.05f;

	public float tapHapticStrength = 0.5f;

	public float tagHapticDuration = 0.15f;

	public float tagHapticStrength = 1f;

	public float taggedHapticDuration = 0.35f;

	public float taggedHapticStrength = 1f;

	public float taggedTime;

	public float tagCooldown;

	public float slowCooldown = 3f;

	public float maxTagDistance = 2.2f;

	public float maxStiltTagDistance = 3.2f;

	public VRRig offlineVRRig;

	[FormerlySerializedAs("offlineVRRig_guidedRef")]
	public GuidedRefReceiverFieldInfo offlineVRRig_gRef = new GuidedRefReceiverFieldInfo(false);

	public GameObject thirdPersonCamera;

	public GameObject mainCamera;

	public bool testTutorial;

	public bool disableTutorial;

	public bool frameRateUpdated;

	public GameObject leftHandTriggerCollider;

	public GameObject rightHandTriggerCollider;

	public AudioSource leftHandSlideSource;

	public AudioSource rightHandSlideSource;

	public AudioSource bodySlideSource;

	public bool overrideNotInFocus;

	private Vector3 leftRaycastSweep;

	private Vector3 leftHeadRaycastSweep;

	private Vector3 rightRaycastSweep;

	private Vector3 rightHeadRaycastSweep;

	private Vector3 headRaycastSweep;

	private Vector3 bodyRaycastSweep;

	private global::UnityEngine.XR.InputDevice rightDevice;

	private global::UnityEngine.XR.InputDevice leftDevice;

	private bool primaryButtonPressRight;

	private bool secondaryButtonPressRight;

	private bool primaryButtonPressLeft;

	private bool secondaryButtonPressLeft;

	private RaycastHit hitInfo;

	public NetPlayer otherPlayer;

	private NetPlayer tryPlayer;

	private NetPlayer touchedPlayer;

	private Vector3 topVector;

	private Vector3 bottomVector;

	private Vector3 bodyVector;

	private Vector3 dirFromHitToHand;

	private int audioClipIndex;

	private global::UnityEngine.XR.InputDevice inputDevice;

	private bool wasInOverlay;

	private PhotonView tempView;

	private NetPlayer tempCreator;

	private float cacheHandTapVolume;

	public GorillaTagger.StatusEffect currentStatus;

	public float statusStartTime;

	public float statusEndTime;

	private float refreshRate;

	private float baseSlideControl;

	private int gorillaTagColliderLayerMask;

	private RaycastHit[] nonAllocRaycastHits = new RaycastHit[30];

	private Collider[] colliderOverlaps = new Collider[30];

	private Dictionary<Collider, VRRig> tagRigDict = new Dictionary<Collider, VRRig>();

	private int nonAllocHits;

	private bool xrSubsystemIsActive;

	public string loadedDeviceName = "";

	[SerializeField]
	private int _framesForHandTrigger = 5;

	private GorillaTagger.DebouncedBool _leftHandDown;

	private GorillaTagger.DebouncedBool _rightHandDown;

	[SerializeField]
	private LayerMask BaseMirrorCameraCullingMask;

	public Watchable<int> MirrorCameraCullingMask;

	private float[] leftHapticsBuffer;

	private float[] rightHapticsBuffer;

	private Coroutine leftHapticsRoutine;

	private Coroutine rightHapticsRoutine;

	private Callback<GameOverlayActivated_t> gameOverlayActivatedCb;

	private bool isGameOverlayActive;

	private float? tagRadiusOverride;

	private int tagRadiusOverrideFrame = -1;

	private static Action onPlayerSpawnedRootCallback;

	private struct StiltTagData
	{
		public bool isLeftHand;

		public bool hasCurrentPosition;

		public bool hasLastPosition;

		public Vector3 currentPositionForTag;

		public Vector3 lastPositionForTag;

		public bool wasTouching;

		public float lastTap;

		public float lastUpTap;

		public bool canTag;

		public bool canStun;
	}

	public enum StatusEffect
	{
		None,
		Frozen,
		Slowed,
		Dead,
		Infected,
		It
	}

	private class DebouncedBool
	{
		public bool Value { get; private set; }

		public bool JustEnabled { get; private set; }

		public bool WasStablyEnabled { get; private set; }

		public DebouncedBool(int callsUntilDisable, bool initialValue = false)
		{
			this._callsUntilStable = callsUntilDisable;
			this.Value = initialValue;
			this._lastValue = initialValue;
		}

		public void Set(bool value)
		{
			this._lastValue = this.Value;
			if (!value)
			{
				this.WasStablyEnabled = false;
				this._callsSinceDisable++;
				if (this._callsSinceDisable == this._callsUntilStable)
				{
					this.Value = false;
				}
			}
			else
			{
				this.Value = true;
				this._callsSinceDisable = 0;
				this._callsSinceEnable++;
				if (this._callsSinceEnable >= this._callsUntilStable)
				{
					this.WasStablyEnabled = true;
				}
			}
			this.JustEnabled = this.Value && !this._lastValue;
		}

		private readonly int _callsUntilStable;

		private int _callsSinceDisable;

		private int _callsSinceEnable;

		private bool _lastValue;
	}
}
