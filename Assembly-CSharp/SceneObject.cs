using System;
using UnityEngine;

[Serializable]
public class SceneObject : IEquatable<SceneObject>
{
	public Type GetObjectType()
	{
		if (string.IsNullOrWhiteSpace(this.typeString))
		{
			return null;
		}
		if (this.typeString.Contains("ProxyType"))
		{
			return ProxyType.Parse(this.typeString);
		}
		return Type.GetType(this.typeString);
	}

	public SceneObject(int classID, ulong fileID)
	{
		this.classID = classID;
		this.fileID = fileID;
		this.typeString = UnityYaml.ClassIDToType[classID].AssemblyQualifiedName;
	}

	public bool Equals(SceneObject other)
	{
		return this.fileID == other.fileID && this.classID == other.classID;
	}

	public override bool Equals(object obj)
	{
		SceneObject sceneObject = obj as SceneObject;
		return sceneObject != null && this.Equals(sceneObject);
	}

	public override int GetHashCode()
	{
		int num = this.classID;
		int num2 = StaticHash.Compute((long)this.fileID);
		return StaticHash.Compute(num, num2);
	}

	public static bool operator ==(SceneObject x, SceneObject y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(SceneObject x, SceneObject y)
	{
		return !x.Equals(y);
	}

	public int classID;

	public ulong fileID;

	[SerializeField]
	public string typeString;

	public string json;
}
