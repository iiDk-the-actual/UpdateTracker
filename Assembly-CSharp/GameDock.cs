using System;
using System.Collections.Generic;
using UnityEngine;

public class GameDock : MonoBehaviour
{
	private void Awake()
	{
		this.docked = new List<GameEntity>(1);
		if (this.dockMarker == null)
		{
			this.dockMarker = base.transform;
		}
	}

	private void OnEnable()
	{
	}

	public bool CanDock(GameDockable dockable)
	{
		return !(dockable == null) && (this.dockType != GameDockType.GRToolDock || this.GetDockedCount() <= 0);
	}

	public int GetDockedCount()
	{
		return this.docked.Count;
	}

	public void OnDock(GameEntity attachedGameEntity, GameEntity attachedToGameEntity)
	{
		this.dockSound.Play(null);
		this.docked.Add(attachedGameEntity);
		this.dockHaptic.PlayIfSnappedLocal(attachedToGameEntity);
	}

	public void OnUndock(GameEntity gameEntity, GameEntity attachedToGameEntity)
	{
		this.undockSound.Play(null);
		this.docked.Remove(gameEntity);
	}

	public GameEntity gameEntity;

	public GameDockType dockType;

	public float dockRadius = 0.15f;

	public AbilitySound dockSound;

	public AbilitySound undockSound;

	public AbilityHaptic dockHaptic;

	public Transform dockMarker;

	private List<GameEntity> docked;
}
