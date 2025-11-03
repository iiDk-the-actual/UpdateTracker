using System;
using System.Collections;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class GorillaTagCompetitiveRankCosmetic : MonoBehaviour, ISpawnable
{
	public bool IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	public void OnSpawn(VRRig rig)
	{
		if (this.forWardrobe && !this.myRig)
		{
			this.TryGetRig();
			return;
		}
		this.myRig = rig;
		this.myRig.OnRankedSubtierChanged += this.OnRankedScoreChanged;
		this.OnRankedScoreChanged(this.myRig.GetCurrentRankedSubTier(false), this.myRig.GetCurrentRankedSubTier(true));
	}

	public void OnDespawn()
	{
	}

	private void OnEnable()
	{
		if (this.forWardrobe)
		{
			this.UpdateDisplayedCosmetic(-1, -1);
			if (!this.TryGetRig())
			{
				base.StartCoroutine(this.DoFindRig());
			}
		}
	}

	private void OnDisable()
	{
		if (this.forWardrobe && this.myRig)
		{
			this.myRig.OnRankedSubtierChanged -= this.OnRankedScoreChanged;
			this.myRig = null;
		}
	}

	private IEnumerator DoFindRig()
	{
		WaitForSeconds intervalWait = new WaitForSeconds(0.1f);
		while (!this.TryGetRig())
		{
			yield return intervalWait;
		}
		yield break;
	}

	private bool TryGetRig()
	{
		GorillaTagger instance = GorillaTagger.Instance;
		this.myRig = ((instance != null) ? instance.offlineVRRig : null);
		if (this.myRig)
		{
			this.myRig.OnRankedSubtierChanged += this.OnRankedScoreChanged;
			this.OnRankedScoreChanged(this.myRig.GetCurrentRankedSubTier(false), this.myRig.GetCurrentRankedSubTier(true));
			return true;
		}
		return false;
	}

	private void OnRankedScoreChanged(int questRank, int pcRank)
	{
		this.UpdateDisplayedCosmetic(questRank, pcRank);
	}

	private void UpdateDisplayedCosmetic(int questRank, int pcRank)
	{
		if (this.rankCosmetics == null)
		{
			return;
		}
		int num = (this.usePCELO ? pcRank : questRank);
		if (num <= 0)
		{
			num = 0;
		}
		for (int i = 0; i < this.rankCosmetics.Length; i++)
		{
			this.rankCosmetics[i].SetActive(i == num);
		}
	}

	[Tooltip("If enabled, display PC rank. Otherwise, display Quest rank")]
	[SerializeField]
	private bool usePCELO;

	[SerializeField]
	private bool forWardrobe;

	[SerializeField]
	private VRRig myRig;

	[SerializeField]
	private GameObject[] rankCosmetics;
}
