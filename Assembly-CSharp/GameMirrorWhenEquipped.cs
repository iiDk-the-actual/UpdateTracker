using System;
using UnityEngine;

public class GameMirrorWhenEquipped : MonoBehaviour
{
	private void Awake()
	{
		if (this.m_gameEntity == null)
		{
			this.m_gameEntity = base.GetComponent<GameEntity>();
		}
		if (this.m_xformsToMirror == null)
		{
			this.m_xformsToMirror = Array.Empty<Transform>();
		}
	}

	protected void OnEnable()
	{
		GameEntity gameEntity = this.m_gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this._HandleGameEntityOnEquipChanged));
		GameEntity gameEntity2 = this.m_gameEntity;
		gameEntity2.OnSnapped = (Action)Delegate.Combine(gameEntity2.OnSnapped, new Action(this._HandleGameEntityOnEquipChanged));
		GameEntity gameEntity3 = this.m_gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Combine(gameEntity3.OnReleased, new Action(this._HandleGameEntityOnEquipChanged));
		GameEntity gameEntity4 = this.m_gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Combine(gameEntity4.OnUnsnapped, new Action(this._HandleGameEntityOnEquipChanged));
	}

	protected void OnDisable()
	{
		GameEntity gameEntity = this.m_gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Remove(gameEntity.OnGrabbed, new Action(this._HandleGameEntityOnEquipChanged));
		GameEntity gameEntity2 = this.m_gameEntity;
		gameEntity2.OnSnapped = (Action)Delegate.Remove(gameEntity2.OnSnapped, new Action(this._HandleGameEntityOnEquipChanged));
		GameEntity gameEntity3 = this.m_gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Remove(gameEntity3.OnReleased, new Action(this._HandleGameEntityOnEquipChanged));
		GameEntity gameEntity4 = this.m_gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Remove(gameEntity4.OnUnsnapped, new Action(this._HandleGameEntityOnEquipChanged));
	}

	private void _HandleGameEntityOnEquipChanged()
	{
		if (this.m_shouldOnlyMirrorWhenSnapped && this.m_gameEntity.snappedJoint == SnapJointType.None)
		{
			return;
		}
		Vector3 vector = ((this.m_gameEntity.EquippedHandedness == this.m_handednessToMirror) ? new Vector3(-1f, 1f, 1f) : Vector3.one);
		for (int i = 0; i < this.m_xformsToMirror.Length; i++)
		{
			this.m_xformsToMirror[i].localScale = vector;
		}
	}

	[SerializeField]
	private GameEntity m_gameEntity;

	[SerializeField]
	private Transform[] m_xformsToMirror;

	[SerializeField]
	private bool m_shouldOnlyMirrorWhenSnapped = true;

	[Tooltip("Set the X axis scale to -1 if the gadget is attached (held or snapped) to the selected side.")]
	[SerializeField]
	private EHandedness m_handednessToMirror = EHandedness.Right;
}
