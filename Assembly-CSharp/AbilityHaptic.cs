using System;

[Serializable]
public class AbilityHaptic
{
	public void PlayIfHeldLocal(GameEntity gameEntity)
	{
		if (gameEntity == null || !gameEntity.IsHeldByLocalPlayer())
		{
			return;
		}
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(gameEntity.heldByActorNumber);
		if (gamePlayer == null)
		{
			return;
		}
		int num = gamePlayer.FindHandIndex(gameEntity.id);
		if (num == -1)
		{
			return;
		}
		GorillaTagger.Instance.StartVibration(GamePlayer.IsLeftHand(num), this.strength, this.duration);
	}

	public void PlayIfSnappedLocal(GameEntity gameEntity)
	{
		if (gameEntity == null || !gameEntity.IsSnappedByLocalPlayer())
		{
			return;
		}
		GameSnappable component = gameEntity.GetComponent<GameSnappable>();
		if (component == null)
		{
			return;
		}
		if (component.IsSnappedToLeftArm())
		{
			GorillaTagger.Instance.StartVibration(true, this.strength, this.duration);
		}
		if (component.IsSnappedToRightArm())
		{
			GorillaTagger.Instance.StartVibration(false, this.strength, this.duration);
		}
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(gameEntity.heldByActorNumber);
		if (gamePlayer == null)
		{
			return;
		}
		int num = gamePlayer.FindHandIndex(gameEntity.id);
		if (num == -1)
		{
			return;
		}
		GorillaTagger.Instance.StartVibration(GamePlayer.IsLeftHand(num), this.strength, this.duration);
	}

	public float strength = 0.2f;

	public float duration = 0.1f;
}
