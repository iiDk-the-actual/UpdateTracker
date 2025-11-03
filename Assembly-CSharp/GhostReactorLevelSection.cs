using System;
using System.Collections.Generic;
using UnityEngine;

public class GhostReactorLevelSection : MonoBehaviour
{
	public Transform Anchor
	{
		get
		{
			return this.anchorTransform;
		}
	}

	public List<Transform> Anchors
	{
		get
		{
			return this.anchors;
		}
	}

	public GhostReactorLevelSection.SectionType Type
	{
		get
		{
			return this.sectionType;
		}
	}

	public BoxCollider BoundingCollider
	{
		get
		{
			return this.boundingCollider;
		}
	}

	private void Awake()
	{
		this.spawnPointGroupLookup = new GhostReactorLevelSection.SpawnPointGroup[10];
		for (int i = 0; i < this.spawnPointGroups.Count; i++)
		{
			this.spawnPointGroups[i].SpawnPointIndexes = new List<int>();
			int type = (int)this.spawnPointGroups[i].type;
			if (type < this.spawnPointGroupLookup.Length)
			{
				this.spawnPointGroupLookup[type] = this.spawnPointGroups[i];
			}
		}
		this.hazardousMaterials = new List<GRHazardousMaterial>(32);
		base.GetComponentsInChildren<GRHazardousMaterial>(this.hazardousMaterials);
		for (int j = 0; j < this.patrolPaths.Count; j++)
		{
			if (this.patrolPaths[j] == null)
			{
				Debug.LogErrorFormat("Why does {0} have a null patrol path at index {1}", new object[]
				{
					base.gameObject.name,
					j
				});
			}
			else
			{
				this.patrolPaths[j].index = j;
			}
		}
		this.prePlacedGameEntities = new List<GameEntity>(128);
		base.GetComponentsInChildren<GameEntity>(this.prePlacedGameEntities);
		for (int k = 0; k < this.prePlacedGameEntities.Count; k++)
		{
			this.prePlacedGameEntities[k].gameObject.SetActive(false);
		}
		this.renderers = new List<Renderer>(512);
		this.hidden = false;
		base.GetComponentsInChildren<Renderer>(false, this.renderers);
		for (int l = this.renderers.Count - 1; l >= 0; l--)
		{
			if (this.renderers[l] == null || !this.renderers[l].enabled)
			{
				this.renderers.RemoveAt(l);
			}
		}
		if (this.boundingCollider == null)
		{
			Debug.LogWarningFormat("Missing Bounding Collider for section {0}", new object[] { base.gameObject.name });
		}
	}

	public static void RandomizeIndices(List<int> list, int count, ref SRand randomGenerator)
	{
		list.Clear();
		for (int i = 0; i < count; i++)
		{
			list.Add(i);
		}
		randomGenerator.Shuffle<int>(list);
	}

	public void InitLevelSection(int sectionIndex, GhostReactor reactor)
	{
		this.index = sectionIndex;
		for (int i = 0; i < this.hazardousMaterials.Count; i++)
		{
			this.hazardousMaterials[i].Init(reactor);
		}
	}

	public void SpawnSectionEntities(ref SRand randomGenerator, GameEntityManager gameEntityManager, GhostReactor reactor, List<GhostReactorSpawnConfig> spawnConfigs, float respawnCount)
	{
		if (spawnConfigs == null)
		{
			spawnConfigs = this.spawnConfigs;
		}
		if (spawnConfigs != null && spawnConfigs.Count > 0)
		{
			GhostReactorSpawnConfig ghostReactorSpawnConfig = spawnConfigs[randomGenerator.NextInt(spawnConfigs.Count)];
			Debug.LogFormat("Spawn Ghost Reactor Level Section {0} {1}", new object[]
			{
				base.gameObject.name,
				ghostReactorSpawnConfig.name
			});
			for (int i = 0; i < this.spawnPointGroups.Count; i++)
			{
				this.spawnPointGroups[i].CurrentIndex = 0;
				this.spawnPointGroups[i].NeedsRandomization = true;
			}
			for (int j = 0; j < ghostReactorSpawnConfig.entitySpawnGroups.Count; j++)
			{
				int num = ghostReactorSpawnConfig.entitySpawnGroups[j].spawnCount;
				if (num > 0)
				{
					int spawnPointType = (int)ghostReactorSpawnConfig.entitySpawnGroups[j].spawnPointType;
					if (spawnPointType < this.spawnPointGroupLookup.Length)
					{
						GhostReactorLevelSection.SpawnPointGroup spawnPointGroup = this.spawnPointGroupLookup[spawnPointType];
						if (spawnPointGroup != null)
						{
							if (spawnPointGroup.NeedsRandomization)
							{
								spawnPointGroup.NeedsRandomization = false;
								GhostReactorLevelSection.RandomizeIndices(spawnPointGroup.SpawnPointIndexes, spawnPointGroup.spawnPoints.Count, ref randomGenerator);
							}
							num = Mathf.Min(num, spawnPointGroup.spawnPoints.Count);
							for (int k = 0; k < num; k++)
							{
								int currentIndex = spawnPointGroup.CurrentIndex;
								GREntitySpawnPoint nextSpawnPoint = spawnPointGroup.GetNextSpawnPoint();
								nextSpawnPoint == null;
								GameEntity entity = ghostReactorSpawnConfig.entitySpawnGroups[j].entity;
								if (ghostReactorSpawnConfig.entitySpawnGroups[j].randomEntity != null)
								{
									ghostReactorSpawnConfig.entitySpawnGroups[j].randomEntity.TryForRandomItem(reactor, ref randomGenerator, out entity, 0);
								}
								if (!(entity == null))
								{
									int staticHash = entity.name.GetStaticHash();
									long num2 = -1L;
									if (nextSpawnPoint.applyScale)
									{
										num2 = BitPackUtils.PackWorldPosForNetwork(nextSpawnPoint.transform.localScale);
									}
									else if (spawnPointGroup.type == GhostReactorSpawnConfig.SpawnPointType.Enemy || spawnPointGroup.type == GhostReactorSpawnConfig.SpawnPointType.Pest || nextSpawnPoint.patrolPath != null)
									{
										int num3 = 255;
										if (nextSpawnPoint.patrolPath != null)
										{
											num3 = nextSpawnPoint.patrolPath.index;
										}
										int num4 = (int)respawnCount;
										if (randomGenerator.NextFloat() < respawnCount - (float)num4)
										{
											num4++;
										}
										GhostReactor.EnemyEntityCreateData enemyEntityCreateData;
										enemyEntityCreateData.respawnCount = num4;
										enemyEntityCreateData.sectionIndex = this.index;
										enemyEntityCreateData.patrolIndex = num3;
										num2 = enemyEntityCreateData.Pack();
									}
									GameEntityCreateData gameEntityCreateData = new GameEntityCreateData
									{
										entityTypeId = staticHash,
										position = nextSpawnPoint.transform.position,
										rotation = nextSpawnPoint.transform.rotation,
										createData = num2
									};
									GhostReactorLevelSection.tempCreateEntitiesList.Add(gameEntityCreateData);
									if (GhostReactorLevelSection.tempCreateEntitiesList.Count > 25)
									{
										gameEntityManager.RequestCreateItems(GhostReactorLevelSection.tempCreateEntitiesList);
										GhostReactorLevelSection.tempCreateEntitiesList.Clear();
									}
								}
							}
						}
					}
				}
			}
			for (int l = 0; l < this.prePlacedGameEntities.Count; l++)
			{
				int staticHash2 = this.prePlacedGameEntities[l].gameObject.name.GetStaticHash();
				if (!gameEntityManager.FactoryHasEntity(staticHash2))
				{
					Debug.LogErrorFormat("Cannot Find Entity in Factory {0} {1} Trying to spawn in {2}", new object[]
					{
						this.prePlacedGameEntities[l].gameObject.name,
						staticHash2,
						base.gameObject.name
					});
				}
				else
				{
					GameEntityCreateData gameEntityCreateData2 = new GameEntityCreateData
					{
						entityTypeId = staticHash2,
						position = this.prePlacedGameEntities[l].transform.position,
						rotation = this.prePlacedGameEntities[l].transform.rotation,
						createData = 0L
					};
					GhostReactorLevelSection.tempCreateEntitiesList.Add(gameEntityCreateData2);
					if (GhostReactorLevelSection.tempCreateEntitiesList.Count > 25)
					{
						gameEntityManager.RequestCreateItems(GhostReactorLevelSection.tempCreateEntitiesList);
						GhostReactorLevelSection.tempCreateEntitiesList.Clear();
					}
				}
			}
		}
	}

	public void RespawnEntity(ref SRand randomGenerator, GameEntityManager gameEntityManager, int entityId, long entityCreateData)
	{
		if (0 > this.spawnPointGroupLookup.Length)
		{
			return;
		}
		GhostReactorLevelSection.SpawnPointGroup spawnPointGroup = this.spawnPointGroupLookup[0];
		int count = spawnPointGroup.spawnPoints.Count;
		if (count > 3)
		{
			this.rotatingIndexForRespawn = (this.rotatingIndexForRespawn + randomGenerator.NextInt(1, 1 + spawnPointGroup.spawnPoints.Count / 2)) % spawnPointGroup.spawnPoints.Count;
		}
		else if (count > 1)
		{
			this.rotatingIndexForRespawn = (this.rotatingIndexForRespawn + 1) % count;
		}
		else
		{
			this.rotatingIndexForRespawn = 0;
		}
		GREntitySpawnPoint grentitySpawnPoint = spawnPointGroup.spawnPoints[this.rotatingIndexForRespawn];
		GhostReactor.EnemyEntityCreateData enemyEntityCreateData = GhostReactor.EnemyEntityCreateData.Unpack(entityCreateData);
		enemyEntityCreateData.patrolIndex = ((grentitySpawnPoint.patrolPath != null) ? grentitySpawnPoint.patrolPath.index : 255);
		long num = enemyEntityCreateData.Pack();
		gameEntityManager.RequestCreateItem(entityId, grentitySpawnPoint.transform.position, grentitySpawnPoint.transform.rotation, num);
	}

	public GRPatrolPath GetPatrolPath(int patrolPathIndex)
	{
		if (patrolPathIndex >= 0 && patrolPathIndex < this.patrolPaths.Count)
		{
			return this.patrolPaths[patrolPathIndex];
		}
		return null;
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
		float distSq = this.GetDistSq(playerPos);
		float num = 1024f;
		float num2 = 1296f;
		if (this.hidden && distSq < num)
		{
			this.hidden = false;
			this.Hide(false);
			return;
		}
		if (!this.hidden && distSq > num2)
		{
			this.hidden = true;
			this.Hide(true);
		}
	}

	public float GetDistSq(Vector3 pos)
	{
		return (this.boundingCollider.ClosestPoint(pos) - pos).sqrMagnitude;
	}

	public Transform GetAnchor(int anchorIndex)
	{
		return this.anchors[anchorIndex];
	}

	private const float SHOW_DIST = 32f;

	private const float HIDE_DIST = 36f;

	private const int MAX_CREATE_PER_RPC = 25;

	[SerializeField]
	private GhostReactorLevelSection.SectionType sectionType;

	[SerializeField]
	[Tooltip("Single Anchor Transform used for End Caps and Blockers")]
	private Transform anchorTransform;

	[SerializeField]
	[Tooltip("A List of Anchors used as in and out connections for Hubs")]
	private List<Transform> anchors = new List<Transform>();

	[SerializeField]
	private List<GhostReactorLevelSection.SpawnPointGroup> spawnPointGroups;

	[SerializeField]
	private List<GhostReactorSpawnConfig> spawnConfigs;

	[SerializeField]
	private List<GRPatrolPath> patrolPaths;

	[SerializeField]
	private BoxCollider boundingCollider;

	private List<Renderer> renderers;

	private bool hidden;

	private List<GRHazardousMaterial> hazardousMaterials;

	[HideInInspector]
	public GhostReactorLevelSectionConnector sectionConnector;

	[HideInInspector]
	public int hubAnchorIndex;

	private int index;

	private GhostReactorLevelSection.SpawnPointGroup[] spawnPointGroupLookup;

	private List<GameEntity> prePlacedGameEntities;

	public static List<GameEntityCreateData> tempCreateEntitiesList = new List<GameEntityCreateData>(32);

	private int rotatingIndexForRespawn;

	public enum SectionType
	{
		Hub,
		EndCap,
		Blocker
	}

	[Serializable]
	public class SpawnPointGroup
	{
		public bool NeedsRandomization
		{
			get
			{
				return this.needsRandomization;
			}
			set
			{
				this.needsRandomization = value;
			}
		}

		public int CurrentIndex
		{
			get
			{
				return this.currentIndex;
			}
			set
			{
				this.currentIndex = value;
			}
		}

		public List<int> SpawnPointIndexes
		{
			get
			{
				return this.spawnPointIndexes;
			}
			set
			{
				this.spawnPointIndexes = value;
			}
		}

		public GREntitySpawnPoint GetNextSpawnPoint()
		{
			GREntitySpawnPoint grentitySpawnPoint = this.spawnPoints[this.spawnPointIndexes[this.currentIndex]];
			this.currentIndex = (this.currentIndex + 1) % this.spawnPointIndexes.Count;
			return grentitySpawnPoint;
		}

		public GhostReactorSpawnConfig.SpawnPointType type;

		public List<GREntitySpawnPoint> spawnPoints;

		private List<int> spawnPointIndexes;

		private bool needsRandomization;

		private int currentIndex;
	}
}
