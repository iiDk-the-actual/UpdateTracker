using System;

public struct GameBallId
{
	public GameBallId(int index)
	{
		this.index = index;
	}

	public bool IsValid()
	{
		return this.index != -1;
	}

	public static bool operator ==(GameBallId obj1, GameBallId obj2)
	{
		return obj1.index == obj2.index;
	}

	public static bool operator !=(GameBallId obj1, GameBallId obj2)
	{
		return obj1.index != obj2.index;
	}

	public override bool Equals(object obj)
	{
		GameBallId gameBallId = (GameBallId)obj;
		return this.index == gameBallId.index;
	}

	public override int GetHashCode()
	{
		return this.index.GetHashCode();
	}

	public static GameBallId Invalid = new GameBallId(-1);

	public int index;
}
