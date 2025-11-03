using System;
using System.Collections;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using TMPro;
using UnityEngine;

public class RotatingQuestBadge : MonoBehaviour, ISpawnable
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
		this.myRig.OnQuestScoreChanged += this.OnProgressScoreChanged;
		this.OnProgressScoreChanged(this.myRig.GetCurrentQuestScore());
	}

	public void OnDespawn()
	{
	}

	private void OnEnable()
	{
		if (this.forWardrobe)
		{
			this.SetBadgeLevel(-1);
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
			this.myRig.OnQuestScoreChanged -= this.OnProgressScoreChanged;
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
			this.myRig.OnQuestScoreChanged += this.OnProgressScoreChanged;
			this.OnProgressScoreChanged(this.myRig.GetCurrentQuestScore());
			return true;
		}
		return false;
	}

	private void OnProgressScoreChanged(int score)
	{
		score = Mathf.Clamp(score, 0, 99999);
		this.displayField.text = score.ToString();
		this.UpdateBadge(score);
	}

	private void UpdateBadge(int score)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < this.badgeLevels.Length; i++)
		{
			if (this.badgeLevels[i].requiredPoints <= score && this.badgeLevels[i].requiredPoints > num)
			{
				num = this.badgeLevels[i].requiredPoints;
				num2 = i;
			}
		}
		this.SetBadgeLevel(num2);
	}

	private void SetBadgeLevel(int level)
	{
		level = Mathf.Clamp(level, 0, this.badgeLevels.Length - 1);
		for (int i = 0; i < this.badgeLevels.Length; i++)
		{
			this.badgeLevels[i].badge.SetActive(i == level);
		}
	}

	[SerializeField]
	private TextMeshPro displayField;

	[SerializeField]
	private bool forWardrobe;

	[SerializeField]
	private VRRig myRig;

	[SerializeField]
	private RotatingQuestBadge.BadgeLevel[] badgeLevels;

	[Serializable]
	public struct BadgeLevel
	{
		public GameObject badge;

		public int requiredPoints;
	}
}
