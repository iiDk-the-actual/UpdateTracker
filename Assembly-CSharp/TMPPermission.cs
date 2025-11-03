using System;
using Newtonsoft.Json;

[Serializable]
public class TMPPermission
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("enabled")]
	public bool Enabled { get; set; }

	[JsonProperty("managedBy")]
	public ManagedBy ManagedBy { get; set; }
}
