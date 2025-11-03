using System;

public struct TimeSince
{
	public double secondsElapsed
	{
		get
		{
			double totalSeconds = (DateTime.UtcNow - this._dt).TotalSeconds;
			if (totalSeconds <= 2147483647.0)
			{
				return totalSeconds;
			}
			return 2147483647.0;
		}
	}

	public float secondsElapsedFloat
	{
		get
		{
			return (float)this.secondsElapsed;
		}
	}

	public int secondsElapsedInt
	{
		get
		{
			return (int)this.secondsElapsed;
		}
	}

	public uint secondsElapsedUint
	{
		get
		{
			return (uint)this.secondsElapsed;
		}
	}

	public long secondsElapsedLong
	{
		get
		{
			return (long)this.secondsElapsed;
		}
	}

	public TimeSpan secondsElapsedSpan
	{
		get
		{
			return TimeSpan.FromSeconds(this.secondsElapsed);
		}
	}

	public TimeSince(DateTime dt)
	{
		this._dt = dt;
	}

	public TimeSince(int elapsed)
	{
		this._dt = DateTime.UtcNow.AddSeconds((double)(-(double)elapsed));
	}

	public TimeSince(uint elapsed)
	{
		this._dt = DateTime.UtcNow.AddSeconds(-1.0 * elapsed);
	}

	public TimeSince(float elapsed)
	{
		this._dt = DateTime.UtcNow.AddSeconds((double)(-(double)elapsed));
	}

	public TimeSince(double elapsed)
	{
		this._dt = DateTime.UtcNow.AddSeconds(-elapsed);
	}

	public TimeSince(long elapsed)
	{
		this._dt = DateTime.UtcNow.AddSeconds((double)(-(double)elapsed));
	}

	public TimeSince(TimeSpan elapsed)
	{
		this._dt = DateTime.UtcNow.Add(-elapsed);
	}

	public bool HasElapsed(int seconds)
	{
		return this.secondsElapsedInt >= seconds;
	}

	public bool HasElapsed(uint seconds)
	{
		return this.secondsElapsedUint >= seconds;
	}

	public bool HasElapsed(float seconds)
	{
		return this.secondsElapsedFloat >= seconds;
	}

	public bool HasElapsed(double seconds)
	{
		return this.secondsElapsed >= seconds;
	}

	public bool HasElapsed(long seconds)
	{
		return this.secondsElapsedLong >= seconds;
	}

	public bool HasElapsed(TimeSpan seconds)
	{
		return this.secondsElapsedSpan >= seconds;
	}

	public void Reset()
	{
		this._dt = DateTime.UtcNow;
	}

	public bool HasElapsed(int seconds, bool resetOnElapsed)
	{
		if (!resetOnElapsed)
		{
			return this.secondsElapsedInt >= seconds;
		}
		if (this.secondsElapsedInt < seconds)
		{
			return false;
		}
		this.Reset();
		return true;
	}

	public bool HasElapsed(uint seconds, bool resetOnElapsed)
	{
		if (!resetOnElapsed)
		{
			return this.secondsElapsedUint >= seconds;
		}
		if (this.secondsElapsedUint < seconds)
		{
			return false;
		}
		this.Reset();
		return true;
	}

	public bool HasElapsed(float seconds, bool resetOnElapsed)
	{
		if (!resetOnElapsed)
		{
			return this.secondsElapsedFloat >= seconds;
		}
		if (this.secondsElapsedFloat < seconds)
		{
			return false;
		}
		this.Reset();
		return true;
	}

	public bool HasElapsed(double seconds, bool resetOnElapsed)
	{
		if (!resetOnElapsed)
		{
			return this.secondsElapsed >= seconds;
		}
		if (this.secondsElapsed < seconds)
		{
			return false;
		}
		this.Reset();
		return true;
	}

	public bool HasElapsed(long seconds, bool resetOnElapsed)
	{
		if (!resetOnElapsed)
		{
			return this.secondsElapsedLong >= seconds;
		}
		if (this.secondsElapsedLong < seconds)
		{
			return false;
		}
		this.Reset();
		return true;
	}

	public bool HasElapsed(TimeSpan seconds, bool resetOnElapsed)
	{
		if (!resetOnElapsed)
		{
			return this.secondsElapsedSpan >= seconds;
		}
		if (this.secondsElapsedSpan < seconds)
		{
			return false;
		}
		this.Reset();
		return true;
	}

	public override string ToString()
	{
		return string.Format("{0:F3} seconds since {{{1:s}", this.secondsElapsed, this._dt);
	}

	public override int GetHashCode()
	{
		return StaticHash.Compute(this._dt);
	}

	public static TimeSince Now()
	{
		return new TimeSince(DateTime.UtcNow);
	}

	public static implicit operator long(TimeSince ts)
	{
		return ts.secondsElapsedLong;
	}

	public static implicit operator double(TimeSince ts)
	{
		return ts.secondsElapsed;
	}

	public static implicit operator float(TimeSince ts)
	{
		return ts.secondsElapsedFloat;
	}

	public static implicit operator int(TimeSince ts)
	{
		return ts.secondsElapsedInt;
	}

	public static implicit operator uint(TimeSince ts)
	{
		return ts.secondsElapsedUint;
	}

	public static implicit operator TimeSpan(TimeSince ts)
	{
		return ts.secondsElapsedSpan;
	}

	public static implicit operator TimeSince(int elapsed)
	{
		return new TimeSince(elapsed);
	}

	public static implicit operator TimeSince(uint elapsed)
	{
		return new TimeSince(elapsed);
	}

	public static implicit operator TimeSince(float elapsed)
	{
		return new TimeSince(elapsed);
	}

	public static implicit operator TimeSince(double elapsed)
	{
		return new TimeSince(elapsed);
	}

	public static implicit operator TimeSince(long elapsed)
	{
		return new TimeSince(elapsed);
	}

	public static implicit operator TimeSince(TimeSpan elapsed)
	{
		return new TimeSince(elapsed);
	}

	public static implicit operator TimeSince(DateTime dt)
	{
		return new TimeSince(dt);
	}

	private DateTime _dt;

	private const double INT32_MAX = 2147483647.0;
}
