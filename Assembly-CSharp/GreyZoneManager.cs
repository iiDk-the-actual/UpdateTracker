using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GreyZoneManager : MonoBehaviourPun, IPunObservable, IInRoomCallbacks
{
	public bool GreyZoneActive
	{
		get
		{
			return this.greyZoneActive;
		}
	}

	public bool GreyZoneAvailable
	{
		get
		{
			bool flag = false;
			if (GorillaComputer.instance != null)
			{
				flag = GorillaComputer.instance.GetServerTime().DayOfYear >= this.greyZoneAvailableDayOfYear;
			}
			return flag;
		}
	}

	public int GravityFactorSelection
	{
		get
		{
			return this.gravityFactorOptionSelection;
		}
	}

	public bool TickRunning
	{
		get
		{
			return this._tickRunning;
		}
		set
		{
			this._tickRunning = value;
		}
	}

	public bool HasAuthority
	{
		get
		{
			return !PhotonNetwork.InRoom || base.photonView.IsMine;
		}
	}

	public float SummoningProgress
	{
		get
		{
			return this.summoningProgress;
		}
	}

	public void RegisterSummoner(GreyZoneSummoner summoner)
	{
		if (!this.activeSummoners.Contains(summoner))
		{
			this.activeSummoners.Add(summoner);
		}
	}

	public void DeregisterSummoner(GreyZoneSummoner summoner)
	{
		if (this.activeSummoners.Contains(summoner))
		{
			this.activeSummoners.Remove(summoner);
		}
	}

	public void RegisterMoon(MoonController moon)
	{
		this.moonController = moon;
	}

	public void UnregisterMoon(MoonController moon)
	{
		if (this.moonController == moon)
		{
			this.moonController = null;
		}
	}

	public void ActivateGreyZoneAuthority()
	{
		this.greyZoneActive = true;
		this.photonConnectedDuringActivation = PhotonNetwork.InRoom;
		this.greyZoneActivationTime = (this.photonConnectedDuringActivation ? PhotonNetwork.Time : ((double)Time.time));
		this.ActivateGreyZoneLocal();
	}

	private void ActivateGreyZoneLocal()
	{
		Shader.SetGlobalInt(this._GreyZoneActive, 1);
		GTPlayer instance = GTPlayer.Instance;
		if (instance != null)
		{
			instance.SetGravityOverride(this, new Action<GTPlayer>(this.GravityOverrideFunction));
			this.gravityOverrideSet = true;
		}
		if (MusicManager.Instance != null)
		{
			MusicManager.Instance.FadeOutMusic(2f);
		}
		if (this.audioFadeCoroutine != null)
		{
			base.StopCoroutine(this.audioFadeCoroutine);
		}
		this.audioFadeCoroutine = base.StartCoroutine(this.FadeAudioIn(this.greyZoneAmbience, this.greyZoneAmbienceVolume, this.ambienceFadeTime));
		if (this.greyZoneAmbience != null)
		{
			this.greyZoneAmbience.GTPlay();
		}
		this.greyZoneParticles.gameObject.SetActive(true);
		this.summoningProgress = 1f;
		this.UpdateSummonerVisuals();
		for (int i = 0; i < this.activeSummoners.Count; i++)
		{
			this.activeSummoners[i].OnGreyZoneActivated();
		}
		if (this.OnGreyZoneActivated != null)
		{
			this.OnGreyZoneActivated();
		}
	}

	public void DeactivateGreyZoneAuthority()
	{
		this.greyZoneActive = false;
		foreach (KeyValuePair<int, ValueTuple<VRRig, GreyZoneSummoner>> keyValuePair in this.summoningPlayers)
		{
			this.summoningPlayerProgress[keyValuePair.Key] = 0f;
		}
		this.DeactivateGreyZoneLocal();
	}

	private void DeactivateGreyZoneLocal()
	{
		Shader.SetGlobalInt(this._GreyZoneActive, 0);
		if (MusicManager.Instance != null)
		{
			MusicManager.Instance.FadeInMusic(4f);
		}
		if (this.audioFadeCoroutine != null)
		{
			base.StopCoroutine(this.audioFadeCoroutine);
		}
		this.audioFadeCoroutine = base.StartCoroutine(this.FadeAudioOut(this.greyZoneAmbience, this.ambienceFadeTime));
		this.greyZoneParticles.gameObject.SetActive(false);
		this.summoningProgress = 0f;
		this.UpdateSummonerVisuals();
		if (this.OnGreyZoneDeactivated != null)
		{
			this.OnGreyZoneDeactivated();
		}
	}

	public void ForceStopGreyZone()
	{
		this.greyZoneActive = false;
		Shader.SetGlobalInt(this._GreyZoneActive, 0);
		GTPlayer instance = GTPlayer.Instance;
		if (instance != null)
		{
			instance.UnsetGravityOverride(this);
		}
		this.gravityOverrideSet = false;
		if (this.moonController != null)
		{
			this.moonController.UpdateDistance(1f);
		}
		if (MusicManager.Instance != null)
		{
			MusicManager.Instance.FadeInMusic(0f);
		}
		if (this.greyZoneAmbience != null)
		{
			this.greyZoneAmbience.volume = 0f;
			this.greyZoneAmbience.GTStop();
		}
		this.greyZoneParticles.gameObject.SetActive(false);
		this.summoningProgress = 0f;
		this.UpdateSummonerVisuals();
		if (this.OnGreyZoneDeactivated != null)
		{
			this.OnGreyZoneDeactivated();
		}
	}

	public void GravityOverrideFunction(GTPlayer player)
	{
		this.gravityReductionAmount = 0f;
		if (this.moonController != null)
		{
			this.gravityReductionAmount = Mathf.InverseLerp(1f - this.skyMonsterDistGravityRampBuffer, this.skyMonsterDistGravityRampBuffer, this.moonController.Distance);
		}
		float num = Mathf.Lerp(1f, this.gravityFactorOptions[this.gravityFactorOptionSelection], this.gravityReductionAmount);
		player.AddForce(Physics.gravity * num * player.scale, ForceMode.Acceleration);
	}

	private IEnumerator FadeAudioIn(AudioSource source, float maxVolume, float duration)
	{
		if (source != null)
		{
			float startingVolume = source.volume;
			float startTime = Time.time;
			source.GTPlay();
			for (float num = 0f; num < 1f; num = (Time.time - startTime) / duration)
			{
				source.volume = Mathf.Lerp(startingVolume, maxVolume, num);
				yield return null;
			}
			source.volume = maxVolume;
		}
		yield break;
	}

	private IEnumerator FadeAudioOut(AudioSource source, float duration)
	{
		if (source != null)
		{
			float startingVolume = source.volume;
			float startTime = Time.time;
			for (float num = 0f; num < 1f; num = (Time.time - startTime) / duration)
			{
				source.volume = Mathf.Lerp(startingVolume, 0f, num);
				yield return null;
			}
			source.volume = 0f;
			source.Stop();
		}
		yield break;
	}

	public void VRRigEnteredSummonerProximity(VRRig rig, GreyZoneSummoner summoner)
	{
		if (!this.summoningPlayers.ContainsKey(rig.Creator.ActorNumber))
		{
			this.summoningPlayers.Add(rig.Creator.ActorNumber, new ValueTuple<VRRig, GreyZoneSummoner>(rig, summoner));
			this.summoningPlayerProgress.Add(rig.Creator.ActorNumber, 0f);
		}
	}

	public void VRRigExitedSummonerProximity(VRRig rig, GreyZoneSummoner summoner)
	{
		if (this.summoningPlayers.ContainsKey(rig.Creator.ActorNumber))
		{
			this.summoningPlayers.Remove(rig.Creator.ActorNumber);
			this.summoningPlayerProgress.Remove(rig.Creator.ActorNumber);
		}
	}

	private void UpdateSummonerVisuals()
	{
		bool greyZoneAvailable = this.GreyZoneAvailable;
		for (int i = 0; i < this.activeSummoners.Count; i++)
		{
			this.activeSummoners[i].UpdateProgressFeedback(greyZoneAvailable);
		}
	}

	private void ValidateSummoningPlayers()
	{
		this.invalidSummoners.Clear();
		foreach (KeyValuePair<int, ValueTuple<VRRig, GreyZoneSummoner>> keyValuePair in this.summoningPlayers)
		{
			VRRig item = keyValuePair.Value.Item1;
			GreyZoneSummoner item2 = keyValuePair.Value.Item2;
			if (item.Creator.ActorNumber != keyValuePair.Key || (item.head.rigTarget.position - item2.SummoningFocusPoint).sqrMagnitude > item2.SummonerMaxDistance * item2.SummonerMaxDistance)
			{
				this.invalidSummoners.Add(keyValuePair.Key);
			}
		}
		foreach (int num in this.invalidSummoners)
		{
			this.summoningPlayers.Remove(num);
			this.summoningPlayerProgress.Remove(num);
		}
	}

	private int DayNightOverrideFunction(int inputIndex)
	{
		int num = 0;
		int num2 = 8;
		int num3 = inputIndex - num;
		int num4 = num2 - inputIndex;
		if (num3 <= 0 || num4 <= 0)
		{
			return inputIndex;
		}
		if (num4 > num3)
		{
			return num2;
		}
		return num;
	}

	private void Awake()
	{
		if (GreyZoneManager.Instance == null)
		{
			GreyZoneManager.Instance = this;
			this.greyZoneAmbienceVolume = this.greyZoneAmbience.volume;
			return;
		}
		Object.Destroy(this);
	}

	private void OnEnable()
	{
		if (this.forceTimeOfDayToNight)
		{
			BetterDayNightManager instance = BetterDayNightManager.instance;
			if (instance != null)
			{
				instance.SetTimeIndexOverrideFunction(new Func<int, int>(this.DayNightOverrideFunction));
			}
		}
	}

	private void OnDisable()
	{
		this.ForceStopGreyZone();
		if (this.forceTimeOfDayToNight)
		{
			BetterDayNightManager instance = BetterDayNightManager.instance;
			if (instance != null)
			{
				instance.UnsetTimeIndexOverrideFunction();
			}
		}
	}

	private void Update()
	{
		if (this.HasAuthority)
		{
			this.AuthorityUpdate();
		}
		this.SharedUpdate();
	}

	private void AuthorityUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (this.greyZoneActive)
		{
			this.summoningProgress = 1f;
			double num;
			if (this.photonConnectedDuringActivation && PhotonNetwork.InRoom)
			{
				num = PhotonNetwork.Time;
			}
			else if (!this.photonConnectedDuringActivation && !PhotonNetwork.InRoom)
			{
				num = (double)Time.time;
			}
			else
			{
				num = -100.0;
			}
			if (num > this.greyZoneActivationTime + (double)this.greyZoneActiveDuration || num < this.greyZoneActivationTime - 10.0)
			{
				this.DeactivateGreyZoneAuthority();
				return;
			}
		}
		else if (this.GreyZoneAvailable)
		{
			this.roomPlayerList = PhotonNetwork.PlayerList;
			int num2 = 1;
			if (this.roomPlayerList != null && this.roomPlayerList.Length != 0)
			{
				num2 = Mathf.Max((this.roomPlayerList.Length + 1) / 2, 1);
			}
			float num3 = 0f;
			float num4 = 1f / this.summoningActivationTime;
			foreach (KeyValuePair<int, ValueTuple<VRRig, GreyZoneSummoner>> keyValuePair in this.summoningPlayers)
			{
				VRRig item = keyValuePair.Value.Item1;
				GreyZoneSummoner item2 = keyValuePair.Value.Item2;
				float num5 = this.summoningPlayerProgress[keyValuePair.Key];
				Vector3 vector = item2.SummoningFocusPoint - item.leftHand.rigTarget.position;
				Vector3 vector2 = -item.leftHand.rigTarget.right;
				bool flag = Vector3.Dot(vector, vector2) > 0f;
				Vector3 vector3 = item2.SummoningFocusPoint - item.rightHand.rigTarget.position;
				Vector3 right = item.rightHand.rigTarget.right;
				bool flag2 = Vector3.Dot(vector3, right) > 0f;
				if (flag && flag2)
				{
					num5 = Mathf.MoveTowards(num5, 1f, num4 * deltaTime);
				}
				else
				{
					num5 = Mathf.MoveTowards(num5, 0f, num4 * deltaTime);
				}
				num3 += num5;
				this.summoningPlayerProgress[keyValuePair.Key] = num5;
			}
			float num6 = 0.95f;
			this.summoningProgress = Mathf.Clamp01(num3 / num6 / (float)num2);
			this.UpdateSummonerVisuals();
			if (this.summoningProgress > 0.99f)
			{
				this.ActivateGreyZoneAuthority();
			}
		}
	}

	private void SharedUpdate()
	{
		GTPlayer instance = GTPlayer.Instance;
		if (this.greyZoneActive)
		{
			Vector3 vector = Vector3.ClampMagnitude(instance.InstantaneousVelocity * this.particlePredictiveSpawnVelocityFactor, this.particlePredictiveSpawnMaxDist);
			this.greyZoneParticles.transform.position = instance.HeadCenterPosition + Vector3.down * 0.5f + vector;
		}
		else if (this.gravityOverrideSet && this.gravityReductionAmount < 0.01f)
		{
			instance.UnsetGravityOverride(this);
			this.gravityOverrideSet = false;
		}
		float num = (this.greyZoneActive ? 0f : 1f);
		float num2 = (this.greyZoneActive ? this.skyMonsterMovementEnterTime : this.skyMonsterMovementExitTime);
		if (this.moonController != null && this.moonController.Distance != num)
		{
			float num3 = Mathf.SmoothDamp(this.moonController.Distance, num, ref this.skyMonsterMovementVelocity, num2);
			if ((double)Mathf.Abs(num3 - num) < 0.001)
			{
				num3 = num;
			}
			this.moonController.UpdateDistance(num3);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(this.greyZoneActive);
			stream.SendNext(this.greyZoneActivationTime);
			stream.SendNext(this.photonConnectedDuringActivation);
			stream.SendNext(this.gravityFactorOptionSelection);
			stream.SendNext(this.summoningProgress);
			return;
		}
		if (stream.IsReading && info.Sender.IsMasterClient)
		{
			bool flag = this.greyZoneActive;
			this.greyZoneActive = (bool)stream.ReceiveNext();
			this.greyZoneActivationTime = ((double)stream.ReceiveNext()).GetFinite();
			this.photonConnectedDuringActivation = (bool)stream.ReceiveNext();
			this.gravityFactorOptionSelection = (int)stream.ReceiveNext();
			this.summoningProgress = ((float)stream.ReceiveNext()).ClampSafe(0f, 1f);
			this.UpdateSummonerVisuals();
			if (this.greyZoneActive && !flag)
			{
				this.ActivateGreyZoneLocal();
				return;
			}
			if (!this.greyZoneActive && flag)
			{
				this.DeactivateGreyZoneLocal();
			}
		}
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
		this.ValidateSummoningPlayers();
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
		this.ValidateSummoningPlayers();
	}

	[OnEnterPlay_SetNull]
	public static volatile GreyZoneManager Instance;

	[SerializeField]
	private float greyZoneActiveDuration = 90f;

	[SerializeField]
	private float[] gravityFactorOptions = new float[] { 0.25f, 0.5f, 0.75f };

	[SerializeField]
	private int gravityFactorOptionSelection = 1;

	[SerializeField]
	private float summoningActivationTime = 3f;

	[SerializeField]
	private AudioSource greyZoneAmbience;

	[SerializeField]
	private float ambienceFadeTime = 4f;

	[SerializeField]
	private bool forceTimeOfDayToNight;

	[SerializeField]
	private float skyMonsterMovementEnterTime = 4.5f;

	[SerializeField]
	private float skyMonsterMovementExitTime = 3.2f;

	[SerializeField]
	private float skyMonsterDistGravityRampBuffer = 0.15f;

	[SerializeField]
	[Range(0f, 1f)]
	private float gravityReductionAmount = 1f;

	[SerializeField]
	private ParticleSystem greyZoneParticles;

	[SerializeField]
	private float particlePredictiveSpawnMaxDist = 4f;

	[SerializeField]
	private float particlePredictiveSpawnVelocityFactor = 0.5f;

	private bool photonConnectedDuringActivation;

	private double greyZoneActivationTime;

	private bool greyZoneActive;

	private bool _tickRunning;

	private float summoningProgress;

	private List<GreyZoneSummoner> activeSummoners = new List<GreyZoneSummoner>();

	private Dictionary<int, ValueTuple<VRRig, GreyZoneSummoner>> summoningPlayers = new Dictionary<int, ValueTuple<VRRig, GreyZoneSummoner>>();

	private Dictionary<int, float> summoningPlayerProgress = new Dictionary<int, float>();

	private HashSet<int> invalidSummoners = new HashSet<int>();

	private Coroutine audioFadeCoroutine;

	private Player[] roomPlayerList;

	private ShaderHashId _GreyZoneActive = new ShaderHashId("_GreyZoneActive");

	private MoonController moonController;

	private float skyMonsterMovementVelocity;

	private bool gravityOverrideSet;

	private float greyZoneAmbienceVolume = 0.15f;

	private int greyZoneAvailableDayOfYear = new DateTime(2024, 10, 25).DayOfYear;

	public Action OnGreyZoneActivated;

	public Action OnGreyZoneDeactivated;
}
