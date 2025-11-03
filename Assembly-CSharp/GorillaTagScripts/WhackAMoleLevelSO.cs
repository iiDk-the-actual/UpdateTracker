using System;
using UnityEngine;

namespace GorillaTagScripts
{
	[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WhackAMoleLevelSetting", order = 1)]
	public class WhackAMoleLevelSO : ScriptableObject
	{
		public int GetMinScore(bool isCoop)
		{
			if (!isCoop)
			{
				return this.minScore;
			}
			return this.minScore * 2;
		}

		public int levelNumber;

		public float levelDuration;

		[Tooltip("For how long do the moles stay visible?")]
		public float showMoleDuration;

		[Tooltip("How fast we pick a random new mole?")]
		public float pickNextMoleTime;

		[Tooltip("Minimum score to get in order to be able to proceed to the next level")]
		[SerializeField]
		private int minScore;

		[Tooltip("Chance of each mole being a hazard mole at the start, and end, of the level.")]
		public Vector2 hazardMoleChance = new Vector2(0f, 0.5f);

		[Tooltip("Minimum number of moles selected as level progresses.")]
		public Vector2 minimumMoleCount = new Vector2(1f, 2f);

		[Tooltip("Minimum number of moles selected as level progresses.")]
		public Vector2 maximumMoleCount = new Vector2(1.5f, 3f);
	}
}
