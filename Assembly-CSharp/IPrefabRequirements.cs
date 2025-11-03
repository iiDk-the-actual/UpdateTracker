using System;
using System.Collections.Generic;

public interface IPrefabRequirements
{
	IEnumerable<GameEntity> RequiredPrefabs { get; }
}
