using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Modio.Mods;
using TMPro;
using UnityEngine;

public class VirtualStumpTeleporter : MonoBehaviour, IBuildValidation, IGorillaSliceableSimple
{
	public bool BuildValidationCheck()
	{
		if (this.netSerializer.IsNull())
		{
			Debug.LogError("VStump Teleporter \"" + base.gameObject.GetPath() + "\" needs a reference to a VirtualStumpTeleporterSerializer for networked FX to function. Check out the teleporter prefabs in arcade or the stump", this);
			return false;
		}
		return true;
	}

	public void SliceUpdate()
	{
		if (!this.accessDenied && NetworkSystem.Instance.netState != NetSystemState.Idle && NetworkSystem.Instance.netState != NetSystemState.InGame)
		{
			this.DenyAccess();
		}
		if (this.accessDenied && (NetworkSystem.Instance.netState == NetSystemState.Idle || NetworkSystem.Instance.netState == NetSystemState.InGame) && !UGCPermissionManager.IsUGCDisabled)
		{
			this.AllowAccess();
		}
	}

	public void OnEnable()
	{
		if (this.netSerializer.IsNull())
		{
			Debug.LogWarning("[VStumpTeleporter.OnEnable] Net Serializer is null for \"" + base.gameObject.GetPath() + "\", networked teleport FX will not function.");
		}
		if (UGCPermissionManager.IsUGCDisabled || (NetworkSystem.Instance.netState != NetSystemState.Idle && NetworkSystem.Instance.netState != NetSystemState.InGame))
		{
			ushort num = VirtualStumpTeleporter.lastLoggingHandsMsgId;
			VirtualStumpTeleporter.lastLoggingHandsMsgId = 1;
			this.DenyAccess();
		}
		else
		{
			ushort num2 = VirtualStumpTeleporter.lastLoggingHandsMsgId;
			VirtualStumpTeleporter.lastLoggingHandsMsgId = 2;
			this.AllowAccess();
		}
		UGCPermissionManager.SubscribeToUGCEnabled(new Action(this.OnUGCEnabled));
		UGCPermissionManager.SubscribeToUGCDisabled(new Action(this.OnUGCDisabled));
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		this.AllowAccess();
		UGCPermissionManager.UnsubscribeFromUGCEnabled(new Action(this.OnUGCEnabled));
		UGCPermissionManager.UnsubscribeFromUGCDisabled(new Action(this.OnUGCDisabled));
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	private void OnUGCEnabled()
	{
		this.AllowAccess();
		ushort num = VirtualStumpTeleporter.lastLoggingHandsMsgId;
		VirtualStumpTeleporter.lastLoggingHandsMsgId = 3;
	}

	private void OnUGCDisabled()
	{
		this.DenyAccess();
		ushort num = VirtualStumpTeleporter.lastLoggingHandsMsgId;
		VirtualStumpTeleporter.lastLoggingHandsMsgId = 4;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (UGCPermissionManager.IsUGCDisabled || this.accessDenied || this.teleporting || CustomMapManager.WaitingForRoomJoin || CustomMapManager.WaitingForDisconnect)
		{
			return;
		}
		if (other.gameObject == GorillaTagger.Instance.headCollider.gameObject)
		{
			this.triggerEntryTime = Time.time;
			this.ShowCountdownText();
		}
	}

	public void OnTriggerStay(Collider other)
	{
		if (UGCPermissionManager.IsUGCDisabled || this.accessDenied)
		{
			return;
		}
		if (other.gameObject == GorillaTagger.Instance.headCollider.gameObject && this.triggerEntryTime >= 0f)
		{
			this.UpdateCountdownText();
			if (!this.teleporting && this.triggerEntryTime + this.stayInTriggerDuration <= Time.time)
			{
				this.TeleportPlayer();
				this.HideCountdownText();
			}
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (UGCPermissionManager.IsUGCDisabled || this.accessDenied)
		{
			return;
		}
		if (other.gameObject == GorillaTagger.Instance.headCollider.gameObject)
		{
			this.triggerEntryTime = -1f;
			this.HideCountdownText();
		}
	}

	private void ShowCountdownText()
	{
		if (UGCPermissionManager.IsUGCDisabled || this.accessDenied)
		{
			return;
		}
		if (!this.countdownTexts.IsNullOrEmpty<TMP_Text>())
		{
			int num = 1 + Mathf.FloorToInt(this.stayInTriggerDuration);
			for (int i = 0; i < this.countdownTexts.Length; i++)
			{
				if (!this.countdownTexts[i].IsNull())
				{
					this.countdownTexts[i].text = num.ToString();
					this.countdownTexts[i].gameObject.SetActive(true);
				}
			}
		}
	}

	private void HideCountdownText()
	{
		if (!this.countdownTexts.IsNullOrEmpty<TMP_Text>())
		{
			for (int i = 0; i < this.countdownTexts.Length; i++)
			{
				if (!this.countdownTexts[i].IsNull())
				{
					this.countdownTexts[i].text = "";
					this.countdownTexts[i].gameObject.SetActive(false);
				}
			}
		}
	}

	private void UpdateCountdownText()
	{
		if (UGCPermissionManager.IsUGCDisabled || this.accessDenied)
		{
			return;
		}
		if (!this.countdownTexts.IsNullOrEmpty<TMP_Text>())
		{
			float num = this.stayInTriggerDuration - (Time.time - this.triggerEntryTime);
			int num2 = 1 + Mathf.FloorToInt(num);
			for (int i = 0; i < this.countdownTexts.Length; i++)
			{
				if (!this.countdownTexts[i].IsNull())
				{
					this.countdownTexts[i].text = num2.ToString();
				}
			}
		}
	}

	public void TeleportPlayer()
	{
		if (UGCPermissionManager.IsUGCDisabled || this.accessDenied)
		{
			return;
		}
		if (!this.teleporting)
		{
			this.teleporting = true;
			CustomMapManager.TeleportToVirtualStump(this, new Action<bool>(this.FinishTeleport));
		}
	}

	private void FinishTeleport(bool success = true)
	{
		if (this.teleporting)
		{
			this.teleporting = false;
			this.triggerEntryTime = -1f;
		}
	}

	private void DenyAccess()
	{
		this.accessDenied = true;
		foreach (GameObject gameObject in this.accessDeniedEnabledObjects)
		{
			gameObject.SetActive(true);
		}
		foreach (GameObject gameObject2 in this.accessDeniedDisabledObjects)
		{
			gameObject2.SetActive(false);
		}
	}

	private void AllowAccess()
	{
		if (UGCPermissionManager.IsUGCDisabled)
		{
			return;
		}
		this.accessDenied = false;
		foreach (GameObject gameObject in this.accessDeniedEnabledObjects)
		{
			gameObject.SetActive(false);
		}
		foreach (GameObject gameObject2 in this.accessDeniedDisabledObjects)
		{
			gameObject2.SetActive(true);
		}
	}

	private short GetIndex()
	{
		if (!this.netSerializer.IsNotNull())
		{
			return -1;
		}
		return this.netSerializer.GetTeleporterIndex(this);
	}

	public GTZone GetZone()
	{
		return this.entranceZone;
	}

	public GorillaNetworkJoinTrigger GetExitVStumpJoinTrigger()
	{
		return this.exitVStumpJoinTrigger;
	}

	public Transform GetReturnTransform()
	{
		return this.returnLocation;
	}

	public long GetAutoLoadMapModId()
	{
		return this.autoLoadMapModId;
	}

	public GameModeType GetAutoLoadGamemode()
	{
		return this.autoLoadGamemode;
	}

	public GameModeType GetReturnGamemode()
	{
		return this.forcedGamemodeUponReturn;
	}

	public void PlayTeleportEffects(bool forLocalPlayer, bool toVStump, AudioSource vStumpSFXAudioSource = null, bool sendRPC = false)
	{
		if (sendRPC && this.netSerializer.IsNotNull())
		{
			this.netSerializer.NotifyPlayerTeleporting(this.GetIndex(), vStumpSFXAudioSource);
		}
		ParticleSystem particleSystem;
		if (toVStump)
		{
			particleSystem = this.teleportToVStumpVFX;
			if (forLocalPlayer && vStumpSFXAudioSource.IsNotNull() && !this.teleportingPlayerSoundClips.IsNullOrEmpty<AudioClip>())
			{
				vStumpSFXAudioSource.clip = this.teleportingPlayerSoundClips[Random.Range(0, this.teleportingPlayerSoundClips.Count)];
				vStumpSFXAudioSource.Play();
			}
			if (!forLocalPlayer && this.teleporterSFXAudioSource.IsNotNull() && !this.observerSoundClips.IsNullOrEmpty<AudioClip>())
			{
				this.teleporterSFXAudioSource.clip = this.observerSoundClips[Random.Range(0, this.observerSoundClips.Count)];
				this.teleporterSFXAudioSource.Play();
			}
		}
		else
		{
			particleSystem = this.returnFromVStumpVFX;
			if (this.teleporterSFXAudioSource.IsNotNull())
			{
				if (forLocalPlayer && !this.teleportingPlayerSoundClips.IsNullOrEmpty<AudioClip>())
				{
					this.teleporterSFXAudioSource.clip = this.teleportingPlayerSoundClips[Random.Range(0, this.teleportingPlayerSoundClips.Count)];
				}
				else if (!forLocalPlayer && !this.observerSoundClips.IsNullOrEmpty<AudioClip>())
				{
					this.teleporterSFXAudioSource.clip = this.observerSoundClips[Random.Range(0, this.observerSoundClips.Count)];
				}
				this.teleporterSFXAudioSource.Play();
			}
		}
		if (particleSystem.IsNotNull())
		{
			particleSystem.Play();
		}
	}

	[SerializeField]
	private float stayInTriggerDuration = 3f;

	[SerializeField]
	private TMP_Text[] countdownTexts;

	[SerializeField]
	private GameObject[] handHoldObjects;

	[SerializeField]
	private List<GameObject> accessDeniedDisabledObjects = new List<GameObject>();

	[SerializeField]
	private List<GameObject> accessDeniedEnabledObjects = new List<GameObject>();

	[SerializeField]
	private Transform returnLocation;

	[SerializeField]
	private GTZone entranceZone = GTZone.arcade;

	[SerializeField]
	private GorillaNetworkJoinTrigger exitVStumpJoinTrigger;

	[SerializeField]
	private long autoLoadMapModId = ModId.Null;

	[SerializeField]
	private GameModeType autoLoadGamemode = GameModeType.None;

	[SerializeField]
	private GameModeType forcedGamemodeUponReturn = GameModeType.None;

	[SerializeField]
	private ParticleSystem teleportToVStumpVFX;

	[SerializeField]
	private ParticleSystem returnFromVStumpVFX;

	[SerializeField]
	private AudioSource teleporterSFXAudioSource;

	[SerializeField]
	private List<AudioClip> teleportingPlayerSoundClips = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> observerSoundClips = new List<AudioClip>();

	[SerializeField]
	private VirtualStumpTeleporterSerializer netSerializer;

	private VirtualStumpTeleporterSerializer mySerializer;

	private bool accessDenied;

	private bool teleporting;

	private float triggerEntryTime = -1f;

	[OnEnterPlay_Set(0)]
	private static ushort lastLoggingHandsMsgId;
}
