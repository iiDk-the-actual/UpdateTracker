using System;
using UnityEngine;

[Serializable]
public struct FrameStamp
{
	public int framesElapsed
	{
		get
		{
			return Time.frameCount - this._lastFrame;
		}
	}

	public static FrameStamp Now()
	{
		return new FrameStamp
		{
			_lastFrame = Time.frameCount
		};
	}

	public override string ToString()
	{
		return string.Format("{0} frames elapsed", this.framesElapsed);
	}

	public override int GetHashCode()
	{
		return StaticHash.Compute(this._lastFrame);
	}

	public static implicit operator int(FrameStamp fs)
	{
		return fs.framesElapsed;
	}

	public static implicit operator FrameStamp(int framesElapsed)
	{
		return new FrameStamp
		{
			_lastFrame = Time.frameCount - framesElapsed
		};
	}

	private int _lastFrame;
}
