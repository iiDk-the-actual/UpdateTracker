using System;

public struct GameEntityId
{
	public bool IsValid()
	{
		return this.index != -1;
	}

	public static bool operator ==(GameEntityId obj1, GameEntityId obj2)
	{
		return obj1.index == obj2.index;
	}

	public static bool operator !=(GameEntityId obj1, GameEntityId obj2)
	{
		return obj1.index != obj2.index;
	}

	public override bool Equals(object obj)
	{
		GameEntityId gameEntityId = (GameEntityId)obj;
		return this.index == gameEntityId.index;
	}

	public override int GetHashCode()
	{
		return this.index.GetHashCode();
	}

	public static GameEntityId Invalid = new GameEntityId
	{
		index = -1
	};

	public int index;
}
