using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class SIResourceDeposit : MonoBehaviour, ISIResourceDeposit
{
	public bool IsAuthority
	{
		get
		{
			return this.SIManager.gameEntityManager.IsAuthority();
		}
	}

	public SuperInfectionManager SIManager
	{
		get
		{
			return this.superInfection.siManager;
		}
	}

	private void OnEnable()
	{
		if (this._displayResources == null || this._displayResources.Count == 0)
		{
			List<SIResource> resourcePrefabs = this.superInfection.ResourcePrefabs;
			if (resourcePrefabs != null && resourcePrefabs.Count > 0)
			{
				this._displayResources = new List<GameObject>();
				for (int i = 0; i < Mathf.Min(resourcePrefabs.Count, this.resourceDisplays.Length); i++)
				{
					GameObject gameObject = resourcePrefabs[i].gameObject;
					bool activeSelf = gameObject.activeSelf;
					try
					{
						if (activeSelf)
						{
							gameObject.SetActive(false);
						}
						GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, this.resourceDisplays[i].transform);
						gameObject2.transform.localScale = new Vector3(0.27f, 0.27f, 0.27f);
						this._displayResources.Add(gameObject2);
						foreach (MonoBehaviour monoBehaviour in gameObject2.GetComponentsInChildren<MonoBehaviour>(true))
						{
							monoBehaviour.enabled = false;
							Object.Destroy(monoBehaviour);
						}
						Rigidbody component = gameObject2.GetComponent<Rigidbody>();
						if (component != null)
						{
							Object.Destroy(component);
						}
						gameObject2.SetLayerRecursively(UnityLayer.Default);
						gameObject2.SetActive(true);
					}
					finally
					{
						if (activeSelf)
						{
							gameObject.SetActive(true);
						}
					}
				}
			}
		}
	}

	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.netPlayer != null)
		{
			stream.SendNext(this.netPlayer.ActorNr);
		}
		else
		{
			stream.SendNext(-1);
		}
		stream.SendNext((int)this.netResourceType);
		stream.SendNext((int)this.netLimitedDepositType);
		stream.SendNext(this.netShowPopup);
		this.netShowPopup = false;
	}

	public void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		this.netPlayer = SIPlayer.Get((int)stream.ReceiveNext());
		this.netResourceType = (SIResource.ResourceType)((int)stream.ReceiveNext());
		this.netLimitedDepositType = (SIResource.LimitedDepositType)((int)stream.ReceiveNext());
		if ((bool)stream.ReceiveNext())
		{
			this.LocalShowPopup(this.netPlayer, this.netResourceType, this.netLimitedDepositType);
		}
	}

	private void LocalShowPopup(SIPlayer player, SIResource.ResourceType resourceType, SIResource.LimitedDepositType limitedDepositType)
	{
		if (limitedDepositType == SIResource.LimitedDepositType.None)
		{
			this.depositBin.SetActive(true);
		}
		this.popupScreen.EnableAndResetTimer();
		this.depositText.text = string.Format("{0} COLLECTED {1}\n(TOTAL {2})", player.gamePlayer.rig.Creator.SanitizedNickName, resourceType.GetName<SIResource.ResourceType>(), player.GetResourceAmount(resourceType));
		this.depositImage.sprite = ((resourceType == SIResource.ResourceType.TechPoint) ? this.resourceImageSprites[0] : this.resourceImageSprites[1]);
	}

	public void ResourceDeposited(SIResource resource)
	{
		bool flag = false;
		if (resource.lastPlayerHeld.gamePlayer.IsLocal() && !resource.localDeposited)
		{
			this.AuthShowPopup(resource);
			resource.HandleDepositLocal(resource.lastPlayerHeld);
			resource.lastPlayerHeld.GatherResource(resource.type, resource.limitedDepositType, 1);
			this.superInfection.siManager.CallRPC(SuperInfectionManager.ClientToAuthorityRPC.ResourceDepositDeposited, new object[]
			{
				resource.myGameEntity.GetNetId(),
				this.index
			});
			flag = true;
		}
		if (this.superInfection.siManager.gameEntityManager.IsAuthority())
		{
			resource.HandleDepositAuth(resource.lastPlayerHeld);
			this.superInfection.siManager.gameEntityManager.RequestDestroyItem(resource.myGameEntity.id);
			this.AuthShowPopup(resource);
			flag = true;
		}
		if (flag)
		{
			this.LocalShowPopup(resource.lastPlayerHeld, resource.type, resource.limitedDepositType);
		}
	}

	private void AuthShowPopup(SIResource resource)
	{
		this.netPlayer = resource.lastPlayerHeld;
		this.netResourceType = resource.type;
		this.netLimitedDepositType = resource.limitedDepositType;
		this.netShowPopup = true;
	}

	public int index;

	public Text depositText;

	public Image depositImage;

	public DisableGameObjectDelayed popupScreen;

	public SuperInfection superInfection;

	public Sprite[] resourceImageSprites;

	public GameObject depositBin;

	[SerializeField]
	private Transform[] resourceDisplays;

	public SIPlayer netPlayer;

	public SIResource.ResourceType netResourceType;

	public SIResource.LimitedDepositType netLimitedDepositType;

	private bool netShowPopup;

	public List<SIUIPlayerQuestDisplay> questDisplays;

	private List<GameObject> _displayResources;
}
