using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Photon.Pun;
using UnityEngine;

public abstract class ArcadeGame : MonoBehaviour
{
	protected virtual void Awake()
	{
		this.InitializeMemoryStreams();
	}

	public void InitializeMemoryStreams()
	{
		if (!this.memoryStreamsInitialized)
		{
			this.netStateMemStream = new MemoryStream(this.netStateBuffer, true);
			this.netStateMemStreamAlt = new MemoryStream(this.netStateBufferAlt, true);
			this.memoryStreamsInitialized = true;
		}
	}

	public void SetMachine(ArcadeMachine machine)
	{
		this.machine = machine;
	}

	protected bool getButtonState(int player, ArcadeButtons button)
	{
		return this.playerInputs[player].HasFlag(button);
	}

	public void OnInputStateChange(int player, ArcadeButtons buttons)
	{
		for (int i = 1; i < 256; i += i)
		{
			ArcadeButtons arcadeButtons = (ArcadeButtons)i;
			bool flag = buttons.HasFlag(arcadeButtons);
			bool flag2 = this.playerInputs[player].HasFlag(arcadeButtons);
			if (flag != flag2)
			{
				if (flag)
				{
					this.ButtonDown(player, arcadeButtons);
				}
				else
				{
					this.ButtonUp(player, arcadeButtons);
				}
			}
		}
		this.playerInputs[player] = buttons;
	}

	public abstract byte[] GetNetworkState();

	public abstract void SetNetworkState(byte[] obj);

	protected static void WrapNetState(object ns, MemoryStream stream)
	{
		if (stream == null)
		{
			Debug.LogWarning("Null MemoryStream passed to WrapNetState");
			return;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		stream.SetLength(0L);
		stream.Position = 0L;
		binaryFormatter.Serialize(stream, ns);
	}

	protected static object UnwrapNetState(byte[] b)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(b);
		memoryStream.Position = 0L;
		object obj = binaryFormatter.Deserialize(memoryStream);
		memoryStream.Close();
		return obj;
	}

	protected void SwapNetStateBuffersAndStreams()
	{
		byte[] array = this.netStateBufferAlt;
		byte[] array2 = this.netStateBuffer;
		this.netStateBuffer = array;
		this.netStateBufferAlt = array2;
		MemoryStream memoryStream = this.netStateMemStreamAlt;
		MemoryStream memoryStream2 = this.netStateMemStream;
		this.netStateMemStream = memoryStream;
		this.netStateMemStreamAlt = memoryStream2;
	}

	protected void PlaySound(int clipId, int prio = 3)
	{
		this.machine.PlaySound(clipId, prio);
	}

	protected bool IsPlayerLocallyControlled(int player)
	{
		return this.machine.IsPlayerLocallyControlled(player);
	}

	protected abstract void ButtonUp(int player, ArcadeButtons button);

	protected abstract void ButtonDown(int player, ArcadeButtons button);

	public abstract void OnTimeout();

	public virtual void ReadPlayerDataPUN(int player, PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public virtual void WritePlayerDataPUN(int player, PhotonStream stream, PhotonMessageInfo info)
	{
	}

	[SerializeField]
	public Vector2 Scale = new Vector2(1f, 1f);

	private ArcadeButtons[] playerInputs = new ArcadeButtons[4];

	public AudioClip[] audioClips;

	private ArcadeMachine machine;

	protected static int NetStateBufferSize = 512;

	protected byte[] netStateBuffer = new byte[ArcadeGame.NetStateBufferSize];

	protected byte[] netStateBufferAlt = new byte[ArcadeGame.NetStateBufferSize];

	protected MemoryStream netStateMemStream;

	protected MemoryStream netStateMemStreamAlt;

	public bool memoryStreamsInitialized;
}
