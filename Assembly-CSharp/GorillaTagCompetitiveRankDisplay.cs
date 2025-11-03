using System;
using TMPro;
using UnityEngine;

public class GorillaTagCompetitiveRankDisplay : MonoBehaviour
{
	private void OnEnable()
	{
		VRRig.LocalRig.OnRankedSubtierChanged += this.HandleRankedSubtierChanged;
		this.HandleRankedSubtierChanged(0, 0);
	}

	private void OnDisable()
	{
		VRRig.LocalRig.OnRankedSubtierChanged -= this.HandleRankedSubtierChanged;
	}

	public void HandleRankedSubtierChanged(int questSubTier, int pcSubTier)
	{
		float currentELO = RankedProgressionManager.Instance.GetCurrentELO();
		int progressionRankIndex = RankedProgressionManager.Instance.GetProgressionRankIndex(currentELO);
		this.UpdateRankIcons(progressionRankIndex);
		this.UpdateRankProgress(RankedProgressionManager.Instance.GetProgressionRankProgress());
	}

	private void UpdateRankIcons(int currentRank)
	{
		this.currentRankSprite.sprite = RankedProgressionManager.Instance.GetProgressionRankIcon(currentRank);
		this.currentRank_Name.text = RankedProgressionManager.Instance.GetProgressionRankName().ToUpper();
		bool flag = currentRank < RankedProgressionManager.Instance.MaxRank;
		bool flag2 = currentRank > 0;
		this.nextRankSprite.gameObject.SetActive(flag);
		this.nextText.gameObject.SetActive(flag);
		this.nextRank_Name.gameObject.SetActive(flag);
		if (flag)
		{
			this.nextRankSprite.sprite = RankedProgressionManager.Instance.GetNextProgressionRankIcon(currentRank);
			this.nextRank_Name.text = RankedProgressionManager.Instance.GetNextProgressionRankName(currentRank).ToUpper();
		}
		this.prevRankSprite.gameObject.SetActive(flag2);
		this.prevText.gameObject.SetActive(flag2);
		this.prevRank_Name.gameObject.SetActive(flag2);
		if (flag2)
		{
			this.prevRankSprite.sprite = RankedProgressionManager.Instance.GetPrevProgressionRankIcon(currentRank);
			this.prevRank_Name.text = RankedProgressionManager.Instance.GetPrevProgressionRankName(currentRank).ToUpper();
		}
	}

	private void UpdateRankProgress(float percent)
	{
		percent = Mathf.Clamp01(percent);
		Vector2 size = this.progressBar.size;
		size.x = this.progressBarSize * percent;
		this.progressBar.size = size;
	}

	[SerializeField]
	private SpriteRenderer progressBar;

	[SerializeField]
	private float progressBarSize = 100f;

	[SerializeField]
	private SpriteRenderer currentRankSprite;

	[SerializeField]
	private SpriteRenderer prevRankSprite;

	[SerializeField]
	private SpriteRenderer nextRankSprite;

	[SerializeField]
	private TextMeshPro currentRank_Name;

	[SerializeField]
	private TextMeshPro prevText;

	[SerializeField]
	private TextMeshPro nextText;

	[SerializeField]
	private TextMeshPro prevRank_Name;

	[SerializeField]
	private TextMeshPro nextRank_Name;
}
