using System;
using Fusion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[NetworkBehaviourWeaved(128)]
public class ArcadeMachine : NetworkComponent
{
	protected override void Awake()
	{
		base.Awake();
		this.audioSource = base.GetComponent<AudioSource>();
	}

	protected override void Start()
	{
		base.Start();
		if (this.arcadeGame != null && this.arcadeGame.Scale.x > 0f && this.arcadeGame.Scale.y > 0f)
		{
			this.arcadeGameInstance = global::UnityEngine.Object.Instantiate<ArcadeGame>(this.arcadeGame, this.screen.transform);
			this.arcadeGameInstance.transform.localScale = new Vector3(1f / this.arcadeGameInstance.Scale.x, 1f / this.arcadeGameInstance.Scale.y, 1f);
			this.screen.forceRenderingOff = true;
			this.arcadeGameInstance.SetMachine(this);
		}
	}

	public void PlaySound(int soundId, int priority)
	{
		if (!this.audioSource.isPlaying || this.audioSourcePriority >= priority)
		{
			this.audioSource.GTStop();
			this.audioSourcePriority = priority;
			this.audioSource.clip = this.arcadeGameInstance.audioClips[soundId];
			this.audioSource.GTPlay();
			if (this.networkSynchronized && base.IsMine)
			{
				base.GetView.RPC("ArcadeGameInstance_OnPlaySound_RPC", RpcTarget.Others, new object[] { soundId });
			}
		}
	}

	public bool IsPlayerLocallyControlled(int player)
	{
		return this.sticks[player].heldByLocalPlayer;
	}

	internal override void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		base.OnEnable();
		for (int i = 0; i < this.sticks.Length; i++)
		{
			this.sticks[i].Init(this, i);
		}
	}

	internal override void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
	}

	[PunRPC]
	private void ArcadeGameInstance_OnPlaySound_RPC(int id, PhotonMessageInfo info)
	{
		if (!info.Sender.IsMasterClient || id > this.arcadeGameInstance.audioClips.Length || id < 0 || !this.soundCallLimit.CheckCallTime(Time.time))
		{
			return;
		}
		this.audioSource.GTStop();
		this.audioSource.clip = this.arcadeGameInstance.audioClips[id];
		this.audioSource.GTPlay();
	}

	public void OnJoystickStateChange(int player, ArcadeButtons buttons)
	{
		if (this.arcadeGameInstance != null)
		{
			this.arcadeGameInstance.OnInputStateChange(player, buttons);
		}
	}

	public bool IsControllerInUse(int player)
	{
		if (base.IsMine)
		{
			return this.playersPerJoystick[player] != null && Time.time < this.playerIdleTimeouts[player];
		}
		return (this.buttonsStateValue & (1 << player * 8)) != 0;
	}

	[Networked]
	[Capacity(128)]
	[NetworkedWeaved(0, 128)]
	[NetworkedWeavedArray(128, 1, typeof(ElementReaderWriterByte))]
	public unsafe NetworkArray<byte> Data
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing ArcadeMachine.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			return new NetworkArray<byte>((byte*)(this.Ptr + 0), 128, ElementReaderWriterByte.GetInstance());
		}
	}

	public override void WriteDataFusion()
	{
	}

	public override void ReadDataFusion()
	{
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public void ReadPlayerDataPUN(int player, PhotonStream stream, PhotonMessageInfo info)
	{
		this.arcadeGameInstance.ReadPlayerDataPUN(player, stream, info);
	}

	public void WritePlayerDataPUN(int player, PhotonStream stream, PhotonMessageInfo info)
	{
		this.arcadeGameInstance.WritePlayerDataPUN(player, stream, info);
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		NetworkBehaviourUtils.InitializeNetworkArray<byte>(this.Data, this._Data, "Data");
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		NetworkBehaviourUtils.CopyFromNetworkArray<byte>(this.Data, ref this._Data);
	}

	[SerializeField]
	private ArcadeGame arcadeGame;

	[SerializeField]
	private ArcadeMachineJoystick[] sticks;

	[SerializeField]
	private Renderer screen;

	[SerializeField]
	private bool networkSynchronized = true;

	[SerializeField]
	private CallLimiter soundCallLimit;

	private int buttonsStateValue;

	private AudioSource audioSource;

	private int audioSourcePriority;

	private ArcadeGame arcadeGameInstance;

	private Player[] playersPerJoystick = new Player[4];

	private float[] playerIdleTimeouts = new float[4];

	[WeaverGenerated]
	[SerializeField]
	[DefaultForProperty("Data", 0, 128)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private byte[] _Data;
}
