using System;
using System.IO;
using UnityEngine;

public interface IGameEntityZoneComponent
{
	void OnZoneCreate();

	void OnZoneInit();

	void OnZoneClear(ZoneClearReason reason);

	void OnCreateGameEntity(GameEntity entity);

	void SerializeZoneData(BinaryWriter writer);

	void DeserializeZoneData(BinaryReader reader);

	void SerializeZoneEntityData(BinaryWriter writer, GameEntity entity);

	void DeserializeZoneEntityData(BinaryReader reader, GameEntity entity);

	void SerializeZonePlayerData(BinaryWriter writer, int actorNumber);

	void DeserializeZonePlayerData(BinaryReader reader, int actorNumber);

	bool IsZoneReady();

	bool ShouldClearZone();

	long ProcessMigratedGameEntityCreateData(GameEntity entity, long createData);

	bool ValidateMigratedGameEntity(int netId, int entityTypeId, Vector3 position, Quaternion rotation, long createData, int actorNr);
}
