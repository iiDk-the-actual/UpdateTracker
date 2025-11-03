using System;
using System.Collections.Generic;

public struct TelemetryData
{
	public string EventName;

	public string[] CustomTags;

	public Dictionary<string, string> BodyData;
}
