using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GTScene : IEquatable<GTScene>
{
	public string alias
	{
		get
		{
			return this._alias;
		}
	}

	public string name
	{
		get
		{
			return this._name;
		}
	}

	public string path
	{
		get
		{
			return this._path;
		}
	}

	public string guid
	{
		get
		{
			return this._guid;
		}
	}

	public int buildIndex
	{
		get
		{
			return this._buildIndex;
		}
	}

	public bool includeInBuild
	{
		get
		{
			return this._includeInBuild;
		}
	}

	public bool isLoaded
	{
		get
		{
			return SceneManager.GetSceneByBuildIndex(this._buildIndex).isLoaded;
		}
	}

	public bool hasAlias
	{
		get
		{
			return !string.IsNullOrWhiteSpace(this._alias);
		}
	}

	public GTScene(string name, string path, string guid, int buildIndex, bool includeInBuild)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException("name");
		}
		if (string.IsNullOrWhiteSpace(path))
		{
			throw new ArgumentNullException("path");
		}
		if (string.IsNullOrWhiteSpace(guid))
		{
			throw new ArgumentNullException("guid");
		}
		this._name = name;
		this._path = path;
		this._guid = guid;
		this._buildIndex = buildIndex;
		this._includeInBuild = includeInBuild;
	}

	public override int GetHashCode()
	{
		return this._guid.GetHashCode();
	}

	public override string ToString()
	{
		return this.ToJson(false);
	}

	public bool Equals(GTScene other)
	{
		return this._guid.Equals(other._guid) && this._name == other._name && this._path == other._path;
	}

	public override bool Equals(object obj)
	{
		GTScene gtscene = obj as GTScene;
		return gtscene != null && this.Equals(gtscene);
	}

	public static bool operator ==(GTScene x, GTScene y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(GTScene x, GTScene y)
	{
		return !x.Equals(y);
	}

	public void LoadAsync()
	{
		if (this.isLoaded)
		{
			return;
		}
		SceneManager.LoadSceneAsync(this._buildIndex, LoadSceneMode.Additive);
	}

	public void UnloadAsync()
	{
		if (!this.isLoaded)
		{
			return;
		}
		SceneManager.UnloadSceneAsync(this._buildIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
	}

	public static GTScene FromAsset(object sceneAsset)
	{
		return null;
	}

	public static GTScene From(object editorBuildSettingsScene)
	{
		return null;
	}

	[SerializeField]
	private string _alias;

	[SerializeField]
	private string _name;

	[SerializeField]
	private string _path;

	[SerializeField]
	private string _guid;

	[SerializeField]
	private int _buildIndex;

	[SerializeField]
	private bool _includeInBuild;
}
