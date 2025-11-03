using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuilderTableSerializationConfig", menuName = "Gorilla Tag/Builder/Serialization", order = 0)]
public class BuilderTableSerializationConfig : ScriptableObject
{
	public string tableConfigurationKey;

	public string titleDataKey;

	public string startingMapConfigKey;

	public List<string> scanSlotMothershipKeys;

	public string scanSlotDevKey;

	public string publishedScanMothershipKey;

	public string timeAppend;

	public string playfabScanKey;

	public string sharedBlocksApiBaseURL;

	public string recentVotesPrefsKey;

	public string localMapsPrefsKey;
}
