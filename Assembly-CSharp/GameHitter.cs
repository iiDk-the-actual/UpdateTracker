using System;
using System.Collections.Generic;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

public class GameHitter : MonoBehaviour, IGameEntityComponent
{
	private void Awake()
	{
		this.components = new List<IGameHitter>(1);
		base.GetComponentsInChildren<IGameHitter>(this.components);
		this.attributes = base.GetComponent<GRAttributes>();
	}

	public void OnEntityInit()
	{
		GRTool component = base.GetComponent<GRTool>();
		if (component != null)
		{
			component.onToolUpgraded += this.OnToolUpgraded;
			this.OnToolUpgraded(component);
		}
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private void OnToolUpgraded(GRTool tool)
	{
		if (this.attributes.HasValueForAttribute(GRAttributeType.KnockbackMultiplier))
		{
			this.knockbackMultiplier = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.KnockbackMultiplier);
		}
	}

	public void ApplyHit(GameHitData hitData)
	{
		if (this.hitFx.hitSound != null)
		{
			this.hitFx.hitSound.Play(null);
		}
		if (this.hitFx.hitEffect != null)
		{
			this.hitFx.hitEffect.Stop();
			this.hitFx.hitEffect.Play();
		}
		for (int i = 0; i < this.components.Count; i++)
		{
			this.components[i].OnSuccessfulHit(hitData);
		}
		if (this.gameEntity.IsHeldByLocalPlayer())
		{
			this.PlayVibration(GorillaTagger.Instance.tapHapticStrength, 0.2f);
			GamePlayer gamePlayer = GamePlayer.GetGamePlayer(this.gameEntity.heldByActorNumber);
			if (gamePlayer != null)
			{
				int num = gamePlayer.FindHandIndex(this.gameEntity.id);
				if (num != -1)
				{
					GTPlayer.Instance.TempFreezeHand(GamePlayer.IsLeftHand(num), 0.15f);
				}
			}
		}
		if (GRNoiseEventManager.instance != null)
		{
			GRNoiseEventManager.instance.AddNoiseEvent(hitData.hitPosition, 1f, 1f);
		}
	}

	public void ApplyHitToPlayer(GRPlayer player, Vector3 hitPosition)
	{
		this.hitFx.hitSound.Play(null);
		if (this.hitFx.hitEffect != null)
		{
			this.hitFx.hitEffect.Play();
		}
		for (int i = 0; i < this.components.Count; i++)
		{
			this.components[i].OnSuccessfulHitPlayer(player, hitPosition);
		}
	}

	private void PlayVibration(float strength, float duration)
	{
		if (!this.gameEntity.IsHeldByLocalPlayer())
		{
			return;
		}
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(this.gameEntity.heldByActorNumber);
		if (gamePlayer == null)
		{
			return;
		}
		int num = gamePlayer.FindHandIndex(this.gameEntity.id);
		if (num == -1)
		{
			return;
		}
		GorillaTagger.Instance.StartVibration(GamePlayer.IsLeftHand(num), strength, duration);
	}

	private T GetParentEnemy<T>(Collider collider) where T : MonoBehaviour
	{
		Transform transform = collider.transform;
		while (transform != null)
		{
			T component = transform.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return default(T);
	}

	public int CalcHitAmount(GameHitType hitType, GameHittable hittable, GameEntity hitByEntity)
	{
		int num = 0;
		if (hitByEntity != null)
		{
			GRAttributes component = hitByEntity.GetComponent<GRAttributes>();
			if (component != null)
			{
				switch (hitType)
				{
				case GameHitType.Club:
					num = component.CalculateFinalValueForAttribute(this.damageAttribute);
					break;
				case GameHitType.Flash:
					num = component.CalculateFinalValueForAttribute(this.flashDamageAttribute);
					break;
				case GameHitType.Shield:
					num = component.CalculateFinalValueForAttribute(this.shieldDamageAttribute);
					break;
				}
			}
		}
		return num;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!this.hitOnCollision)
		{
			return;
		}
		float num = this.gameEntity.GetVelocity().sqrMagnitude;
		if (this.gameEntity.lastHeldByActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
		{
			return;
		}
		bool flag = false;
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(this.gameEntity.heldByActorNumber);
		if (gamePlayer != null)
		{
			float handSpeed = GamePlayerLocal.instance.GetHandSpeed(gamePlayer.FindHandIndex(this.gameEntity.id));
			num = handSpeed * handSpeed;
		}
		if (num < this.minSwingSpeed * this.minSwingSpeed)
		{
			return;
		}
		double timeAsDouble = Time.timeAsDouble;
		if (timeAsDouble < this.hitCooldownEnd)
		{
			return;
		}
		Collider collider = collision.collider;
		GameHittable parentEnemy = this.GetParentEnemy<GameHittable>(collider);
		if (parentEnemy != null)
		{
			Vector3 vector = parentEnemy.transform.position - base.transform.position;
			vector.Normalize();
			if (!flag && gamePlayer != null)
			{
				vector = GamePlayerLocal.instance.GetHandVelocity(gamePlayer.FindHandIndex(this.gameEntity.id)).normalized;
			}
			float num2 = Mathf.Sqrt(num);
			num2 = Mathf.Min(num2, this.maxImpulseSpeed);
			vector *= num2;
			Vector3 position = parentEnemy.transform.position;
			GameHitData gameHitData = new GameHitData
			{
				hitTypeId = (int)this.hitType,
				hitEntityId = parentEnemy.gameEntity.id,
				hitByEntityId = this.gameEntity.id,
				hitEntityPosition = position,
				hitImpulse = vector * this.knockbackMultiplier,
				hitPosition = collision.GetContact(0).point,
				hitAmount = this.CalcHitAmount(this.hitType, parentEnemy, this.gameEntity)
			};
			if (parentEnemy.IsHitValid(gameHitData))
			{
				parentEnemy.RequestHit(gameHitData);
				this.hitCooldownEnd = timeAsDouble + 0.10000000149011612;
			}
		}
	}

	public GameEntity gameEntity;

	public GameHitType hitType;

	public GRAttributeType damageAttribute = GRAttributeType.BatonDamage;

	public GRAttributeType flashDamageAttribute = GRAttributeType.FlashDamage;

	public GRAttributeType shieldDamageAttribute = GRAttributeType.BatonDamage;

	public float minSwingSpeed = 1.5f;

	public GameHitFx hitFx;

	private GRAttributes attributes;

	public float knockbackMultiplier = 1f;

	public float maxImpulseSpeed = 4.5f;

	private List<IGameHitter> components;

	private double hitCooldownEnd;

	public bool hitOnCollision = true;
}
