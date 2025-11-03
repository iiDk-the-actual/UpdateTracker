using System;

public abstract class RankedMultiplayerStatistic
{
	public override string ToString()
	{
		return string.Empty;
	}

	public abstract void Load();

	protected abstract void Save();

	public abstract bool TrySetValue(string valAsString);

	public virtual string WriteToJson()
	{
		return string.Format("{{{0}:\"{1}\"}}", this.name, this.ToString());
	}

	public bool IsValid { get; protected set; }

	public RankedMultiplayerStatistic(string n, RankedMultiplayerStatistic.SerializationType sType = RankedMultiplayerStatistic.SerializationType.Mothership)
	{
		this.serializationType = sType;
		this.name = n;
		this.IsValid = this.serializationType != RankedMultiplayerStatistic.SerializationType.Mothership;
		RankedMultiplayerStatistic.SerializationType serializationType = this.serializationType;
	}

	protected virtual void HandleUserDataSetSuccess(string keyName)
	{
		if (keyName == this.name)
		{
			this.IsValid = true;
		}
	}

	protected virtual void HandleUserDataGetSuccess(string keyName, string value)
	{
		if (keyName == this.name)
		{
			if (this.TrySetValue(value))
			{
				this.IsValid = true;
				return;
			}
			this.Save();
		}
	}

	protected void HandleUserDataGetFailure(string keyName)
	{
		if (keyName == this.name)
		{
			this.Save();
			this.IsValid = true;
		}
	}

	protected void HandleUserDataSetFailure(string keyName)
	{
		if (keyName == this.name)
		{
			this.Save();
		}
	}

	protected RankedMultiplayerStatistic.SerializationType serializationType = RankedMultiplayerStatistic.SerializationType.Mothership;

	public string name;

	public enum SerializationType
	{
		None,
		Mothership,
		PlayerPrefs
	}
}
