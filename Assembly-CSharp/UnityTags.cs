using System;
using System.Collections.Generic;

public static class UnityTags
{
	// Note: this type is marked as 'beforefieldinit'.
	static UnityTags()
	{
		Dictionary<string, UnityTag> dictionary = new Dictionary<string, UnityTag>();
		dictionary["Untagged"] = UnityTag.Untagged;
		dictionary["Respawn"] = UnityTag.Respawn;
		dictionary["Finish"] = UnityTag.Finish;
		dictionary["EditorOnly"] = UnityTag.EditorOnly;
		dictionary["MainCamera"] = UnityTag.MainCamera;
		dictionary["Player"] = UnityTag.Player;
		dictionary["GameController"] = UnityTag.GameController;
		dictionary["SceneChanger"] = UnityTag.SceneChanger;
		dictionary["PlayerOffset"] = UnityTag.PlayerOffset;
		dictionary["GorillaTagManager"] = UnityTag.GorillaTagManager;
		dictionary["GorillaTagCollider"] = UnityTag.GorillaTagCollider;
		dictionary["GorillaPlayer"] = UnityTag.GorillaPlayer;
		dictionary["GorillaObject"] = UnityTag.GorillaObject;
		dictionary["GorillaGameManager"] = UnityTag.GorillaGameManager;
		dictionary["GorillaCosmetic"] = UnityTag.GorillaCosmetic;
		dictionary["projectile"] = UnityTag.projectile;
		dictionary["FxTemporaire"] = UnityTag.FxTemporaire;
		dictionary["SlingshotProjectile"] = UnityTag.SlingshotProjectile;
		dictionary["SlingshotProjectileTrail"] = UnityTag.SlingshotProjectileTrail;
		dictionary["SlingshotProjectilePlayerImpactFX"] = UnityTag.SlingshotProjectilePlayerImpactFX;
		dictionary["SlingshotProjectileSurfaceImpactFX"] = UnityTag.SlingshotProjectileSurfaceImpactFX;
		dictionary["BalloonPopFX"] = UnityTag.BalloonPopFX;
		dictionary["WorldShareableItem"] = UnityTag.WorldShareableItem;
		dictionary["HornsSlingshotProjectile"] = UnityTag.HornsSlingshotProjectile;
		dictionary["HornsSlingshotProjectileTrail"] = UnityTag.HornsSlingshotProjectileTrail;
		dictionary["HornsSlingshotProjectilePlayerImpactFX"] = UnityTag.HornsSlingshotProjectilePlayerImpactFX;
		dictionary["HornsSlingshotProjectileSurfaceImpactFX"] = UnityTag.HornsSlingshotProjectileSurfaceImpactFX;
		dictionary["FryingPan"] = UnityTag.FryingPan;
		dictionary["LeafPile_ImpactFX"] = UnityTag.LeafPileImpactFX;
		dictionary["BalloonPopFx"] = UnityTag.BalloonPopFx;
		dictionary["CloudSlingshot_Projectile"] = UnityTag.CloudSlingshotProjectile;
		dictionary["CloudSlingshot_ProjectileTrail"] = UnityTag.CloudSlingshotProjectileTrail;
		dictionary["CloudSlingshot_ProjectilePlayerImpactFX"] = UnityTag.CloudSlingshotProjectilePlayerImpactFX;
		dictionary["CloudSlingshot_ProjectileSurfaceImpactFX"] = UnityTag.CloudSlingshotProjectileSurfaceImpactFX;
		dictionary["SnowballProjectile"] = UnityTag.SnowballProjectile;
		dictionary["SnowballProjectileImpactFX"] = UnityTag.SnowballProjectileImpactFX;
		dictionary["CupidBow_Projectile"] = UnityTag.CupidBowProjectile;
		dictionary["CupidBow_ProjectileTrail"] = UnityTag.CupidBowProjectileTrail;
		dictionary["CupidBow_ProjectileSurfaceImpactFX"] = UnityTag.CupidBowProjectileSurfaceImpactFX;
		dictionary["NoCrazyCheck"] = UnityTag.NoCrazyCheck;
		dictionary["IceSlingshot_Projectile"] = UnityTag.IceSlingshotProjectile;
		dictionary["IceSlingshot_ProjectileSurfaceImpactFX"] = UnityTag.IceSlingshotProjectileSurfaceImpactFX;
		dictionary["IceSlingshot_ProjectileTrail"] = UnityTag.IceSlingshotProjectileTrail;
		dictionary["ElfBow_Projectile"] = UnityTag.ElfBowProjectile;
		dictionary["ElfBow_ProjectileSurfaceImpactFX"] = UnityTag.ElfBowProjectileSurfaceImpactFX;
		dictionary["ElfBow_ProjectileTrail"] = UnityTag.ElfBowProjectileTrail;
		dictionary["RenderIfSmall"] = UnityTag.RenderIfSmall;
		dictionary["DeleteOnNonBetaBuild"] = UnityTag.DeleteOnNonBetaBuild;
		dictionary["DeleteOnNonDebugBuild"] = UnityTag.DeleteOnNonDebugBuild;
		dictionary["FlagColoringCauldon"] = UnityTag.FlagColoringCauldon;
		dictionary["WaterRippleEffect"] = UnityTag.WaterRippleEffect;
		dictionary["WaterSplashEffect"] = UnityTag.WaterSplashEffect;
		dictionary["FireworkMortarProjectile"] = UnityTag.FireworkMortarProjectile;
		dictionary["FireworkMortarProjectileImpactFX"] = UnityTag.FireworkMortarProjectileImpactFX;
		dictionary["WaterBalloonProjectile"] = UnityTag.WaterBalloonProjectile;
		dictionary["WaterBalloonProjectileImpactFX"] = UnityTag.WaterBalloonProjectileImpactFX;
		dictionary["PlayerHeadTrigger"] = UnityTag.PlayerHeadTrigger;
		dictionary["WizardStaff"] = UnityTag.WizardStaff;
		dictionary["LurkerGhost"] = UnityTag.LurkerGhost;
		dictionary["HauntedObject"] = UnityTag.HauntedObject;
		dictionary["WanderingGhost"] = UnityTag.WanderingGhost;
		dictionary["LavaSurfaceRock"] = UnityTag.LavaSurfaceRock;
		dictionary["LavaRockProjectile"] = UnityTag.LavaRockProjectile;
		dictionary["LavaRockProjectileImpactFX"] = UnityTag.LavaRockProjectileImpactFX;
		dictionary["MoltenSlingshot_Projectile"] = UnityTag.MoltenSlingshotProjectile;
		dictionary["MoltenSlingshot_ProjectileTrail"] = UnityTag.MoltenSlingshotProjectileTrail;
		dictionary["MoltenSlingshot_ProjectileSurfaceImpactFX"] = UnityTag.MoltenSlingshotProjectileSurfaceImpactFX;
		dictionary["MoltenSlingshot_ProjectilePlayerImpactFX"] = UnityTag.MoltenSlingshotProjectilePlayerImpactFX;
		dictionary["SpiderBow_Projectile"] = UnityTag.SpiderBowProjectile;
		dictionary["SpiderBow_ProjectileTrail"] = UnityTag.SpiderBowProjectileTrail;
		dictionary["SpiderBow_ProjectileSurfaceImpactFX"] = UnityTag.SpiderBowProjectileSurfaceImpactFX;
		dictionary["SpiderBow_ProjectilePlayerImpactFX"] = UnityTag.SpiderBowProjectilePlayerImpactFX;
		dictionary["ZoneRoot"] = UnityTag.ZoneRoot;
		dictionary["DontProcessMaterials"] = UnityTag.DontProcessMaterials;
		dictionary["OrnamentProjectileSurfaceImpactFX"] = UnityTag.OrnamentProjectileSurfaceImpactFX;
		dictionary["BucketGiftCane"] = UnityTag.BucketGiftCane;
		dictionary["BucketGiftCoal"] = UnityTag.BucketGiftCoal;
		dictionary["BucketGiftRoll"] = UnityTag.BucketGiftRoll;
		dictionary["BucketGiftRound"] = UnityTag.BucketGiftRound;
		dictionary["BucketGiftSquare"] = UnityTag.BucketGiftSquare;
		dictionary["OrnamentProjectile"] = UnityTag.OrnamentProjectile;
		dictionary["OrnamentShatterFX"] = UnityTag.OrnamentShatterFX;
		dictionary["ScienceCandyProjectile"] = UnityTag.ScienceCandyProjectile;
		dictionary["ScienceCandyImpactFX"] = UnityTag.ScienceCandyImpactFX;
		dictionary["PaperAirplaneProjectile"] = UnityTag.PaperAirplaneProjectile;
		dictionary["DevilBow_Projectile"] = UnityTag.DevilBowProjectile;
		dictionary["DevilBow_ProjectileTrail"] = UnityTag.DevilBowProjectileTrail;
		dictionary["DevilBow_ProjectileSurfaceImpactFX"] = UnityTag.DevilBowProjectileSurfaceImpactFX;
		dictionary["DevilBow_ProjectilePlayerImpactFX"] = UnityTag.DevilBowProjectilePlayerImpactFX;
		dictionary["Fire_FX"] = UnityTag.FireFX;
		dictionary["FishFood"] = UnityTag.FishFood;
		dictionary["FishFoodImpactFX"] = UnityTag.FishFoodImpactFX;
		dictionary["LeafNinjaStarProjectile"] = UnityTag.LeafNinjaStarProjectile;
		dictionary["LeafNinjaStarProjectileC1"] = UnityTag.LeafNinjaStarProjectileC1;
		dictionary["LeafNinjaStarProjectileC2"] = UnityTag.LeafNinjaStarProjectileC2;
		dictionary["SamuraiBow_Projectile"] = UnityTag.SamuraiBowProjectile;
		dictionary["SamuraiBow_ProjectileTrail"] = UnityTag.SamuraiBowProjectileTrail;
		dictionary["SamuraiBow_ProjectileSurfaceImpactFX"] = UnityTag.SamuraiBowProjectileSurfaceImpactFX;
		dictionary["SamuraiBow_ProjectilePlayerImpactFX"] = UnityTag.SamuraiBowProjectilePlayerImpactFX;
		dictionary["DragonSling_Projectile"] = UnityTag.DragonSlingProjectile;
		dictionary["DragonSling_ProjectileTrail"] = UnityTag.DragonSlingProjectileTrail;
		dictionary["DragonSling_ProjectileSurfaceImpactFX"] = UnityTag.DragonSlingProjectileSurfaceImpactFX;
		dictionary["DragonSling_ProjectilePlayerImpactFX"] = UnityTag.DragonSlingProjectilePlayerImpactFX;
		dictionary["Fireball_Projectile"] = UnityTag.FireballProjectile;
		dictionary["StealthHandTapFX"] = UnityTag.StealthHandTapFX;
		dictionary["EnvPieceTree01"] = UnityTag.EnvPieceTree01;
		dictionary["FxSnapPiecePlaced"] = UnityTag.FxSnapPiecePlaced;
		dictionary["FxSnapPieceDisconnected"] = UnityTag.FxSnapPieceDisconnected;
		dictionary["FxSnapPieceGrabbed"] = UnityTag.FxSnapPieceGrabbed;
		dictionary["FxSnapPieceLocationLock"] = UnityTag.FxSnapPieceLocationLock;
		dictionary["CyberNinjaStarProjectile"] = UnityTag.CyberNinjaStarProjectile;
		dictionary["RoomLight"] = UnityTag.RoomLight;
		dictionary["SamplesInfoPanel"] = UnityTag.SamplesInfoPanel;
		dictionary["GorillaHandLeft"] = UnityTag.GorillaHandLeft;
		dictionary["GorillaHandRight"] = UnityTag.GorillaHandRight;
		dictionary["GorillaHandSocket"] = UnityTag.GorillaHandSocket;
		dictionary["PlayingCardProjectile"] = UnityTag.PlayingCardProjectile;
		dictionary["RottenPumpkinProjectile"] = UnityTag.RottenPumpkinProjectile;
		dictionary["FxSnapPieceRecycle"] = UnityTag.FxSnapPieceRecycle;
		dictionary["FxSnapPieceDispenser"] = UnityTag.FxSnapPieceDispenser;
		dictionary["AppleProjectile"] = UnityTag.AppleProjectile;
		dictionary["AppleProjectileSurfaceImpactFX"] = UnityTag.AppleProjectileSurfaceImpactFX;
		dictionary["RecyclerForceVolumeFX"] = UnityTag.RecyclerForceVolumeFX;
		dictionary["FxSnapPieceTooHeavy"] = UnityTag.FxSnapPieceTooHeavy;
		dictionary["FxBuilderPrivatePlotClaimed"] = UnityTag.FxBuilderPrivatePlotClaimed;
		dictionary["TrickTreat_Candy"] = UnityTag.TrickTreatCandy;
		dictionary["TrickTreat_Eyeball"] = UnityTag.TrickTreatEyeball;
		dictionary["TrickTreat_Bat"] = UnityTag.TrickTreatBat;
		dictionary["TrickTreat_Bomb"] = UnityTag.TrickTreatBomb;
		dictionary["TrickTreat_SurfaceImpact"] = UnityTag.TrickTreatSurfaceImpact;
		dictionary["TrickTreat_Bat_Impact"] = UnityTag.TrickTreatBatImpact;
		dictionary["TrickTreat_Bomb_Impact"] = UnityTag.TrickTreatBombImpact;
		dictionary["GuardianSlapFX"] = UnityTag.GuardianSlapFX;
		dictionary["GuardianSlamFX"] = UnityTag.GuardianSlamFX;
		dictionary["GuardianIdolLandedFX"] = UnityTag.GuardianIdolLandedFX;
		dictionary["GuardianIdolFallFX"] = UnityTag.GuardianIdolFallFX;
		dictionary["GuardianIdolTappedFX"] = UnityTag.GuardianIdolTappedFX;
		dictionary["VotingRockProjectile"] = UnityTag.VotingRockProjectile;
		dictionary["LeafPile_ImpactFX_Medium"] = UnityTag.LeafPileImpactFXMedium;
		dictionary["LeafPile_ImpactFX_Small"] = UnityTag.LeafPileImpactFXSmall;
		dictionary["WoodenSword"] = UnityTag.WoodenSword;
		dictionary["WoodenShield"] = UnityTag.WoodenShield;
		dictionary["FxBuilderShrink"] = UnityTag.FxBuilderShrink;
		dictionary["FxBuilderGrow"] = UnityTag.FxBuilderGrow;
		dictionary["FxSnapPieceWreathJump"] = UnityTag.FxSnapPieceWreathJump;
		dictionary["ElfLauncherElf"] = UnityTag.ElfLauncherElf;
		dictionary["RubberBandCar"] = UnityTag.RubberBandCar;
		dictionary["SnowPile_ImpactFX"] = UnityTag.SnowPileImpactFX;
		dictionary["Firecrackers_Projectile"] = UnityTag.FirecrackersProjectile;
		dictionary["PaperAirplaneSquare_Projectile"] = UnityTag.PaperAirplaneSquareProjectile;
		dictionary["SmokeBomb_Projectile"] = UnityTag.SmokeBombProjectile;
		dictionary["ThrowableHeart_Projectile"] = UnityTag.ThrowableHeartProjectile;
		dictionary["SunFlowers"] = UnityTag.SunFlowers;
		dictionary["RobotCannon_Projectile"] = UnityTag.RobotCannonProjectile;
		dictionary["RobotCannon_ProjectileImpact"] = UnityTag.RobotCannonProjectileImpact;
		dictionary["SmokeBomb_ExplosionEffect"] = UnityTag.SmokeBombExplosionEffect;
		dictionary["FireCracker_ExplosionEffect"] = UnityTag.FireCrackerExplosionEffect;
		dictionary["GorillaMouth"] = UnityTag.GorillaMouth;
		UnityTags.StringToTag = dictionary;
	}

	public static readonly string[] StringValues = new string[]
	{
		"Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController", "SceneChanger", "PlayerOffset", "GorillaTagManager",
		"GorillaTagCollider", "GorillaPlayer", "GorillaObject", "GorillaGameManager", "GorillaCosmetic", "projectile", "FxTemporaire", "SlingshotProjectile", "SlingshotProjectileTrail", "SlingshotProjectilePlayerImpactFX",
		"SlingshotProjectileSurfaceImpactFX", "BalloonPopFX", "WorldShareableItem", "HornsSlingshotProjectile", "HornsSlingshotProjectileTrail", "HornsSlingshotProjectilePlayerImpactFX", "HornsSlingshotProjectileSurfaceImpactFX", "FryingPan", "LeafPile_ImpactFX", "BalloonPopFx",
		"CloudSlingshot_Projectile", "CloudSlingshot_ProjectileTrail", "CloudSlingshot_ProjectilePlayerImpactFX", "CloudSlingshot_ProjectileSurfaceImpactFX", "SnowballProjectile", "SnowballProjectileImpactFX", "CupidBow_Projectile", "CupidBow_ProjectileTrail", "CupidBow_ProjectileSurfaceImpactFX", "NoCrazyCheck",
		"IceSlingshot_Projectile", "IceSlingshot_ProjectileSurfaceImpactFX", "IceSlingshot_ProjectileTrail", "ElfBow_Projectile", "ElfBow_ProjectileSurfaceImpactFX", "ElfBow_ProjectileTrail", "RenderIfSmall", "DeleteOnNonBetaBuild", "DeleteOnNonDebugBuild", "FlagColoringCauldon",
		"WaterRippleEffect", "WaterSplashEffect", "FireworkMortarProjectile", "FireworkMortarProjectileImpactFX", "WaterBalloonProjectile", "WaterBalloonProjectileImpactFX", "PlayerHeadTrigger", "WizardStaff", "LurkerGhost", "HauntedObject",
		"WanderingGhost", "LavaSurfaceRock", "LavaRockProjectile", "LavaRockProjectileImpactFX", "MoltenSlingshot_Projectile", "MoltenSlingshot_ProjectileTrail", "MoltenSlingshot_ProjectileSurfaceImpactFX", "MoltenSlingshot_ProjectilePlayerImpactFX", "SpiderBow_Projectile", "SpiderBow_ProjectileTrail",
		"SpiderBow_ProjectileSurfaceImpactFX", "SpiderBow_ProjectilePlayerImpactFX", "ZoneRoot", "DontProcessMaterials", "OrnamentProjectileSurfaceImpactFX", "BucketGiftCane", "BucketGiftCoal", "BucketGiftRoll", "BucketGiftRound", "BucketGiftSquare",
		"OrnamentProjectile", "OrnamentShatterFX", "ScienceCandyProjectile", "ScienceCandyImpactFX", "PaperAirplaneProjectile", "DevilBow_Projectile", "DevilBow_ProjectileTrail", "DevilBow_ProjectileSurfaceImpactFX", "DevilBow_ProjectilePlayerImpactFX", "Fire_FX",
		"FishFood", "FishFoodImpactFX", "LeafNinjaStarProjectile", "LeafNinjaStarProjectileC1", "LeafNinjaStarProjectileC2", "SamuraiBow_Projectile", "SamuraiBow_ProjectileTrail", "SamuraiBow_ProjectileSurfaceImpactFX", "SamuraiBow_ProjectilePlayerImpactFX", "DragonSling_Projectile",
		"DragonSling_ProjectileTrail", "DragonSling_ProjectileSurfaceImpactFX", "DragonSling_ProjectilePlayerImpactFX", "Fireball_Projectile", "StealthHandTapFX", "EnvPieceTree01", "FxSnapPiecePlaced", "FxSnapPieceDisconnected", "FxSnapPieceGrabbed", "FxSnapPieceLocationLock",
		"CyberNinjaStarProjectile", "RoomLight", "SamplesInfoPanel", "GorillaHandLeft", "GorillaHandRight", "GorillaHandSocket", "PlayingCardProjectile", "RottenPumpkinProjectile", "FxSnapPieceRecycle", "FxSnapPieceDispenser",
		"AppleProjectile", "AppleProjectileSurfaceImpactFX", "RecyclerForceVolumeFX", "FxSnapPieceTooHeavy", "FxBuilderPrivatePlotClaimed", "TrickTreat_Candy", "TrickTreat_Eyeball", "TrickTreat_Bat", "TrickTreat_Bomb", "TrickTreat_SurfaceImpact",
		"TrickTreat_Bat_Impact", "TrickTreat_Bomb_Impact", "GuardianSlapFX", "GuardianSlamFX", "GuardianIdolLandedFX", "GuardianIdolFallFX", "GuardianIdolTappedFX", "VotingRockProjectile", "LeafPile_ImpactFX_Medium", "LeafPile_ImpactFX_Small",
		"WoodenSword", "WoodenShield", "FxBuilderShrink", "FxBuilderGrow", "FxSnapPieceWreathJump", "ElfLauncherElf", "RubberBandCar", "SnowPile_ImpactFX", "Firecrackers_Projectile", "PaperAirplaneSquare_Projectile",
		"SmokeBomb_Projectile", "ThrowableHeart_Projectile", "SunFlowers", "RobotCannon_Projectile", "RobotCannon_ProjectileImpact", "SmokeBomb_ExplosionEffect", "FireCracker_ExplosionEffect", "GorillaMouth"
	};

	public static readonly Dictionary<string, UnityTag> StringToTag;
}
