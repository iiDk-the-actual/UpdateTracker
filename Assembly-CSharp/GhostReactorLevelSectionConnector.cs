using System;
using System.Collections.Generic;
using UnityEngine;

public class GhostReactorLevelSectionConnector : MonoBehaviour
{
	private void Awake()
	{
		this.prePlacedGameEntities = new List<GameEntity>(128);
		base.GetComponentsInChildren<GameEntity>(this.prePlacedGameEntities);
		for (int i = 0; i < this.prePlacedGameEntities.Count; i++)
		{
			this.prePlacedGameEntities[i].gameObject.SetActive(false);
		}
		this.renderers = new List<Renderer>(512);
		this.hidden = false;
		base.GetComponentsInChildren<Renderer>(this.renderers);
		if (this.boundingCollider == null)
		{
			Debug.LogWarningFormat("Missing Bounding Collider for section {0}", new object[] { base.gameObject.name });
		}
	}

	public void Init(GhostReactorManager grManager)
	{
		if (grManager.IsAuthority())
		{
			if (this.gateEntity != null)
			{
				grManager.gameEntityManager.RequestCreateItem(this.gateEntity.name.GetStaticHash(), this.gateSpawnPoint.position, this.gateSpawnPoint.rotation, 0L);
			}
			for (int i = 0; i < this.prePlacedGameEntities.Count; i++)
			{
				int staticHash = this.prePlacedGameEntities[i].gameObject.name.GetStaticHash();
				if (!grManager.gameEntityManager.FactoryHasEntity(staticHash))
				{
					Debug.LogErrorFormat("Cannot Find Entity in Factory {0} {1}", new object[]
					{
						this.prePlacedGameEntities[i].gameObject.name,
						staticHash
					});
				}
				else
				{
					GameEntityCreateData gameEntityCreateData = new GameEntityCreateData
					{
						entityTypeId = staticHash,
						position = this.prePlacedGameEntities[i].transform.position,
						rotation = this.prePlacedGameEntities[i].transform.rotation,
						createData = 0L
					};
					GhostReactorLevelSection.tempCreateEntitiesList.Add(gameEntityCreateData);
				}
			}
			grManager.gameEntityManager.RequestCreateItems(GhostReactorLevelSection.tempCreateEntitiesList);
			GhostReactorLevelSection.tempCreateEntitiesList.Clear();
		}
	}

	public void Hide(bool hide)
	{
		for (int i = 0; i < this.renderers.Count; i++)
		{
			if (!(this.renderers[i] == null))
			{
				this.renderers[i].enabled = !hide;
			}
		}
	}

	public void UpdateDisable(Vector3 playerPos)
	{
		if (this.boundingCollider == null)
		{
			return;
		}
		float sqrMagnitude = (this.boundingCollider.ClosestPoint(playerPos) - playerPos).sqrMagnitude;
		float num = 324f;
		float num2 = 484f;
		if (this.hidden && sqrMagnitude < num)
		{
			this.hidden = false;
			this.Hide(false);
			return;
		}
		if (!this.hidden && sqrMagnitude > num2)
		{
			this.hidden = true;
			this.Hide(true);
		}
	}

	public Transform hubAnchor;

	public Transform sectionAnchor;

	public Transform gateSpawnPoint;

	public GameEntity gateEntity;

	public GhostReactorLevelSectionConnector.Direction direction;

	public BoxCollider boundingCollider;

	public List<Transform> pathNodes;

	private const float SHOW_DIST = 18f;

	private const float HIDE_DIST = 22f;

	private List<GameEntity> prePlacedGameEntities;

	private List<Renderer> renderers;

	private bool hidden;

	public enum Direction
	{
		Down = -1,
		Forward,
		Up
	}
}
