using System;
using System.Collections.Generic;
using GorillaGameModes;
using UnityEngine;

public class GameModeSpecificObjectRegistry : MonoBehaviour
{
	private void OnEnable()
	{
		GameModeSpecificObject.OnAwake += this.GameModeSpecificObject_OnAwake;
		GameModeSpecificObject.OnDestroyed += this.GameModeSpecificObject_OnDestroyed;
		GameMode.OnStartGameMode += this.GameMode_OnStartGameMode;
	}

	private void OnDisable()
	{
		GameModeSpecificObject.OnAwake -= this.GameModeSpecificObject_OnAwake;
		GameModeSpecificObject.OnDestroyed -= this.GameModeSpecificObject_OnDestroyed;
		GameMode.OnStartGameMode -= this.GameMode_OnStartGameMode;
	}

	private void GameModeSpecificObject_OnAwake(GameModeSpecificObject obj)
	{
		foreach (GameModeType gameModeType in obj.GameModes)
		{
			if (!this.gameModeSpecificObjects.ContainsKey(gameModeType))
			{
				this.gameModeSpecificObjects.Add(gameModeType, new List<GameModeSpecificObject>());
			}
			this.gameModeSpecificObjects[gameModeType].Add(obj);
		}
		if (GameMode.ActiveGameMode == null)
		{
			obj.gameObject.SetActive(obj.Validation == GameModeSpecificObject.ValidationMethod.Exclusion);
			return;
		}
		obj.gameObject.SetActive(obj.CheckValid(GameMode.ActiveGameMode.GameType()));
	}

	private void GameModeSpecificObject_OnDestroyed(GameModeSpecificObject obj)
	{
		foreach (GameModeType gameModeType in obj.GameModes)
		{
			if (this.gameModeSpecificObjects.ContainsKey(gameModeType))
			{
				this.gameModeSpecificObjects[gameModeType].Remove(obj);
			}
		}
	}

	private void GameMode_OnStartGameMode(GameModeType newGameModeType)
	{
		if (this.currentGameType == newGameModeType)
		{
			return;
		}
		if (this.gameModeSpecificObjects.ContainsKey(this.currentGameType))
		{
			foreach (GameModeSpecificObject gameModeSpecificObject in this.gameModeSpecificObjects[this.currentGameType])
			{
				gameModeSpecificObject.gameObject.SetActive(gameModeSpecificObject.CheckValid(newGameModeType));
			}
		}
		if (this.gameModeSpecificObjects.ContainsKey(newGameModeType))
		{
			foreach (GameModeSpecificObject gameModeSpecificObject2 in this.gameModeSpecificObjects[newGameModeType])
			{
				gameModeSpecificObject2.gameObject.SetActive(gameModeSpecificObject2.CheckValid(newGameModeType));
			}
		}
		this.currentGameType = newGameModeType;
	}

	private Dictionary<GameModeType, List<GameModeSpecificObject>> gameModeSpecificObjects = new Dictionary<GameModeType, List<GameModeSpecificObject>>();

	private GameModeType currentGameType = GameModeType.Count;
}
