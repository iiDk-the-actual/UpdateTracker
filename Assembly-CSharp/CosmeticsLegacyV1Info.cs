using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class CosmeticsLegacyV1Info
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetPlayFabId(string unityItemId, string unityDisplayName, string unityOverrideDisplayName, out string playFabId)
	{
		return CosmeticsLegacyV1Info.k_v1DisplayNames_to_playFabIds.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_v1DisplayNames_to_playFabIds.TryGetValue(unityDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_v1DisplayNames_to_playFabIds.TryGetValue(unityOverrideDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_special.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_special.TryGetValue(unityDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_special.TryGetValue(unityOverrideDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_packs.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_packs.TryGetValue(unityDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_packs.TryGetValue(unityOverrideDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_oldPacks.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_oldPacks.TryGetValue(unityDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_oldPacks.TryGetValue(unityOverrideDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_unused.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_unused.TryGetValue(unityDisplayName, out playFabId) || CosmeticsLegacyV1Info.k_unused.TryGetValue(unityOverrideDisplayName, out playFabId);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetPlayFabId(string unityItemId, out string playFabId, bool logErrors = false)
	{
		return CosmeticsLegacyV1Info.k_v1DisplayNames_to_playFabIds.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_special.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_packs.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_oldPacks.TryGetValue(unityItemId, out playFabId) || CosmeticsLegacyV1Info.k_unused.TryGetValue(unityItemId, out playFabId);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetBodyDockAllObjectsIndexes(string playFabId, out int[] bdAllIndexes)
	{
		return CosmeticsLegacyV1Info._k_playFabId_to_bodyDockPositions_allObjects_indexes.TryGetValue(playFabId, out bdAllIndexes);
	}

	public const int k_bodyDockPositions_allObjects_length = 224;

	private static readonly Dictionary<string, string> k_special = new Dictionary<string, string> { { "Slingshot", "Slingshot" } };

	private static readonly Dictionary<string, string> k_packs = new Dictionary<string, string>
	{
		{ "TUXEDO SET", "LSAAO." },
		{ "EXPLORER SET", "LSAAN." },
		{ "SANTA SET 22", "LSAAP." },
		{ "SNOWMAN SET", "LSAAQ." },
		{ "EVIL SANTA SET", "LSAAR." },
		{ "Day 1 Pack", "LSAAP2." },
		{ "DAY 1 PACK", "LSAAP2." },
		{ "LAUNCH BUNDLE", "LSAAP2." },
		{ "LSAAP.2. (1)", "LSAAP2." },
		{ "POLAR BEAR SET", "LSAAT." },
		{ "WIZARD SET", "LSAAV." },
		{ "KNIGHT SET", "LSAAW." },
		{ "BARBARIAN SET", "LSAAX." },
		{ "ORC SET", "LSAAY." },
		{ "LSAAS.", "LSAAS." },
		{ "LSAAU.", "LSAAU." },
		{ "MERFOLK SET", "LSAAZ." },
		{ "SCUBA SET", "LSABA." },
		{ "SAFARI SET", "LSABB." },
		{ "CRYSTAL CAVERNS SET", "LSABC." },
		{ "SPIDER MONKE PACK", "LSABD." },
		{ "HOLIDAY FIR PACK", "LSABE." },
		{ "MAD SCIENTIST PACK", "LSABF." },
		{ "I LAVA YOU PACK", "LSABG." },
		{ "BEEKEEPER PACK", "LSABH." },
		{ "LEAF NINJA PACK", "LSABJ." },
		{ "MONKE MONK PACK", "LSABK." },
		{ "GLAM ROCKER PACK", "LSABL." }
	};

	private static readonly Dictionary<string, string> k_oldPacks = new Dictionary<string, string>
	{
		{ "CLOWN SET", "CLOWN SET" },
		{ "VAMPIRE SET", "VAMPIRE SET" },
		{ "WEREWOLF SET", "WEREWOLF SET" },
		{ "STAR PRINCESS SET", "STAR PRINCESS SET" },
		{ "SANTA SET", "SANTA SET" },
		{ "CARDBOARD ARMOR SET", "CARDBOARD ARMOR SET" },
		{ "SPIKED ARMOR SET", "SPIKED ARMOR SET" },
		{ "GORILLA ARMOR SET", "GORILLA ARMOR SET" },
		{ "SHERIFF SET", "SHERIFF SET" },
		{ "ROBOT SET", "ROBOT SET" },
		{ "CLOWN 22 SET", "CLOWN 22 SET" },
		{ "SUPER HERO SET", "SUPER HERO SET" },
		{ "UNICORN PRINCESS SET", "UNICORN PRINCESS SET" }
	};

	private static readonly Dictionary<string, string> k_unused = new Dictionary<string, string>
	{
		{ "HIGH TECH SLINGSHOT", "HIGH TECH SLINGSHOT" },
		{ "THROWABLE SQUISHY EYEBALL", "THROWABLE SQUISHY EYEBALL" }
	};

	private static readonly Dictionary<string, string> k_v1DisplayNames_to_playFabIds = new Dictionary<string, string>
	{
		{ "TREE PIN", "LBAAA." },
		{ "BOWTIE", "LBAAB." },
		{ "BASIC SCARF", "LBAAC." },
		{ "ADMINISTRATOR BADGE", "LBAAD." },
		{ "EARLY ACCESS", "LBAAE." },
		{ "CRYSTALS PIN", "LBAAF." },
		{ "CANYON PIN", "LBAAG." },
		{ "CITY PIN", "LBAAH." },
		{ "GORILLA PIN", "LBAAI." },
		{ "NECK SCARF", "LBAAJ." },
		{ "MOD STICK", "LBAAK." },
		{ "CLOWN FRILL", "LBAAL." },
		{ "VAMPIRE COLLAR", "LBAAM." },
		{ "WEREWOLF CLAWS", "LBAAN." },
		{ "STAR PRINCESS WAND", "LBAAO." },
		{ "TURKEY LEG", "LBAAP." },
		{ "TURKEY FINGER PUPPET", "LBAAQ." },
		{ "CANDY CANE", "LBAAR." },
		{ "SPARKLER", "LBAAS." },
		{ "ICICLE", "LBAAT." },
		{ "CHEST HEART", "LBAAU." },
		{ "RED ROSE", "LBAAV." },
		{ "PINK ROSE", "LBAAW." },
		{ "BLACK ROSE", "LBAAX." },
		{ "GOLD ROSE", "LBAAY." },
		{ "GT1 BADGE", "LBAAZ." },
		{ "THUMB PARTYHATS", "LBABA." },
		{ "REGULAR WRENCH", "LBABB." },
		{ "GOLD WRENCH", "LBABC." },
		{ "REGULAR FORK AND KNIFE", "LBABD." },
		{ "GOLD FORK AND KNIFE", "LBABE." },
		{ "FOUR LEAF CLOVER", "LBABF." },
		{ "GOLDEN FOUR LEAF CLOVER", "LBABG." },
		{ "MOUNTAIN PIN", "LBABH." },
		{ "YELLOW RAIN SHAWL", "LBABI." },
		{ "POCKET GORILLA BUN YELLOW", "LBABJ." },
		{ "POCKET GORILLA BUN BLUE", "LBABK." },
		{ "POCKET GORILLA BUN PINK", "LBABL." },
		{ "BONGOS", "LBABM." },
		{ "DRUM SET", "LBABN." },
		{ "SPILLED ICE CREAM", "LBABO." },
		{ "FLAMINGO FLOATIE", "LBABP." },
		{ "PAINTBALL SNOW VEST", "LBABQ." },
		{ "PAINTBALL FOREST VEST", "LBABR." },
		{ "CARDBOARD ARMOR", "LBABS." },
		{ "GORILLA ARMOR", "LBABT." },
		{ "SPIKED ARMOR", "LBABU." },
		{ "CLOWN VEST", "LBABV." },
		{ "ROBOT BODY", "LBABW." },
		{ "SHERIFF VEST", "LBABX." },
		{ "SUPER HERO BODY", "LBABY." },
		{ "UNICORN TUTU", "LBABZ." },
		{ "BIG EYEBROWS", "LFAAA." },
		{ "NOSE RING", "LFAAB." },
		{ "BASIC EARRINGS", "LFAAC." },
		{ "TRIPLE EARRINGS", "LFAAD." },
		{ "EYEBROW STUD", "LFAAE." },
		{ "TRIANGLE SUNGLASSES", "LFAAF." },
		{ "SKULL MASK", "LFAAG." },
		{ "RIGHT EYEPATCH", "LFAAH." },
		{ "LEFT EYEPATCH", "LFAAI." },
		{ "DOUBLE EYEPATCH", "LFAAJ." },
		{ "GOGGLES", "LFAAK." },
		{ "SURGICAL MASK", "LFAAL." },
		{ "TORTOISESHELL SUNGLASSES", "LFAAM." },
		{ "AVIATORS", "LFAAN." },
		{ "ROUND SUNGLASSES", "LFAAO." },
		{ "WITCH NOSE", "LFAAP." },
		{ "MUMMY WRAP", "LFAAQ." },
		{ "CLOWN NOSE", "LFAAR." },
		{ "VAMPIRE FANGS", "LFAAS." },
		{ "WEREWOLF FACE", "LFAAT." },
		{ "STAR PRINCESS GLASSES", "LFAAU." },
		{ "MAPLE LEAF", "LFAAV." },
		{ "FACE SCARF", "LFAAW." },
		{ "SANTA BEARD", "LFAAX." },
		{ "ORNAMENT EARRINGS", "LFAAY." },
		{ "2022 GLASSES", "LFAAZ." },
		{ "NOSE SNOWFLAKE", "LFABA." },
		{ "ROSY CHEEKS", "LFABB." },
		{ "BOXY SUNGLASSES", "LFABC." },
		{ "HEART GLASSES", "LFABD." },
		{ "COOKIE JAR", "LFABE." },
		{ "BITE ONION", "LFABF." },
		{ "EMPEROR NOSE BUTTERFLY", "LFABG." },
		{ "FOREHEAD EGG", "LFABH." },
		{ "LIGHTNING MAKEUP", "LFABI." },
		{ "BLUE SHUTTERS", "LFABJ." },
		{ "BLACK SHUTTERS", "LFABK." },
		{ "GREEN SHUTTERS", "LFABL." },
		{ "RED SHUTTERS", "LFABM." },
		{ "SUNBURN", "LFABN." },
		{ "SUNSCREEN", "LFABO." },
		{ "PAINTBALL FOREST VISOR", "LFABP." },
		{ "PAINTBALL SNOW VISOR", "LFABQ." },
		{ "PAINTBALL GORILLA VISOR", "LFABR." },
		{ "BULGING GOOGLY EYES", "LFABS." },
		{ "CLOWN NOSE 22", "LFABT." },
		{ "SHERIFF MUSTACHE", "LFABU." },
		{ "SLINKY EYES", "LFABV." },
		{ "MOUTH WHEAT", "LFABW." },
		{ "BANANA HAT", "LHAAA." },
		{ "CAT EARS", "LHAAB." },
		{ "PARTY HAT", "LHAAC." },
		{ "USHANKA", "LHAAD." },
		{ "SWEATBAND", "LHAAE." },
		{ "BASEBALL CAP", "LHAAF." },
		{ "GOLDEN HEAD", "LHAAG." },
		{ "FOREHEAD MIRROR", "LHAAH." },
		{ "PINEAPPLE HAT", "LHAAI." },
		{ "WITCH HAT", "LHAAJ." },
		{ "COCONUT", "LHAAK." },
		{ "SUNHAT", "LHAAL." },
		{ "CLOCHE", "LHAAM." },
		{ "COWBOY HAT", "LHAAN." },
		{ "FEZ", "LHAAO." },
		{ "TOP HAT", "LHAAP." },
		{ "BASIC BEANIE", "LHAAQ." },
		{ "WHITE FEDORA", "LHAAR." },
		{ "FLOWER CROWN", "LHAAS." },
		{ "PAPERBAG HAT", "LHAAT." },
		{ "PUMPKIN HAT", "LHAAU." },
		{ "CLOWN WIG", "LHAAV." },
		{ "VAMPIRE WIG", "LHAAW." },
		{ "WEREWOLF EARS", "LHAAX." },
		{ "STAR PRINCESS TIARA", "LHAAY." },
		{ "PIRATE BANDANA", "LHAAZ." },
		{ "SUNNY SUNHAT", "LHABA." },
		{ "CHROME COWBOY HAT", "LHABB." },
		{ "CHEFS HAT", "LHABC." },
		{ "SANTA HAT", "LHABD." },
		{ "SNOWMAN HAT", "LHABE." },
		{ "GIFT HAT", "LHABF." },
		{ "ELF HAT", "LHABG." },
		{ "ORANGE POMPOM HAT", "LHABH." },
		{ "BLUE POMPOM HAT", "LHABI." },
		{ "STRIPE POMPOM HAT", "LHABJ." },
		{ "PATTERN POMPOM HAT", "LHABK." },
		{ "WHITE EARMUFFS", "LHABL." },
		{ "BLACK EARMUFFS", "LHABM." },
		{ "GREEN EARMUFFS", "LHABN." },
		{ "PINK EARMUFFS", "LHABO." },
		{ "HEADPHONES1", "LHABP." },
		{ "BOX OF CHOCOLATES HAT", "LHABQ." },
		{ "HEART POMPOM HAT", "LHABR." },
		{ "PLUNGER HAT", "LHABS." },
		{ "SAUCEPAN HAT", "LHABT." },
		{ "WHITE BUNNY EARS", "LHABU." },
		{ "BROWN BUNNY EARS", "LHABV." },
		{ "LEPRECHAUN HAT", "LHABW." },
		{ "BLUE LILY HAT", "LHABX." },
		{ "PURPLE LILY HAT", "LHABY." },
		{ "YELLOW RAIN HAT", "LHABZ." },
		{ "PAINTED EGG HAT", "LHACA." },
		{ "BLACK LONGHAIR WIG", "LHACB." },
		{ "RED LONGHAIR WIG", "LHACC." },
		{ "ELECTRO HELM", "LHACD." },
		{ "SEAGULL", "LHACE." },
		{ "ROCKIN MOHAWK", "LHACF." },
		{ "SPIKED HELMET", "LHACG." },
		{ "CARDBOARD HELMET", "LHACH." },
		{ "CLOWN CAP", "LHACI." },
		{ "PUMPKIN HEAD HAPPY", "LHACJ." },
		{ "PUMPKIN HEAD SCARY", "LHACK." },
		{ "ROBOT HEAD", "LHACL." },
		{ "SHERIFF HAT", "LHACM." },
		{ "UNICORN CROWN", "LHACN." },
		{ "SUPER HERO HEADBAND", "LHACO." },
		{ "PIE HAT", "LHACP." },
		{ "SCARECROW HAT", "LHACQ." },
		{ "CHERRY BLOSSOM BRANCH", "LMAAA." },
		{ "CHERRY BLOSSOM BRANCH ROSE GOLD", "LMAAB." },
		{ "YELLOW HAND BOOTS", "LMAAC." },
		{ "CLOUD HAND BOOTS", "LMAAD." },
		{ "GOLDEN HAND BOOTS", "LMAAE." },
		{ "BLACK UMBRELLA", "LMAAF." },
		{ "COLORFUL UMBRELLA", "LMAAG." },
		{ "GOLDEN UMBRELLA", "LMAAH." },
		{ "ACOUSTIC GUITAR", "LMAAI." },
		{ "GOLDEN ACOUSTIC GUITAR", "LMAAJ." },
		{ "ELECTRIC GUITAR", "LMAAK." },
		{ "GOLDEN ELECTRIC GUITAR", "LMAAL." },
		{ "BUBBLER", "LMAAM." },
		{ "POPSICLE", "LMAAN." },
		{ "RUBBER DUCK", "LMAAO." },
		{ "STAR BALLOON", "LMAAP." },
		{ "STAR BALLON", "LMAAP." },
		{ "STICKABLE TAR.GET", "LMAAQ." },
		{ "STICKABLE TARGET", "LMAAQ." },
		{ "DIAMOND BALLOON", "LMAAR." },
		{ "CHOCOLATE DONUT BALLOON", "LMAAS." },
		{ "HEART BALLOON", "LMAAT." },
		{ "FINGER FLAG", "LMAAU." },
		{ "HIGH TECH S.LINGSHOT", "LMAAV." },
		{ "UNICORN STAFF", "LMAAW." },
		{ "GHOST BALLOON", "LMAAX." },
		{ "GIANT CANDY BAR", "LMAAY." },
		{ "CANDY BAR FUN SIZE", "LMAAZ." },
		{ "SPIDER WEB UMBRELLA", "LMABA." },
		{ "DEADSHOT", "LMABB." },
		{ "YORICK", "LMABC." },
		{ "PINK DONUT BALLOON", "LMABD." },
		{ "TURKEY TOY", "LMABE." },
		{ "CRANBERRY CAN", "LMABF." },
		{ "FRYING PAN", "LMABG." },
		{ "BALLOON TURKEY", "LMABH." },
		{ "CANDY APPLE", "LMABI." },
		{ "CARAMEL APPLE", "LMABJ." },
		{ "PIE SLICE", "LMABK." },
		{ "LADLE", "LMABL." },
		{ "TURKEY LEG 22", "LMABM." },
		{ "CORN ON THE COB", "LMABN." },
		{ "FINGER OLIVES", "LMABO." }
	};

	private static readonly Dictionary<string, int[]> _k_playFabId_to_bodyDockPositions_allObjects_indexes = new Dictionary<string, int[]>
	{
		{
			"LMAAC.",
			new int[] { 0, 1 }
		},
		{
			"LMAAD.",
			new int[] { 2, 3 }
		},
		{
			"LMAAE.",
			new int[] { 4, 5 }
		},
		{
			"LMAAK.",
			new int[] { 6, 7 }
		},
		{
			"LMAAL.",
			new int[] { 8, 9 }
		},
		{
			"LMAAF.",
			new int[] { 10 }
		},
		{
			"LBABE.",
			new int[] { 11, 12 }
		},
		{
			"LBABD.",
			new int[] { 13, 14 }
		},
		{
			"LMAAG.",
			new int[] { 15 }
		},
		{
			"LMAAH.",
			new int[] { 16 }
		},
		{
			"LMAAO.",
			new int[] { 17 }
		},
		{
			"LBABB.",
			new int[] { 18 }
		},
		{
			"LBAAP.",
			new int[] { 19 }
		},
		{
			"LBAAS.",
			new int[] { 20 }
		},
		{
			"LBAAT.",
			new int[] { 21 }
		},
		{
			"LBAAY.",
			new int[] { 22 }
		},
		{
			"LBABC.",
			new int[] { 23 }
		},
		{
			"LBAAW.",
			new int[] { 24 }
		},
		{
			"LBAAV.",
			new int[] { 25 }
		},
		{
			"LBABF.",
			new int[] { 26 }
		},
		{
			"LBAAX.",
			new int[] { 27 }
		},
		{
			"LBAAK.",
			new int[] { 28 }
		},
		{
			"LBABG.",
			new int[] { 29 }
		},
		{
			"LBAAO.",
			new int[] { 30 }
		},
		{
			"LMAAA.",
			new int[] { 31 }
		},
		{
			"LMAAB.",
			new int[] { 32 }
		},
		{
			"LMAAM.",
			new int[] { 33 }
		},
		{
			"LMAAN.",
			new int[] { 34 }
		},
		{
			"LBAAR.",
			new int[] { 35 }
		},
		{
			"LMAAQ.",
			new int[] { 36 }
		},
		{
			"LMAAP.",
			new int[] { 37 }
		},
		{
			"LMAAR.",
			new int[] { 38 }
		},
		{
			"LMAAS.",
			new int[] { 39 }
		},
		{
			"LMABD.",
			new int[] { 40 }
		},
		{
			"LMAAT.",
			new int[] { 41 }
		},
		{
			"LMABA.",
			new int[] { 42 }
		},
		{
			"LMAAW.",
			new int[] { 43 }
		},
		{
			"LMAAX.",
			new int[] { 44 }
		},
		{
			"LMAAY.",
			new int[] { 45 }
		},
		{
			"LMAAZ.",
			new int[] { 46 }
		},
		{
			"LMABC.",
			new int[] { 47 }
		},
		{
			"LMABE.",
			new int[] { 48 }
		},
		{
			"LMABF.",
			new int[] { 49 }
		},
		{
			"LMABI.",
			new int[] { 50 }
		},
		{
			"LMABJ.",
			new int[] { 51 }
		},
		{
			"LMABH.",
			new int[] { 52 }
		},
		{
			"LMABG.",
			new int[] { 53 }
		},
		{
			"LMABL.",
			new int[] { 54 }
		},
		{
			"LMABM.",
			new int[] { 55 }
		},
		{
			"LMABK.",
			new int[] { 56 }
		},
		{
			"LMABN.",
			new int[] { 57 }
		},
		{
			"LMABS.",
			new int[] { 58 }
		},
		{
			"LMABR.",
			new int[] { 59 }
		},
		{
			"LMABT.",
			new int[] { 60 }
		},
		{
			"LMABP.",
			new int[] { 61 }
		},
		{
			"LMABQ.",
			new int[] { 62 }
		},
		{
			"LMABU.",
			new int[] { 63 }
		},
		{
			"LMABW.",
			new int[] { 64 }
		},
		{
			"LMABX.",
			new int[] { 65 }
		},
		{
			"LMACB.",
			new int[] { 66 }
		},
		{
			"LMACC.",
			new int[] { 67 }
		},
		{
			"LMACD.",
			new int[] { 68 }
		},
		{
			"LMACI.",
			new int[] { 69 }
		},
		{
			"LMACJ.",
			new int[] { 70 }
		},
		{
			"LMACL.",
			new int[] { 71 }
		},
		{
			"LMACR.",
			new int[] { 72 }
		},
		{
			"LMACQ.",
			new int[] { 73 }
		},
		{
			"LMACS.",
			new int[] { 74 }
		},
		{
			"LMACP.",
			new int[] { 75 }
		},
		{
			"LMACT.",
			new int[] { 76 }
		},
		{
			"LMACV.",
			new int[] { 77 }
		},
		{
			"LMACW.",
			new int[] { 78 }
		},
		{
			"LMACY.",
			new int[] { 79 }
		},
		{
			"LMADA.",
			new int[] { 80 }
		},
		{
			"LMADB.",
			new int[] { 81 }
		},
		{
			"LMADD.",
			new int[] { 82 }
		},
		{
			"LMADE.",
			new int[] { 83 }
		},
		{
			"LMADH.",
			new int[] { 84 }
		},
		{
			"LMADJ.",
			new int[] { 85 }
		},
		{
			"LMADK.",
			new int[] { 86 }
		},
		{
			"LMADL.",
			new int[] { 87 }
		},
		{
			"LMADM.",
			new int[] { 88 }
		},
		{
			"LMADN.",
			new int[] { 89 }
		},
		{
			"LMADQ.",
			new int[] { 90 }
		},
		{
			"LMADR.",
			new int[] { 91 }
		},
		{
			"LMADS.",
			new int[] { 92 }
		},
		{
			"LMADV.",
			new int[] { 93 }
		},
		{
			"LMADW.",
			new int[] { 94 }
		},
		{
			"LMADX.",
			new int[] { 95 }
		},
		{
			"LMADZ.",
			new int[] { 96 }
		},
		{
			"LMAEA.",
			new int[] { 97 }
		},
		{
			"LMAEB.",
			new int[] { 98 }
		},
		{
			"LMAEC.",
			new int[] { 99 }
		},
		{
			"LMAED.",
			new int[] { 100 }
		},
		{
			"LMAEF.",
			new int[] { 101 }
		},
		{
			"LMAEG.",
			new int[] { 102 }
		},
		{
			"LMAEH.",
			new int[] { 103 }
		},
		{
			"LMADY.",
			new int[] { 104 }
		},
		{
			"LMAEK.",
			new int[] { 105 }
		},
		{
			"LMAEL.",
			new int[] { 106 }
		},
		{
			"LMAEM.",
			new int[] { 107 }
		},
		{
			"LMAEN.",
			new int[] { 108 }
		},
		{
			"LMAEP.",
			new int[] { 109 }
		},
		{
			"LMAEQ.",
			new int[] { 110 }
		},
		{
			"LMAES.",
			new int[] { 111 }
		},
		{
			"LMAEU.",
			new int[] { 112 }
		},
		{
			"LMAER.",
			new int[] { 113 }
		},
		{
			"LMAET.",
			new int[] { 114 }
		},
		{
			"LMAFH.",
			new int[] { 115 }
		},
		{
			"LMAFA.",
			new int[] { 116 }
		},
		{
			"LMAFB.",
			new int[] { 117 }
		},
		{
			"LMAFC.",
			new int[] { 118 }
		},
		{
			"LMAFD.",
			new int[] { 119 }
		},
		{
			"LMAFE.",
			new int[] { 120 }
		},
		{
			"LMAFF.",
			new int[] { 121 }
		},
		{
			"LMAFI.",
			new int[] { 122 }
		},
		{
			"LMAFG.",
			new int[] { 123 }
		},
		{
			"LMAFJ.",
			new int[] { 124 }
		},
		{
			"LMAFL.",
			new int[] { 125 }
		},
		{
			"LMAFM.",
			new int[] { 126 }
		},
		{
			"LMAFO.",
			new int[] { 127 }
		},
		{
			"LMAFP.",
			new int[] { 128 }
		},
		{
			"LMAFR.",
			new int[] { 129 }
		},
		{
			"LMAFS.",
			new int[] { 130 }
		},
		{
			"LMAFQ.",
			new int[] { 131 }
		},
		{
			"LMAFT.",
			new int[] { 132 }
		},
		{
			"LMAFU.",
			new int[] { 133 }
		},
		{
			"LMAFV.",
			new int[] { 134 }
		},
		{
			"LMAFW.",
			new int[] { 135 }
		},
		{
			"LMAFZ.",
			new int[] { 136 }
		},
		{
			"LMAGA.",
			new int[] { 137 }
		},
		{
			"LMAGC.",
			new int[] { 138 }
		},
		{
			"LMAGB.",
			new int[] { 139 }
		},
		{
			"LMAGF.",
			new int[] { 140 }
		},
		{
			"LMAGG.",
			new int[] { 141 }
		},
		{
			"LMAGI.",
			new int[] { 142 }
		},
		{
			"LMAGK.",
			new int[] { 143 }
		},
		{
			"LMAGL.",
			new int[] { 144 }
		},
		{
			"LMAGN.",
			new int[] { 145 }
		},
		{
			"LMAGO.",
			new int[] { 146 }
		},
		{
			"LMAGQ.",
			new int[] { 147 }
		},
		{
			"LMAGZ.",
			new int[] { 148 }
		},
		{
			"LMAGS.",
			new int[] { 149 }
		},
		{
			"LMAGV.",
			new int[] { 150 }
		},
		{
			"LMAGW.",
			new int[] { 151 }
		},
		{
			"LMAGY.",
			new int[] { 152 }
		},
		{
			"LMAHA.",
			new int[] { 153 }
		},
		{
			"LMAHB.",
			new int[] { 154 }
		},
		{
			"LMAHD.",
			new int[] { 155 }
		},
		{
			"LMAHE.",
			new int[] { 156 }
		},
		{
			"LMAHF.",
			new int[] { 157 }
		},
		{
			"LMAHG.",
			new int[] { 158 }
		},
		{
			"LMAHI.",
			new int[] { 159 }
		},
		{
			"LMAHJ.",
			new int[] { 160 }
		},
		{
			"LMAHK.",
			new int[] { 161 }
		},
		{
			"LMAHO.",
			new int[] { 162 }
		},
		{
			"LMAHM.",
			new int[] { 163 }
		},
		{
			"LMAHN.",
			new int[] { 164 }
		},
		{
			"LMAHP.",
			new int[] { 165 }
		},
		{
			"LMAHS.",
			new int[] { 166 }
		},
		{
			"LMAHT.",
			new int[] { 167 }
		},
		{
			"LMAHU.",
			new int[] { 168 }
		},
		{
			"LMAHV.",
			new int[] { 169 }
		},
		{
			"LMAHZ.",
			new int[] { 170 }
		},
		{
			"LMAIA.",
			new int[] { 171 }
		},
		{
			"LMAHW.",
			new int[] { 172 }
		},
		{
			"LMAHY.",
			new int[] { 173 }
		},
		{
			"LMAHX.",
			new int[] { 174 }
		},
		{
			"LMAII.",
			new int[] { 175 }
		},
		{
			"LMAIH.",
			new int[] { 176 }
		},
		{
			"LMAIJ.",
			new int[] { 177 }
		},
		{
			"LMAIK.",
			new int[] { 178 }
		},
		{
			"LMAIL.",
			new int[] { 179 }
		},
		{
			"LMAIN.",
			new int[] { 180 }
		},
		{
			"LMAIQ.",
			new int[] { 181 }
		},
		{
			"LMAIS.",
			new int[] { 182 }
		},
		{
			"LMAIT.",
			new int[] { 183 }
		},
		{
			"LMAIU.",
			new int[] { 184 }
		},
		{
			"LMAIX.",
			new int[] { 185 }
		},
		{
			"LMAIW.",
			new int[] { 186 }
		},
		{
			"LMAIV.",
			new int[] { 187 }
		},
		{
			"LMAIY.",
			new int[] { 188 }
		},
		{
			"LMAIZ.",
			new int[] { 189 }
		},
		{
			"LMAJA.",
			new int[] { 190 }
		},
		{
			"LMAJB.",
			new int[] { 191 }
		},
		{
			"LMAJC.",
			new int[] { 192 }
		},
		{
			"LMAJD.",
			new int[] { 193 }
		},
		{
			"LMAJE.",
			new int[] { 194 }
		},
		{
			"LMAJF.",
			new int[] { 195 }
		},
		{
			"LMAJH.",
			new int[] { 196 }
		},
		{
			"LMAJI.",
			new int[] { 197 }
		},
		{
			"LMAJJ.",
			new int[] { 198 }
		},
		{
			"LMAJN.",
			new int[] { 199 }
		},
		{
			"LMAJK.",
			new int[] { 200 }
		},
		{
			"LMAJL.",
			new int[] { 201 }
		},
		{
			"LMAJM.",
			new int[] { 202 }
		},
		{
			"LMAJS.",
			new int[] { 203 }
		},
		{
			"LMAJT.",
			new int[] { 204 }
		},
		{
			"LMAJU.",
			new int[] { 205 }
		},
		{
			"LMAJW.",
			new int[] { 206 }
		},
		{
			"LMAJX.",
			new int[] { 207 }
		},
		{
			"LMAJZ.",
			new int[] { 208 }
		},
		{
			"LMAKA.",
			new int[] { 209 }
		},
		{
			"LMAKB.",
			new int[] { 210 }
		},
		{
			"LMAJV.",
			new int[] { 211 }
		},
		{
			"Slingshot",
			new int[] { 212 }
		},
		{
			"HIGH TECH SLINGSHOT",
			new int[] { 213 }
		},
		{
			"LMABB.",
			new int[] { 214 }
		},
		{
			"LMABV.",
			new int[] { 215 }
		},
		{
			"LMACU.",
			new int[] { 216 }
		},
		{
			"LMADC.",
			new int[] { 217 }
		},
		{
			"LMADU.",
			new int[] { 218 }
		},
		{
			"LMAGJ.",
			new int[] { 219 }
		},
		{
			"LMAGR.",
			new int[] { 220 }
		},
		{
			"LMAIG.",
			new int[] { 221 }
		},
		{
			"LMAJQ.",
			new int[] { 222 }
		},
		{
			"LMAJP.",
			new int[] { 223 }
		}
	};
}
