using System;
using System.Collections.Generic;
using GorillaGameModes;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GorillaGuardianZoneManager : MonoBehaviourPunCallbacks, IPunObservable, IGorillaSliceableSimple
{
	public NetPlayer CurrentGuardian
	{
		get
		{
			return this.guardianPlayer;
		}
	}

	public void Awake()
	{
		GorillaGuardianZoneManager.zoneManagers.Add(this);
		this.idol.gameObject.SetActive(false);
		foreach (Transform transform in this.idolPositions)
		{
			transform.gameObject.SetActive(false);
		}
		GorillaGuardianManager gorillaGuardianManager = GameMode.ActiveGameMode as GorillaGuardianManager;
		if (gorillaGuardianManager != null && gorillaGuardianManager.isPlaying && PhotonNetwork.IsMasterClient)
		{
			this.StartPlaying();
		}
	}

	private void Start()
	{
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
	}

	public void OnDestroy()
	{
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.OnZoneChanged));
		GorillaGuardianZoneManager.zoneManagers.Remove(this);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void SliceUpdate()
	{
		float idolActivationDisplay = this._idolActivationDisplay;
		float num = 0f;
		if (this._currentActivationTime < 0f)
		{
			this._idolActivationDisplay = 0f;
			this._progressing = false;
		}
		else
		{
			num = Mathf.Min(Time.time - this._lastTappedTime, this.activationTimePerTap);
			this._progressing = num < this.activationTimePerTap;
			this._idolActivationDisplay = (this._currentActivationTime + num) / this.requiredActivationTime;
		}
		if (idolActivationDisplay != this._idolActivationDisplay)
		{
			this.idol.UpdateActivationProgress(this._currentActivationTime + num, this._progressing);
		}
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		this.StopPlaying();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (this.guardianPlayer == null || this.guardianPlayer.GetPlayerRef() == otherPlayer)
		{
			this.SetGuardian(null);
		}
		NetPlayer previousGuardian = this._previousGuardian;
		if (((previousGuardian != null) ? previousGuardian.GetPlayerRef() : null) == otherPlayer)
		{
			this._previousGuardian = null;
		}
	}

	private void OnZoneChanged()
	{
		bool flag = ZoneManagement.IsInZone(this.zone);
		if (flag != this._zoneIsActive || !this._zoneStateChanged)
		{
			this._zoneIsActive = flag;
			this.idol.OnZoneActiveStateChanged(this._zoneIsActive);
			this._zoneStateChanged = true;
		}
		if (!this._zoneIsActive)
		{
			return;
		}
		GorillaGuardianManager gorillaGuardianManager = GameMode.ActiveGameMode as GorillaGuardianManager;
		if (gorillaGuardianManager != null && gorillaGuardianManager.isPlaying && gorillaGuardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer) && this.guardianPlayer != null && this.guardianPlayer != NetworkSystem.Instance.LocalPlayer)
		{
			gorillaGuardianManager.RequestEjectGuardian(NetworkSystem.Instance.LocalPlayer);
		}
	}

	public void StartPlaying()
	{
		if (!this.IsZoneValid())
		{
			return;
		}
		this._currentActivationTime = -1f;
		if (this.guardianPlayer != null && !this.guardianPlayer.InRoom())
		{
			this.SetGuardian(null);
			this._previousGuardian = null;
		}
		this.idol.gameObject.SetActive(true);
		this.SelectNextIdol();
		this.SetIdolPosition(this.currentIdol);
	}

	public void StopPlaying()
	{
		this._currentActivationTime = -1f;
		this.currentIdol = -1;
		this.idol.gameObject.SetActive(false);
		this._progressing = false;
		this._lastTappedTime = 0f;
		this.SetGuardian(null);
		this._previousGuardian = null;
	}

	public void SetScaleCenterPoint(Transform scaleCenterPoint)
	{
		this.guardianSizeChanger.SetScaleCenterPoint(scaleCenterPoint);
	}

	public void IdolWasTapped(NetPlayer tapper)
	{
		if (tapper != null && (!GameMode.ParticipatingPlayers.Contains(tapper) || tapper == this.guardianPlayer))
		{
			return;
		}
		if (!this.IsZoneValid())
		{
			return;
		}
		if (this.UpdateTapCount(tapper))
		{
			this.IdolActivated(tapper);
		}
	}

	public bool IsZoneValid()
	{
		return NetworkSystem.Instance.SessionIsPrivate || ZoneManagement.IsInZone(this.zone);
	}

	private bool UpdateTapCount(NetPlayer tapper)
	{
		if (this.guardianPlayer == null && this._previousGuardian == null)
		{
			return true;
		}
		if (this._currentActivationTime < 0f)
		{
			this._currentActivationTime = 0f;
			this._lastTappedTime = Time.time;
		}
		if (!this._progressing)
		{
			float num = Mathf.Min(Time.time - this._lastTappedTime, this.activationTimePerTap);
			this._lastTappedTime = Time.time;
			if (num + this._currentActivationTime >= this.requiredActivationTime)
			{
				return true;
			}
			this._currentActivationTime += num;
		}
		return false;
	}

	private void IdolActivated(NetPlayer activater)
	{
		this._currentActivationTime = -1f;
		this.SetGuardian(activater);
		this.SelectNextIdol();
		this.MoveIdolPosition(this.currentIdol);
	}

	public void SetGuardian(NetPlayer newGuardian)
	{
		if (this.guardianPlayer == newGuardian)
		{
			return;
		}
		if (this.guardianPlayer != null)
		{
			if (NetworkSystem.Instance.LocalPlayer == this.guardianPlayer)
			{
				this.PlayerLostGuardianSFX.Play();
			}
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(this.guardianPlayer, out rigContainer))
			{
				rigContainer.Rig.EnableGuardianEjectWatch(false);
				this.guardianSizeChanger.unacceptRig(rigContainer.Rig);
				int num = (RoomSystem.JoinedRoom ? rigContainer.netView.ViewID : rigContainer.CachedNetViewID);
				if (GorillaTagger.Instance.offlineVRRig.grabbedRopeIndex == num)
				{
					GorillaTagger.Instance.offlineVRRig.DroppedByPlayer(rigContainer.Rig, Vector3.zero);
					if (this.guardianPlayer == NetworkSystem.Instance.LocalPlayer)
					{
						bool flag = GorillaTagger.Instance.offlineVRRig.grabbedRopeBoneIndex == 1;
						EquipmentInteractor.instance.UpdateHandEquipment(null, flag);
					}
				}
			}
		}
		this._previousGuardian = this.guardianPlayer;
		this.guardianPlayer = newGuardian;
		if (this.guardianPlayer != null)
		{
			if (NetworkSystem.Instance.LocalPlayer == this.guardianPlayer)
			{
				this.PlayerGainGuardianSFX.Play();
			}
			else
			{
				this.ObserverGainGuardianSFX.Play();
			}
			RigContainer rigContainer2;
			if (VRRigCache.Instance.TryGetVrrig(this.guardianPlayer, out rigContainer2))
			{
				rigContainer2.Rig.EnableGuardianEjectWatch(true);
				this.guardianSizeChanger.acceptRig(rigContainer2.Rig);
			}
			PlayerGameEvents.GameModeCompleteRound();
			if (NetworkSystem.Instance.LocalPlayer == this.guardianPlayer)
			{
				PlayerGameEvents.GameModeObjectiveTriggered();
			}
		}
	}

	public bool IsPlayerGuardian(NetPlayer player)
	{
		return player == this.guardianPlayer;
	}

	private int SelectNextIdol()
	{
		if (this.idolPositions == null || this.idolPositions.Count == 0)
		{
			GTDev.Log<string>("No Guardian Idols possible to select.", null);
			return -1;
		}
		this.currentIdol = this.SelectRandomIdol();
		return this.currentIdol;
	}

	private int SelectRandomIdol()
	{
		int num;
		if (this.currentIdol != -1 && this.idolPositions.Count > 1)
		{
			num = (this.currentIdol + Random.Range(1, this.idolPositions.Count)) % this.idolPositions.Count;
		}
		else
		{
			num = Random.Range(0, this.idolPositions.Count);
		}
		return num;
	}

	private int SelectFarthestFromGuardian()
	{
		if (!(GorillaGameManager.instance is GorillaGuardianManager))
		{
			return this.SelectRandomIdol();
		}
		RigContainer rigContainer;
		if (this.guardianPlayer != null && VRRigCache.Instance.TryGetVrrig(this.guardianPlayer, out rigContainer))
		{
			Vector3 position = rigContainer.transform.position;
			int num = -1;
			float num2 = 0f;
			for (int i = 0; i < this.idolPositions.Count; i++)
			{
				float num3 = Vector3.SqrMagnitude(this.idolPositions[i].transform.position - position);
				if (num3 > num2)
				{
					num2 = num3;
					num = i;
				}
			}
			if (num != -1)
			{
				return num;
			}
		}
		return this.SelectRandomIdol();
	}

	private int SelectFarFromNearestPlayer()
	{
		List<Transform> list = this.SortByDistanceToNearestPlayer();
		if (list.Count > 1 && this.currentIdol >= 0 && this.currentIdol < list.Count)
		{
			list.Remove(this.idolPositions[this.currentIdol]);
		}
		int num = Random.Range(list.Count / 2, list.Count);
		Transform transform = list[num];
		return this.idolPositions.IndexOf(transform);
	}

	private List<Transform> SortByDistanceToNearestPlayer()
	{
		GorillaGuardianZoneManager.<>c__DisplayClass49_0 CS$<>8__locals1 = new GorillaGuardianZoneManager.<>c__DisplayClass49_0();
		CS$<>8__locals1.playerPositions = new List<Vector3>();
		foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
		{
			if (!(vrrig == null))
			{
				CS$<>8__locals1.playerPositions.Add(vrrig.transform.position);
			}
		}
		this._sortedIdolPositions.Clear();
		foreach (Transform transform in this.idolPositions)
		{
			this._sortedIdolPositions.Add(transform);
		}
		this._sortedIdolPositions.Sort(new Comparison<Transform>(CS$<>8__locals1.<SortByDistanceToNearestPlayer>g__CompareNearestPlayerDistance|0));
		return this._sortedIdolPositions;
	}

	public void TriggerIdolKnockback()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		for (int i = 0; i < RoomSystem.PlayersInRoom.Count; i++)
		{
			RigContainer rigContainer;
			if ((this.knockbackIncludesGuardian || RoomSystem.PlayersInRoom[i] != this.guardianPlayer) && VRRigCache.Instance.TryGetVrrig(RoomSystem.PlayersInRoom[i], out rigContainer))
			{
				Vector3 vector = rigContainer.Rig.transform.position - this.idol.transform.position;
				if (Vector3.SqrMagnitude(vector) < this.idolKnockbackRadius * this.idolKnockbackRadius)
				{
					Vector3 vector2 = (vector - Vector3.up * Vector3.Dot(Vector3.up, vector)).normalized * this.idolKnockbackStrengthHoriz + Vector3.up * this.idolKnockbackStrengthVert;
					RoomSystem.LaunchPlayer(RoomSystem.PlayersInRoom[i], vector2);
				}
			}
		}
	}

	private void SetIdolPosition(int index)
	{
		if (index < 0 || index >= this.idolPositions.Count)
		{
			GTDev.Log<string>("Invalid index received", null);
			return;
		}
		this.idol.gameObject.SetActive(true);
		this.idol.SetPosition(this.idolPositions[index].position);
	}

	private void MoveIdolPosition(int index)
	{
		if (index < 0 || index >= this.idolPositions.Count)
		{
			GTDev.Log<string>("Invalid index received", null);
			return;
		}
		this.idol.gameObject.SetActive(true);
		this.idol.MovePositions(this.idolPositions[index].position);
		if (base.photonView.IsMine)
		{
			this.idolMoveCount++;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		GorillaGuardianManager gorillaGuardianManager = GameMode.ActiveGameMode as GorillaGuardianManager;
		if (gorillaGuardianManager == null || !gorillaGuardianManager.isPlaying || player != NetworkSystem.Instance.MasterClient)
		{
			return;
		}
		if (stream.IsWriting)
		{
			stream.SendNext((this.guardianPlayer != null) ? this.guardianPlayer.ActorNumber : 0);
			stream.SendNext(this._currentActivationTime);
			stream.SendNext(this.currentIdol);
			stream.SendNext(this.idolMoveCount);
			return;
		}
		int num = (int)stream.ReceiveNext();
		float num2 = (float)stream.ReceiveNext();
		int num3 = (int)stream.ReceiveNext();
		int num4 = (int)stream.ReceiveNext();
		if (float.IsNaN(num2) || float.IsInfinity(num2))
		{
			return;
		}
		this.SetGuardian((num != 0) ? NetworkSystem.Instance.GetPlayer(num) : null);
		if (num2 != this._currentActivationTime)
		{
			this._currentActivationTime = num2;
			this._lastTappedTime = Time.time;
		}
		if (num3 != this.currentIdol || num4 != this.idolMoveCount)
		{
			if (this.currentIdol == -1)
			{
				this.SetIdolPosition(num3);
			}
			else
			{
				this.MoveIdolPosition(num3);
			}
			this.currentIdol = num3;
			this.idolMoveCount = num4;
		}
	}

	public static List<GorillaGuardianZoneManager> zoneManagers = new List<GorillaGuardianZoneManager>();

	[SerializeField]
	private GTZone zone;

	[SerializeField]
	private SizeChanger guardianSizeChanger;

	[SerializeField]
	private TappableGuardianIdol idol;

	[SerializeField]
	private List<Transform> idolPositions;

	[Space]
	[SerializeField]
	private float requiredActivationTime = 10f;

	[SerializeField]
	private float activationTimePerTap = 1f;

	[Space]
	[SerializeField]
	private bool knockbackIncludesGuardian = true;

	[SerializeField]
	private float idolKnockbackRadius = 6f;

	[SerializeField]
	private float idolKnockbackStrengthVert = 12f;

	[SerializeField]
	private float idolKnockbackStrengthHoriz = 15f;

	[Space]
	[SerializeField]
	private SoundBankPlayer PlayerGainGuardianSFX;

	[SerializeField]
	private SoundBankPlayer PlayerLostGuardianSFX;

	[SerializeField]
	private SoundBankPlayer ObserverGainGuardianSFX;

	private NetPlayer guardianPlayer;

	private NetPlayer _previousGuardian;

	private int currentIdol = -1;

	private int idolMoveCount;

	private List<Transform> _sortedIdolPositions = new List<Transform>();

	private float _currentActivationTime = -1f;

	private float _lastTappedTime;

	private bool _progressing;

	private float _idolActivationDisplay;

	private bool _zoneIsActive;

	private bool _zoneStateChanged;
}
