using System;
using UnityEngine;

public class FortuneResults : ScriptableObject
{
	private void OnValidate()
	{
		this.totalChance = 0f;
		for (int i = 0; i < this.fortuneResults.Length; i++)
		{
			this.totalChance += this.fortuneResults[i].weightedChance;
		}
	}

	public FortuneResults.FortuneResult GetResult()
	{
		float num = Random.Range(0f, this.totalChance);
		int i = 0;
		while (i < this.fortuneResults.Length)
		{
			FortuneResults.FortuneCategory fortuneCategory = this.fortuneResults[i];
			if (num <= fortuneCategory.weightedChance)
			{
				if (fortuneCategory.textResults.Length == 0)
				{
					return new FortuneResults.FortuneResult(FortuneResults.FortuneCategoryType.Invalid, -1);
				}
				int num2 = Random.Range(0, fortuneCategory.textResults.Length);
				return new FortuneResults.FortuneResult(fortuneCategory.fortuneType, num2);
			}
			else
			{
				num -= fortuneCategory.weightedChance;
				i++;
			}
		}
		return new FortuneResults.FortuneResult(FortuneResults.FortuneCategoryType.Invalid, -1);
	}

	public string GetResultText(FortuneResults.FortuneResult result)
	{
		for (int i = 0; i < this.fortuneResults.Length; i++)
		{
			if (this.fortuneResults[i].fortuneType == result.fortuneType && result.resultIndex >= 0 && result.resultIndex < this.fortuneResults[i].textResults.Length)
			{
				return this.fortuneResults[i].textResults[result.resultIndex];
			}
		}
		return "!! Invalid Fortune !!";
	}

	[SerializeField]
	private FortuneResults.FortuneCategory[] fortuneResults;

	[SerializeField]
	private float totalChance;

	public enum FortuneCategoryType
	{
		Invalid,
		Positive,
		Neutral,
		Negative,
		Seasonal
	}

	[Serializable]
	public struct FortuneCategory
	{
		public FortuneResults.FortuneCategoryType fortuneType;

		public float weightedChance;

		public string[] textResults;
	}

	public struct FortuneResult
	{
		public FortuneResult(FortuneResults.FortuneCategoryType fortuneType, int resultIndex)
		{
			this.fortuneType = fortuneType;
			this.resultIndex = resultIndex;
		}

		public FortuneResults.FortuneCategoryType fortuneType;

		public int resultIndex;
	}
}
