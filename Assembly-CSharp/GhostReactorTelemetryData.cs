using System;
using System.Collections.Generic;

public struct GhostReactorTelemetryData
{
	public string EventName;

	public string[] CustomTags;

	public Dictionary<string, object> BodyData;
}
