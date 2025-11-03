using System;
using System.IO;

public interface IGameEntitySerialize
{
	void OnGameEntitySerialize(BinaryWriter writer);

	void OnGameEntityDeserialize(BinaryReader reader);
}
