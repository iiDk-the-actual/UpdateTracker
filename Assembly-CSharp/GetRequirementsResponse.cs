using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class GetRequirementsResponse
{
	[JsonProperty("age")]
	public int? Age { get; set; }

	public int? PlatformMinimumAge { get; set; }

	[JsonProperty("ageStatus")]
	public SessionStatus AgeStatus { get; set; }

	[JsonProperty("digitalContentAge")]
	public int DigitalConsentAge { get; set; }

	[JsonProperty("minimumAge")]
	public int MinimumAge { get; set; }

	[JsonProperty("civilAge")]
	public int CivilAge { get; set; }

	[JsonProperty("approvedAgeCollectionMethods")]
	public List<ApprovedAgeCollectionMethods> ApprovedAgeCollectionMethods { get; set; }
}
