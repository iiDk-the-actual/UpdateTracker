using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GorillaExtensions;
using GT_CustomMapSupportRuntime;
using UnityEngine;

public class CustomMapsGameManager : MonoBehaviour, IGameEntityZoneComponent
{
	private void Awake()
	{
		if (CustomMapsGameManager.instance.IsNotNull())
		{
			Object.Destroy(this);
			return;
		}
		CustomMapsGameManager.instance = this;
		this.customMapsAgents = new Dictionary<int, AIAgent>(Constants.aiAgentLimit);
		CustomMapsGameManager.tempCreateEntitiesList = new List<GameEntityCreateData>(Constants.aiAgentLimit);
	}

	private void Start()
	{
	}

	public void CreatePlacedEntities(List<MapEntity> entities)
	{
		if (!this.gameEntityManager.IsAuthority())
		{
			GTDev.LogError<string>("CustomMapsManager::CreateAIAgents not the authority", null);
			return;
		}
		int gameAgentCount = this.gameAgentManager.GetGameAgentCount();
		if (gameAgentCount >= Constants.aiAgentLimit)
		{
			GTDev.LogError<string>("[CustomMapsGameManager::CreateAIAgents] Failed to create agent. Max Agent count " + string.Format("({0}) has been reached!", Constants.aiAgentLimit), null);
			return;
		}
		CustomMapsGameManager.tempCreateEntitiesList.Clear();
		int num = ((Constants.aiAgentLimit - gameAgentCount < 0) ? 0 : (Constants.aiAgentLimit - gameAgentCount));
		int num2 = Mathf.Min(entities.Count, num);
		if (num2 < entities.Count)
		{
			GTDev.LogWarning<string>(string.Format("[CustomMapsGameManager::CreateAIAgents] Only creating {0} out of the ", num2) + string.Format("requested {0} agents. Max Agent count ({1}) has been reached.!", entities.Count, Constants.aiAgentLimit), null);
		}
		for (int i = 0; i < num2; i++)
		{
			if (entities[i].IsNull())
			{
				Debug.Log(string.Format("[CustomMapsGameManager::CreateAIAgents] Requested entity to create is null! {0}/{1}", i, entities.Count));
			}
			else
			{
				int num3 = ((entities[i] is AIAgent) ? "CustomMapsAIAgent".GetStaticHash() : "CustomMapsGrabbableEntity".GetStaticHash());
				if (!this.gameEntityManager.FactoryHasEntity(num3))
				{
					Debug.LogErrorFormat("[CustomMapsManager::CreateAIAgents] Cannot Find Entity in Factory {0} {1}", new object[]
					{
						entities[i].gameObject.name,
						num3
					});
				}
				else
				{
					GameEntityCreateData gameEntityCreateData = new GameEntityCreateData
					{
						entityTypeId = num3,
						position = entities[i].transform.position,
						rotation = entities[i].transform.rotation,
						createData = entities[i].GetPackedCreateData()
					};
					CustomMapsGameManager.tempCreateEntitiesList.Add(gameEntityCreateData);
				}
			}
		}
		if (CustomMapsGameManager.tempCreateEntitiesList.Count > 0)
		{
			this.gameEntityManager.RequestCreateItems(CustomMapsGameManager.tempCreateEntitiesList);
			CustomMapsGameManager.tempCreateEntitiesList.Clear();
		}
	}

	public void TEST_Spawning()
	{
		GTDev.Log<string>("CustomMapsGameManager::TEST_Spawn starting spawn", null);
		base.StartCoroutine(this.TEST_Spawn());
	}

	private IEnumerator TEST_Spawn()
	{
		while (this.spawnCount < 10)
		{
			yield return new WaitForSeconds(5f);
			GTDev.Log<string>("CustomMapsGameManager::TEST_Spawn spawning enemy", null);
			this.TEST_index = ((this.TEST_index == 5) ? 3 : 5);
			this.SpawnEnemyFromPoint("79e43963", this.TEST_index);
			this.spawnCount++;
		}
		yield break;
	}

	public GameEntityId SpawnEnemyFromPoint(string spawnPointId, int enemyTypeId)
	{
		AISpawnPoint aispawnPoint;
		if (!AISpawnManager.instance.GetSpawnPoint(spawnPointId, out aispawnPoint))
		{
			GTDev.LogError<string>("CustomMapsGameManager::SpawnEnemyFromPoint cannot find spawn point", null);
			return GameEntityId.Invalid;
		}
		return this.SpawnEnemyAtLocation(enemyTypeId, aispawnPoint.transform.position, aispawnPoint.transform.rotation);
	}

	public GameEntityId SpawnEnemyAtLocation(int enemyTypeId, Vector3 position, Quaternion rotation)
	{
		if (!this.gameEntityManager.IsAuthority())
		{
			GTDev.LogError<string>("[CustomMapsGameManager::SpawnEnemyAtLocation] Failed: Not Authority", null);
			return GameEntityId.Invalid;
		}
		if (this.gameEntityManager.GetGameEntities().Count >= Constants.aiAgentLimit)
		{
			GTDev.LogError<string>(string.Format("[CustomMapsGameManager::SpawnEnemyAtLocation] Failed: Max Agents ({0}) reached.", Constants.aiAgentLimit), null);
			return GameEntityId.Invalid;
		}
		int staticHash = "CustomMapsAIAgent".GetStaticHash();
		if (!this.gameEntityManager.FactoryHasEntity(staticHash))
		{
			GTDev.LogError<string>("[CustomMapsGameManager::SpawnEnemyAtLocation] Failed cannot find entity type", null);
			return GameEntityId.Invalid;
		}
		return this.gameEntityManager.RequestCreateItem(staticHash, position, rotation, (long)enemyTypeId);
	}

	public void SpawnEnemyClient(int enemyTypeId, int agentId)
	{
		if (this.gameEntityManager.IsAuthority())
		{
			return;
		}
		if (enemyTypeId == -1)
		{
			return;
		}
		AIAgent aiagent;
		if (AISpawnManager.HasInstance && AISpawnManager.instance.SpawnEnemy(enemyTypeId, out aiagent))
		{
			aiagent.transform.parent = AISpawnManager.instance.transform;
			this.customMapsAgents[agentId] = aiagent;
			return;
		}
		MapEntity mapEntity;
		if (MapSpawnManager.instance.SpawnEntity(enemyTypeId, out mapEntity))
		{
			aiagent = (AIAgent)mapEntity;
			aiagent.transform.parent = AISpawnManager.instance.transform;
			this.customMapsAgents[agentId] = aiagent;
			return;
		}
	}

	public GameEntityId SpawnGrabbableAtLocation(int enemyTypeId, Vector3 position, Quaternion rotation)
	{
		if (!this.gameEntityManager.IsAuthority())
		{
			GTDev.LogError<string>("[CustomMapsGameManager::SpawnGrabbableAtLocation] Failed: Not Authority", null);
			return GameEntityId.Invalid;
		}
		if (this.gameEntityManager.GetGameEntities().Count >= Constants.aiAgentLimit)
		{
			GTDev.LogError<string>(string.Format("[CustomMapsGameManager::SpawnGrabbableAtLocation] Failed: Max Entities ({0}) reached.", Constants.aiAgentLimit), null);
			return GameEntityId.Invalid;
		}
		int staticHash = "CustomMapsGrabbableEntity".GetStaticHash();
		if (!this.gameEntityManager.FactoryHasEntity(staticHash))
		{
			GTDev.LogError<string>("[CustomMapsGameManager::SpawnGrabbableAtLocation] Failed cannot find entity type", null);
			return GameEntityId.Invalid;
		}
		return this.gameEntityManager.RequestCreateItem(staticHash, position, rotation, (long)enemyTypeId);
	}

	public long ProcessMigratedGameEntityCreateData(GameEntity entity, long createData)
	{
		return createData;
	}

	public bool ValidateMigratedGameEntity(int netId, int entityTypeId, Vector3 position, Quaternion rotation, long createData, int actorNr)
	{
		return false;
	}

	private bool IsAuthority()
	{
		return this.gameEntityManager.IsAuthority();
	}

	private bool IsDriver()
	{
		return CustomMapsTerminal.IsDriver;
	}

	public void OnZoneCreate()
	{
	}

	public void OnZoneInit()
	{
		if (CustomMapsGameManager.agentsToCreateOnZoneInit.IsNullOrEmpty<MapEntity>())
		{
			return;
		}
		this.CreatePlacedEntities(CustomMapsGameManager.agentsToCreateOnZoneInit);
		CustomMapsGameManager.agentsToCreateOnZoneInit.Clear();
	}

	public void OnZoneClear(ZoneClearReason reason)
	{
	}

	public bool ShouldClearZone()
	{
		return true;
	}

	public bool IsZoneReady()
	{
		return CustomMapLoader.CanLoadEntities && NetworkSystem.Instance.InRoom;
	}

	public void OnCreateGameEntity(GameEntity entity)
	{
	}

	private void SetupCollisions(GameObject go)
	{
	}

	public void SerializeZoneData(BinaryWriter writer)
	{
	}

	public void DeserializeZoneData(BinaryReader reader)
	{
	}

	public void SerializeZoneEntityData(BinaryWriter writer, GameEntity entity)
	{
	}

	public void DeserializeZoneEntityData(BinaryReader reader, GameEntity entity)
	{
	}

	public void SerializeZonePlayerData(BinaryWriter writer, int actorNumber)
	{
	}

	public void DeserializeZonePlayerData(BinaryReader reader, int actorNumber)
	{
	}

	public static GameEntityManager GetEntityManager()
	{
		if (CustomMapsGameManager.instance.IsNotNull())
		{
			return CustomMapsGameManager.instance.gameEntityManager;
		}
		return null;
	}

	public static GameAgentManager GetAgentManager()
	{
		if (CustomMapsGameManager.instance.IsNotNull())
		{
			return CustomMapsGameManager.instance.gameAgentManager;
		}
		return null;
	}

	public static CustomMapsAIBehaviourController GetBehaviorControllerForEntity(GameEntityId entityId)
	{
		GameEntityManager entityManager = CustomMapsGameManager.GetEntityManager();
		if (entityManager.IsNull())
		{
			return null;
		}
		GameEntity gameEntity = entityManager.GetGameEntity(entityId);
		if (gameEntity.IsNull())
		{
			return null;
		}
		return gameEntity.gameObject.GetComponent<CustomMapsAIBehaviourController>();
	}

	public static void AddAgentsToCreate(List<MapEntity> entitiesToCreate)
	{
		if (CustomMapsGameManager.instance.IsNull())
		{
			return;
		}
		if (entitiesToCreate.IsNullOrEmpty<MapEntity>())
		{
			return;
		}
		CustomMapsGameManager.agentsToCreateOnZoneInit.AddRange(entitiesToCreate);
	}

	public void OnPlayerHit(GameEntityId hitByEntityId, GRPlayer player, Vector3 hitPosition)
	{
		this.ghostReactorManager.RequestEnemyHitPlayer(GhostReactor.EnemyType.CustomMapsEnemy, hitByEntityId, player, hitPosition);
	}

	public GameEntityManager gameEntityManager;

	public GameAgentManager gameAgentManager;

	public GhostReactorManager ghostReactorManager;

	public static CustomMapsGameManager instance;

	private const string AGENT_PREFAB_NAME = "CustomMapsAIAgent";

	private const string GRABBABLE_PREFAB_NAME = "CustomMapsGrabbableEntity";

	private Dictionary<int, AIAgent> customMapsAgents;

	private static List<GameEntityCreateData> tempCreateEntitiesList = new List<GameEntityCreateData>(128);

	private static List<MapEntity> agentsToCreateOnZoneInit = new List<MapEntity>(128);

	private int TEST_index;

	private int spawnCount;
}
