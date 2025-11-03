using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
[Serializable]
public enum QuestCategory
{
	NONE,
	Social,
	Exploration,
	Gameplay,
	GameRound,
	Tag
}
