using System;
using UnityEngine;

namespace GorillaNetworking.Store
{
	public class StandTypeData
	{
		public StandTypeData(string[] spawnData)
		{
			this.departmentID = spawnData[0];
			this.displayID = spawnData[1];
			this.standID = spawnData[2];
			this.bustType = spawnData[3];
			if (spawnData.Length == 5)
			{
				this.playFabID = spawnData[4];
			}
			Debug.Log(string.Concat(new string[] { "StoreStuff: StandTypeData: ", this.departmentID, "\n", this.displayID, "\n", this.standID, "\n", this.bustType, "\n", this.playFabID }));
		}

		public StandTypeData(string departmentID, string displayID, string standID, HeadModel_CosmeticStand.BustType bustType, string playFabID)
		{
			this.departmentID = departmentID;
			this.displayID = displayID;
			this.standID = standID;
			this.bustType = bustType.ToString();
			this.playFabID = playFabID;
		}

		public string departmentID = "";

		public string displayID = "";

		public string standID = "";

		public string bustType = "";

		public string playFabID = "";

		public enum EStandDataID
		{
			departmentID,
			displayID,
			standID,
			bustType,
			playFabID,
			Count
		}
	}
}
