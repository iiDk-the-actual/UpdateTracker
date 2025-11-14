using System;
using System.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

public class GraphicsStateCollectionManager : MonoBehaviour
{
	private GraphicsStateCollection FindExistingCollection()
	{
		for (int i = 0; i < this.collections.Length; i++)
		{
			if (this.collections[i] != null && this.collections[i].runtimePlatform == Application.platform && this.collections[i].graphicsDeviceType == SystemInfo.graphicsDeviceType && this.collections[i].qualityLevelName == QualitySettings.names[QualitySettings.GetQualityLevel()])
			{
				return this.collections[i];
			}
		}
		return null;
	}

	private void Awake()
	{
		if (GraphicsStateCollectionManager.Instance != null && GraphicsStateCollectionManager.Instance != this)
		{
			Debug.LogError("Only one instance of GraphicsStateCollectionManager is allowed!");
			Object.Destroy(base.gameObject);
			return;
		}
		GraphicsStateCollectionManager.Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		if (this.mode == GraphicsStateCollectionManager.Mode.Tracing)
		{
			this.m_GraphicsStateCollection = this.FindExistingCollection();
			if (this.m_GraphicsStateCollection != null)
			{
				this.m_OutputCollectionName = "SharedAssets/GraphicsStateCollections/" + this.m_GraphicsStateCollection.name;
			}
			else
			{
				int qualityLevel = QualitySettings.GetQualityLevel();
				string text = QualitySettings.names[qualityLevel];
				text = text.Replace(" ", "");
				this.m_OutputCollectionName = string.Concat(new object[]
				{
					"SharedAssets/GraphicsStateCollections/",
					"GfxState_",
					Application.platform,
					"_",
					SystemInfo.graphicsDeviceType.ToString(),
					"_",
					text
				});
				this.m_GraphicsStateCollection = new GraphicsStateCollection();
			}
			Debug.Log("Tracing started for GraphicsStateCollection by Scene '" + SceneManager.GetActiveScene().name + "'.");
			this.m_GraphicsStateCollection.BeginTrace();
			this._autoSaveRoutine = base.StartCoroutine(this.AutoSaveRoutine());
			return;
		}
		GraphicsStateCollection graphicsStateCollection = this.FindExistingCollection();
		if (graphicsStateCollection != null)
		{
			Debug.Log(string.Concat(new string[]
			{
				"Scene '",
				SceneManager.GetActiveScene().name,
				"' started warming up ",
				graphicsStateCollection.totalGraphicsStateCount.ToString(),
				" GraphicsState entries."
			}));
			graphicsStateCollection.WarmUp(default(JobHandle));
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus && this.mode == GraphicsStateCollectionManager.Mode.Tracing && this.m_GraphicsStateCollection != null)
		{
			Debug.Log("Focus changed. Sending collection to Editor with " + this.m_GraphicsStateCollection.totalGraphicsStateCount.ToString() + " GraphicsState entries.");
			this.m_GraphicsStateCollection.SendToEditor(this.m_OutputCollectionName);
		}
	}

	private void OnDestroy()
	{
		if (this._autoSaveRoutine != null)
		{
			base.StopCoroutine(this._autoSaveRoutine);
		}
		if (this.mode == GraphicsStateCollectionManager.Mode.Tracing && this.m_GraphicsStateCollection != null)
		{
			this.m_GraphicsStateCollection.EndTrace();
			Debug.Log("Sending collection to Editor with " + this.m_GraphicsStateCollection.totalGraphicsStateCount.ToString() + " GraphicsState entries.");
			this.m_GraphicsStateCollection.SendToEditor(this.m_OutputCollectionName);
		}
	}

	private IEnumerator AutoSaveRoutine()
	{
		for (;;)
		{
			yield return new WaitForSeconds(5f);
			if (this.mode == GraphicsStateCollectionManager.Mode.Tracing && this.m_GraphicsStateCollection != null)
			{
				Debug.Log("Auto-saving collection with " + this.m_GraphicsStateCollection.totalGraphicsStateCount.ToString() + " GraphicsState entries.");
				this.m_GraphicsStateCollection.SendToEditor(this.m_OutputCollectionName);
			}
		}
		yield break;
	}

	public GraphicsStateCollectionManager.Mode mode;

	public static GraphicsStateCollectionManager Instance;

	public GraphicsStateCollection[] collections;

	private const string k_CollectionFolderPath = "SharedAssets/GraphicsStateCollections/";

	private string m_OutputCollectionName;

	private GraphicsStateCollection m_GraphicsStateCollection;

	private Coroutine _autoSaveRoutine;

	public enum Mode
	{
		Tracing,
		WarmUp
	}
}
