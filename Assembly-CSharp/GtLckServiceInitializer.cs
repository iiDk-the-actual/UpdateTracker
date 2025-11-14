using System;
using Liv.Lck;
using Liv.Lck.DependencyInjection;
using UnityEngine;

[DefaultExecutionOrder(-950)]
public class GtLckServiceInitializer : MonoBehaviour
{
	private void Awake()
	{
		LckDiContainer instance = LckDiContainer.Instance;
		if (instance.HasService<ILckService>())
		{
			Debug.LogWarning("LCK: Service already configured. Skipping custom GT initialisation.");
			return;
		}
		Debug.Log("LCK: Initializing with GT-SPECIFIC overrides.");
		LckServiceInitializer.ConfigureServices(instance, this._qualityConfig, delegate(LckDiContainer container)
		{
			container.AddSingleton<ILckCosmeticsFeatureFlagManager, LckCosmeticsFeatureFlagManagerPlayFab>();
		});
	}

	[Header("LCK Configuration")]
	[Tooltip("Assign the LCK Quality Config ScriptableObject here.")]
	[SerializeField]
	private LckQualityConfig _qualityConfig;
}
