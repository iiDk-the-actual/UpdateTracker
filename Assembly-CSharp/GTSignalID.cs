using System;

[Serializable]
public struct GTSignalID : IEquatable<GTSignalID>, IEquatable<int>
{
	public override bool Equals(object obj)
	{
		if (obj is GTSignalID)
		{
			GTSignalID gtsignalID = (GTSignalID)obj;
			return this.Equals(gtsignalID);
		}
		if (obj is int)
		{
			int num = (int)obj;
			return this.Equals(num);
		}
		return false;
	}

	public bool Equals(GTSignalID other)
	{
		return this._id == other._id;
	}

	public bool Equals(int other)
	{
		return this._id == other;
	}

	public override int GetHashCode()
	{
		return this._id;
	}

	public static bool operator ==(GTSignalID x, GTSignalID y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(GTSignalID x, GTSignalID y)
	{
		return !x.Equals(y);
	}

	public static implicit operator int(GTSignalID sid)
	{
		return sid._id;
	}

	public static implicit operator GTSignalID(string s)
	{
		return new GTSignalID
		{
			_id = GTSignal.ComputeID(s)
		};
	}

	private int _id;
}
