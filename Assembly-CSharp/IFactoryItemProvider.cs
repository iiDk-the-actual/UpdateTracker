using System;
using System.Collections.Generic;

public interface IFactoryItemProvider
{
	IEnumerable<GameEntity> GetFactoryItems();
}
