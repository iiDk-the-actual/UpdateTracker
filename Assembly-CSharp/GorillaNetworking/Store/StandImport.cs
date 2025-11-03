using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GorillaNetworking.Store
{
	public class StandImport
	{
		public void DecomposeFromTitleDataString(string data)
		{
			string[] array = data.Split("\\n", StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				this.DecomposeStandDataTitleData(array[i]);
			}
		}

		public void DecomposeStandDataTitleData(string dataString)
		{
			string[] array = dataString.Split("\\t", StringSplitOptions.None);
			if (array.Length == 5)
			{
				this.standData.Add(new StandTypeData(array));
				return;
			}
			if (array.Length == 4)
			{
				this.standData.Add(new StandTypeData(array));
				return;
			}
			string text = "";
			foreach (string text2 in array)
			{
				text = text + text2 + "|";
			}
			Debug.LogError("Store Importer Data String is not valid : " + text);
		}

		public void DeserializeFromJSON(string JSONString)
		{
			this.standData = JsonConvert.DeserializeObject<List<StandTypeData>>(JSONString);
		}

		public void DecomposeStandData(string dataString)
		{
			string[] array = dataString.Split('\t', StringSplitOptions.None);
			if (array.Length == 5)
			{
				this.standData.Add(new StandTypeData(array));
				return;
			}
			if (array.Length == 4)
			{
				this.standData.Add(new StandTypeData(array));
				return;
			}
			string text = "";
			foreach (string text2 in array)
			{
				text = text + text2 + "|";
			}
			Debug.LogError("Store Importer Data String is not valid : " + text);
		}

		public List<StandTypeData> standData = new List<StandTypeData>();
	}
}
