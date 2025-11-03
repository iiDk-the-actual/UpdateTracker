using System;
using System.Collections.Generic;
using GameObjectScheduling;
using GorillaNetworking;
using UnityEngine;

namespace GorillaGameModes
{
	[CreateAssetMenu(fileName = "New Game Mode Zone Map", menuName = "Game Settings/Game Mode Zone Map", order = 2)]
	public class GameModeZoneMapping : ScriptableObject
	{
		public HashSet<GameModeType> AllModes
		{
			get
			{
				this.init();
				return this.allModes;
			}
		}

		private void init()
		{
			if (this.allModes != null)
			{
				return;
			}
			this.allModes = new HashSet<GameModeType>();
			for (int i = 0; i < this.defaultGameModes.Length; i++)
			{
				if (!this.allModes.Contains(this.defaultGameModes[i]))
				{
					this.allModes.Add(this.defaultGameModes[i]);
				}
			}
			this.publicZoneGameModesLookup = new Dictionary<GTZone, HashSet<GameModeType>>();
			this.privateZoneGameModesLookup = new Dictionary<GTZone, HashSet<GameModeType>>();
			for (int j = 0; j < this.zoneGameModes.Length; j++)
			{
				for (int k = 0; k < this.zoneGameModes[j].zone.Length; k++)
				{
					this.publicZoneGameModesLookup.Add(this.zoneGameModes[j].zone[k], new HashSet<GameModeType>(this.zoneGameModes[j].modes));
					for (int l = 0; l < this.zoneGameModes[j].modes.Length; l++)
					{
						if (!this.allModes.Contains(this.zoneGameModes[j].modes[l]))
						{
							this.allModes.Add(this.zoneGameModes[j].modes[l]);
						}
					}
					if (this.zoneGameModes[j].privateModes.Length != 0)
					{
						this.privateZoneGameModesLookup.Add(this.zoneGameModes[j].zone[k], new HashSet<GameModeType>(this.zoneGameModes[j].privateModes));
						for (int m = 0; m < this.zoneGameModes[j].privateModes.Length; m++)
						{
							if (!this.allModes.Contains(this.zoneGameModes[j].privateModes[m]))
							{
								this.allModes.Add(this.zoneGameModes[j].privateModes[m]);
							}
						}
					}
					else
					{
						this.privateZoneGameModesLookup.Add(this.zoneGameModes[j].zone[k], new HashSet<GameModeType>(this.zoneGameModes[j].modes));
					}
				}
			}
			this.modeNameLookup = new Dictionary<GameModeType, string>();
			for (int n = 0; n < this.gameModeNameOverrides.Length; n++)
			{
				this.modeNameLookup.Add(this.gameModeNameOverrides[n].mode, this.gameModeNameOverrides[n].displayName);
			}
			this.isNewLookup = new HashSet<GameModeType>(this.newThisUpdate);
			this.gameModeTypeCountdownsLookup = new Dictionary<GameModeType, CountdownTextDate>();
			for (int num = 0; num < this.gameModeTypeCountdowns.Length; num++)
			{
				this.gameModeTypeCountdownsLookup.Add(this.gameModeTypeCountdowns[num].mode, this.gameModeTypeCountdowns[num].countdownTextDate);
			}
		}

		public HashSet<GameModeType> GetModesForZone(GTZone zone, bool isPrivate)
		{
			this.init();
			if (isPrivate && this.privateZoneGameModesLookup.ContainsKey(zone))
			{
				return this.privateZoneGameModesLookup[zone];
			}
			if (this.publicZoneGameModesLookup.ContainsKey(zone))
			{
				return this.publicZoneGameModesLookup[zone];
			}
			return new HashSet<GameModeType>(this.defaultGameModes);
		}

		internal string GetModeName(GameModeType mode)
		{
			this.init();
			if (this.modeNameLookup.ContainsKey(mode))
			{
				return this.modeNameLookup[mode];
			}
			return mode.ToString().ToUpper();
		}

		internal bool IsNew(GameModeType mode)
		{
			this.init();
			return this.isNewLookup.Contains(mode);
		}

		internal CountdownTextDate GetCountdown(GameModeType mode)
		{
			this.init();
			if (this.gameModeTypeCountdownsLookup.ContainsKey(mode))
			{
				return this.gameModeTypeCountdownsLookup[mode];
			}
			return null;
		}

		internal GameModeType VerifyModeForZone(GTZone zone, GameModeType mode, bool isPrivate)
		{
			if (GorillaComputer.instance.IsPlayerInVirtualStump())
			{
				zone = GTZone.customMaps;
			}
			if (zone == GTZone.none)
			{
				if (this.allModes.Contains(mode))
				{
					return mode;
				}
				return GameModeType.Casual;
			}
			else
			{
				HashSet<GameModeType> hashSet;
				if (isPrivate && this.privateZoneGameModesLookup.ContainsKey(zone))
				{
					hashSet = this.privateZoneGameModesLookup[zone];
				}
				else if (this.publicZoneGameModesLookup.ContainsKey(zone))
				{
					hashSet = this.publicZoneGameModesLookup[zone];
				}
				else
				{
					hashSet = new HashSet<GameModeType>(this.defaultGameModes);
				}
				if (hashSet.Contains(mode))
				{
					return mode;
				}
				using (HashSet<GameModeType>.Enumerator enumerator = hashSet.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						return enumerator.Current;
					}
				}
				return GameModeType.Casual;
			}
		}

		[SerializeField]
		private GameModeNameOverrides[] gameModeNameOverrides;

		[SerializeField]
		private GameModeType[] defaultGameModes;

		[SerializeField]
		private ZoneGameModes[] zoneGameModes;

		[SerializeField]
		private GameModeTypeCountdown[] gameModeTypeCountdowns;

		[SerializeField]
		private GameModeType[] newThisUpdate;

		private Dictionary<GTZone, HashSet<GameModeType>> publicZoneGameModesLookup;

		private Dictionary<GTZone, HashSet<GameModeType>> privateZoneGameModesLookup;

		private Dictionary<GameModeType, string> modeNameLookup;

		private HashSet<GameModeType> isNewLookup;

		private Dictionary<GameModeType, CountdownTextDate> gameModeTypeCountdownsLookup;

		private HashSet<GameModeType> allModes;
	}
}
