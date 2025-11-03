using System;
using System.Collections.Generic;

[Serializable]
public class ScenePerformanceData
{
	public ScenePerformanceData(string mapName, int gorillaCount, int droppedFrames, int msHigh, int medianMS, int medianFPS, int medianDrawCalls, List<int> msCaptures)
	{
		this._mapName = mapName;
		this._gorillaCount = gorillaCount;
		this._droppedFrames = droppedFrames;
		this._msHigh = msHigh;
		this._medianMS = medianMS;
		this._medianFPS = medianFPS;
		this._medianDrawCallCount = medianDrawCalls;
		this._msCaptures = new List<int>(msCaptures);
	}

	public string _mapName;

	public int _gorillaCount;

	public int _droppedFrames;

	public int _msHigh;

	public int _medianMS;

	public int _medianFPS;

	public int _medianDrawCallCount;

	public List<int> _msCaptures;
}
