using System;
using UnityEngine;

namespace GorillaTag.CosmeticSystem
{
	[CreateAssetMenu(fileName = "UntitledSeason_SeasonSO", menuName = "- Gorilla Tag/SeasonSO", order = 0)]
	public class SeasonSO : ScriptableObject
	{
		[Delayed]
		public GTDateTimeSerializable releaseDate = new GTDateTimeSerializable(1);

		[Delayed]
		public string seasonName;
	}
}
