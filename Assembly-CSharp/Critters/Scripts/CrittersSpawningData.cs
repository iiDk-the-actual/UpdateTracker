using System;
using System.Collections.Generic;
using UnityEngine;

namespace Critters.Scripts
{
	public class CrittersSpawningData : MonoBehaviour
	{
		public void InitializeSpawnCollection()
		{
			for (int i = 0; i < this.SpawnParametersList.Count; i++)
			{
				for (int j = 0; j < this.SpawnParametersList[i].ChancesToSpawn; j++)
				{
					this.templateCollection.Add(i);
				}
			}
		}

		public int GetRandomTemplate()
		{
			int num = Random.Range(0, this.templateCollection.Count - 1);
			return this.templateCollection[num];
		}

		public List<CrittersSpawningData.CreatureSpawnParameters> SpawnParametersList;

		private List<int> templateCollection = new List<int>();

		[Serializable]
		public class CreatureSpawnParameters
		{
			public CritterTemplate Template;

			public int ChancesToSpawn;

			[HideInInspector]
			[NonSerialized]
			public int StartingIndex;
		}
	}
}
