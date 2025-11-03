using System;
using System.Collections.Generic;
using LitJson;
using Newtonsoft.Json;
using UnityEngine;

namespace GorillaNetworking.Store
{
	public class StoreUpdateEvent
	{
		public StoreUpdateEvent()
		{
		}

		public StoreUpdateEvent(string pedestalID, string itemName, DateTime startTimeUTC, DateTime endTimeUTC)
		{
			this.PedestalID = pedestalID;
			this.ItemName = itemName;
			this.StartTimeUTC = startTimeUTC;
			this.EndTimeUTC = endTimeUTC;
		}

		public static string SerializeAsJSon(StoreUpdateEvent storeEvent)
		{
			return JsonUtility.ToJson(storeEvent);
		}

		public static string SerializeArrayAsJSon(StoreUpdateEvent[] storeEvents)
		{
			return JsonConvert.SerializeObject(storeEvents);
		}

		public static StoreUpdateEvent DeserializeFromJSon(string json)
		{
			return JsonUtility.FromJson<StoreUpdateEvent>(json);
		}

		public static StoreUpdateEvent[] DeserializeFromJSonArray(string json)
		{
			List<StoreUpdateEvent> list = JsonMapper.ToObject<List<StoreUpdateEvent>>(json);
			list.Sort((StoreUpdateEvent x, StoreUpdateEvent y) => x.StartTimeUTC.CompareTo(y.StartTimeUTC));
			return list.ToArray();
		}

		public static List<StoreUpdateEvent> DeserializeFromJSonList(string json)
		{
			List<StoreUpdateEvent> list = JsonMapper.ToObject<List<StoreUpdateEvent>>(json);
			list.Sort((StoreUpdateEvent x, StoreUpdateEvent y) => x.StartTimeUTC.CompareTo(y.StartTimeUTC));
			return list;
		}

		public string PedestalID;

		public string ItemName;

		public DateTime StartTimeUTC;

		public DateTime EndTimeUTC;
	}
}
