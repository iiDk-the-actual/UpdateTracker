using System;

internal interface IGorillaSerializeableScene : IGorillaSerializeable
{
	void OnSceneLinking(GorillaSerializerScene serializer);

	void OnNetworkObjectDisable();

	void OnNetworkObjectEnable();
}
