using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.Cosmetics;
using GorillaTagScripts;
using KID.Model;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using PlayFab;
using PlayFab.ClientModels;
using TagEffects;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRRig : MonoBehaviour, IWrappedSerializable, INetworkStruct, IPreDisable, IUserCosmeticsCallback, IGorillaSliceableSimple, ITickSystemPost, IEyeScannable
{
	private void CosmeticsV2_Awake()
	{
		if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			this.Handle_CosmeticsV2_OnPostInstantiateAllPrefabs_DoEnableAllCosmetics();
			return;
		}
		if (!this._isListeningFor_OnPostInstantiateAllPrefabs)
		{
			this._isListeningFor_OnPostInstantiateAllPrefabs = true;
			CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs = (Action)Delegate.Combine(CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs, new Action(this.Handle_CosmeticsV2_OnPostInstantiateAllPrefabs_DoEnableAllCosmetics));
		}
	}

	private void CosmeticsV2_OnDestroy()
	{
		if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			this.Handle_CosmeticsV2_OnPostInstantiateAllPrefabs_DoEnableAllCosmetics();
			return;
		}
		CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs = (Action)Delegate.Remove(CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs, new Action(this.Handle_CosmeticsV2_OnPostInstantiateAllPrefabs_DoEnableAllCosmetics));
	}

	internal void Handle_CosmeticsV2_OnPostInstantiateAllPrefabs_DoEnableAllCosmetics()
	{
		CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs = (Action)Delegate.Remove(CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs, new Action(this.Handle_CosmeticsV2_OnPostInstantiateAllPrefabs_DoEnableAllCosmetics));
		this.CheckForEarlyAccess();
		this.BuildInitialize_AfterCosmeticsV2Instantiated();
		this.SetCosmeticsActive(false);
	}

	public Vector3 syncPos
	{
		get
		{
			return this.netSyncPos.CurrentSyncTarget;
		}
		set
		{
			this.netSyncPos.SetNewSyncTarget(value);
		}
	}

	public Material myDefaultSkinMaterialInstance
	{
		get
		{
			return this.bodyRenderer.myDefaultSkinMaterialInstance;
		}
	}

	public GameObject[] cosmetics
	{
		get
		{
			return this._cosmetics;
		}
		set
		{
			this._cosmetics = value;
		}
	}

	public GameObject[] overrideCosmetics
	{
		get
		{
			return this._overrideCosmetics;
		}
		set
		{
			this._overrideCosmetics = value;
		}
	}

	internal void SetTaggedBy(VRRig taggingRig)
	{
		this.taggedById = taggingRig.OwningNetPlayer.ActorNumber;
	}

	public HashSet<string> TemporaryCosmetics
	{
		get
		{
			return this._temporaryCosmetics;
		}
	}

	internal bool InitializedCosmetics
	{
		get
		{
			return this.initializedCosmetics;
		}
		set
		{
			this.initializedCosmetics = value;
		}
	}

	public CosmeticRefRegistry cosmeticReferences { get; private set; }

	public void SetPitchShiftCosmeticsDirty()
	{
		this.pitchShiftCosmeticsDirty = true;
	}

	public void BreakHandLinks()
	{
		this.leftHandLink.BreakLink();
		this.rightHandLink.BreakLink();
	}

	public bool IsInHandHoldChainWithOtherPlayer(int otherPlayer)
	{
		return HandLink.IsHandInChainWithOtherPlayer(this.leftHandLink, otherPlayer) || HandLink.IsHandInChainWithOtherPlayer(this.rightHandLink, otherPlayer);
	}

	public float LastTouchedGroundAtNetworkTime { get; private set; }

	public float LastHandTouchedGroundAtNetworkTime { get; private set; }

	public bool HasBracelet
	{
		get
		{
			return this.reliableState.HasBracelet;
		}
	}

	public int CosmeticStepIndex
	{
		get
		{
			return this.newSwappedCosmetics.Count;
		}
	}

	public float LastCosmeticSwapTime { get; private set; } = float.PositiveInfinity;

	public void SetCosmeticSwapper(CosmeticSwapper swapper, float timeout)
	{
		this.cosmeticSwapper = swapper;
		this.cosmeticStepsDuration = timeout;
	}

	public void AddNewSwappedCosmetic(CosmeticSwapper.CosmeticState state)
	{
		this.newSwappedCosmetics.Push(state);
		this.LastCosmeticSwapTime = Time.time;
	}

	public void MarkFinalCosmeticStep()
	{
		this.isAtFinalCosmeticStep = true;
		this.LastCosmeticSwapTime = Time.time;
	}

	public void UnmarkFinalCosmeticStep()
	{
		this.isAtFinalCosmeticStep = false;
	}

	public Vector3 GetMouthPosition()
	{
		return this.MouthPosition.position;
	}

	public GorillaSkin CurrentCosmeticSkin { get; set; }

	public GorillaSkin CurrentModeSkin { get; set; }

	public GorillaSkin TemporaryEffectSkin { get; set; }

	public bool PostTickRunning { get; set; }

	public VRRig.PartyMemberStatus GetPartyMemberStatus()
	{
		if (this.partyMemberStatus == VRRig.PartyMemberStatus.NeedsUpdate)
		{
			this.partyMemberStatus = (FriendshipGroupDetection.Instance.IsInMyGroup(this.creator.UserId) ? VRRig.PartyMemberStatus.InLocalParty : VRRig.PartyMemberStatus.NotInLocalParty);
		}
		return this.partyMemberStatus;
	}

	public bool IsLocalPartyMember
	{
		get
		{
			return this.GetPartyMemberStatus() != VRRig.PartyMemberStatus.NotInLocalParty;
		}
	}

	public void ClearPartyMemberStatus()
	{
		this.partyMemberStatus = VRRig.PartyMemberStatus.NeedsUpdate;
	}

	public int ActiveTransferrableObjectIndex(int idx)
	{
		return this.reliableState.activeTransferrableObjectIndex[idx];
	}

	public int ActiveTransferrableObjectIndexLength()
	{
		return this.reliableState.activeTransferrableObjectIndex.Length;
	}

	public void SetActiveTransferrableObjectIndex(int idx, int v)
	{
		if (this.reliableState.activeTransferrableObjectIndex[idx] != v)
		{
			this.reliableState.activeTransferrableObjectIndex[idx] = v;
			this.reliableState.SetIsDirty();
		}
	}

	public TransferrableObject.PositionState TransferrablePosStates(int idx)
	{
		return this.reliableState.transferrablePosStates[idx];
	}

	public void SetTransferrablePosStates(int idx, TransferrableObject.PositionState v)
	{
		if (this.reliableState.transferrablePosStates[idx] != v)
		{
			this.reliableState.transferrablePosStates[idx] = v;
			this.reliableState.SetIsDirty();
		}
	}

	public TransferrableObject.ItemStates TransferrableItemStates(int idx)
	{
		return this.reliableState.transferrableItemStates[idx];
	}

	public void SetTransferrableItemStates(int idx, TransferrableObject.ItemStates v)
	{
		if (this.reliableState.transferrableItemStates[idx] != v)
		{
			this.reliableState.transferrableItemStates[idx] = v;
			this.reliableState.SetIsDirty();
		}
	}

	public void SetTransferrableDockPosition(int idx, BodyDockPositions.DropPositions v)
	{
		if (this.reliableState.transferableDockPositions[idx] != v)
		{
			this.reliableState.transferableDockPositions[idx] = v;
			this.reliableState.SetIsDirty();
		}
	}

	public BodyDockPositions.DropPositions TransferrableDockPosition(int idx)
	{
		return this.reliableState.transferableDockPositions[idx];
	}

	public int WearablePackedStates
	{
		get
		{
			return this.reliableState.wearablesPackedStates;
		}
		set
		{
			if (this.reliableState.wearablesPackedStates != value)
			{
				this.reliableState.wearablesPackedStates = value;
				this.reliableState.SetIsDirty();
			}
		}
	}

	public int LeftThrowableProjectileIndex
	{
		get
		{
			return this.reliableState.lThrowableProjectileIndex;
		}
		set
		{
			if (this.reliableState.lThrowableProjectileIndex != value)
			{
				this.reliableState.lThrowableProjectileIndex = value;
				this.reliableState.SetIsDirty();
			}
		}
	}

	public int RightThrowableProjectileIndex
	{
		get
		{
			return this.reliableState.rThrowableProjectileIndex;
		}
		set
		{
			if (this.reliableState.rThrowableProjectileIndex != value)
			{
				this.reliableState.rThrowableProjectileIndex = value;
				this.reliableState.SetIsDirty();
			}
		}
	}

	public Color32 LeftThrowableProjectileColor
	{
		get
		{
			return this.reliableState.lThrowableProjectileColor;
		}
		set
		{
			if (!this.reliableState.lThrowableProjectileColor.Equals(value))
			{
				this.reliableState.lThrowableProjectileColor = value;
				this.reliableState.SetIsDirty();
			}
		}
	}

	public Color32 RightThrowableProjectileColor
	{
		get
		{
			return this.reliableState.rThrowableProjectileColor;
		}
		set
		{
			if (!this.reliableState.rThrowableProjectileColor.Equals(value))
			{
				this.reliableState.rThrowableProjectileColor = value;
				this.reliableState.SetIsDirty();
			}
		}
	}

	public Color32 GetThrowableProjectileColor(bool isLeftHand)
	{
		if (!isLeftHand)
		{
			return this.RightThrowableProjectileColor;
		}
		return this.LeftThrowableProjectileColor;
	}

	public void SetThrowableProjectileColor(bool isLeftHand, Color32 color)
	{
		if (isLeftHand)
		{
			this.LeftThrowableProjectileColor = color;
			return;
		}
		this.RightThrowableProjectileColor = color;
	}

	public void SetRandomThrowableModelIndex(int randModelIndex)
	{
		this.RandomThrowableIndex = randModelIndex;
	}

	public int GetRandomThrowableModelIndex()
	{
		return this.RandomThrowableIndex;
	}

	private int RandomThrowableIndex
	{
		get
		{
			return this.reliableState.randomThrowableIndex;
		}
		set
		{
			if (this.reliableState.randomThrowableIndex != value)
			{
				this.reliableState.randomThrowableIndex = value;
				this.reliableState.SetIsDirty();
			}
		}
	}

	public bool IsMicEnabled
	{
		get
		{
			return this.reliableState.isMicEnabled;
		}
		set
		{
			if (this.reliableState.isMicEnabled != value)
			{
				this.reliableState.isMicEnabled = value;
				this.reliableState.SetIsDirty();
			}
		}
	}

	public int SizeLayerMask
	{
		get
		{
			return this.reliableState.sizeLayerMask;
		}
		set
		{
			if (this.reliableState.sizeLayerMask != value)
			{
				this.reliableState.sizeLayerMask = value;
				this.reliableState.SetIsDirty();
			}
		}
	}

	public float scaleFactor
	{
		get
		{
			return this.scaleMultiplier * this.nativeScale;
		}
	}

	public float ScaleMultiplier
	{
		get
		{
			return this.scaleMultiplier;
		}
		set
		{
			this.scaleMultiplier = value;
		}
	}

	public float NativeScale
	{
		get
		{
			return this.nativeScale;
		}
		set
		{
			this.nativeScale = value;
		}
	}

	public NetPlayer Creator
	{
		get
		{
			return this.creator;
		}
	}

	internal bool Initialized
	{
		get
		{
			return this.initialized;
		}
	}

	public float SpeakingLoudness
	{
		get
		{
			return this.speakingLoudness;
		}
		set
		{
			this.speakingLoudness = value;
		}
	}

	internal HandEffectContext LeftHandEffect
	{
		get
		{
			return this._leftHandEffect;
		}
	}

	internal HandEffectContext RightHandEffect
	{
		get
		{
			return this._rightHandEffect;
		}
	}

	internal HandEffectContext ExtraLeftHandEffect
	{
		get
		{
			return this._extraLeftHandEffect;
		}
	}

	internal HandEffectContext ExtraRightHandEffect
	{
		get
		{
			return this._extraRightHandEffect;
		}
	}

	public bool RigBuildFullyInitialized
	{
		get
		{
			return this._rigBuildFullyInitialized;
		}
	}

	public GamePlayer GamePlayerRef
	{
		get
		{
			if (this._gamePlayerRef == null)
			{
				this._gamePlayerRef = base.GetComponent<GamePlayer>();
			}
			return this._gamePlayerRef;
		}
	}

	public void BuildInitialize()
	{
		this.fxSettings = Object.Instantiate<FXSystemSettings>(this.sharedFXSettings);
		this.fxSettings.forLocalRig = this.isOfflineVRRig;
		this.lastPosition = base.transform.position;
		if (!this.isOfflineVRRig)
		{
			base.transform.parent = null;
		}
		SizeManager component = base.GetComponent<SizeManager>();
		if (component != null)
		{
			component.BuildInitialize();
		}
		this.myMouthFlap = base.GetComponent<GorillaMouthFlap>();
		this.mySpeakerLoudness = base.GetComponent<GorillaSpeakerLoudness>();
		if (this.myReplacementVoice == null)
		{
			this.myReplacementVoice = base.GetComponentInChildren<ReplacementVoice>();
		}
		this.myEyeExpressions = base.GetComponent<GorillaEyeExpressions>();
	}

	public void BuildInitialize_AfterCosmeticsV2Instantiated()
	{
		if (!this._rigBuildFullyInitialized)
		{
			Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
			foreach (GameObject gameObject in this.cosmetics)
			{
				GameObject gameObject2;
				if (!dictionary.TryGetValue(gameObject.name, out gameObject2))
				{
					dictionary.Add(gameObject.name, gameObject);
				}
			}
			foreach (GameObject gameObject3 in this.overrideCosmetics)
			{
				GameObject gameObject2;
				if (dictionary.TryGetValue(gameObject3.name, out gameObject2) && gameObject2.name == gameObject3.name)
				{
					gameObject2.name = "OVERRIDDEN";
				}
			}
			this.cosmetics = this.cosmetics.Concat(this.overrideCosmetics).ToArray<GameObject>();
		}
		this.cosmeticsObjectRegistry.Initialize(this.cosmetics);
		this._rigBuildFullyInitialized = true;
	}

	private void Awake()
	{
		this.CosmeticsV2_Awake();
		PlayFabAuthenticator instance = PlayFabAuthenticator.instance;
		instance.OnSafetyUpdate = (Action<bool>)Delegate.Combine(instance.OnSafetyUpdate, new Action<bool>(this.UpdateName));
		if (this.isOfflineVRRig)
		{
			VRRig.gLocalRig = this;
			this.BuildInitialize();
		}
		this.SharedStart();
	}

	private void ApplyColorCode()
	{
		float num = 0f;
		float @float = PlayerPrefs.GetFloat("redValue", num);
		float float2 = PlayerPrefs.GetFloat("greenValue", num);
		float float3 = PlayerPrefs.GetFloat("blueValue", num);
		GorillaTagger.Instance.UpdateColor(@float, float2, float3);
	}

	private void SharedStart()
	{
		if (this.isInitialized)
		{
			return;
		}
		this.lastScaleFactor = this.scaleFactor;
		this.isInitialized = true;
		this.myBodyDockPositions = base.GetComponent<BodyDockPositions>();
		this.reliableState.SharedStart(this.isOfflineVRRig, this.myBodyDockPositions);
		this.concatStringOfCosmeticsAllowed = "";
		this.bodyRenderer.SharedStart();
		this.initialized = false;
		if (this.isOfflineVRRig)
		{
			if (CosmeticsController.hasInstance && CosmeticsController.instance.v2_allCosmeticsInfoAssetRef_isLoaded)
			{
				CosmeticsController.instance.currentWornSet.LoadFromPlayerPreferences(CosmeticsController.instance);
			}
			if (Application.platform == RuntimePlatform.Android && this.spectatorSkin != null)
			{
				Object.Destroy(this.spectatorSkin);
			}
			this.initialized = true;
		}
		else if (!this.isOfflineVRRig)
		{
			if (this.spectatorSkin != null)
			{
				Object.Destroy(this.spectatorSkin);
			}
			this.head.syncPos = -this.headBodyOffset;
		}
		GorillaSkin.ShowActiveSkin(this);
		base.Invoke("ApplyColorCode", 1f);
		List<Material> list = new List<Material>();
		this.mainSkin.GetSharedMaterials(list);
		this.layerChanger = base.GetComponent<LayerChanger>();
		if (this.layerChanger != null)
		{
			this.layerChanger.InitializeLayers(base.transform);
		}
		this.frozenEffectMinY = this.frozenEffect.transform.localScale.y;
		this.frozenEffectMinHorizontalScale = this.frozenEffect.transform.localScale.x;
		this.rightIndex.Initialize();
		this.rightMiddle.Initialize();
		this.rightThumb.Initialize();
		this.leftIndex.Initialize();
		this.leftMiddle.Initialize();
		this.leftThumb.Initialize();
	}

	public void SliceUpdate()
	{
		float time = Time.time;
		if (this._nextUpdateTime < 0f)
		{
			this._nextUpdateTime = time + 1f;
			return;
		}
		if (time < this._nextUpdateTime)
		{
			return;
		}
		this._nextUpdateTime = time + 1f;
		if (RoomSystem.JoinedRoom && NetworkSystem.Instance.IsMasterClient && global::GorillaGameModes.GameMode.ActiveNetworkHandler.IsNull())
		{
			global::GorillaGameModes.GameMode.LoadGameModeFromProperty();
		}
	}

	public bool IsItemAllowed(string itemName)
	{
		if (itemName == "Slingshot")
		{
			return NetworkSystem.Instance.InRoom && GorillaGameManager.instance is GorillaPaintbrawlManager;
		}
		if (BuilderSetManager.instance.GetStarterSetsConcat().Contains(itemName))
		{
			return true;
		}
		if (this.concatStringOfCosmeticsAllowed == null)
		{
			return false;
		}
		if (this.concatStringOfCosmeticsAllowed.Contains(itemName) || PlayerCosmeticsSystem.IsTemporaryCosmeticAllowed(this, itemName))
		{
			return true;
		}
		bool canTryOn = CosmeticsController.instance.GetItemFromDict(itemName).canTryOn;
		return this.inTryOnRoom && canTryOn;
	}

	public void ApplyLocalTrajectoryOverride(Vector3 overrideVelocity)
	{
		this.LocalTrajectoryOverrideBlend = 1f;
		this.LocalTrajectoryOverridePosition = base.transform.position;
		this.LocalTrajectoryOverrideVelocity = overrideVelocity;
	}

	public bool IsLocalTrajectoryOverrideActive()
	{
		return this.LocalTrajectoryOverrideBlend > 0f;
	}

	public void ApplyLocalGrabOverride(bool isBody, bool isLeftHand, Transform grabbingHand)
	{
		this.localOverrideIsBody = isBody;
		this.localOverrideIsLeftHand = isLeftHand;
		this.localOverrideGrabbingHand = grabbingHand;
		this.localGrabOverrideBlend = 1f;
	}

	public void ClearLocalGrabOverride()
	{
		this.localGrabOverrideBlend = -1f;
	}

	public void RemoteRigUpdate()
	{
		if (this.scaleFactor != this.lastScaleFactor)
		{
			this.ScaleUpdate();
		}
		if (this.voiceAudio != null)
		{
			float num = 1f;
			if (this.IsHaunted)
			{
				num = this.HauntedVoicePitch;
			}
			else if (this.UsingHauntedRing)
			{
				num = this.HauntedRingVoicePitch;
			}
			else if (this.PitchShiftCosmetics.Count > 0)
			{
				if (this.pitchShiftCosmeticsDirty)
				{
					this.cosmeticPitchShift = 0f;
					for (int i = 0; i < this.PitchShiftCosmetics.Count; i++)
					{
						this.cosmeticPitchShift += this.PitchShiftCosmetics[i].Pitch;
					}
					this.cosmeticPitchShift /= (float)this.PitchShiftCosmetics.Count;
					this.pitchShiftCosmeticsDirty = false;
				}
				num = this.cosmeticPitchShift;
			}
			else
			{
				float num2 = GorillaTagger.Instance.offlineVRRig.scaleFactor / this.scaleFactor;
				float num3 = this.voicePitchForRelativeScale.Evaluate(num2);
				if (float.IsNaN(num3) || num3 <= 0f)
				{
					Debug.LogError("Voice pitch curve is invalid, please fix!");
				}
				else
				{
					num = num3;
				}
			}
			if (!Mathf.Approximately(this.voiceAudio.pitch, num))
			{
				this.voiceAudio.pitch = num;
			}
		}
		this.jobPos = base.transform.position;
		if (Time.time > this.timeSpawned + this.doNotLerpConstant)
		{
			this.jobPos = Vector3.Lerp(base.transform.position, this.SanitizeVector3(this.syncPos), this.lerpValueBody * 0.66f);
			if (this.currentRopeSwing && this.currentRopeSwingTarget)
			{
				Vector3 vector;
				if (this.grabbedRopeIsLeft)
				{
					vector = this.currentRopeSwingTarget.position - this.leftHandTransform.position;
				}
				else
				{
					vector = this.currentRopeSwingTarget.position - this.rightHandTransform.position;
				}
				if (this.shouldLerpToRope)
				{
					this.jobPos += Vector3.Lerp(Vector3.zero, vector, this.lastRopeGrabTimer * 4f);
					if (this.lastRopeGrabTimer < 1f)
					{
						this.lastRopeGrabTimer += Time.deltaTime;
					}
				}
				else
				{
					this.jobPos += vector;
				}
			}
			else if (this.currentHoldParent)
			{
				Transform transform;
				if (this.grabbedRopeIsBody)
				{
					transform = this.bodyTransform;
				}
				else if (this.grabbedRopeIsLeft)
				{
					transform = this.leftHandTransform;
				}
				else
				{
					transform = this.rightHandTransform;
				}
				this.jobPos += this.currentHoldParent.TransformPoint(this.grabbedRopeOffset) - transform.position;
			}
			else if (this.mountedMonkeBlock || this.mountedMovingSurface)
			{
				Transform transform2 = (this.movingSurfaceIsMonkeBlock ? this.mountedMonkeBlock.transform : this.mountedMovingSurface.transform);
				Vector3 vector2 = Vector3.zero;
				Vector3 vector3 = this.jobPos - base.transform.position;
				Transform transform3;
				if (this.mountedMovingSurfaceIsBody)
				{
					transform3 = this.bodyTransform;
				}
				else if (this.mountedMovingSurfaceIsLeft)
				{
					transform3 = this.leftHandTransform;
				}
				else
				{
					transform3 = this.rightHandTransform;
				}
				vector2 = transform2.TransformPoint(this.mountedMonkeBlockOffset) - (transform3.position + vector3);
				if (this.shouldLerpToMovingSurface)
				{
					this.lastMountedSurfaceTimer += Time.deltaTime;
					this.jobPos += Vector3.Lerp(Vector3.zero, vector2, this.lastMountedSurfaceTimer * 4f);
					if (this.lastMountedSurfaceTimer * 4f >= 1f)
					{
						this.shouldLerpToMovingSurface = false;
					}
				}
				else
				{
					this.jobPos += vector2;
				}
			}
		}
		else
		{
			this.jobPos = this.SanitizeVector3(this.syncPos);
		}
		if (this.LocalTrajectoryOverrideBlend > 0f)
		{
			this.LocalTrajectoryOverrideBlend -= Time.deltaTime / this.LocalTrajectoryOverrideDuration;
			this.LocalTrajectoryOverrideVelocity += Physics.gravity * Time.deltaTime * 0.5f;
			Vector3 vector4;
			Vector3 vector5;
			if (this.LocalTestMovementCollision(this.LocalTrajectoryOverridePosition, this.LocalTrajectoryOverrideVelocity, out vector4, out vector5))
			{
				this.LocalTrajectoryOverrideVelocity = vector4;
				this.LocalTrajectoryOverridePosition = vector5;
			}
			else
			{
				this.LocalTrajectoryOverridePosition += this.LocalTrajectoryOverrideVelocity * Time.deltaTime;
			}
			this.LocalTrajectoryOverrideVelocity += Physics.gravity * Time.deltaTime * 0.5f;
			this.jobPos = Vector3.Lerp(this.jobPos, this.LocalTrajectoryOverridePosition, this.LocalTrajectoryOverrideBlend);
		}
		else if (this.localGrabOverrideBlend > 0f)
		{
			this.localGrabOverrideBlend -= Time.deltaTime / this.LocalGrabOverrideDuration;
			if (this.localOverrideGrabbingHand != null)
			{
				Transform transform4;
				if (this.localOverrideIsBody)
				{
					transform4 = this.bodyTransform;
				}
				else if (this.localOverrideIsLeftHand)
				{
					transform4 = this.leftHandTransform;
				}
				else
				{
					transform4 = this.rightHandTransform;
				}
				this.jobPos += this.localOverrideGrabbingHand.TransformPoint(this.grabbedRopeOffset) - transform4.position;
			}
		}
		if (Time.time > this.timeSpawned + this.doNotLerpConstant)
		{
			this.jobRotation = Quaternion.Lerp(base.transform.rotation, this.SanitizeQuaternion(this.syncRotation), this.lerpValueBody);
		}
		else
		{
			this.jobRotation = this.SanitizeQuaternion(this.syncRotation);
		}
		this.head.syncPos = base.transform.rotation * -this.headBodyOffset * this.scaleFactor;
		this.head.MapOther(this.lerpValueBody);
		this.rightHand.MapOther(this.lerpValueBody);
		this.leftHand.MapOther(this.lerpValueBody);
		this.rightIndex.MapOtherFinger((float)(this.handSync % 10) / 10f, this.lerpValueFingers);
		this.rightMiddle.MapOtherFinger((float)(this.handSync % 100) / 100f, this.lerpValueFingers);
		this.rightThumb.MapOtherFinger((float)(this.handSync % 1000) / 1000f, this.lerpValueFingers);
		this.leftIndex.MapOtherFinger((float)(this.handSync % 10000) / 10000f, this.lerpValueFingers);
		this.leftMiddle.MapOtherFinger((float)(this.handSync % 100000) / 100000f, this.lerpValueFingers);
		this.leftThumb.MapOtherFinger((float)(this.handSync % 1000000) / 1000000f, this.lerpValueFingers);
		this.leftHandHoldableStatus = this.handSync % 10000000 / 1000000;
		this.rightHandHoldableStatus = this.handSync % 100000000 / 10000000;
	}

	private void ScaleUpdate()
	{
		this.frameScale = Mathf.MoveTowards(this.lastScaleFactor, this.scaleFactor, Time.deltaTime * 4f);
		base.transform.localScale = Vector3.one * this.frameScale;
		this.lastScaleFactor = this.frameScale;
	}

	public void AddLateUpdateCallback(ICallBack action)
	{
		this.lateUpdateCallbacks.Add(in action);
	}

	public void RemoveLateUpdateCallback(ICallBack action)
	{
		this.lateUpdateCallbacks.Remove(in action);
	}

	public void PostTick()
	{
		GTPlayer instance = GTPlayer.Instance;
		if (this.isOfflineVRRig)
		{
			if (GorillaGameManager.instance != null)
			{
				this.speedArray = GorillaGameManager.instance.LocalPlayerSpeed();
				instance.jumpMultiplier = this.speedArray[1];
				instance.maxJumpSpeed = this.speedArray[0];
			}
			else
			{
				instance.jumpMultiplier = 1.1f;
				instance.maxJumpSpeed = 6.5f;
			}
			this.nativeScale = instance.NativeScale;
			this.scaleMultiplier = instance.ScaleMultiplier;
			if (this.scaleFactor != this.lastScaleFactor)
			{
				this.ScaleUpdate();
			}
			base.transform.eulerAngles = new Vector3(0f, this.mainCamera.transform.rotation.eulerAngles.y, 0f);
			this.syncPos = this.mainCamera.transform.position + this.headConstraint.rotation * this.head.trackingPositionOffset * this.lastScaleFactor + base.transform.rotation * this.headBodyOffset * this.lastScaleFactor;
			base.transform.position = this.syncPos;
			this.head.MapMine(this.lastScaleFactor, this.playerOffsetTransform);
			this.rightHand.MapMine(this.lastScaleFactor, this.playerOffsetTransform);
			this.leftHand.MapMine(this.lastScaleFactor, this.playerOffsetTransform);
			this.rightIndex.MapMyFinger(this.lerpValueFingers);
			this.rightMiddle.MapMyFinger(this.lerpValueFingers);
			this.rightThumb.MapMyFinger(this.lerpValueFingers);
			this.leftIndex.MapMyFinger(this.lerpValueFingers);
			this.leftMiddle.MapMyFinger(this.lerpValueFingers);
			this.leftThumb.MapMyFinger(this.lerpValueFingers);
			bool flag = instance.IsGroundedHand || instance.IsThrusterActive;
			bool isGroundedButt = instance.IsGroundedButt;
			bool isLeftGrabbing = EquipmentInteractor.instance.isLeftGrabbing;
			bool flag2 = isLeftGrabbing && EquipmentInteractor.instance.CanGrabLeft();
			bool isRightGrabbing = EquipmentInteractor.instance.isRightGrabbing;
			bool flag3 = isRightGrabbing && EquipmentInteractor.instance.CanGrabRight();
			this.LastTouchedGroundAtNetworkTime = instance.LastTouchedGroundAtNetworkTime;
			this.LastHandTouchedGroundAtNetworkTime = instance.LastHandTouchedGroundAtNetworkTime;
			HandLink handLink = this.leftHandLink;
			if (handLink != null)
			{
				handLink.LocalUpdate(flag, isGroundedButt, isLeftGrabbing, flag2);
			}
			HandLink handLink2 = this.rightHandLink;
			if (handLink2 != null)
			{
				handLink2.LocalUpdate(flag, isGroundedButt, isRightGrabbing, flag3);
			}
			if (GorillaTagger.Instance.loadedDeviceName == "Oculus")
			{
				this.mainSkin.enabled = OVRManager.hasInputFocus;
			}
			this.bodyRenderer.ActiveBody.enabled = !instance.inOverlay;
			int i = this.loudnessCheckFrame - 1;
			this.loudnessCheckFrame = i;
			if (i < 0)
			{
				this.SpeakingLoudness = 0f;
				if (this.shouldSendSpeakingLoudness && this.netView)
				{
					PhotonVoiceView component = this.netView.GetComponent<PhotonVoiceView>();
					if (component && component.RecorderInUse)
					{
						MicWrapper micWrapper = component.RecorderInUse.InputSource as MicWrapper;
						if (micWrapper != null)
						{
							int num = this.replacementVoiceDetectionDelay;
							if (num > this.voiceSampleBuffer.Length)
							{
								Array.Resize<float>(ref this.voiceSampleBuffer, num);
							}
							float[] array = this.voiceSampleBuffer;
							if (micWrapper != null && micWrapper.Mic != null && micWrapper.Mic.samples >= num && micWrapper.Mic.GetData(array, micWrapper.Mic.samples - num))
							{
								float num2 = 0f;
								for (int j = 0; j < num; j++)
								{
									float num3 = Mathf.Sqrt(array[j]);
									if (num3 > num2)
									{
										num2 = num3;
									}
								}
								this.SpeakingLoudness = num2;
							}
						}
					}
				}
				this.loudnessCheckFrame = 10;
			}
			if (PhotonNetwork.InRoom && Time.time > this.nextLocalVelocityStoreTimestamp)
			{
				this.AddVelocityToQueue(base.transform.position, PhotonNetwork.Time);
				this.nextLocalVelocityStoreTimestamp = Time.time + 0.1f;
			}
		}
		if (this.leftHandLink.IsLinkActive())
		{
			VRRig myRig = this.leftHandLink.grabbedLink.myRig;
			if (this.isLocal && myRig.inDuplicationZone && myRig.duplicationZone.IsApplyingDisplacement)
			{
				this.leftHandLink.BreakLink();
			}
			else
			{
				this.leftHandLink.SnapHandsTogether();
			}
		}
		if (this.rightHandLink.IsLinkActive())
		{
			VRRig myRig2 = this.rightHandLink.grabbedLink.myRig;
			if (this.isLocal && myRig2.inDuplicationZone && myRig2.duplicationZone.IsApplyingDisplacement)
			{
				this.rightHandLink.BreakLink();
			}
			else
			{
				this.rightHandLink.SnapHandsTogether();
			}
		}
		if (this.creator != null)
		{
			if (GorillaGameManager.instance != null)
			{
				GorillaGameManager.instance.UpdatePlayerAppearance(this);
			}
			else if (this.setMatIndex != 0)
			{
				this.ChangeMaterialLocal(0);
				this.ForceResetFrozenEffect();
			}
		}
		if (this.inDuplicationZone)
		{
			this.renderTransform.position = base.transform.position + this.duplicationZone.VisualOffsetForRigs;
		}
		if (this.frozenEffect.activeSelf)
		{
			GorillaFreezeTagManager gorillaFreezeTagManager = GorillaGameManager.instance as GorillaFreezeTagManager;
			if (gorillaFreezeTagManager != null)
			{
				this.UpdateFrozen(Time.deltaTime, gorillaFreezeTagManager.freezeDuration);
			}
		}
		if (this.TemporaryCosmeticEffects.Count > 0)
		{
			foreach (KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> keyValuePair in this.TemporaryCosmeticEffects.ToArray<KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect>>())
			{
				if (Time.time - keyValuePair.Value.EffectStartedTime >= keyValuePair.Value.EffectDuration)
				{
					this.RemoveTemporaryCosmeticEffects(keyValuePair);
				}
			}
		}
		if (this.isOfflineVRRig && this.cosmeticSwapper.IsNotNull() && this.newSwappedCosmetics.Count > 0)
		{
			if (this.cosmeticSwapper.GetCurrentMode() == CosmeticSwapper.SwapMode.StepByStep)
			{
				if (this.isAtFinalCosmeticStep && this.cosmeticSwapper.ShouldHoldFinalStep())
				{
					if (Time.time - this.LastCosmeticSwapTime <= this.cosmeticStepsDuration)
					{
						return;
					}
					this.isAtFinalCosmeticStep = false;
				}
				if (Time.time - this.LastCosmeticSwapTime > this.cosmeticStepsDuration)
				{
					CosmeticSwapper.CosmeticState cosmeticState = this.newSwappedCosmetics.Pop();
					this.cosmeticSwapper.RestorePreviousCosmetic(cosmeticState, this);
					this.LastCosmeticSwapTime = Time.time;
					if (this.newSwappedCosmetics.Count == 0)
					{
						this.isAtFinalCosmeticStep = false;
					}
				}
			}
			else if (this.cosmeticSwapper.GetCurrentMode() == CosmeticSwapper.SwapMode.AllAtOnce && Time.time - this.LastCosmeticSwapTime > this.cosmeticStepsDuration)
			{
				while (this.newSwappedCosmetics.Count > 0)
				{
					CosmeticSwapper.CosmeticState cosmeticState2 = this.newSwappedCosmetics.Pop();
					this.cosmeticSwapper.RestorePreviousCosmetic(cosmeticState2, this);
				}
				this.LastCosmeticSwapTime = float.PositiveInfinity;
				this.isAtFinalCosmeticStep = false;
			}
		}
		this.lateUpdateCallbacks.TryRunCallbacks();
	}

	public void UpdateFrozen(float dt, float freezeDuration)
	{
		Vector3 localScale = this.frozenEffect.transform.localScale;
		Vector3 vector = localScale;
		vector.y = Mathf.Lerp(this.frozenEffectMinY, this.frozenEffectMaxY, this.frozenTimeElapsed / freezeDuration);
		localScale = new Vector3(localScale.x, vector.y, localScale.z);
		this.frozenEffect.transform.localScale = localScale;
		this.frozenTimeElapsed += dt;
	}

	private void RemoveTemporaryCosmeticEffects(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect)
	{
		if (effect.Key == CosmeticEffectsOnPlayers.EFFECTTYPE.Skin)
		{
			bool flag;
			if (effect.Value.newSkin != null && GorillaSkin.GetActiveSkin(this, out flag) == effect.Value.newSkin)
			{
				GorillaSkin.ApplyToRig(this, null, GorillaSkin.SkinType.temporaryEffect);
			}
		}
		else if (effect.Key == CosmeticEffectsOnPlayers.EFFECTTYPE.TagWithKnockback)
		{
			this.DisableHitWithKnockBack(effect);
		}
		this.TemporaryCosmeticEffects.Remove(effect.Key);
	}

	public void SpawnSkinEffects(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect)
	{
		GorillaSkin.ApplyToRig(this, effect.Value.newSkin, GorillaSkin.SkinType.temporaryEffect);
		this.TemporaryCosmeticEffects.TryAdd(effect.Key, effect.Value);
	}

	public void EnableHitWithKnockBack(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect)
	{
		this.TemporaryCosmeticEffects.TryAdd(effect.Key, effect.Value);
	}

	private void DisableHitWithKnockBack(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect)
	{
		if (this.TemporaryCosmeticEffects.ContainsKey(effect.Key) && effect.Value.knockbackVFX)
		{
			GameObject gameObject = ObjectPools.instance.Instantiate(effect.Value.knockbackVFX, base.transform.position, true);
			if (gameObject != null)
			{
				gameObject.gameObject.transform.SetParent(base.transform);
				gameObject.gameObject.transform.localPosition = Vector3.zero;
			}
		}
	}

	public void DisableHitWithKnockBack()
	{
		foreach (KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> keyValuePair in this.TemporaryCosmeticEffects.ToArray<KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect>>())
		{
			bool flag;
			if (keyValuePair.Key == CosmeticEffectsOnPlayers.EFFECTTYPE.TagWithKnockback)
			{
				this.DisableHitWithKnockBack(keyValuePair);
				this.TemporaryCosmeticEffects.Remove(keyValuePair.Key);
			}
			else if (keyValuePair.Key == CosmeticEffectsOnPlayers.EFFECTTYPE.Skin && keyValuePair.Value.newSkin != null && GorillaSkin.GetActiveSkin(this, out flag) == keyValuePair.Value.newSkin)
			{
				GorillaSkin.ApplyToRig(this, null, GorillaSkin.SkinType.temporaryEffect);
				this.TemporaryCosmeticEffects.Remove(keyValuePair.Key);
			}
		}
	}

	public void ApplyInstanceKnockBack(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect)
	{
		this.TemporaryCosmeticEffects.TryAdd(effect.Key, effect.Value);
	}

	public void ActivateVOEffect(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect)
	{
		this.TemporaryCosmeticEffects.TryAdd(effect.Key, effect.Value);
	}

	public bool TryGetCosmeticVoiceOverride(CosmeticEffectsOnPlayers.EFFECTTYPE key, out CosmeticEffectsOnPlayers.CosmeticEffect value)
	{
		if (this.TemporaryCosmeticEffects == null)
		{
			value = null;
			return false;
		}
		return this.TemporaryCosmeticEffects.TryGetValue(key, out value);
	}

	public void PlayCosmeticEffectSFX(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect)
	{
		this.TemporaryCosmeticEffects.TryAdd(effect.Key, effect.Value);
		int num = global::UnityEngine.Random.Range(0, effect.Value.sfxAudioClip.Count);
		this.tagSound.PlayOneShot(effect.Value.sfxAudioClip[num]);
	}

	public void SpawnVFXEffect(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect)
	{
		GameObject gameObject = ObjectPools.instance.Instantiate(effect.Value.VFXGameObject, base.transform.position, true);
		if (gameObject != null)
		{
			gameObject.gameObject.transform.SetParent(base.transform);
			gameObject.gameObject.transform.localPosition = Vector3.zero;
		}
	}

	public bool IsPlayerMeshHidden
	{
		get
		{
			return !this.mainSkin.enabled;
		}
	}

	public void SetPlayerMeshHidden(bool hide)
	{
		this.mainSkin.enabled = !hide;
		this.faceSkin.enabled = !hide;
		this.nameTagAnchor.SetActive(!hide);
		this.UpdateMatParticles(-1);
	}

	public void SetInvisibleToLocalPlayer(bool invisible)
	{
		if (this.IsInvisibleToLocalPlayer == invisible)
		{
			return;
		}
		this.IsInvisibleToLocalPlayer = invisible;
		this.nameTagAnchor.SetActive(!invisible);
		this.UpdateFriendshipBracelet();
	}

	public void ChangeLayer(string layerName)
	{
		if (this.layerChanger != null)
		{
			this.layerChanger.ChangeLayer(base.transform.parent, layerName);
		}
		GTPlayer.Instance.ChangeLayer(layerName);
	}

	public void RestoreLayer()
	{
		if (this.layerChanger != null)
		{
			this.layerChanger.RestoreOriginalLayers();
		}
		GTPlayer.Instance.RestoreLayer();
	}

	public void SetHeadBodyOffset()
	{
	}

	public void VRRigResize(float ratioVar)
	{
		this.ratio *= ratioVar;
	}

	public int ReturnHandPosition()
	{
		return 0 + Mathf.FloorToInt(this.rightIndex.calcT * 9.99f) + Mathf.FloorToInt(this.rightMiddle.calcT * 9.99f) * 10 + Mathf.FloorToInt(this.rightThumb.calcT * 9.99f) * 100 + Mathf.FloorToInt(this.leftIndex.calcT * 9.99f) * 1000 + Mathf.FloorToInt(this.leftMiddle.calcT * 9.99f) * 10000 + Mathf.FloorToInt(this.leftThumb.calcT * 9.99f) * 100000 + this.leftHandHoldableStatus * 1000000 + this.rightHandHoldableStatus * 10000000;
	}

	public void OnDestroy()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (this.currentRopeSwingTarget && this.currentRopeSwingTarget.gameObject)
		{
			Object.Destroy(this.currentRopeSwingTarget.gameObject);
		}
		this.ClearRopeData();
	}

	private InputStruct SerializeWriteShared()
	{
		InputStruct inputStruct = default(InputStruct);
		inputStruct.headRotation = BitPackUtils.PackQuaternionForNetwork(this.head.rigTarget.localRotation);
		inputStruct.rightHandLong = BitPackUtils.PackHandPosRotForNetwork(this.rightHand.rigTarget.localPosition, this.rightHand.rigTarget.localRotation);
		inputStruct.leftHandLong = BitPackUtils.PackHandPosRotForNetwork(this.leftHand.rigTarget.localPosition, this.leftHand.rigTarget.localRotation);
		inputStruct.position = BitPackUtils.PackWorldPosForNetwork(base.transform.position);
		inputStruct.handPosition = this.ReturnHandPosition();
		inputStruct.taggedById = (short)this.taggedById;
		int num = Mathf.Clamp(Mathf.RoundToInt(base.transform.rotation.eulerAngles.y + 360f) % 360, 0, 360);
		int num2 = Mathf.RoundToInt(Mathf.Clamp01(this.SpeakingLoudness) * 255f);
		bool flag = this.leftHandLink.IsLinkActive() || this.rightHandLink.IsLinkActive();
		GorillaGameManager activeGameMode = global::GorillaGameModes.GameMode.ActiveGameMode;
		bool flag2 = activeGameMode != null && activeGameMode.GameType() == GameModeType.PropHunt;
		int num3 = num + (this.remoteUseReplacementVoice ? 512 : 0) + ((this.grabbedRopeIndex != -1) ? 1024 : 0) + (this.grabbedRopeIsPhotonView ? 2048 : 0) + (flag ? 4096 : 0) + (this.hoverboardVisual.IsHeld ? 8192 : 0) + (this.hoverboardVisual.IsLeftHanded ? 16384 : 0) + ((this.mountedMovingSurfaceId != -1) ? 32768 : 0) + (flag2 ? 65536 : 0) + (this.propHuntHandFollower.IsLeftHand ? 131072 : 0) + (this.leftHandLink.CanBeGrabbed() ? 262144 : 0) + (this.rightHandLink.CanBeGrabbed() ? 524288 : 0) + (num2 << 24);
		inputStruct.packedFields = num3;
		inputStruct.packedCompetitiveData = this.PackCompetitiveData();
		if (this.grabbedRopeIndex != -1)
		{
			inputStruct.grabbedRopeIndex = this.grabbedRopeIndex;
			inputStruct.ropeBoneIndex = this.grabbedRopeBoneIndex;
			inputStruct.ropeGrabIsLeft = this.grabbedRopeIsLeft;
			inputStruct.ropeGrabIsBody = this.grabbedRopeIsBody;
			inputStruct.ropeGrabOffset = this.grabbedRopeOffset;
		}
		if (this.grabbedRopeIndex == -1 && this.mountedMovingSurfaceId != -1)
		{
			inputStruct.grabbedRopeIndex = this.mountedMovingSurfaceId;
			inputStruct.ropeGrabIsLeft = this.mountedMovingSurfaceIsLeft;
			inputStruct.ropeGrabIsBody = this.mountedMovingSurfaceIsBody;
			inputStruct.ropeGrabOffset = this.mountedMonkeBlockOffset;
		}
		if (this.hoverboardVisual.IsHeld)
		{
			inputStruct.hoverboardPosRot = BitPackUtils.PackHandPosRotForNetwork(this.hoverboardVisual.NominalLocalPosition, this.hoverboardVisual.NominalLocalRotation);
			inputStruct.hoverboardColor = BitPackUtils.PackColorForNetwork(this.hoverboardVisual.boardColor);
		}
		if (flag2)
		{
			inputStruct.propHuntPosRot = this.propHuntHandFollower.GetRelativePosRotLong();
		}
		if (flag)
		{
			this.leftHandLink.Write(out inputStruct.isGroundedHand, out inputStruct.isGroundedButt, out inputStruct.leftHandGrabbedActorNumber, out inputStruct.leftGrabbedHandIsLeft);
			this.rightHandLink.Write(out inputStruct.isGroundedHand, out inputStruct.isGroundedButt, out inputStruct.rightHandGrabbedActorNumber, out inputStruct.rightGrabbedHandIsLeft);
			inputStruct.lastTouchedGroundAtTime = this.LastTouchedGroundAtNetworkTime;
			inputStruct.lastHandTouchedGroundAtTime = this.LastHandTouchedGroundAtNetworkTime;
		}
		return inputStruct;
	}

	private void SerializeReadShared(InputStruct data)
	{
		VRMap vrmap = this.head;
		Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(data.headRotation);
		(ref vrmap.syncRotation).SetValueSafe(in quaternion);
		BitPackUtils.UnpackHandPosRotFromNetwork(data.rightHandLong, out this.tempVec, out this.tempQuat);
		this.rightHand.syncPos = this.tempVec;
		(ref this.rightHand.syncRotation).SetValueSafe(in this.tempQuat);
		BitPackUtils.UnpackHandPosRotFromNetwork(data.leftHandLong, out this.tempVec, out this.tempQuat);
		this.leftHand.syncPos = this.tempVec;
		(ref this.leftHand.syncRotation).SetValueSafe(in this.tempQuat);
		this.syncPos = BitPackUtils.UnpackWorldPosFromNetwork(data.position);
		this.handSync = data.handPosition;
		int packedFields = data.packedFields;
		int num = packedFields & 511;
		this.syncRotation.eulerAngles = this.SanitizeVector3(new Vector3(0f, (float)num, 0f));
		this.remoteUseReplacementVoice = (packedFields & 512) != 0;
		int num2 = (packedFields >> 24) & 255;
		this.SpeakingLoudness = (float)num2 / 255f;
		this.UpdateReplacementVoice();
		this.UnpackCompetitiveData(data.packedCompetitiveData);
		this.taggedById = (int)data.taggedById;
		bool flag = (packedFields & 1024) != 0;
		this.grabbedRopeIsPhotonView = (packedFields & 2048) != 0;
		if (flag)
		{
			this.grabbedRopeIndex = data.grabbedRopeIndex;
			this.grabbedRopeBoneIndex = data.ropeBoneIndex;
			this.grabbedRopeIsLeft = data.ropeGrabIsLeft;
			this.grabbedRopeIsBody = data.ropeGrabIsBody;
			(ref this.grabbedRopeOffset).SetValueSafe(in data.ropeGrabOffset);
		}
		else
		{
			this.grabbedRopeIndex = -1;
		}
		bool flag2 = (packedFields & 32768) != 0;
		if (!flag && flag2)
		{
			this.mountedMovingSurfaceId = data.grabbedRopeIndex;
			this.mountedMovingSurfaceIsLeft = data.ropeGrabIsLeft;
			this.mountedMovingSurfaceIsBody = data.ropeGrabIsBody;
			(ref this.mountedMonkeBlockOffset).SetValueSafe(in data.ropeGrabOffset);
			this.movingSurfaceIsMonkeBlock = data.movingSurfaceIsMonkeBlock;
		}
		else
		{
			this.mountedMovingSurfaceId = -1;
		}
		bool flag3 = (packedFields & 8192) != 0;
		bool flag4 = (packedFields & 16384) != 0;
		if (flag3)
		{
			Vector3 vector;
			Quaternion quaternion2;
			BitPackUtils.UnpackHandPosRotFromNetwork(data.hoverboardPosRot, out vector, out quaternion2);
			Color color = BitPackUtils.UnpackColorFromNetwork(data.hoverboardColor);
			if ((in quaternion2).IsValid())
			{
				this.hoverboardVisual.SetIsHeld(flag4, vector.ClampMagnitudeSafe(1f), quaternion2, color);
			}
		}
		else if (this.hoverboardVisual.gameObject.activeSelf)
		{
			this.hoverboardVisual.SetNotHeld();
		}
		if ((packedFields & 65536) != 0)
		{
			bool flag5 = (packedFields & 131072) != 0;
			Vector3 vector2;
			Quaternion quaternion3;
			BitPackUtils.UnpackHandPosRotFromNetwork(data.propHuntPosRot, out vector2, out quaternion3);
			this.propHuntHandFollower.SetProp(flag5, vector2, quaternion3);
		}
		if (this.grabbedRopeIsPhotonView)
		{
			this.localGrabOverrideBlend = -1f;
		}
		Vector3 position = base.transform.position;
		this.leftHandLink.Read(this.leftHand.syncPos, this.syncRotation, position, data.isGroundedHand, data.isGroundedButt, (packedFields & 262144) != 0, data.leftHandGrabbedActorNumber, data.leftGrabbedHandIsLeft);
		this.rightHandLink.Read(this.rightHand.syncPos, this.syncRotation, position, data.isGroundedHand, data.isGroundedButt, (packedFields & 524288) != 0, data.rightHandGrabbedActorNumber, data.rightGrabbedHandIsLeft);
		this.LastTouchedGroundAtNetworkTime = data.lastTouchedGroundAtTime;
		this.LastHandTouchedGroundAtNetworkTime = data.lastHandTouchedGroundAtTime;
		this.UpdateRopeData();
		this.UpdateMovingMonkeBlockData();
		this.AddVelocityToQueue(this.syncPos, data.serverTimeStamp);
	}

	void IWrappedSerializable.OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
	{
		InputStruct inputStruct = this.SerializeWriteShared();
		stream.SendNext(inputStruct.headRotation);
		stream.SendNext(inputStruct.rightHandLong);
		stream.SendNext(inputStruct.leftHandLong);
		stream.SendNext(inputStruct.position);
		stream.SendNext(inputStruct.handPosition);
		stream.SendNext(inputStruct.packedFields);
		stream.SendNext(inputStruct.packedCompetitiveData);
		if (this.grabbedRopeIndex != -1)
		{
			stream.SendNext(inputStruct.grabbedRopeIndex);
			stream.SendNext(inputStruct.ropeBoneIndex);
			stream.SendNext(inputStruct.ropeGrabIsLeft);
			stream.SendNext(inputStruct.ropeGrabIsBody);
			stream.SendNext(inputStruct.ropeGrabOffset);
		}
		else if (this.mountedMovingSurfaceId != -1)
		{
			stream.SendNext(inputStruct.grabbedRopeIndex);
			stream.SendNext(inputStruct.ropeGrabIsLeft);
			stream.SendNext(inputStruct.ropeGrabIsBody);
			stream.SendNext(inputStruct.ropeGrabOffset);
			stream.SendNext(inputStruct.movingSurfaceIsMonkeBlock);
		}
		if ((inputStruct.packedFields & 8192) != 0)
		{
			stream.SendNext(inputStruct.hoverboardPosRot);
			stream.SendNext(inputStruct.hoverboardColor);
		}
		if ((inputStruct.packedFields & 4096) != 0)
		{
			stream.SendNext(inputStruct.isGroundedHand);
			stream.SendNext(inputStruct.isGroundedButt);
			stream.SendNext(inputStruct.leftHandGrabbedActorNumber);
			stream.SendNext(inputStruct.leftGrabbedHandIsLeft);
			stream.SendNext(inputStruct.rightHandGrabbedActorNumber);
			stream.SendNext(inputStruct.rightGrabbedHandIsLeft);
			stream.SendNext(inputStruct.lastTouchedGroundAtTime);
			stream.SendNext(inputStruct.lastHandTouchedGroundAtTime);
		}
		if ((inputStruct.packedFields & 65536) != 0)
		{
			stream.SendNext(inputStruct.propHuntPosRot);
		}
	}

	void IWrappedSerializable.OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
	{
		double sentServerTime = info.SentServerTime;
		InputStruct inputStruct = new InputStruct
		{
			headRotation = (int)stream.ReceiveNext(),
			rightHandLong = (long)stream.ReceiveNext(),
			leftHandLong = (long)stream.ReceiveNext(),
			position = (long)stream.ReceiveNext(),
			handPosition = (int)stream.ReceiveNext(),
			packedFields = (int)stream.ReceiveNext(),
			packedCompetitiveData = (short)stream.ReceiveNext()
		};
		bool flag = (inputStruct.packedFields & 1024) != 0;
		bool flag2 = (inputStruct.packedFields & 32768) != 0;
		if (flag)
		{
			inputStruct.grabbedRopeIndex = (int)stream.ReceiveNext();
			inputStruct.ropeBoneIndex = (int)stream.ReceiveNext();
			inputStruct.ropeGrabIsLeft = (bool)stream.ReceiveNext();
			inputStruct.ropeGrabIsBody = (bool)stream.ReceiveNext();
			inputStruct.ropeGrabOffset = (Vector3)stream.ReceiveNext();
		}
		else if (flag2)
		{
			inputStruct.grabbedRopeIndex = (int)stream.ReceiveNext();
			inputStruct.ropeGrabIsLeft = (bool)stream.ReceiveNext();
			inputStruct.ropeGrabIsBody = (bool)stream.ReceiveNext();
			inputStruct.ropeGrabOffset = (Vector3)stream.ReceiveNext();
		}
		if ((inputStruct.packedFields & 8192) != 0)
		{
			inputStruct.hoverboardPosRot = (long)stream.ReceiveNext();
			inputStruct.hoverboardColor = (short)stream.ReceiveNext();
		}
		if ((inputStruct.packedFields & 4096) != 0)
		{
			inputStruct.isGroundedHand = (bool)stream.ReceiveNext();
			inputStruct.isGroundedButt = (bool)stream.ReceiveNext();
			inputStruct.leftHandGrabbedActorNumber = (int)stream.ReceiveNext();
			inputStruct.leftGrabbedHandIsLeft = (bool)stream.ReceiveNext();
			inputStruct.rightHandGrabbedActorNumber = (int)stream.ReceiveNext();
			inputStruct.rightGrabbedHandIsLeft = (bool)stream.ReceiveNext();
			inputStruct.lastTouchedGroundAtTime = (float)stream.ReceiveNext();
			inputStruct.lastHandTouchedGroundAtTime = (float)stream.ReceiveNext();
		}
		if ((inputStruct.packedFields & 65536) != 0)
		{
			inputStruct.propHuntPosRot = (long)stream.ReceiveNext();
		}
		inputStruct.serverTimeStamp = info.SentServerTime;
		this.SerializeReadShared(inputStruct);
	}

	public object OnSerializeWrite()
	{
		InputStruct inputStruct = this.SerializeWriteShared();
		double num = NetworkSystem.Instance.SimTick / 1000.0;
		inputStruct.serverTimeStamp = num;
		return inputStruct;
	}

	public void OnSerializeRead(object objectData)
	{
		InputStruct inputStruct = (InputStruct)objectData;
		this.SerializeReadShared(inputStruct);
	}

	private void UpdateExtrapolationTarget()
	{
		float num = (float)(NetworkSystem.Instance.SimTime - this.remoteLatestTimestamp);
		num -= 0.15f;
		num = Mathf.Clamp(num, -0.5f, 0.5f);
		this.syncPos += this.remoteVelocity * num;
		this.remoteCorrectionNeeded = this.syncPos - base.transform.position;
		if (this.remoteCorrectionNeeded.magnitude > 1.5f && this.grabbedRopeIndex <= 0)
		{
			base.transform.position = this.syncPos;
			this.remoteCorrectionNeeded = Vector3.zero;
		}
	}

	private void UpdateRopeData()
	{
		if (this.previousGrabbedRope == this.grabbedRopeIndex && this.previousGrabbedRopeBoneIndex == this.grabbedRopeBoneIndex && this.previousGrabbedRopeWasLeft == this.grabbedRopeIsLeft && this.previousGrabbedRopeWasBody == this.grabbedRopeIsBody)
		{
			return;
		}
		this.ClearRopeData();
		if (this.grabbedRopeIndex != -1)
		{
			GorillaRopeSwing gorillaRopeSwing;
			if (this.grabbedRopeIsPhotonView)
			{
				PhotonView photonView = PhotonView.Find(this.grabbedRopeIndex);
				GorillaClimbable gorillaClimbable;
				HandHoldXSceneRef handHoldXSceneRef;
				VRRigSerializer vrrigSerializer;
				if (photonView.TryGetComponent<GorillaClimbable>(out gorillaClimbable))
				{
					this.currentHoldParent = photonView.transform;
				}
				else if (photonView.TryGetComponent<HandHoldXSceneRef>(out handHoldXSceneRef))
				{
					GameObject targetObject = handHoldXSceneRef.targetObject;
					this.currentHoldParent = ((targetObject != null) ? targetObject.transform : null);
				}
				else if (photonView && photonView.TryGetComponent<VRRigSerializer>(out vrrigSerializer))
				{
					this.currentHoldParent = ((this.grabbedRopeBoneIndex == 1) ? vrrigSerializer.VRRig.leftHandHoldsPlayer.transform : vrrigSerializer.VRRig.rightHandHoldsPlayer.transform);
				}
			}
			else if (RopeSwingManager.instance.TryGetRope(this.grabbedRopeIndex, out gorillaRopeSwing) && gorillaRopeSwing != null)
			{
				if (this.currentRopeSwingTarget == null || this.currentRopeSwingTarget.gameObject == null)
				{
					this.currentRopeSwingTarget = new GameObject("RopeSwingTarget").transform;
				}
				if (gorillaRopeSwing.AttachRemotePlayer(this.creator.ActorNumber, this.grabbedRopeBoneIndex, this.currentRopeSwingTarget, this.grabbedRopeOffset))
				{
					this.currentRopeSwing = gorillaRopeSwing;
				}
				this.lastRopeGrabTimer = 0f;
			}
		}
		else if (this.previousGrabbedRope != -1)
		{
			PhotonView photonView2 = PhotonView.Find(this.previousGrabbedRope);
			VRRigSerializer vrrigSerializer2;
			if (photonView2 && photonView2.TryGetComponent<VRRigSerializer>(out vrrigSerializer2) && vrrigSerializer2.VRRig == VRRig.LocalRig)
			{
				EquipmentInteractor.instance.ForceDropEquipment(this.bodyHolds);
				EquipmentInteractor.instance.ForceDropEquipment(this.leftHolds);
				EquipmentInteractor.instance.ForceDropEquipment(this.rightHolds);
			}
		}
		this.shouldLerpToRope = true;
		this.previousGrabbedRope = this.grabbedRopeIndex;
		this.previousGrabbedRopeBoneIndex = this.grabbedRopeBoneIndex;
		this.previousGrabbedRopeWasLeft = this.grabbedRopeIsLeft;
		this.previousGrabbedRopeWasBody = this.grabbedRopeIsBody;
	}

	private void UpdateMovingMonkeBlockData()
	{
		if (this.mountedMonkeBlockOffset.sqrMagnitude > 2f)
		{
			this.mountedMovingSurfaceId = -1;
			this.mountedMovingSurfaceIsLeft = false;
			this.mountedMovingSurfaceIsBody = false;
			this.mountedMonkeBlock = null;
			this.mountedMovingSurface = null;
		}
		if (this.prevMovingSurfaceID == this.mountedMovingSurfaceId && this.movingSurfaceWasBody == this.mountedMovingSurfaceIsBody && this.movingSurfaceWasLeft == this.mountedMovingSurfaceIsLeft && this.movingSurfaceWasMonkeBlock == this.movingSurfaceIsMonkeBlock)
		{
			return;
		}
		if (this.mountedMovingSurfaceId == -1)
		{
			this.mountedMovingSurfaceIsLeft = false;
			this.mountedMovingSurfaceIsBody = false;
			this.mountedMonkeBlock = null;
			this.mountedMovingSurface = null;
		}
		else if (this.movingSurfaceIsMonkeBlock)
		{
			this.mountedMonkeBlock = null;
			BuilderTable builderTable;
			if (BuilderTable.TryGetBuilderTableForZone(this.zoneEntity.currentZone, out builderTable))
			{
				this.mountedMonkeBlock = builderTable.GetPiece(this.mountedMovingSurfaceId);
			}
			if (this.mountedMonkeBlock == null)
			{
				this.mountedMovingSurfaceId = -1;
				this.mountedMovingSurfaceIsLeft = false;
				this.mountedMovingSurfaceIsBody = false;
				this.mountedMonkeBlock = null;
				this.mountedMovingSurface = null;
			}
		}
		else if (MovingSurfaceManager.instance == null || !MovingSurfaceManager.instance.TryGetMovingSurface(this.mountedMovingSurfaceId, out this.mountedMovingSurface))
		{
			this.mountedMovingSurfaceId = -1;
			this.mountedMovingSurfaceIsLeft = false;
			this.mountedMovingSurfaceIsBody = false;
			this.mountedMonkeBlock = null;
			this.mountedMovingSurface = null;
		}
		if (this.mountedMovingSurfaceId != -1 && this.prevMovingSurfaceID == -1)
		{
			this.shouldLerpToMovingSurface = true;
			this.lastMountedSurfaceTimer = 0f;
		}
		this.prevMovingSurfaceID = this.mountedMovingSurfaceId;
		this.movingSurfaceWasLeft = this.mountedMovingSurfaceIsLeft;
		this.movingSurfaceWasBody = this.mountedMovingSurfaceIsBody;
		this.movingSurfaceWasMonkeBlock = this.movingSurfaceIsMonkeBlock;
	}

	public static void AttachLocalPlayerToMovingSurface(int blockId, bool isLeft, bool isBody, Vector3 offset, bool isMonkeBlock)
	{
		if (GorillaTagger.hasInstance && GorillaTagger.Instance.offlineVRRig)
		{
			GorillaTagger.Instance.offlineVRRig.mountedMovingSurfaceId = blockId;
			GorillaTagger.Instance.offlineVRRig.mountedMovingSurfaceIsLeft = isLeft;
			GorillaTagger.Instance.offlineVRRig.mountedMovingSurfaceIsBody = isBody;
			GorillaTagger.Instance.offlineVRRig.movingSurfaceIsMonkeBlock = isMonkeBlock;
			GorillaTagger.Instance.offlineVRRig.mountedMonkeBlockOffset = offset;
		}
	}

	public static void DetachLocalPlayerFromMovingSurface()
	{
		if (GorillaTagger.hasInstance && GorillaTagger.Instance.offlineVRRig)
		{
			GorillaTagger.Instance.offlineVRRig.mountedMovingSurfaceId = -1;
		}
	}

	public static void AttachLocalPlayerToPhotonView(PhotonView view, XRNode xrNode, Vector3 offset, Vector3 velocity)
	{
		if (GorillaTagger.hasInstance && GorillaTagger.Instance.offlineVRRig)
		{
			GorillaTagger.Instance.offlineVRRig.grabbedRopeIndex = view.ViewID;
			GorillaTagger.Instance.offlineVRRig.grabbedRopeIsLeft = xrNode == XRNode.LeftHand;
			GorillaTagger.Instance.offlineVRRig.grabbedRopeOffset = offset;
			GorillaTagger.Instance.offlineVRRig.grabbedRopeIsPhotonView = true;
		}
	}

	public static void DetachLocalPlayerFromPhotonView()
	{
		if (GorillaTagger.hasInstance && GorillaTagger.Instance.offlineVRRig)
		{
			GorillaTagger.Instance.offlineVRRig.grabbedRopeIndex = -1;
		}
	}

	private void ClearRopeData()
	{
		if (this.currentRopeSwing)
		{
			this.currentRopeSwing.DetachRemotePlayer(this.creator.ActorNumber);
		}
		if (this.currentRopeSwingTarget)
		{
			this.currentRopeSwingTarget.SetParent(null);
		}
		this.currentRopeSwing = null;
		this.currentHoldParent = null;
	}

	public void ChangeMaterial(int materialIndex, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			this.ChangeMaterialLocal(materialIndex);
		}
	}

	public void UpdateFrozenEffect(bool enable)
	{
		if (this.frozenEffect != null && ((!this.frozenEffect.activeSelf && enable) || (this.frozenEffect.activeSelf && !enable)))
		{
			this.frozenEffect.SetActive(enable);
			if (enable)
			{
				this.frozenTimeElapsed = 0f;
			}
			else
			{
				Vector3 localScale = this.frozenEffect.transform.localScale;
				localScale = new Vector3(localScale.x, this.frozenEffectMinY, localScale.z);
				this.frozenEffect.transform.localScale = localScale;
			}
		}
		if (this.iceCubeLeft != null && ((!this.iceCubeLeft.activeSelf && enable) || (this.iceCubeLeft.activeSelf && !enable)))
		{
			this.iceCubeLeft.SetActive(enable);
		}
		if (this.iceCubeRight != null && ((!this.iceCubeRight.activeSelf && enable) || (this.iceCubeRight.activeSelf && !enable)))
		{
			this.iceCubeRight.SetActive(enable);
		}
	}

	public void ForceResetFrozenEffect()
	{
		this.frozenEffect.SetActive(false);
		this.iceCubeRight.SetActive(false);
		this.iceCubeLeft.SetActive(false);
	}

	public void ChangeMaterialLocal(int materialIndex)
	{
		if (this.setMatIndex == materialIndex)
		{
			return;
		}
		int num = this.setMatIndex;
		this.setMatIndex = materialIndex;
		if (this.setMatIndex > -1 && this.setMatIndex < this.materialsToChangeTo.Length)
		{
			this.bodyRenderer.SetMaterialIndex(materialIndex);
		}
		this.UpdateMatParticles(materialIndex);
		if (materialIndex > 0 && VRRig.LocalRig != this)
		{
			this.PlayTaggedEffect();
		}
		Action<int, int> onMaterialIndexChanged = this.OnMaterialIndexChanged;
		if (onMaterialIndexChanged == null)
		{
			return;
		}
		onMaterialIndexChanged(num, this.setMatIndex);
	}

	public void PlayTaggedEffect()
	{
		TagEffectPack tagEffectPack = null;
		quaternion quaternion = base.transform.rotation;
		TagEffectsLibrary.EffectType effectType = ((VRRig.LocalRig == this) ? TagEffectsLibrary.EffectType.FIRST_PERSON : TagEffectsLibrary.EffectType.THIRD_PERSON);
		if (GorillaGameManager.instance != null && this.OwningNetPlayer == null)
		{
			GorillaGameManager.instance.lastTaggedActorNr.TryGetValue(this.OwningNetPlayer.ActorNumber, out this.taggedById);
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(this.taggedById);
		RigContainer rigContainer;
		if (player != null && VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			tagEffectPack = rigContainer.Rig.CosmeticEffectPack;
			if (tagEffectPack && tagEffectPack.shouldFaceTagger && effectType == TagEffectsLibrary.EffectType.THIRD_PERSON)
			{
				quaternion = Quaternion.LookRotation((rigContainer.Rig.transform.position - base.transform.position).normalized);
			}
		}
		TagEffectsLibrary.PlayEffect(base.transform, false, this.scaleFactor, effectType, this.CosmeticEffectPack, tagEffectPack, quaternion);
	}

	public void UpdateMatParticles(int materialIndex)
	{
		if (this.lavaParticleSystem != null)
		{
			if (!this.isOfflineVRRig && materialIndex == 2 && this.lavaParticleSystem.isStopped)
			{
				this.lavaParticleSystem.Play();
			}
			else if (!this.isOfflineVRRig && this.lavaParticleSystem.isPlaying)
			{
				this.lavaParticleSystem.Stop();
			}
		}
		if (this.rockParticleSystem != null)
		{
			if (!this.isOfflineVRRig && materialIndex == 1 && this.rockParticleSystem.isStopped)
			{
				this.rockParticleSystem.Play();
			}
			else if (!this.isOfflineVRRig && this.rockParticleSystem.isPlaying)
			{
				this.rockParticleSystem.Stop();
			}
		}
		if (this.iceParticleSystem != null)
		{
			if (!this.isOfflineVRRig && materialIndex == 3 && this.rockParticleSystem.isStopped)
			{
				this.iceParticleSystem.Play();
			}
			else if (!this.isOfflineVRRig && this.iceParticleSystem.isPlaying)
			{
				this.iceParticleSystem.Stop();
			}
		}
		if (this.snowFlakeParticleSystem != null)
		{
			if (!this.isOfflineVRRig && materialIndex == 14 && this.snowFlakeParticleSystem.isStopped)
			{
				this.snowFlakeParticleSystem.Play();
				return;
			}
			if (!this.isOfflineVRRig && this.snowFlakeParticleSystem.isPlaying)
			{
				this.snowFlakeParticleSystem.Stop();
			}
		}
	}

	public void InitializeNoobMaterial(float red, float green, float blue, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "RPC_InitializeNoobMaterial");
		NetworkSystem.Instance.GetPlayer(info.senderID);
		string userID = NetworkSystem.Instance.GetUserID(info.senderID);
		if (info.senderID == NetworkSystem.Instance.GetOwningPlayerID(this.rigSerializer.gameObject) && (!this.initialized || (this.initialized && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(userID)) || (this.initialized && CosmeticWardrobeProximityDetector.IsUserNearWardrobe(userID))))
		{
			this.initialized = true;
			blue = blue.ClampSafe(0f, 1f);
			red = red.ClampSafe(0f, 1f);
			green = green.ClampSafe(0f, 1f);
			this.InitializeNoobMaterialLocal(red, green, blue);
		}
	}

	public void InitializeNoobMaterialLocal(float red, float green, float blue)
	{
		Color color = new Color(red, green, blue);
		color.r = Mathf.Clamp(color.r, 0f, 1f);
		color.g = Mathf.Clamp(color.g, 0f, 1f);
		color.b = Mathf.Clamp(color.b, 0f, 1f);
		this.bodyRenderer.UpdateColor(color);
		this.SetColor(color);
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags);
		this.UpdateName(flag);
	}

	public void UpdateName(bool isNamePermissionEnabled)
	{
		if (!this.isOfflineVRRig && this.creator != null)
		{
			string text = ((isNamePermissionEnabled && GorillaComputer.instance.NametagsEnabled) ? this.creator.NickName : this.creator.DefaultName);
			this.playerNameVisible = this.NormalizeName(true, text);
		}
		else if (this.showName && NetworkSystem.Instance != null)
		{
			this.playerNameVisible = ((isNamePermissionEnabled && GorillaComputer.instance.NametagsEnabled) ? NetworkSystem.Instance.GetMyNickName() : NetworkSystem.Instance.GetMyDefaultName());
		}
		this.SetNameTagText(this.playerNameVisible);
		if (this.creator != null)
		{
			this.creator.SanitizedNickName = this.playerNameVisible;
		}
		Action onPlayerNameVisibleChanged = this.OnPlayerNameVisibleChanged;
		if (onPlayerNameVisibleChanged == null)
		{
			return;
		}
		onPlayerNameVisibleChanged();
	}

	public void SetNameTagText(string name)
	{
		this.playerNameVisible = name;
		this.playerText1.text = name;
		Action<RigContainer> onNameChanged = this.OnNameChanged;
		if (onNameChanged == null)
		{
			return;
		}
		onNameChanged(this.rigContainer);
	}

	public void UpdateName()
	{
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Custom_Nametags);
		bool flag = (permissionDataByFeature.Enabled || permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PLAYER) && permissionDataByFeature.ManagedBy != Permission.ManagedByEnum.PROHIBITED;
		this.UpdateName(flag);
	}

	public string NormalizeName(bool doIt, string text)
	{
		if (doIt)
		{
			int length = text.Length;
			text = new string(Array.FindAll<char>(text.ToCharArray(), (char c) => Utils.IsASCIILetterOrDigit(c)));
			int length2 = text.Length;
			if (length2 > 0 && length == length2 && GorillaComputer.instance.CheckAutoBanListForName(text))
			{
				if (text.Length > 12)
				{
					text = text.Substring(0, 11);
				}
				text = text.ToUpper();
			}
			else
			{
				text = "BADGORILLA";
			}
		}
		return text;
	}

	public void SetJumpLimitLocal(float maxJumpSpeed)
	{
		GTPlayer.Instance.maxJumpSpeed = maxJumpSpeed;
	}

	public void SetJumpMultiplierLocal(float jumpMultiplier)
	{
		GTPlayer.Instance.jumpMultiplier = jumpMultiplier;
	}

	public void RequestMaterialColor(int askingPlayerID, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "RequestMaterialColor");
		Player playerRef = ((PunNetPlayer)NetworkSystem.Instance.GetPlayer(info.senderID)).PlayerRef;
		if (this.netView.IsMine)
		{
			this.netView.GetView.RPC("RPC_InitializeNoobMaterial", playerRef, new object[]
			{
				this.myDefaultSkinMaterialInstance.color.r,
				this.myDefaultSkinMaterialInstance.color.g,
				this.myDefaultSkinMaterialInstance.color.b
			});
		}
	}

	public void RequestCosmetics(PhotonMessageInfoWrapped info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		if (this.netView.IsMine && CosmeticsController.hasInstance)
		{
			if (CosmeticsController.instance.isHidingCosmeticsFromRemotePlayers)
			{
				this.netView.SendRPC("RPC_HideAllCosmetics", info.Sender, Array.Empty<object>());
				return;
			}
			int[] array = CosmeticsController.instance.currentWornSet.ToPackedIDArray();
			int[] array2 = CosmeticsController.instance.tryOnSet.ToPackedIDArray();
			this.netView.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", player, new object[] { array, array2, false });
		}
	}

	public void PlayTagSoundLocal(int soundIndex, float soundVolume, bool stopCurrentAudio)
	{
		if (soundIndex < 0 || soundIndex >= this.clipToPlay.Length)
		{
			return;
		}
		this.tagSound.volume = Mathf.Min(0.25f, soundVolume);
		if (stopCurrentAudio)
		{
			this.tagSound.Stop();
		}
		this.tagSound.GTPlayOneShot(this.clipToPlay[soundIndex], 1f);
	}

	public void AssignDrumToMusicDrums(int drumIndex, AudioSource drum)
	{
		if (drumIndex >= 0 && drumIndex < this.musicDrums.Length && drum != null)
		{
			this.musicDrums[drumIndex] = drum;
		}
	}

	public void PlayDrum(int drumIndex, float drumVolume, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "RPC_PlayDrum");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			this.senderRig = rigContainer.Rig;
		}
		if (this.senderRig == null || this.senderRig.muted)
		{
			return;
		}
		if (drumIndex < 0 || drumIndex >= this.musicDrums.Length || (this.senderRig.transform.position - base.transform.position).sqrMagnitude > 9f || !float.IsFinite(drumVolume))
		{
			GorillaNot.instance.SendReport("inappropriate tag data being sent drum", player.UserId, player.NickName);
			return;
		}
		AudioSource audioSource = (this.netView.IsMine ? GorillaTagger.Instance.offlineVRRig.musicDrums[drumIndex] : this.musicDrums[drumIndex]);
		if (!audioSource.gameObject.activeInHierarchy)
		{
			return;
		}
		float instrumentVolume = GorillaComputer.instance.instrumentVolume;
		audioSource.time = 0f;
		audioSource.volume = Mathf.Max(Mathf.Min(instrumentVolume, drumVolume * instrumentVolume), 0f);
		audioSource.GTPlay();
	}

	public int AssignInstrumentToInstrumentSelfOnly(TransferrableObject instrument)
	{
		if (instrument == null)
		{
			return -1;
		}
		if (!this.instrumentSelfOnly.Contains(instrument))
		{
			this.instrumentSelfOnly.Add(instrument);
		}
		return this.instrumentSelfOnly.IndexOf(instrument);
	}

	public void PlaySelfOnlyInstrument(int selfOnlyIndex, int noteIndex, float instrumentVol, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "RPC_PlaySelfOnlyInstrument");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		if (player == this.netView.Owner && !this.muted)
		{
			if (selfOnlyIndex >= 0 && selfOnlyIndex < this.instrumentSelfOnly.Count && float.IsFinite(instrumentVol))
			{
				if (this.instrumentSelfOnly[selfOnlyIndex].gameObject.activeSelf)
				{
					this.instrumentSelfOnly[selfOnlyIndex].PlayNote(noteIndex, Mathf.Max(Mathf.Min(GorillaComputer.instance.instrumentVolume, instrumentVol * GorillaComputer.instance.instrumentVolume), 0f) / 2f);
					return;
				}
			}
			else
			{
				GorillaNot.instance.SendReport("inappropriate tag data being sent self only instrument", player.UserId, player.NickName);
			}
		}
	}

	public void PlayHandTapLocal(int audioClipIndex, bool isLeftHand, float tapVolume)
	{
		if (audioClipIndex > -1 && audioClipIndex < GTPlayer.Instance.materialData.Count)
		{
			GTPlayer.MaterialData materialData = GTPlayer.Instance.materialData[audioClipIndex];
			AudioSource audioSource = (isLeftHand ? this.leftHandPlayer : this.rightHandPlayer);
			audioSource.volume = tapVolume;
			AudioClip audioClip = (materialData.overrideAudio ? materialData.audio : GTPlayer.Instance.materialData[0].audio);
			audioSource.GTPlayOneShot(audioClip, 1f);
		}
	}

	internal HandEffectContext GetHandEffect(bool isLeftHand, StiltID stiltID)
	{
		if (stiltID == StiltID.None)
		{
			if (!isLeftHand)
			{
				return this.RightHandEffect;
			}
			return this.LeftHandEffect;
		}
		else
		{
			if (!isLeftHand)
			{
				return this.ExtraRightHandEffect;
			}
			return this.ExtraLeftHandEffect;
		}
	}

	internal void SetHandEffectData(HandEffectContext effectContext, int audioClipIndex, bool isDownTap, bool isLeftHand, StiltID stiltID, float handTapVolume, float handTapSpeed, Vector3 dirFromHitToHand)
	{
		VRMap vrmap = (isLeftHand ? this.leftHand : this.rightHand);
		Vector3 vector = dirFromHitToHand * this.tapPointDistance * this.scaleFactor;
		if (this.isOfflineVRRig)
		{
			Vector3 vector2 = vrmap.rigTarget.rotation * vrmap.trackingPositionOffset * this.scaleFactor;
			Vector3 vector3 = ((stiltID != StiltID.None) ? GTPlayer.Instance.GetHandPosition(isLeftHand, stiltID) : (vrmap.rigTarget.position - vector2 + vector));
			effectContext.position = vector3;
			effectContext.handSoundSource.transform.position = vector3;
		}
		else
		{
			Quaternion quaternion = vrmap.rigTarget.parent.rotation * vrmap.syncRotation;
			Vector3 vector4 = this.netSyncPos.GetPredictedFuture() - base.transform.position;
			Vector3 vector2 = quaternion * vrmap.trackingPositionOffset * this.scaleFactor;
			effectContext.position = vrmap.rigTarget.parent.TransformPoint(vrmap.netSyncPos.GetPredictedFuture()) - vector2 + vector + vector4;
		}
		GTPlayer.MaterialData handSurfaceData = this.GetHandSurfaceData(audioClipIndex);
		HandTapOverrides handTapOverrides = (isDownTap ? effectContext.DownTapOverrides : effectContext.UpTapOverrides);
		List<int> prefabHashes = effectContext.prefabHashes;
		int num = 0;
		HashWrapper hashWrapper = (handTapOverrides.overrideSurfacePrefab ? handTapOverrides.surfaceTapPrefab : GTPlayer.Instance.materialDatasSO.surfaceEffects[handSurfaceData.surfaceEffectIndex]);
		prefabHashes[num] = in hashWrapper;
		effectContext.prefabHashes[1] = (ref handTapOverrides.overrideGamemodePrefab ? in handTapOverrides.gamemodeTapPrefab : (RoomSystem.JoinedRoom && global::GorillaGameModes.GameMode.ActiveGameMode.IsNotNull()) ? global::GorillaGameModes.GameMode.ActiveGameMode.SpecialHandFX(this.creator, this.rigContainer) : (-1));
		effectContext.soundFX = (handTapOverrides.overrideSound ? handTapOverrides.tapSound : handSurfaceData.audio);
		effectContext.isDownTap = isDownTap;
		effectContext.isLeftHand = isLeftHand;
		effectContext.soundVolume = handTapVolume * this.handSpeedToVolumeModifier;
		effectContext.soundPitch = 1f;
		effectContext.speed = handTapSpeed;
		effectContext.color = this.playerColor;
	}

	internal GTPlayer.MaterialData GetHandSurfaceData(int index)
	{
		List<GTPlayer.MaterialData> materialData = GTPlayer.Instance.materialData;
		GTPlayer.MaterialData materialData2;
		if (index >= 0 && index < materialData.Count)
		{
			materialData2 = materialData[index];
		}
		else
		{
			materialData2 = materialData[0];
		}
		if (!materialData2.overrideAudio)
		{
			materialData2 = materialData[0];
		}
		return materialData2;
	}

	public void PlaySplashEffect(Vector3 splashPosition, Quaternion splashRotation, float splashScale, float boundingRadius, bool bigSplash, bool enteringWater, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "RPC_PlaySplashEffect");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		if (player == this.netView.Owner)
		{
			float num = 10000f;
			if ((in splashPosition).IsValid(in num) && (in splashRotation).IsValid() && float.IsFinite(splashScale) && float.IsFinite(boundingRadius))
			{
				if ((base.transform.position - splashPosition).sqrMagnitude >= 9f)
				{
					return;
				}
				float time = Time.time;
				int num2 = -1;
				float num3 = time + 10f;
				for (int i = 0; i < this.splashEffectTimes.Length; i++)
				{
					if (this.splashEffectTimes[i] < num3)
					{
						num3 = this.splashEffectTimes[i];
						num2 = i;
					}
				}
				if (time - 0.5f > num3)
				{
					this.splashEffectTimes[num2] = time;
					boundingRadius = Mathf.Clamp(boundingRadius, 0.0001f, 0.5f);
					ObjectPools.instance.Instantiate(GTPlayer.Instance.waterParams.rippleEffect, splashPosition, splashRotation, GTPlayer.Instance.waterParams.rippleEffectScale * boundingRadius * 2f, true);
					splashScale = Mathf.Clamp(splashScale, 1E-05f, 1f);
					ObjectPools.instance.Instantiate(GTPlayer.Instance.waterParams.splashEffect, splashPosition, splashRotation, splashScale, true).GetComponent<WaterSplashEffect>().PlayEffect(bigSplash, enteringWater, splashScale, null);
					return;
				}
				return;
			}
		}
		GorillaNot.instance.SendReport("inappropriate tag data being sent splash effect", player.UserId, player.NickName);
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public void RPC_EnableNonCosmeticHandItem(bool enable, bool isLeftHand, RpcInfo info = default(RpcInfo))
	{
		PhotonMessageInfoWrapped photonMessageInfoWrapped = new PhotonMessageInfoWrapped(info);
		this.IncrementRPC(photonMessageInfoWrapped, "EnableNonCosmeticHandItem");
		if (photonMessageInfoWrapped.Sender == this.creator)
		{
			this.senderRig = GorillaGameManager.StaticFindRigForPlayer(photonMessageInfoWrapped.Sender);
			if (this.senderRig == null)
			{
				return;
			}
			if (isLeftHand && this.nonCosmeticLeftHandItem)
			{
				this.senderRig.nonCosmeticLeftHandItem.EnableItem(enable);
				return;
			}
			if (!isLeftHand && this.nonCosmeticRightHandItem)
			{
				this.senderRig.nonCosmeticRightHandItem.EnableItem(enable);
				return;
			}
		}
		else
		{
			GorillaNot.instance.SendReport("inappropriate tag data being sent Enable Non Cosmetic Hand Item", photonMessageInfoWrapped.Sender.UserId, photonMessageInfoWrapped.Sender.NickName);
		}
	}

	[PunRPC]
	public void EnableNonCosmeticHandItemRPC(bool enable, bool isLeftHand, PhotonMessageInfoWrapped info)
	{
		NetPlayer sender = info.Sender;
		this.IncrementRPC(info, "EnableNonCosmeticHandItem");
		if (sender == this.netView.Owner)
		{
			this.senderRig = GorillaGameManager.StaticFindRigForPlayer(sender);
			if (this.senderRig == null)
			{
				return;
			}
			if (isLeftHand && this.nonCosmeticLeftHandItem)
			{
				this.senderRig.nonCosmeticLeftHandItem.EnableItem(enable);
				return;
			}
			if (!isLeftHand && this.nonCosmeticRightHandItem)
			{
				this.senderRig.nonCosmeticRightHandItem.EnableItem(enable);
				return;
			}
		}
		else
		{
			GorillaNot.instance.SendReport("inappropriate tag data being sent Enable Non Cosmetic Hand Item", info.Sender.UserId, info.Sender.NickName);
		}
	}

	public bool IsMakingFistLeft()
	{
		if (this.isOfflineVRRig)
		{
			return ControllerInputPoller.GripFloat(XRNode.LeftHand) > 0.25f && ControllerInputPoller.TriggerFloat(XRNode.LeftHand) > 0.25f;
		}
		return this.leftIndex.calcT > 0.25f && this.leftMiddle.calcT > 0.25f;
	}

	public bool IsMakingFistRight()
	{
		if (this.isOfflineVRRig)
		{
			return ControllerInputPoller.GripFloat(XRNode.RightHand) > 0.25f && ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.25f;
		}
		return this.rightIndex.calcT > 0.25f && this.rightMiddle.calcT > 0.25f;
	}

	public bool IsMakingFiveLeft()
	{
		if (this.isOfflineVRRig)
		{
			return ControllerInputPoller.GripFloat(XRNode.LeftHand) < 0.25f && ControllerInputPoller.TriggerFloat(XRNode.LeftHand) < 0.25f;
		}
		return this.leftIndex.calcT < 0.25f && this.leftMiddle.calcT < 0.25f;
	}

	public bool IsMakingFiveRight()
	{
		if (this.isOfflineVRRig)
		{
			return ControllerInputPoller.GripFloat(XRNode.RightHand) < 0.25f && ControllerInputPoller.TriggerFloat(XRNode.RightHand) < 0.25f;
		}
		return this.rightIndex.calcT < 0.25f && this.rightMiddle.calcT < 0.25f;
	}

	public VRMap GetMakingFist(bool debug, out bool isLeftHand)
	{
		if (this.IsMakingFistRight())
		{
			isLeftHand = false;
			return this.rightHand;
		}
		if (this.IsMakingFistLeft())
		{
			isLeftHand = true;
			return this.leftHand;
		}
		isLeftHand = false;
		return null;
	}

	public void PlayGeodeEffect(Vector3 hitPosition)
	{
		if ((base.transform.position - hitPosition).sqrMagnitude < 9f && this.geodeCrackingSound)
		{
			this.geodeCrackingSound.GTPlay();
		}
	}

	public void PlayClimbSound(AudioClip clip, bool isLeftHand)
	{
		if (isLeftHand)
		{
			this.leftHandPlayer.volume = 0.1f;
			this.leftHandPlayer.clip = clip;
			this.leftHandPlayer.GTPlayOneShot(this.leftHandPlayer.clip, 1f);
			return;
		}
		this.rightHandPlayer.volume = 0.1f;
		this.rightHandPlayer.clip = clip;
		this.rightHandPlayer.GTPlayOneShot(this.rightHandPlayer.clip, 1f);
	}

	public void HideAllCosmetics(PhotonMessageInfo info)
	{
		this.IncrementRPC(info, "HideAllCosmetics");
		if (NetworkSystem.Instance.GetPlayer(info.Sender) == this.netView.Owner)
		{
			this.LocalUpdateCosmeticsWithTryon(CosmeticsController.CosmeticSet.EmptySet, CosmeticsController.CosmeticSet.EmptySet, false);
			return;
		}
		GorillaNot.instance.SendReport("inappropriate tag data being sent update cosmetics", info.Sender.UserId, info.Sender.NickName);
	}

	public void UpdateCosmeticsWithTryon(string[] currentItems, string[] tryOnItems, bool playfx, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "RPC_UpdateCosmeticsWithTryon");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		if (info.Sender == this.netView.Owner && currentItems.Length == 16 && tryOnItems.Length == 16)
		{
			CosmeticsController.CosmeticSet cosmeticSet = new CosmeticsController.CosmeticSet(currentItems, CosmeticsController.instance);
			CosmeticsController.CosmeticSet cosmeticSet2 = new CosmeticsController.CosmeticSet(tryOnItems, CosmeticsController.instance);
			this.LocalUpdateCosmeticsWithTryon(cosmeticSet, cosmeticSet2, playfx);
			return;
		}
		GorillaNot.instance.SendReport("inappropriate tag data being sent update cosmetics with tryon", player.UserId, player.NickName);
	}

	public void UpdateCosmeticsWithTryon(int[] currentItemsPacked, int[] tryOnItemsPacked, bool playfx, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "RPC_UpdateCosmeticsWithTryon");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		if (info.Sender == this.netView.Owner && CosmeticsController.instance.ValidatePackedItems(currentItemsPacked) && CosmeticsController.instance.ValidatePackedItems(tryOnItemsPacked))
		{
			CosmeticsController.CosmeticSet cosmeticSet = new CosmeticsController.CosmeticSet(currentItemsPacked, CosmeticsController.instance);
			CosmeticsController.CosmeticSet cosmeticSet2 = new CosmeticsController.CosmeticSet(tryOnItemsPacked, CosmeticsController.instance);
			this.LocalUpdateCosmeticsWithTryon(cosmeticSet, cosmeticSet2, playfx);
			return;
		}
		GorillaNot.instance.SendReport("inappropriate tag data being sent update cosmetics with tryon", player.UserId, player.NickName);
	}

	public void LocalUpdateCosmeticsWithTryon(CosmeticsController.CosmeticSet newSet, CosmeticsController.CosmeticSet newTryOnSet, bool playfx)
	{
		this.cosmeticSet = newSet;
		this.tryOnSet = newTryOnSet;
		if (this.initializedCosmetics)
		{
			this.SetCosmeticsActive(playfx);
		}
	}

	private void CheckForEarlyAccess()
	{
		if (this.concatStringOfCosmeticsAllowed.Contains("Early Access Supporter Pack"))
		{
			this.concatStringOfCosmeticsAllowed += "LBAAE.LFAAM.LFAAN.LHAAA.LHAAK.LHAAL.LHAAM.LHAAN.LHAAO.LHAAP.LHABA.LHABB.";
		}
		this.InitializedCosmetics = true;
	}

	public void SetCosmeticsActive(bool playfx)
	{
		if (CosmeticsController.instance == null || !CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			return;
		}
		this.prevSet.CopyItems(this.mergedSet);
		this.mergedSet.MergeSets(this.inTryOnRoom ? this.tryOnSet : null, this.cosmeticSet);
		BodyDockPositions component = base.GetComponent<BodyDockPositions>();
		this.mergedSet.ActivateCosmetics(this.prevSet, this, component, this.cosmeticsObjectRegistry);
		if (!playfx)
		{
			return;
		}
		if (this.cosmeticsActivationPS != null)
		{
			this.cosmeticsActivationPS.Play();
		}
		if (this.cosmeticsActivationSBP != null)
		{
			this.cosmeticsActivationSBP.Play();
		}
	}

	public void RefreshCosmetics()
	{
		this.mergedSet.ActivateCosmetics(this.mergedSet, this, this.myBodyDockPositions, this.cosmeticsObjectRegistry);
		this.myBodyDockPositions.RefreshTransferrableItems();
	}

	public void GetCosmeticsPlayFabCatalogData()
	{
		if (CosmeticsController.instance != null)
		{
			PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), delegate(GetUserInventoryResult result)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				foreach (ItemInstance itemInstance in result.Inventory)
				{
					if (!dictionary.ContainsKey(itemInstance.ItemId))
					{
						dictionary[itemInstance.ItemId] = itemInstance.ItemId;
						if (itemInstance.CatalogVersion == CosmeticsController.instance.catalog)
						{
							this.concatStringOfCosmeticsAllowed += itemInstance.ItemId;
						}
					}
				}
				if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
				{
					this.Handle_CosmeticsV2_OnPostInstantiateAllPrefabs_DoEnableAllCosmetics();
				}
			}, delegate(PlayFabError error)
			{
				this.initializedCosmetics = true;
				if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
				{
					this.SetCosmeticsActive(false);
				}
			}, null, null);
		}
		this.concatStringOfCosmeticsAllowed += "Slingshot";
		this.concatStringOfCosmeticsAllowed += BuilderSetManager.instance.GetStarterSetsConcat();
	}

	public void GenerateFingerAngleLookupTables()
	{
		this.GenerateTableIndex(ref this.leftIndex);
		this.GenerateTableIndex(ref this.rightIndex);
		this.GenerateTableMiddle(ref this.leftMiddle);
		this.GenerateTableMiddle(ref this.rightMiddle);
		this.GenerateTableThumb(ref this.leftThumb);
		this.GenerateTableThumb(ref this.rightThumb);
	}

	private void GenerateTableThumb(ref VRMapThumb thumb)
	{
		thumb.angle1Table = new Quaternion[11];
		thumb.angle2Table = new Quaternion[11];
		for (int i = 0; i < thumb.angle1Table.Length; i++)
		{
			thumb.angle1Table[i] = Quaternion.Lerp(thumb.startingAngle1Quat, thumb.closedAngle1Quat, (float)i / 10f);
			thumb.angle2Table[i] = Quaternion.Lerp(thumb.startingAngle2Quat, thumb.closedAngle2Quat, (float)i / 10f);
		}
	}

	private void GenerateTableIndex(ref VRMapIndex index)
	{
		index.angle1Table = new Quaternion[11];
		index.angle2Table = new Quaternion[11];
		index.angle3Table = new Quaternion[11];
		for (int i = 0; i < index.angle1Table.Length; i++)
		{
			index.angle1Table[i] = Quaternion.Lerp(index.startingAngle1Quat, index.closedAngle1Quat, (float)i / 10f);
			index.angle2Table[i] = Quaternion.Lerp(index.startingAngle2Quat, index.closedAngle2Quat, (float)i / 10f);
			index.angle3Table[i] = Quaternion.Lerp(index.startingAngle3Quat, index.closedAngle3Quat, (float)i / 10f);
		}
	}

	private void GenerateTableMiddle(ref VRMapMiddle middle)
	{
		middle.angle1Table = new Quaternion[11];
		middle.angle2Table = new Quaternion[11];
		middle.angle3Table = new Quaternion[11];
		for (int i = 0; i < middle.angle1Table.Length; i++)
		{
			middle.angle1Table[i] = Quaternion.Lerp(middle.startingAngle1Quat, middle.closedAngle1Quat, (float)i / 10f);
			middle.angle2Table[i] = Quaternion.Lerp(middle.startingAngle2Quat, middle.closedAngle2Quat, (float)i / 10f);
			middle.angle3Table[i] = Quaternion.Lerp(middle.startingAngle3Quat, middle.closedAngle3Quat, (float)i / 10f);
		}
	}

	private Quaternion SanitizeQuaternion(Quaternion quat)
	{
		if (float.IsNaN(quat.w) || float.IsNaN(quat.x) || float.IsNaN(quat.y) || float.IsNaN(quat.z) || float.IsInfinity(quat.w) || float.IsInfinity(quat.x) || float.IsInfinity(quat.y) || float.IsInfinity(quat.z))
		{
			return Quaternion.identity;
		}
		return quat;
	}

	private Vector3 SanitizeVector3(Vector3 vec)
	{
		if (float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z) || float.IsInfinity(vec.x) || float.IsInfinity(vec.y) || float.IsInfinity(vec.z))
		{
			return Vector3.zero;
		}
		return Vector3.ClampMagnitude(vec, 5000f);
	}

	private void IncrementRPC(PhotonMessageInfoWrapped info, string sourceCall)
	{
		if (GorillaGameManager.instance != null)
		{
			GorillaNot.IncrementRPCCall(info, sourceCall);
		}
	}

	private void IncrementRPC(PhotonMessageInfo info, string sourceCall)
	{
		if (GorillaGameManager.instance != null)
		{
			GorillaNot.IncrementRPCCall(info, sourceCall);
		}
	}

	private void AddVelocityToQueue(Vector3 position, double serverTime)
	{
		Vector3 vector = Vector3.zero;
		if (this.velocityHistoryList.Count > 0)
		{
			double num = Utils.CalculateNetworkDeltaTime(this.velocityHistoryList[0].time, serverTime);
			if (num == 0.0)
			{
				return;
			}
			vector = (position - this.lastPosition) / (float)num;
		}
		this.velocityHistoryList.Add(new VRRig.VelocityTime(vector, serverTime));
		this.lastPosition = position;
	}

	private Vector3 ReturnVelocityAtTime(double timeToReturn)
	{
		if (this.velocityHistoryList.Count <= 1)
		{
			return Vector3.zero;
		}
		int num = 0;
		int num2 = this.velocityHistoryList.Count - 1;
		int num3 = 0;
		if (num2 == num)
		{
			return this.velocityHistoryList[num].vel;
		}
		while (num2 - num > 1 && num3 < 1000)
		{
			num3++;
			int num4 = (num2 - num) / 2;
			if (this.velocityHistoryList[num4].time > timeToReturn)
			{
				num2 = num4;
			}
			else
			{
				num = num4;
			}
		}
		float num5 = (float)(this.velocityHistoryList[num].time - timeToReturn);
		double num6 = this.velocityHistoryList[num].time - this.velocityHistoryList[num2].time;
		if (num6 == 0.0)
		{
			num6 = 0.001;
		}
		num5 /= (float)num6;
		num5 = Mathf.Clamp(num5, 0f, 1f);
		return Vector3.Lerp(this.velocityHistoryList[num].vel, this.velocityHistoryList[num2].vel, num5);
	}

	public Vector3 LatestVelocity()
	{
		if (this.velocityHistoryList.Count > 0)
		{
			return this.velocityHistoryList[0].vel;
		}
		return Vector3.zero;
	}

	public bool IsPositionInRange(Vector3 position, float range)
	{
		return (this.syncPos - position).IsShorterThan(range * this.scaleFactor);
	}

	public bool CheckTagDistanceRollback(VRRig otherRig, float max, float timeInterval)
	{
		Vector3 vector;
		Vector3 vector2;
		GorillaMath.LineSegClosestPoints(this.syncPos, -this.LatestVelocity() * timeInterval, otherRig.syncPos, -otherRig.LatestVelocity() * timeInterval, out vector, out vector2);
		return Vector3.SqrMagnitude(vector - vector2) < max * max * this.scaleFactor;
	}

	public Vector3 ClampVelocityRelativeToPlayerSafe(Vector3 inVel, float max, float teleportSpeedThreshold = 100f)
	{
		max *= this.scaleFactor;
		Vector3 vector = Vector3.zero;
		(ref vector).SetValueSafe(in inVel);
		Vector3 vector2 = ((this.velocityHistoryList.Count > 0) ? this.velocityHistoryList[0].vel : Vector3.zero);
		if (vector2.sqrMagnitude > teleportSpeedThreshold * teleportSpeedThreshold)
		{
			vector2 = Vector3.zero;
		}
		Vector3 vector3 = vector - vector2;
		vector3 = Vector3.ClampMagnitude(vector3, max);
		vector = vector2 + vector3;
		return vector;
	}

	public event Action<Color> OnColorChanged;

	public event Action OnPlayerNameVisibleChanged;

	public void SetColor(Color color)
	{
		Action<Color> onColorChanged = this.OnColorChanged;
		if (onColorChanged != null)
		{
			onColorChanged(color);
		}
		Action<Color> action = this.onColorInitialized;
		if (action != null)
		{
			action(color);
		}
		this.onColorInitialized = delegate(Color color1)
		{
		};
		this.colorInitialized = true;
		this.playerColor = color;
		if (this.OnDataChange != null)
		{
			this.OnDataChange();
		}
	}

	public void OnColorInitialized(Action<Color> action)
	{
		if (this.colorInitialized)
		{
			action(this.playerColor);
			return;
		}
		this.onColorInitialized = (Action<Color>)Delegate.Combine(this.onColorInitialized, action);
	}

	private void SendScoresToRoom()
	{
		if (this.netView != null && this._scoreUpdated)
		{
			this.netView.SendRPC("RPC_UpdateQuestScore", RpcTarget.Others, new object[] { this.currentQuestScore });
		}
	}

	private void SendScoresToGameModeRoom(GameModeType newGameModeType)
	{
		if (this.netView != null && this._rankedInfoUpdated && newGameModeType != GameModeType.InfectionCompetitive && !this.m_sentRankedScore)
		{
			this.m_sentRankedScore = true;
			this.netView.SendRPC("RPC_UpdateRankedInfo", RpcTarget.Others, new object[] { this.currentRankedELO, this.currentRankedSubTierQuest, this.currentRankedSubTierPC });
		}
	}

	private void SendScoresToNewPlayer(NetPlayer player)
	{
		if (this.netView != null)
		{
			if (this._scoreUpdated)
			{
				this.netView.SendRPC("RPC_UpdateQuestScore", player, new object[] { this.currentQuestScore });
			}
			if (this._rankedInfoUpdated && !this.IsInRankedMode())
			{
				this.netView.SendRPC("RPC_UpdateRankedInfo", player, new object[] { this.currentRankedELO, this.currentRankedSubTierQuest, this.currentRankedSubTierPC });
			}
		}
	}

	public event Action<int> OnQuestScoreChanged;

	public void SetQuestScore(int score)
	{
		this.SetQuestScoreLocal(score);
		Action<int> onQuestScoreChanged = this.OnQuestScoreChanged;
		if (onQuestScoreChanged != null)
		{
			onQuestScoreChanged(this.currentQuestScore);
		}
		if (this.netView != null)
		{
			this.netView.SendRPC("RPC_UpdateQuestScore", RpcTarget.Others, new object[] { this.currentQuestScore });
		}
	}

	public int GetCurrentQuestScore()
	{
		if (!this._scoreUpdated)
		{
			this.SetQuestScoreLocal(ProgressionController.TotalPoints);
		}
		return this.currentQuestScore;
	}

	private void SetQuestScoreLocal(int score)
	{
		this.currentQuestScore = score;
		this._scoreUpdated = true;
	}

	public void UpdateQuestScore(int score, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "UpdateQuestScore");
		NetworkSystem.Instance.GetPlayer(info.senderID);
		if (info.senderID != this.creator.ActorNumber)
		{
			return;
		}
		if (!this.updateQuestCallLimit.CheckCallTime(Time.time))
		{
			return;
		}
		if (score < this.currentQuestScore)
		{
			return;
		}
		this.SetQuestScoreLocal(score);
		Action<int> onQuestScoreChanged = this.OnQuestScoreChanged;
		if (onQuestScoreChanged == null)
		{
			return;
		}
		onQuestScoreChanged(this.currentQuestScore);
	}

	public event Action<int, int> OnRankedSubtierChanged;

	public void SetRankedInfo(float rankedELO, int rankedSubtierQuest, int rankedSubtierPC, bool broadcastToOtherClients = true)
	{
		this.SetRankedInfoLocal(rankedELO, rankedSubtierQuest, rankedSubtierPC);
		Action<int, int> onRankedSubtierChanged = this.OnRankedSubtierChanged;
		if (onRankedSubtierChanged != null)
		{
			onRankedSubtierChanged(rankedSubtierQuest, rankedSubtierPC);
		}
		if (this.netView != null && broadcastToOtherClients)
		{
			this.netView.SendRPC("RPC_UpdateRankedInfo", RpcTarget.Others, new object[] { this.currentRankedELO, this.currentRankedSubTierQuest, this.currentRankedSubTierPC });
		}
	}

	public int GetCurrentRankedSubTier(bool getPC)
	{
		if (!this._rankedInfoUpdated)
		{
			return -1;
		}
		if (!getPC)
		{
			return this.currentRankedSubTierQuest;
		}
		return this.currentRankedSubTierPC;
	}

	private void SetRankedInfoLocal(float rankedELO, int rankedSubTierQuest, int rankedSubTierPC)
	{
		this.currentRankedELO = rankedELO;
		this.currentRankedSubTierQuest = rankedSubTierQuest;
		this.currentRankedSubTierPC = rankedSubTierPC;
		this._rankedInfoUpdated = true;
	}

	private bool IsInRankedMode()
	{
		return global::GorillaGameModes.GameMode.ActiveGameMode != null && global::GorillaGameModes.GameMode.ActiveGameMode.GameType() == GameModeType.InfectionCompetitive;
	}

	public void UpdateRankedInfo(float rankedELO, int rankedSubtierQuest, int rankedSubtierPC, PhotonMessageInfoWrapped info)
	{
		this.IncrementRPC(info, "UpdateRankedInfo");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			return;
		}
		if (!rigContainer.Rig.updateRankedInfoCallLimit.CheckCallTime(Time.time) || info.senderID != this.creator.ActorNumber || !float.IsFinite(rankedELO))
		{
			return;
		}
		if (this.IsInRankedMode())
		{
			return;
		}
		if (RankedProgressionManager.Instance == null || !RankedProgressionManager.Instance.AreValuesValid(rankedELO, rankedSubtierQuest, rankedSubtierPC))
		{
			return;
		}
		this.SetRankedInfoLocal(rankedELO, rankedSubtierQuest, rankedSubtierPC);
		Action<int, int> onRankedSubtierChanged = this.OnRankedSubtierChanged;
		if (onRankedSubtierChanged != null)
		{
			onRankedSubtierChanged(rankedSubtierQuest, rankedSubtierPC);
		}
		RankedProgressionManager.Instance.HandlePlayerRankedInfoReceived(this.creator.ActorNumber, rankedELO, rankedSubtierPC);
	}

	public void OnEnable()
	{
		EyeScannerMono.Register(this);
		GorillaComputer.RegisterOnNametagSettingChanged(new Action<bool>(this.UpdateName));
		if (this.currentRopeSwingTarget != null)
		{
			this.currentRopeSwingTarget.SetParent(null);
		}
		if (!this.isOfflineVRRig)
		{
			PlayerCosmeticsSystem.RegisterCosmeticCallback(this.creator.ActorNumber, this);
		}
		this.bodyRenderer.SetDefaults();
		this.SetInvisibleToLocalPlayer(false);
		if (this.isOfflineVRRig)
		{
			HandHold.HandPositionRequestOverride += this.HandHold_HandPositionRequestOverride;
			HandHold.HandPositionReleaseOverride += this.HandHold_HandPositionReleaseOverride;
			global::GorillaGameModes.GameMode.OnStartGameMode += this.SendScoresToGameModeRoom;
			RoomSystem.JoinedRoomEvent += new Action(this.SendScoresToRoom);
			RoomSystem.PlayerJoinedEvent += new Action<NetPlayer>(this.SendScoresToNewPlayer);
		}
		else
		{
			VRRigJobManager.Instance.RegisterVRRig(this);
		}
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		TickSystem<object>.AddPostTickCallback(this);
	}

	void IPreDisable.PreDisable()
	{
		try
		{
			this.ClearRopeData();
			if (this.currentRopeSwingTarget)
			{
				this.currentRopeSwingTarget.SetParent(base.transform);
			}
			this.EnableHuntWatch(false);
			this.EnablePaintbrawlCosmetics(false);
			this.EnableSuperInfectionHands(false);
			this.ClearPartyMemberStatus();
			this.concatStringOfCosmeticsAllowed = "";
			this.rawCosmeticString = "";
			if (this.cosmeticSet != null)
			{
				this.mergedSet.DeactivateAllCosmetcs(this.myBodyDockPositions, CosmeticsController.instance.nullItem, this.cosmeticsObjectRegistry);
				this.mergedSet.ClearSet(CosmeticsController.instance.nullItem);
				this.prevSet.ClearSet(CosmeticsController.instance.nullItem);
				this.tryOnSet.ClearSet(CosmeticsController.instance.nullItem);
				this.cosmeticSet.ClearSet(CosmeticsController.instance.nullItem);
			}
			if (!this.isOfflineVRRig)
			{
				PlayerCosmeticsSystem.RemoveCosmeticCallback(this.creator.ActorNumber);
				this.pendingCosmeticUpdate = true;
				VRRig.LocalRig.leftHandLink.BreakLinkTo(this.leftHandLink);
				VRRig.LocalRig.leftHandLink.BreakLinkTo(this.rightHandLink);
				VRRig.LocalRig.rightHandLink.BreakLinkTo(this.leftHandLink);
				VRRig.LocalRig.rightHandLink.BreakLinkTo(this.rightHandLink);
			}
		}
		catch (Exception)
		{
		}
	}

	public void OnDisable()
	{
		try
		{
			GorillaSkin.ApplyToRig(this, null, GorillaSkin.SkinType.gameMode);
			this.ChangeMaterialLocal(0);
			GorillaComputer.UnregisterOnNametagSettingChanged(new Action<bool>(this.UpdateName));
			this.netView = null;
			this.voiceAudio = null;
			this.muted = false;
			this.initialized = false;
			this.initializedCosmetics = false;
			this.inTryOnRoom = false;
			this.timeSpawned = 0f;
			this.setMatIndex = 0;
			this.currentCosmeticTries = 0;
			this.velocityHistoryList.Clear();
			this.netSyncPos.Reset();
			this.rightHand.netSyncPos.Reset();
			this.leftHand.netSyncPos.Reset();
			this.ForceResetFrozenEffect();
			this.nativeScale = (this.frameScale = (this.lastScaleFactor = 1f));
			base.transform.localScale = Vector3.one;
			this.currentQuestScore = 0;
			this._scoreUpdated = false;
			this.currentRankedELO = 0f;
			this.currentRankedSubTierQuest = 0;
			this.currentRankedSubTierPC = 0;
			this._rankedInfoUpdated = false;
			this.TemporaryCosmeticEffects.Clear();
			this.m_sentRankedScore = false;
			try
			{
				CallLimitType<CallLimiter>[] callSettings = this.fxSettings.callSettings;
				for (int i = 0; i < callSettings.Length; i++)
				{
					callSettings[i].CallLimitSettings.Reset();
				}
			}
			catch
			{
				Debug.LogError("fxtype missing in fxSettings, please fix or remove this");
			}
		}
		catch (Exception)
		{
		}
		if (this.isOfflineVRRig)
		{
			HandHold.HandPositionRequestOverride -= this.HandHold_HandPositionRequestOverride;
			HandHold.HandPositionReleaseOverride -= this.HandHold_HandPositionReleaseOverride;
			global::GorillaGameModes.GameMode.OnStartGameMode -= this.SendScoresToGameModeRoom;
			RoomSystem.JoinedRoomEvent -= new Action(this.SendScoresToRoom);
			RoomSystem.PlayerJoinedEvent -= new Action<NetPlayer>(this.SendScoresToNewPlayer);
		}
		else
		{
			VRRigJobManager.Instance.DeregisterVRRig(this);
		}
		EyeScannerMono.Unregister(this);
		this.creator = null;
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		TickSystem<object>.RemovePostTickCallback(this);
	}

	private void HandHold_HandPositionReleaseOverride(HandHold hh, bool leftHand)
	{
		if (leftHand)
		{
			this.leftHand.handholdOverrideTarget = null;
			return;
		}
		this.rightHand.handholdOverrideTarget = null;
	}

	private void HandHold_HandPositionRequestOverride(HandHold hh, bool leftHand, Vector3 pos)
	{
		if (leftHand)
		{
			this.leftHand.handholdOverrideTarget = hh.transform;
			this.leftHand.handholdOverrideTargetOffset = pos;
			return;
		}
		this.rightHand.handholdOverrideTarget = hh.transform;
		this.rightHand.handholdOverrideTargetOffset = pos;
	}

	public void NetInitialize()
	{
		this.timeSpawned = Time.time;
		if (NetworkSystem.Instance.InRoom)
		{
			GorillaGameManager instance = GorillaGameManager.instance;
			if (instance != null)
			{
				if (instance is GorillaHuntManager || instance.GameModeName() == "HUNT")
				{
					this.EnableHuntWatch(true);
				}
				else if (instance is GorillaPaintbrawlManager || instance.GameModeName() == "PAINTBRAWL")
				{
					this.EnablePaintbrawlCosmetics(true);
				}
			}
			else
			{
				string gameModeString = NetworkSystem.Instance.GameModeString;
				if (!gameModeString.IsNullOrEmpty())
				{
					string text = gameModeString;
					if (text.Contains("HUNT"))
					{
						this.EnableHuntWatch(true);
					}
					else if (text.Contains("PAINTBRAWL"))
					{
						this.EnablePaintbrawlCosmetics(true);
					}
				}
			}
			this.UpdateFriendshipBracelet();
			if (this.IsLocalPartyMember && !this.isOfflineVRRig)
			{
				FriendshipGroupDetection.Instance.SendVerifyPartyMember(this.creator);
			}
		}
		if (this.netView != null)
		{
			base.transform.position = this.netView.gameObject.transform.position;
			base.transform.rotation = this.netView.gameObject.transform.rotation;
		}
		try
		{
			Action action = VRRig.newPlayerJoined;
			if (action != null)
			{
				action();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
		}
	}

	public void GrabbedByPlayer(VRRig grabbedByRig, bool grabbedBody, bool grabbedLeftHand, bool grabbedWithLeftHand)
	{
		GorillaClimbable gorillaClimbable = (grabbedWithLeftHand ? grabbedByRig.leftHandHoldsPlayer : grabbedByRig.rightHandHoldsPlayer);
		GorillaHandClimber gorillaHandClimber;
		if (grabbedBody)
		{
			gorillaHandClimber = EquipmentInteractor.instance.BodyClimber;
		}
		else if (grabbedLeftHand)
		{
			gorillaHandClimber = EquipmentInteractor.instance.LeftClimber;
		}
		else
		{
			gorillaHandClimber = EquipmentInteractor.instance.RightClimber;
		}
		gorillaHandClimber.SetCanRelease(false);
		GTPlayer.Instance.BeginClimbing(gorillaClimbable, gorillaHandClimber, null);
		this.grabbedRopeIsBody = grabbedBody;
		this.grabbedRopeIsLeft = grabbedLeftHand;
		this.grabbedRopeIndex = grabbedByRig.netView.ViewID;
		this.grabbedRopeBoneIndex = (grabbedWithLeftHand ? 1 : 0);
		this.grabbedRopeOffset = Vector3.zero;
		this.grabbedRopeIsPhotonView = true;
	}

	public void DroppedByPlayer(VRRig grabbedByRig, Vector3 throwVelocity)
	{
		GorillaClimbable currentClimbable = GTPlayer.Instance.CurrentClimbable;
		if (GTPlayer.Instance.isClimbing && (currentClimbable == grabbedByRig.leftHandHoldsPlayer || currentClimbable == grabbedByRig.rightHandHoldsPlayer))
		{
			throwVelocity = Vector3.ClampMagnitude(throwVelocity, 20f);
			GorillaHandClimber currentClimber = GTPlayer.Instance.CurrentClimber;
			GTPlayer.Instance.EndClimbing(currentClimber, false, false);
			GTPlayer.Instance.SetVelocity(throwVelocity);
			this.grabbedRopeIsBody = false;
			this.grabbedRopeIsLeft = false;
			this.grabbedRopeIndex = -1;
			this.grabbedRopeBoneIndex = 0;
			this.grabbedRopeOffset = Vector3.zero;
			this.grabbedRopeIsPhotonView = false;
			return;
		}
		if (VRRig.LocalRig.leftHandLink.IsLinkActive() && VRRig.LocalRig.leftHandLink.grabbedLink.myRig == grabbedByRig)
		{
			throwVelocity = Vector3.ClampMagnitude(throwVelocity, 3f);
			VRRig.LocalRig.leftHandLink.BreakLink();
			VRRig.LocalRig.leftHandLink.RejectGrabsFor(1f);
			GTPlayer.Instance.SetVelocity(throwVelocity);
			return;
		}
		if (VRRig.LocalRig.rightHandLink.IsLinkActive() && VRRig.LocalRig.rightHandLink.grabbedLink.myRig == grabbedByRig)
		{
			throwVelocity = Vector3.ClampMagnitude(throwVelocity, 3f);
			VRRig.LocalRig.rightHandLink.BreakLink();
			VRRig.LocalRig.rightHandLink.RejectGrabsFor(1f);
			GTPlayer.Instance.SetVelocity(throwVelocity);
		}
	}

	public bool IsOnGround(float headCheckDistance, float handCheckDistance, out Vector3 groundNormal)
	{
		GTPlayer instance = GTPlayer.Instance;
		Vector3 position = base.transform.position;
		Vector3 vector;
		RaycastHit raycastHit;
		if (this.LocalCheckCollision(position, Vector3.down * headCheckDistance * this.scaleFactor, instance.headCollider.radius * this.scaleFactor, out vector, out raycastHit))
		{
			groundNormal = raycastHit.normal;
			return true;
		}
		Vector3 position2 = this.leftHand.rigTarget.position;
		if (this.LocalCheckCollision(position2, Vector3.down * handCheckDistance * this.scaleFactor, instance.minimumRaycastDistance * this.scaleFactor, out vector, out raycastHit))
		{
			groundNormal = raycastHit.normal;
			return true;
		}
		Vector3 position3 = this.rightHand.rigTarget.position;
		if (this.LocalCheckCollision(position3, Vector3.down * handCheckDistance * this.scaleFactor, instance.minimumRaycastDistance * this.scaleFactor, out vector, out raycastHit))
		{
			groundNormal = raycastHit.normal;
			return true;
		}
		groundNormal = Vector3.up;
		return false;
	}

	private bool LocalTestMovementCollision(Vector3 startPosition, Vector3 startVelocity, out Vector3 modifiedVelocity, out Vector3 finalPosition)
	{
		GTPlayer instance = GTPlayer.Instance;
		Vector3 vector = startVelocity * Time.deltaTime;
		finalPosition = startPosition + vector;
		modifiedVelocity = startVelocity;
		Vector3 vector2;
		RaycastHit raycastHit;
		bool flag = this.LocalCheckCollision(startPosition, vector, instance.headCollider.radius * this.scaleFactor, out vector2, out raycastHit);
		if (flag)
		{
			finalPosition = vector2 - vector.normalized * 0.01f;
			modifiedVelocity = startVelocity - raycastHit.normal * Vector3.Dot(raycastHit.normal, startVelocity);
		}
		Vector3 position = this.leftHand.rigTarget.position;
		Vector3 vector3;
		RaycastHit raycastHit2;
		bool flag2 = this.LocalCheckCollision(position, vector, instance.minimumRaycastDistance * this.scaleFactor, out vector3, out raycastHit2);
		if (flag2)
		{
			finalPosition = vector3 - (this.leftHand.rigTarget.position - startPosition) - vector.normalized * 0.01f;
			modifiedVelocity = Vector3.zero;
		}
		Vector3 position2 = this.rightHand.rigTarget.position;
		Vector3 vector4;
		RaycastHit raycastHit3;
		bool flag3 = this.LocalCheckCollision(position2, vector, instance.minimumRaycastDistance * this.scaleFactor, out vector4, out raycastHit3);
		if (flag3)
		{
			finalPosition = vector4 - (this.rightHand.rigTarget.position - startPosition) - vector.normalized * 0.01f;
			modifiedVelocity = Vector3.zero;
		}
		return flag || flag2 || flag3;
	}

	public void TrySweptMoveTo(Vector3 targetPosition, out bool handCollided, out bool buttCollided)
	{
		Vector3 position = base.transform.position;
		this.TrySweptOffsetMove(targetPosition - position, out handCollided, out buttCollided);
	}

	public void TrySweptOffsetMove(Vector3 movement, out bool handCollided, out bool buttCollided)
	{
		GTPlayer instance = GTPlayer.Instance;
		Vector3 position = base.transform.position;
		Vector3 vector = position + movement;
		Vector3 vector2 = position;
		handCollided = false;
		buttCollided = false;
		Vector3 vector3;
		RaycastHit raycastHit;
		if (this.LocalCheckCollision(vector2, movement, instance.headCollider.radius * this.scaleFactor, out vector3, out raycastHit))
		{
			if (movement.IsShorterThan(0.01f))
			{
				vector = position;
			}
			else
			{
				vector = vector3 - movement.normalized * 0.01f;
			}
			movement = vector - position;
			buttCollided = true;
		}
		Vector3 position2 = this.leftHand.rigTarget.position;
		Vector3 vector4;
		RaycastHit raycastHit2;
		if (this.LocalCheckCollision(position2, movement, instance.minimumRaycastDistance * this.scaleFactor, out vector4, out raycastHit2))
		{
			if (movement.IsShorterThan(0.01f))
			{
				vector = position;
			}
			else
			{
				vector = vector4 - (this.leftHand.rigTarget.position - position) - movement.normalized * 0.01f;
			}
			movement = vector - position;
			handCollided = true;
		}
		Vector3 position3 = this.rightHand.rigTarget.position;
		Vector3 vector5;
		RaycastHit raycastHit3;
		if (this.LocalCheckCollision(position3, movement, instance.minimumRaycastDistance * this.scaleFactor, out vector5, out raycastHit3))
		{
			if (movement.IsShorterThan(0.01f))
			{
				vector = position;
			}
			else
			{
				vector = vector5 - (this.rightHand.rigTarget.position - position) - movement.normalized * 0.01f;
			}
			movement = vector - position;
			handCollided = true;
		}
		base.transform.position = vector;
	}

	private bool LocalCheckCollision(Vector3 startPosition, Vector3 movement, float radius, out Vector3 finalPosition, out RaycastHit hit)
	{
		GTPlayer instance = GTPlayer.Instance;
		finalPosition = startPosition + movement;
		RaycastHit raycastHit = default(RaycastHit);
		bool flag = false;
		Vector3 normalized = movement.normalized;
		int num = Physics.SphereCastNonAlloc(startPosition, radius, normalized, this.rayCastNonAllocColliders, movement.magnitude, instance.locomotionEnabledLayers.value);
		if (num > 0)
		{
			raycastHit = this.rayCastNonAllocColliders[0];
			for (int i = 0; i < num; i++)
			{
				if (raycastHit.distance > 0f && (!flag || this.rayCastNonAllocColliders[i].distance < raycastHit.distance))
				{
					flag = true;
					raycastHit = this.rayCastNonAllocColliders[i];
				}
			}
		}
		hit = raycastHit;
		if (flag)
		{
			finalPosition = startPosition + normalized * (raycastHit.distance - 0.01f);
			return true;
		}
		return false;
	}

	public void UpdateFriendshipBracelet()
	{
		bool flag = false;
		if (this.isOfflineVRRig)
		{
			bool flag2 = false;
			VRRig.PartyMemberStatus partyMemberStatus = this.GetPartyMemberStatus();
			if (partyMemberStatus != VRRig.PartyMemberStatus.InLocalParty)
			{
				if (partyMemberStatus == VRRig.PartyMemberStatus.NotInLocalParty)
				{
					flag2 = false;
					this.reliableState.isBraceletLeftHanded = false;
				}
			}
			else
			{
				flag2 = true;
				this.reliableState.isBraceletLeftHanded = FriendshipGroupDetection.Instance.DidJoinLeftHanded && !this.huntComputer.activeSelf;
			}
			if (this.reliableState.HasBracelet != flag2 || this.reliableState.braceletBeadColors.Count != FriendshipGroupDetection.Instance.myBeadColors.Count)
			{
				this.reliableState.SetIsDirty();
				flag = this.reliableState.HasBracelet == flag2;
			}
			this.reliableState.braceletBeadColors.Clear();
			if (flag2)
			{
				this.reliableState.braceletBeadColors.AddRange(FriendshipGroupDetection.Instance.myBeadColors);
			}
			this.reliableState.braceletSelfIndex = FriendshipGroupDetection.Instance.MyBraceletSelfIndex;
		}
		if (this.nonCosmeticLeftHandItem != null)
		{
			bool flag3 = this.reliableState.HasBracelet && this.reliableState.isBraceletLeftHanded && !this.IsInvisibleToLocalPlayer;
			this.nonCosmeticLeftHandItem.EnableItem(flag3);
			if (flag3)
			{
				this.friendshipBraceletLeftHand.UpdateBeads(this.reliableState.braceletBeadColors, this.reliableState.braceletSelfIndex);
				if (flag)
				{
					this.friendshipBraceletLeftHand.PlayAppearEffects();
				}
			}
		}
		if (this.nonCosmeticRightHandItem != null)
		{
			bool flag4 = this.reliableState.HasBracelet && !this.reliableState.isBraceletLeftHanded && !this.IsInvisibleToLocalPlayer;
			this.nonCosmeticRightHandItem.EnableItem(flag4);
			if (flag4)
			{
				this.friendshipBraceletRightHand.UpdateBeads(this.reliableState.braceletBeadColors, this.reliableState.braceletSelfIndex);
				if (flag)
				{
					this.friendshipBraceletRightHand.PlayAppearEffects();
				}
			}
		}
	}

	public void EnableHuntWatch(bool on)
	{
		this.huntComputer.SetActive(on);
		if (this.builderResizeWatch != null)
		{
			MeshRenderer component = this.builderResizeWatch.GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.enabled = !on;
			}
		}
	}

	public void EnablePaintbrawlCosmetics(bool on)
	{
		this.paintbrawlBalloons.gameObject.SetActive(on);
	}

	public void EnableBuilderResizeWatch(bool on)
	{
		if (this.builderResizeWatch != null && this.builderResizeWatch.activeSelf != on)
		{
			this.builderResizeWatch.SetActive(on);
			if (this.builderArmShelfLeft != null)
			{
				this.builderArmShelfLeft.gameObject.SetActive(on);
			}
			if (this.builderArmShelfRight != null)
			{
				this.builderArmShelfRight.gameObject.SetActive(on);
			}
		}
		if (this.isOfflineVRRig)
		{
			bool flag = this.reliableState.isBuilderWatchEnabled != on;
			this.reliableState.isBuilderWatchEnabled = on;
			if (flag)
			{
				this.reliableState.SetIsDirty();
			}
		}
	}

	public void EnableGuardianEjectWatch(bool on)
	{
		if (this.guardianEjectWatch != null && this.guardianEjectWatch.activeSelf != on)
		{
			this.guardianEjectWatch.SetActive(on);
		}
	}

	public void EnableVStumpReturnWatch(bool on)
	{
		if (this.vStumpReturnWatch != null && this.vStumpReturnWatch.activeSelf != on)
		{
			this.vStumpReturnWatch.SetActive(on);
		}
	}

	public void EnableRankedTimerWatch(bool on)
	{
		if (this.rankedTimerWatch != null && this.rankedTimerWatch.activeSelf != on)
		{
			this.rankedTimerWatch.SetActive(on);
		}
	}

	public void EnableSuperInfectionHands(bool on)
	{
		if (this.superInfectionHand != null)
		{
			this.superInfectionHand.EnableHands(on);
		}
	}

	private void UpdateReplacementVoice()
	{
		if (this.remoteUseReplacementVoice || this.localUseReplacementVoice || GorillaComputer.instance.voiceChatOn != "TRUE")
		{
			this.voiceAudio.mute = true;
			return;
		}
		this.voiceAudio.mute = false;
	}

	public bool ShouldPlayReplacementVoice()
	{
		return this.netView && !this.netView.IsMine && !(GorillaComputer.instance.voiceChatOn == "OFF") && (this.remoteUseReplacementVoice || this.localUseReplacementVoice || GorillaComputer.instance.voiceChatOn == "FALSE") && this.SpeakingLoudness > this.replacementVoiceLoudnessThreshold;
	}

	public void SetDuplicationZone(RigDuplicationZone duplicationZone)
	{
		this.duplicationZone = duplicationZone;
		this.inDuplicationZone = duplicationZone != null;
	}

	public void ClearDuplicationZone(RigDuplicationZone duplicationZone)
	{
		if (this.duplicationZone == duplicationZone)
		{
			this.SetDuplicationZone(null);
			this.renderTransform.localPosition = Vector3.zero;
		}
	}

	public void ResetTimeSpawned()
	{
		this.timeSpawned = Time.time;
	}

	public void SetGooParticleSystemStatus(bool isLeftHand, bool isEnabled)
	{
		if (isLeftHand)
		{
			if (this.leftHandGooParticleSystem.gameObject.activeSelf != isEnabled)
			{
				this.leftHandGooParticleSystem.gameObject.SetActive(isEnabled);
				return;
			}
		}
		else if (this.rightHandGooParticleSystem.gameObject.activeSelf != isEnabled)
		{
			this.rightHandGooParticleSystem.gameObject.SetActive(isEnabled);
		}
	}

	bool IUserCosmeticsCallback.PendingUpdate
	{
		get
		{
			return this.pendingCosmeticUpdate;
		}
		set
		{
			this.pendingCosmeticUpdate = value;
		}
	}

	public bool IsFrozen { get; set; }

	bool IUserCosmeticsCallback.OnGetUserCosmetics(string cosmetics)
	{
		if (cosmetics == this.rawCosmeticString && this.currentCosmeticTries < this.cosmeticRetries)
		{
			this.currentCosmeticTries++;
			return false;
		}
		this.rawCosmeticString = cosmetics ?? "";
		this.concatStringOfCosmeticsAllowed = this.rawCosmeticString;
		this.concatStringOfCosmeticsAllowed += "LHAJJ.LHAJK.LHAJL.";
		this.InitializedCosmetics = true;
		this.currentCosmeticTries = 0;
		this.CheckForEarlyAccess();
		this.SetCosmeticsActive(false);
		this.myBodyDockPositions.RefreshTransferrableItems();
		NetworkView networkView = this.netView;
		if (networkView != null)
		{
			networkView.SendRPC("RPC_RequestCosmetics", this.creator, Array.Empty<object>());
		}
		return true;
	}

	private short PackCompetitiveData()
	{
		if (!this.turningCompInitialized)
		{
			this.GorillaSnapTurningComp = GorillaTagger.Instance.GetComponent<GorillaSnapTurn>();
			this.turningCompInitialized = true;
		}
		this.fps = Mathf.Min(Mathf.RoundToInt(1f / Time.smoothDeltaTime), 255);
		int num = 0;
		if (this.GorillaSnapTurningComp != null)
		{
			this.turnFactor = this.GorillaSnapTurningComp.turnFactor;
			this.turnType = this.GorillaSnapTurningComp.turnType;
			string text = this.turnType;
			if (!(text == "SNAP"))
			{
				if (text == "SMOOTH")
				{
					num = 2;
				}
			}
			else
			{
				num = 1;
			}
			num *= 10;
			num += this.turnFactor;
		}
		return (short)(this.fps + (num << 8));
	}

	private void UnpackCompetitiveData(short packed)
	{
		int num = 255;
		this.fps = (int)packed & num;
		int num2 = 31;
		int num3 = (packed >> 8) & num2;
		this.turnFactor = num3 % 10;
		int num4 = num3 / 10;
		if (num4 == 1)
		{
			this.turnType = "SNAP";
			return;
		}
		if (num4 != 2)
		{
			this.turnType = "NONE";
			return;
		}
		this.turnType = "SMOOTH";
	}

	private void OnKIDSessionUpdated(bool showCustomNames, Permission.ManagedByEnum managedBy)
	{
		bool flag = (showCustomNames || managedBy == Permission.ManagedByEnum.PLAYER) && managedBy != Permission.ManagedByEnum.PROHIBITED;
		GorillaComputer.instance.SetComputerSettingsBySafety(!flag, new GorillaComputer.ComputerState[] { GorillaComputer.ComputerState.Name }, false);
		bool flag2 = PlayerPrefs.GetInt("nameTagsOn", -1) > 0;
		switch (managedBy)
		{
		case Permission.ManagedByEnum.PLAYER:
			flag = GorillaComputer.instance.NametagsEnabled;
			break;
		case Permission.ManagedByEnum.GUARDIAN:
			flag = showCustomNames && flag2;
			break;
		case Permission.ManagedByEnum.PROHIBITED:
			flag = false;
			break;
		}
		this.UpdateName(flag);
		Debug.Log("[KID] On Session Update - Custom Names Permission changed - Has enabled customNames? [" + flag.ToString() + "]");
	}

	public static VRRig LocalRig
	{
		get
		{
			return VRRig.gLocalRig;
		}
	}

	public bool isLocal
	{
		get
		{
			return VRRig.gLocalRig == this;
		}
	}

	int IEyeScannable.scannableId
	{
		get
		{
			return base.gameObject.GetInstanceID();
		}
	}

	Vector3 IEyeScannable.Position
	{
		get
		{
			return base.transform.position;
		}
	}

	Bounds IEyeScannable.Bounds
	{
		get
		{
			return default(Bounds);
		}
	}

	IList<KeyValueStringPair> IEyeScannable.Entries
	{
		get
		{
			return this.buildEntries();
		}
	}

	private IList<KeyValueStringPair> buildEntries()
	{
		return new KeyValueStringPair[]
		{
			new KeyValueStringPair("Name", this.playerNameVisible),
			new KeyValueStringPair("Color", string.Format("{0}, {1}, {2}", Mathf.RoundToInt(this.playerColor.r * 9f), Mathf.RoundToInt(this.playerColor.g * 9f), Mathf.RoundToInt(this.playerColor.b * 9f)))
		};
	}

	public event Action OnDataChange;

	private bool _isListeningFor_OnPostInstantiateAllPrefabs;

	[OnEnterPlay_SetNull]
	public static Action newPlayerJoined;

	public VRMap head;

	public VRMap rightHand;

	public VRMap leftHand;

	public VRMapThumb leftThumb;

	public VRMapIndex leftIndex;

	public VRMapMiddle leftMiddle;

	public VRMapThumb rightThumb;

	public VRMapIndex rightIndex;

	public VRMapMiddle rightMiddle;

	public CrittersLoudNoise leftHandNoise;

	public CrittersLoudNoise rightHandNoise;

	public CrittersLoudNoise speakingNoise;

	private int previousGrabbedRope = -1;

	private int previousGrabbedRopeBoneIndex;

	private bool previousGrabbedRopeWasLeft;

	private bool previousGrabbedRopeWasBody;

	private GorillaRopeSwing currentRopeSwing;

	private Transform currentHoldParent;

	private Transform currentRopeSwingTarget;

	private float lastRopeGrabTimer;

	private bool shouldLerpToRope;

	[NonSerialized]
	public int grabbedRopeIndex = -1;

	[NonSerialized]
	public int grabbedRopeBoneIndex;

	[NonSerialized]
	public bool grabbedRopeIsLeft;

	[NonSerialized]
	public bool grabbedRopeIsBody;

	[NonSerialized]
	public bool grabbedRopeIsPhotonView;

	[NonSerialized]
	public Vector3 grabbedRopeOffset = Vector3.zero;

	private int prevMovingSurfaceID = -1;

	private bool movingSurfaceWasLeft;

	private bool movingSurfaceWasBody;

	private bool movingSurfaceWasMonkeBlock;

	[NonSerialized]
	public int mountedMovingSurfaceId = -1;

	[NonSerialized]
	private BuilderPiece mountedMonkeBlock;

	[NonSerialized]
	private MovingSurface mountedMovingSurface;

	[NonSerialized]
	public bool mountedMovingSurfaceIsLeft;

	[NonSerialized]
	public bool mountedMovingSurfaceIsBody;

	[NonSerialized]
	public bool movingSurfaceIsMonkeBlock;

	[NonSerialized]
	public Vector3 mountedMonkeBlockOffset = Vector3.zero;

	private float lastMountedSurfaceTimer;

	private bool shouldLerpToMovingSurface;

	[Tooltip("- False in 'Gorilla Player Networked.prefab'.\n- True in 'Local VRRig.prefab/Local Gorilla Player'.\n- False in 'Local VRRig.prefab/Actual Gorilla'")]
	public bool isOfflineVRRig;

	public GameObject mainCamera;

	public Transform playerOffsetTransform;

	public int SDKIndex;

	public bool isMyPlayer;

	public AudioSource leftHandPlayer;

	public AudioSource rightHandPlayer;

	public AudioSource tagSound;

	[SerializeField]
	private float ratio;

	public Transform headConstraint;

	public Vector3 headBodyOffset = Vector3.zero;

	public GameObject headMesh;

	private NetworkVector3 netSyncPos = new NetworkVector3();

	public Vector3 jobPos;

	public Quaternion syncRotation;

	public Quaternion jobRotation;

	public AudioClip[] clipToPlay;

	public AudioClip[] handTapSound;

	public int setMatIndex;

	public float lerpValueFingers;

	public float lerpValueBody;

	public GameObject backpack;

	public Transform leftHandTransform;

	public Transform rightHandTransform;

	public Transform bodyTransform;

	public SkinnedMeshRenderer mainSkin;

	public GorillaSkin defaultSkin;

	public MeshRenderer faceSkin;

	public XRaySkeleton skeleton;

	public GorillaBodyRenderer bodyRenderer;

	public ZoneEntityBSP zoneEntity;

	public Material scoreboardMaterial;

	public GameObject spectatorSkin;

	public int handSync;

	public Material[] materialsToChangeTo;

	public float red;

	public float green;

	public float blue;

	public TextMeshPro playerText1;

	public string playerNameVisible;

	[Tooltip("- True in 'Gorilla Player Networked.prefab'.\n- True in 'Local VRRig.prefab/Local Gorilla Player'.\n- False in 'Local VRRig.prefab/Actual Gorilla'")]
	public bool showName;

	public CosmeticItemRegistry cosmeticsObjectRegistry = new CosmeticItemRegistry();

	[NonSerialized]
	public PropHuntHandFollower propHuntHandFollower;

	[FormerlySerializedAs("cosmetics")]
	public GameObject[] _cosmetics;

	[FormerlySerializedAs("overrideCosmetics")]
	public GameObject[] _overrideCosmetics;

	private int taggedById;

	public string concatStringOfCosmeticsAllowed = "";

	private bool initializedCosmetics;

	private readonly HashSet<string> _temporaryCosmetics = new HashSet<string>();

	public CosmeticsController.CosmeticSet cosmeticSet;

	public CosmeticsController.CosmeticSet tryOnSet;

	public CosmeticsController.CosmeticSet mergedSet;

	public CosmeticsController.CosmeticSet prevSet;

	[NonSerialized]
	public readonly List<GameObject> activeCosmetics = new List<GameObject>(16);

	private int cosmeticRetries = 2;

	private int currentCosmeticTries;

	public SizeManager sizeManager;

	public float pitchScale = 0.3f;

	public float pitchOffset = 1f;

	[NonSerialized]
	public bool IsHaunted;

	public float HauntedVoicePitch = 0.5f;

	public float HauntedHearingVolume = 0.15f;

	[NonSerialized]
	public bool UsingHauntedRing;

	[NonSerialized]
	public float HauntedRingVoicePitch;

	private float cosmeticPitchShift = 1f;

	private bool pitchShiftCosmeticsDirty;

	[NonSerialized]
	public List<VoicePitchShiftCosmetic> PitchShiftCosmetics = new List<VoicePitchShiftCosmetic>();

	public FriendshipBracelet friendshipBraceletLeftHand;

	public NonCosmeticHandItem nonCosmeticLeftHandItem;

	public FriendshipBracelet friendshipBraceletRightHand;

	public NonCosmeticHandItem nonCosmeticRightHandItem;

	public HoverboardVisual hoverboardVisual;

	private int hoverboardEnabledCount;

	public HoldableHand bodyHolds;

	public HoldableHand leftHolds;

	public HoldableHand rightHolds;

	public GorillaClimbable leftHandHoldsPlayer;

	public GorillaClimbable rightHandHoldsPlayer;

	public HandLink leftHandLink;

	public HandLink rightHandLink;

	public GameObject nameTagAnchor;

	public GameObject frozenEffect;

	public GameObject iceCubeLeft;

	public GameObject iceCubeRight;

	public float frozenEffectMaxY;

	public float frozenEffectMaxHorizontalScale = 0.8f;

	public GameObject FPVEffectsParent;

	public Dictionary<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> TemporaryCosmeticEffects = new Dictionary<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect>();

	private float cosmeticStepsDuration;

	private CosmeticSwapper cosmeticSwapper;

	private Stack<CosmeticSwapper.CosmeticState> newSwappedCosmetics = new Stack<CosmeticSwapper.CosmeticState>();

	private bool isAtFinalCosmeticStep;

	private float _nextUpdateTime = -1f;

	public VRRigReliableState reliableState;

	[SerializeField]
	private Transform MouthPosition;

	internal RigContainer rigContainer;

	public Action<RigContainer> OnNameChanged;

	private Vector3 remoteVelocity;

	private double remoteLatestTimestamp;

	private Vector3 remoteCorrectionNeeded;

	private const float REMOTE_CORRECTION_RATE = 5f;

	private const bool USE_NEW_NETCODE = false;

	private float stealthTimer;

	private GorillaAmbushManager stealthManager;

	private LayerChanger layerChanger;

	private float frozenEffectMinY;

	private float frozenEffectMinHorizontalScale;

	private float frozenTimeElapsed;

	public TagEffectPack CosmeticEffectPack;

	private GorillaSnapTurn GorillaSnapTurningComp;

	private bool turningCompInitialized;

	private string turnType = "NONE";

	private int turnFactor;

	private int fps;

	private VRRig.PartyMemberStatus partyMemberStatus;

	public static readonly GTBitOps.BitWriteInfo[] WearablePackedStatesBitWriteInfos = new GTBitOps.BitWriteInfo[]
	{
		new GTBitOps.BitWriteInfo(0, 1),
		new GTBitOps.BitWriteInfo(1, 2),
		new GTBitOps.BitWriteInfo(3, 2),
		new GTBitOps.BitWriteInfo(5, 2),
		new GTBitOps.BitWriteInfo(7, 2),
		new GTBitOps.BitWriteInfo(9, 2),
		new GTBitOps.BitWriteInfo(11, 1),
		new GTBitOps.BitWriteInfo(12, 1),
		new GTBitOps.BitWriteInfo(13, 1)
	};

	public bool inTryOnRoom;

	public bool muted;

	private float lastScaleFactor = 1f;

	private float scaleMultiplier = 1f;

	private float nativeScale = 1f;

	private float timeSpawned;

	public float doNotLerpConstant = 1f;

	public string tempString;

	private Player tempPlayer;

	internal NetPlayer creator;

	private float[] speedArray;

	private double handLerpValues;

	private bool initialized;

	[FormerlySerializedAs("battleBalloons")]
	public PaintbrawlBalloons paintbrawlBalloons;

	private int tempInt;

	public BodyDockPositions myBodyDockPositions;

	public ParticleSystem lavaParticleSystem;

	public ParticleSystem rockParticleSystem;

	public ParticleSystem iceParticleSystem;

	public ParticleSystem snowFlakeParticleSystem;

	public ParticleSystem leftHandGooParticleSystem;

	public ParticleSystem rightHandGooParticleSystem;

	public string tempItemName;

	public CosmeticsController.CosmeticItem tempItem;

	public string tempItemId;

	public int tempItemCost;

	public int leftHandHoldableStatus;

	public int rightHandHoldableStatus;

	[Tooltip("This has to match the drumsAS array in DrumsItem.cs.")]
	[SerializeReference]
	public AudioSource[] musicDrums;

	private List<TransferrableObject> instrumentSelfOnly = new List<TransferrableObject>();

	public AudioSource geodeCrackingSound;

	public float bonkTime;

	public float bonkCooldown = 2f;

	private VRRig tempVRRig;

	public GameObject huntComputer;

	public GameObject builderResizeWatch;

	public BuilderArmShelf builderArmShelfLeft;

	public BuilderArmShelf builderArmShelfRight;

	public GameObject guardianEjectWatch;

	public GameObject vStumpReturnWatch;

	public GameObject rankedTimerWatch;

	public SuperInfectionHandDisplay superInfectionHand;

	public ProjectileWeapon projectileWeapon;

	private PhotonVoiceView myPhotonVoiceView;

	private VRRig senderRig;

	private bool isInitialized;

	private CircularBuffer<VRRig.VelocityTime> velocityHistoryList = new CircularBuffer<VRRig.VelocityTime>(200);

	public int velocityHistoryMaxLength = 200;

	private Vector3 lastPosition;

	public const int splashLimitCount = 4;

	public const float splashLimitCooldown = 0.5f;

	private float[] splashEffectTimes = new float[4];

	internal AudioSource voiceAudio;

	public bool remoteUseReplacementVoice;

	public bool localUseReplacementVoice;

	private MicWrapper currentMicWrapper;

	private IAudioDesc audioDesc;

	private float speakingLoudness;

	public bool shouldSendSpeakingLoudness = true;

	public float replacementVoiceLoudnessThreshold = 0.05f;

	public int replacementVoiceDetectionDelay = 128;

	private GorillaMouthFlap myMouthFlap;

	private GorillaSpeakerLoudness mySpeakerLoudness;

	public ReplacementVoice myReplacementVoice;

	private GorillaEyeExpressions myEyeExpressions;

	[SerializeField]
	internal NetworkView netView;

	[SerializeField]
	internal VRRigSerializer rigSerializer;

	public NetPlayer OwningNetPlayer;

	[SerializeField]
	private FXSystemSettings sharedFXSettings;

	[NonSerialized]
	public FXSystemSettings fxSettings;

	[SerializeField]
	private float tapPointDistance = 0.035f;

	[SerializeField]
	private float handSpeedToVolumeModifier = 0.05f;

	[SerializeField]
	private HandEffectContext _leftHandEffect;

	[SerializeField]
	private HandEffectContext _rightHandEffect;

	[SerializeField]
	private HandEffectContext _extraLeftHandEffect;

	[SerializeField]
	private HandEffectContext _extraRightHandEffect;

	private bool _rigBuildFullyInitialized;

	[SerializeField]
	private Transform renderTransform;

	private GamePlayer _gamePlayerRef;

	private bool playerWasHaunted;

	private float nonHauntedVolume;

	[SerializeField]
	private AnimationCurve voicePitchForRelativeScale;

	private Vector3 LocalTrajectoryOverridePosition;

	private Vector3 LocalTrajectoryOverrideVelocity;

	private float LocalTrajectoryOverrideBlend;

	[SerializeField]
	private float LocalTrajectoryOverrideDuration = 1f;

	private bool localOverrideIsBody;

	private bool localOverrideIsLeftHand;

	private Transform localOverrideGrabbingHand;

	private float localGrabOverrideBlend;

	[SerializeField]
	private float LocalGrabOverrideDuration = 0.25f;

	private float[] voiceSampleBuffer = new float[128];

	private const int CHECK_LOUDNESS_FREQ_FRAMES = 10;

	private CallbackContainer<ICallBack> lateUpdateCallbacks = new CallbackContainer<ICallBack>(5);

	private float nextLocalVelocityStoreTimestamp;

	private bool IsInvisibleToLocalPlayer;

	private const int remoteUseReplacementVoice_BIT = 512;

	private const int grabbedRope_BIT = 1024;

	private const int grabbedRopeIsPhotonView_BIT = 2048;

	private const int isHoldingHandsWithPlayer_BIT = 4096;

	private const int isHoldingHoverboard_BIT = 8192;

	private const int isHoverboardLeftHanded_BIT = 16384;

	private const int isOnMovingSurface_BIT = 32768;

	private const int isPropHunt_BIT = 65536;

	private const int propHuntLeftHand_BIT = 131072;

	private const int isLeftHandGrabbable_BIT = 262144;

	private const int isRightHandGrabbable_BIT = 524288;

	private const int speakingLoudnessVal_BITSHIFT = 24;

	private Vector3 tempVec;

	private Quaternion tempQuat;

	public Action<int, int> OnMaterialIndexChanged;

	[SerializeField]
	private ParticleSystem cosmeticsActivationPS;

	[SerializeField]
	private SoundBankPlayer cosmeticsActivationSBP;

	public Color playerColor;

	public bool colorInitialized;

	private Action<Color> onColorInitialized;

	private bool m_sentRankedScore;

	private int currentQuestScore;

	private bool _scoreUpdated;

	private CallLimiter updateQuestCallLimit = new CallLimiter(1, 0.5f, 0.5f);

	private float currentRankedELO;

	private int currentRankedSubTierQuest;

	private int currentRankedSubTierPC;

	private bool _rankedInfoUpdated;

	internal CallLimiter updateRankedInfoCallLimit = new CallLimiter(2, 60f, 0.5f);

	public const float maxGuardianThrowVelocity = 20f;

	public const float maxRegularThrowVelocity = 3f;

	private RaycastHit[] rayCastNonAllocColliders = new RaycastHit[5];

	private bool inDuplicationZone;

	private RigDuplicationZone duplicationZone;

	private bool pendingCosmeticUpdate = true;

	private string rawCosmeticString = "";

	public List<HandEffectsOverrideCosmetic> CosmeticHandEffectsOverride_Right = new List<HandEffectsOverrideCosmetic>();

	public List<HandEffectsOverrideCosmetic> CosmeticHandEffectsOverride_Left = new List<HandEffectsOverrideCosmetic>();

	private int loudnessCheckFrame;

	private float frameScale;

	private const bool SHOW_SCREENS = false;

	[OnEnterPlay_SetNull]
	private static VRRig gLocalRig;

	public enum PartyMemberStatus
	{
		NeedsUpdate,
		InLocalParty,
		NotInLocalParty
	}

	public enum WearablePackedStateSlots
	{
		Hat,
		LeftHand,
		RightHand,
		Face,
		Pants1,
		Pants2,
		Badge,
		Fur,
		Shirt
	}

	public struct VelocityTime
	{
		public VelocityTime(Vector3 velocity, double velTime)
		{
			this.vel = velocity;
			this.time = velTime;
		}

		public Vector3 vel;

		public double time;
	}
}
