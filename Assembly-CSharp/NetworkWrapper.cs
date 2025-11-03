using System;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.UI;

public class NetworkWrapper : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void AutoInstantiate()
	{
		Object.DontDestroyOnLoad(Object.Instantiate<GameObject>(Resources.Load<GameObject>("P_NetworkWrapper")));
	}

	private void Awake()
	{
		if (this.titleRef != null)
		{
			this.titleRef.text = "PUN";
		}
		this.activeNetworkSystem = base.gameObject.AddComponent<NetworkSystemPUN>();
		this.activeNetworkSystem.AddVoiceSettings(this.VoiceSettings);
		this.activeNetworkSystem.config = this.netSysConfig;
		this.activeNetworkSystem.regionNames = this.networkRegionNames;
		this.activeNetworkSystem.OnPlayerJoined += this.UpdatePlayerCountWrapper;
		this.activeNetworkSystem.OnPlayerLeft += this.UpdatePlayerCountWrapper;
		this.activeNetworkSystem.OnMultiplayerStarted += this.UpdatePlayerCount;
		this.activeNetworkSystem.OnReturnedToSinglePlayer += this.UpdatePlayerCount;
		Debug.Log("<color=green>initialize Network System</color>");
		this.activeNetworkSystem.Initialise();
	}

	private void UpdatePlayerCountWrapper(NetPlayer player)
	{
		this.UpdatePlayerCount();
	}

	private void UpdatePlayerCount()
	{
		if (this.playerCountTextRef == null)
		{
			return;
		}
		if (!this.activeNetworkSystem.IsOnline)
		{
			this.playerCountTextRef.text = string.Format("0/{0}", this.netSysConfig.MaxPlayerCount);
			Debug.Log("Player count updated");
			return;
		}
		Debug.Log("Player count not updated");
		this.playerCountTextRef.text = string.Format("{0}/{1}", this.activeNetworkSystem.AllNetPlayers.Length, this.netSysConfig.MaxPlayerCount);
	}

	[HideInInspector]
	public NetworkSystem activeNetworkSystem;

	public Text titleRef;

	[Header("NetSys settings")]
	public NetworkSystemConfig netSysConfig;

	public string[] networkRegionNames;

	public string[] devNetworkRegionNames;

	[Header("Debug output refs")]
	public Text stateTextRef;

	public Text playerCountTextRef;

	[SerializeField]
	private SO_NetworkVoiceSettings VoiceSettings;

	private const string WrapperResourcePath = "P_NetworkWrapper";
}
