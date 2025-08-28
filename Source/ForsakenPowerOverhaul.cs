using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	[BepInDependency(Jotunn.Main.ModGuid)]
	[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
	
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		const string PluginGUID = "Tidalwave.Valheim.ForsakenPowerOverhaul";
		const string PluginName = "Forsaken Power Overhaul";
		const string PluginVersion = "2.1.1";
		
		// Cached field accessors for performance
		private static FieldInfo PlayerSemanField;
		private static FieldInfo PlayerUniquesField;
		private static FieldInfo PlayerEquipmentModifierValuesField;
		private static FieldInfo PlayerFoodRegenTimerField;
		private static FieldInfo PlayerGuardianSEField;
		private static FieldInfo HumanoidRightItemField;
		private static FieldInfo HumanoidLeftItemField;
		private static FieldInfo HumanoidChestItemField;
		private static FieldInfo HumanoidLegItemField;
		private static FieldInfo HumanoidHelmetItemField;
		private static FieldInfo HumanoidShoulderItemField;
		private static FieldInfo HumanoidUtilityItemField;
		private static FieldInfo ShipPlayersField;
		private static FieldInfo TextsDialogTextsField;
		
		static List<string> List_BossNames = new List<string>{ "Eikthyr", "TheElder", "Bonemass", "Moder", "Yagluth", "Queen", "Fader" };
		static List<string> List_Components = new List<string>{ "Passive", "Equipped", "Active", "Shared" };
		
		static List<StatusEffect_FPO> List_StatusEffect_FPO = new List<StatusEffect_FPO>();
		static List<StatusEffect_FPO> List_StatusEffect_FPO_Passive = new List<StatusEffect_FPO>();
		static List<StatusEffect_FPO> List_StatusEffect_FPO_Equipped = new List<StatusEffect_FPO>();
		static List<StatusEffect_FPO> List_StatusEffect_FPO_Active = new List<StatusEffect_FPO>();
		static List<StatusEffect_FPO> List_StatusEffect_FPO_Shared = new List<StatusEffect_FPO>();
		
		static bool Mod_Auga = false;
		static bool Mod_EpicLoot = false;
		
		void Awake()
		{
			// Initialize cached field accessors for performance
			InitializeFieldAccessors();
			InitializePatchFieldAccessors();
			
			Add_ConfigEntries();
			Add_Buttons();
			Add_StatusEffects();
			Add_Translations();
			
			Harmony New_Harmony = new Harmony(PluginGUID);
			New_Harmony.PatchAll();
			
			PrefabManager.OnVanillaPrefabsAvailable += Add_Prefabs;
		}
		
		private static void InitializeFieldAccessors()
		{
			PlayerSemanField = AccessTools.Field(typeof(Character), "m_seman");
			PlayerUniquesField = AccessTools.Field(typeof(Player), "m_uniques");
			PlayerEquipmentModifierValuesField = AccessTools.Field(typeof(Player), "m_equipmentModifierValues");
			PlayerFoodRegenTimerField = AccessTools.Field(typeof(Player), "m_foodRegenTimer");
			PlayerGuardianSEField = AccessTools.Field(typeof(Player), "m_guardianSE");
			HumanoidRightItemField = AccessTools.Field(typeof(Humanoid), "m_rightItem");
			HumanoidLeftItemField = AccessTools.Field(typeof(Humanoid), "m_leftItem");
			HumanoidChestItemField = AccessTools.Field(typeof(Humanoid), "m_chestItem");
			HumanoidLegItemField = AccessTools.Field(typeof(Humanoid), "m_legItem");
			HumanoidHelmetItemField = AccessTools.Field(typeof(Humanoid), "m_helmetItem");
			HumanoidShoulderItemField = AccessTools.Field(typeof(Humanoid), "m_shoulderItem");
			HumanoidUtilityItemField = AccessTools.Field(typeof(Humanoid), "m_utilityItem");
			ShipPlayersField = AccessTools.Field(typeof(Ship), "m_players");
			TextsDialogTextsField = AccessTools.Field(typeof(TextsDialog), "m_texts");
		}
		
		// High-performance field access helpers
		private static SEMan GetSEMan(Character character) => (SEMan)PlayerSemanField.GetValue(character);
		private static HashSet<string> GetPlayerUniques(Player player) => (HashSet<string>)PlayerUniquesField.GetValue(player);
		private static float GetFoodRegenTimer(Player player) => (float)PlayerFoodRegenTimerField.GetValue(player);
		private static object GetGuardianSE(Player player) => PlayerGuardianSEField.GetValue(player);
		private static object GetEquipmentModifierValues(Player player) => PlayerEquipmentModifierValuesField.GetValue(player);
		private static ItemDrop.ItemData GetRightItem(Humanoid humanoid) => (ItemDrop.ItemData)HumanoidRightItemField.GetValue(humanoid);
		private static ItemDrop.ItemData GetLeftItem(Humanoid humanoid) => (ItemDrop.ItemData)HumanoidLeftItemField.GetValue(humanoid);
		private static ItemDrop.ItemData GetChestItem(Humanoid humanoid) => (ItemDrop.ItemData)HumanoidChestItemField.GetValue(humanoid);
		private static ItemDrop.ItemData GetLegItem(Humanoid humanoid) => (ItemDrop.ItemData)HumanoidLegItemField.GetValue(humanoid);
		private static ItemDrop.ItemData GetHelmetItem(Humanoid humanoid) => (ItemDrop.ItemData)HumanoidHelmetItemField.GetValue(humanoid);
		private static ItemDrop.ItemData GetShoulderItem(Humanoid humanoid) => (ItemDrop.ItemData)HumanoidShoulderItemField.GetValue(humanoid);
		private static ItemDrop.ItemData GetUtilityItem(Humanoid humanoid) => (ItemDrop.ItemData)HumanoidUtilityItemField.GetValue(humanoid);
		private static List<Player> GetShipPlayers(Ship ship) => (List<Player>)ShipPlayersField.GetValue(ship);
		private static List<TextsDialog.TextInfo> GetTextsDialogTexts(TextsDialog dialog) => (List<TextsDialog.TextInfo>)TextsDialogTextsField.GetValue(dialog);
		
		void Start()
		{
			Mod_Auga = Chainloader.PluginInfos.ContainsKey("randyknapp.mods.auga");
			Mod_EpicLoot = Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot");
		}
		
		void Update()
		{
			if(ObjectDB.instance != null)
			{
				if(StatusEffect_FPO_Passive == null)
				{ Add_StatusEffectLists(); }
				
				if(Player.m_localPlayer != null)
				{
					if(!Player.m_localPlayer.IsDead())
					{
						Update_Player_StatusEffects();
						Update_Player_StatusEffects_Equipped();
						
						if(UnityEngine.Input.inputString.Length > 0)
						{
							if(ButtonConfig_PowerCycle != null && ConfigEntry_PowerCycle_Bool.Value)
							{
								if(UnityEngine.Input.GetKeyDown(KeyCode.G)) // Simple fallback for power cycling
								{
									if(Player.m_localPlayer.GetGuardianPowerName().StartsWith("GP_"))
									{ Power_Cycle(); }
								}
							}
						}
					}
				}
			}
		}
		
		static void Add_StatusEffectLists()
		{
			StatusEffect_FPO_Passive = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Passive"));
			StatusEffect_FPO_Eikthyr = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Eikthyr"));
			StatusEffect_FPO_TheElder = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_TheElder"));
			StatusEffect_FPO_Bonemass = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Bonemass"));
			StatusEffect_FPO_Moder = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Moder"));
			StatusEffect_FPO_Yagluth = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Yagluth"));
			StatusEffect_FPO_Queen = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Queen"));
			StatusEffect_FPO_Fader = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Fader"));
			
			StatusEffect_FPO_Eikthyr_Passive = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Eikthyr_Passive"));
			StatusEffect_FPO_TheElder_Passive = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_TheElder_Passive"));
			StatusEffect_FPO_Bonemass_Passive = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Bonemass_Passive"));
			StatusEffect_FPO_Moder_Passive = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Moder_Passive"));
			StatusEffect_FPO_Yagluth_Passive = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Yagluth_Passive"));
			StatusEffect_FPO_Queen_Passive = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Queen_Passive"));
			StatusEffect_FPO_Fader_Passive = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Fader_Passive"));
			
			StatusEffect_FPO_Eikthyr_Equipped = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Eikthyr_Equipped"));
			StatusEffect_FPO_TheElder_Equipped = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_TheElder_Equipped"));
			StatusEffect_FPO_Bonemass_Equipped = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Bonemass_Equipped"));
			StatusEffect_FPO_Moder_Equipped = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Moder_Equipped"));
			StatusEffect_FPO_Yagluth_Equipped = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Yagluth_Equipped"));
			StatusEffect_FPO_Queen_Equipped = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Queen_Equipped"));
			StatusEffect_FPO_Fader_Equipped = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Fader_Equipped"));
			
			StatusEffect_FPO_Eikthyr_Active = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Eikthyr_Active"));
			StatusEffect_FPO_TheElder_Active = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_TheElder_Active"));
			StatusEffect_FPO_Bonemass_Active = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Bonemass_Active"));
			StatusEffect_FPO_Moder_Active = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Moder_Active"));
			StatusEffect_FPO_Yagluth_Active = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Yagluth_Active"));
			StatusEffect_FPO_Queen_Active = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Queen_Active"));
			StatusEffect_FPO_Fader_Active = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Fader_Active"));
			
			StatusEffect_FPO_Eikthyr_Shared = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Eikthyr_Shared"));
			StatusEffect_FPO_TheElder_Shared = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_TheElder_Shared"));
			StatusEffect_FPO_Bonemass_Shared = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Bonemass_Shared"));
			StatusEffect_FPO_Moder_Shared = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Moder_Shared"));
			StatusEffect_FPO_Yagluth_Shared = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Yagluth_Shared"));
			StatusEffect_FPO_Queen_Shared = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Queen_Shared"));
			StatusEffect_FPO_Fader_Shared = (StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Fader_Shared"));
			
			List_StatusEffect_FPO.Clear();
			List_StatusEffect_FPO.Add(StatusEffect_FPO_Eikthyr);
			List_StatusEffect_FPO.Add(StatusEffect_FPO_TheElder);
			List_StatusEffect_FPO.Add(StatusEffect_FPO_Bonemass);
			List_StatusEffect_FPO.Add(StatusEffect_FPO_Moder);
			List_StatusEffect_FPO.Add(StatusEffect_FPO_Yagluth);
			List_StatusEffect_FPO.Add(StatusEffect_FPO_Queen);
			List_StatusEffect_FPO.Add(StatusEffect_FPO_Fader);
			
			List_StatusEffect_FPO_Passive.Clear();
			List_StatusEffect_FPO_Passive.Add(StatusEffect_FPO_Eikthyr_Passive);
			List_StatusEffect_FPO_Passive.Add(StatusEffect_FPO_TheElder_Passive);
			List_StatusEffect_FPO_Passive.Add(StatusEffect_FPO_Bonemass_Passive);
			List_StatusEffect_FPO_Passive.Add(StatusEffect_FPO_Moder_Passive);
			List_StatusEffect_FPO_Passive.Add(StatusEffect_FPO_Yagluth_Passive);
			List_StatusEffect_FPO_Passive.Add(StatusEffect_FPO_Queen_Passive);
			List_StatusEffect_FPO_Passive.Add(StatusEffect_FPO_Fader_Passive);
			
			List_StatusEffect_FPO_Equipped.Clear();
			List_StatusEffect_FPO_Equipped.Add(StatusEffect_FPO_Eikthyr_Equipped);
			List_StatusEffect_FPO_Equipped.Add(StatusEffect_FPO_TheElder_Equipped);
			List_StatusEffect_FPO_Equipped.Add(StatusEffect_FPO_Bonemass_Equipped);
			List_StatusEffect_FPO_Equipped.Add(StatusEffect_FPO_Moder_Equipped);
			List_StatusEffect_FPO_Equipped.Add(StatusEffect_FPO_Yagluth_Equipped);
			List_StatusEffect_FPO_Equipped.Add(StatusEffect_FPO_Queen_Equipped);
			List_StatusEffect_FPO_Equipped.Add(StatusEffect_FPO_Fader_Equipped);
			
			List_StatusEffect_FPO_Active.Clear();
			List_StatusEffect_FPO_Active.Add(StatusEffect_FPO_Eikthyr_Active);
			List_StatusEffect_FPO_Active.Add(StatusEffect_FPO_TheElder_Active);
			List_StatusEffect_FPO_Active.Add(StatusEffect_FPO_Bonemass_Active);
			List_StatusEffect_FPO_Active.Add(StatusEffect_FPO_Moder_Active);
			List_StatusEffect_FPO_Active.Add(StatusEffect_FPO_Yagluth_Active);
			List_StatusEffect_FPO_Active.Add(StatusEffect_FPO_Queen_Active);
			List_StatusEffect_FPO_Active.Add(StatusEffect_FPO_Fader_Active);
			
			List_StatusEffect_FPO_Shared.Clear();
			List_StatusEffect_FPO_Shared.Add(StatusEffect_FPO_Eikthyr_Shared);
			List_StatusEffect_FPO_Shared.Add(StatusEffect_FPO_TheElder_Shared);
			List_StatusEffect_FPO_Shared.Add(StatusEffect_FPO_Bonemass_Shared);
			List_StatusEffect_FPO_Shared.Add(StatusEffect_FPO_Moder_Shared);
			List_StatusEffect_FPO_Shared.Add(StatusEffect_FPO_Yagluth_Shared);
			List_StatusEffect_FPO_Shared.Add(StatusEffect_FPO_Queen_Shared);
			List_StatusEffect_FPO_Shared.Add(StatusEffect_FPO_Fader_Shared);
		}
		
		static void GlobalKeys_Update(BossStone New_BossStone)
		{
			string New_String;
			bool New_Bool = false;
			
			if(New_BossStone.m_itemStand.HaveAttachment())
			{
				New_String = GetBossName(New_BossStone.m_itemStand.m_currentItemName);
				
				if(!ZoneSystem.instance.GetGlobalKey("gk_fpo_bossstone_" + New_String))
				{
					if(New_String != "")
					{
						ZoneSystem.instance.SetGlobalKey("gk_fpo_bossstone_" + New_String);
						New_Bool = true;
					}
				}
			}
			
			if(New_Bool)
			{ ZoneSystem.instance.SetGlobalKey("gk_fpo_bossstone_passive"); }
		}
		
		static List<string> GlobalKeys_Sort()
		{
			List<string> GlobalKeys = new List<string>{ };
			
			foreach(string Boss in List_BossNames)
			{
				if(ZoneSystem.instance.GetGlobalKey("gk_fpo_bossstone_" + Boss))
				{ GlobalKeys.Add("gk_fpo_bossstone_" + Boss); }
			}
			
			return GlobalKeys.ToList();
		}
		
		static void PlayerKeys_Update()
		{
			if(Player.m_localPlayer != null)
			{
				foreach(string GlobalKey in ZoneSystem.instance.GetGlobalKeys())
				{
					string New_String = GetBossName(GlobalKey);
				
					if(New_String != "")
					{ Player.m_localPlayer.AddUniqueKey("GP_" + New_String); }
				}
			}
		}
		
		static void Update_Player_StatusEffects()
		{
			var seman = GetSEMan(Player.m_localPlayer);
			
			if(seman.GetStatusEffect(GetHash("SE_FPO_Passive")))
			{
				if(seman.GetStatusEffect(GetHash("SE_FPO_Passive")).GetTooltipString() != ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_Passive")).GetTooltipString())
				{ seman.RemoveStatusEffect(GetHash("SE_FPO_Passive")); }
			}
			
			if(!seman.GetStatusEffect(GetHash("SE_FPO_Passive")) && ZoneSystem.instance.GetGlobalKey("gk_fpo_bossstone_passive"))
			{ seman.AddStatusEffect(GetHash("SE_FPO_Passive")); }
			
			if(seman.GetStatusEffect(GetHash("CorpseRun")) && !Config_SE_CorpseRun_Bool.Value)
			{ seman.RemoveStatusEffect(GetHash("CorpseRun")); }
		}
		
		static void Update_Player_StatusEffects_Equipped()
		{
			var seman = GetSEMan(Player.m_localPlayer);
			
			foreach(string Power in List_BossNames)
			{
				if(seman.GetStatusEffect(GetHash("SE_FPO_" + Power + "_Equipped")))
				{
					if(seman.GetStatusEffect(GetHash("SE_FPO_" + Power + "_Equipped")).GetTooltipString() != ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_" + Power + "_Equipped")).GetTooltipString())
					{ seman.RemoveStatusEffect(GetHash("SE_FPO_" + Power + "_Equipped")); }
					
					if(Player.m_localPlayer.GetGuardianPowerName() != ("GP_" + Power))
					{ seman.RemoveStatusEffect(GetHash("SE_FPO_" + Power + "_Equipped")); }
				}
				
				if(!seman.GetStatusEffect(GetHash("SE_FPO_" + Power + "_Equipped")) && (Player.m_localPlayer.GetGuardianPowerName() == ("GP_" + Power)))
				{ seman.AddStatusEffect(GetHash("SE_FPO_" + Power + "_Equipped")); }
			}
		}
		
		static void Update_Passive_Stats()
		{
			StatusEffect_FPO_Passive.m_MaxHealth = 0.00F;
			StatusEffect_FPO_Passive.m_HealthRegen = 0;
			StatusEffect_FPO_Passive.m_healthRegenMultiplier = 1.00F;
			StatusEffect_FPO_Passive.m_MaxStamina = 0.00F;
			StatusEffect_FPO_Passive.m_staminaRegenMultiplier = 1.00F;
			StatusEffect_FPO_Passive.m_MaxEitr = 0.00F;
			StatusEffect_FPO_Passive.m_eitrRegenMultiplier = 1.00F;
			StatusEffect_FPO_Passive.m_addMaxCarryWeight = 0.00F;
			StatusEffect_FPO_Passive.m_speedModifier = 0.00F;
			StatusEffect_FPO_Passive.m_jumpModifier = Vector3.zero;
			StatusEffect_FPO_Passive.m_maxMaxFallSpeed = 0.00F;
			StatusEffect_FPO_Passive.m_EquipmentSpeedModifier = 0.00F;
			StatusEffect_FPO_Passive.m_WindSpeedModifier = 0.00F;
			StatusEffect_FPO_Passive.m_runStaminaDrainModifier = 0.00F;
			StatusEffect_FPO_Passive.m_jumpStaminaUseModifier = 0.00F;
			StatusEffect_FPO_Passive.m_stealthModifier = 0.00F;
			StatusEffect_FPO_Passive.m_noiseModifier = 0.00F;
			StatusEffect_FPO_Passive.m_StatusAttributes = new List<StatusEffect.StatusAttribute>{ };
			StatusEffect_FPO_Passive.m_OutgoingDamageTypes = new List<Skills.SkillType>{ };
			StatusEffect_FPO_Passive.m_OutgoingDamageModifiers = new List<float>{ };
			StatusEffect_FPO_Passive.m_fallDamageModifier = 0.00F;
			StatusEffect_FPO_Passive.m_HeatDamageModifier = 0.00F;
			StatusEffect_FPO_Passive.m_mods = new List<HitData.DamageModPair>{ };
			StatusEffect_FPO_Passive.m_ttl = 0.00F;
			StatusEffect_FPO_Passive.m_icon = ObjectDB.instance.GetStatusEffect(GetHash("Rested")).m_icon;
			
			foreach(string GlobalKey in ZoneSystem.instance.GetGlobalKeys())
			{
				if(GlobalKey.StartsWith("gk_fpo_bossstone_") && (GlobalKey != "gk_fpo_bossstone_passive"))
				{
					StatusEffect_FPO New_StatusEffect_FPO = GlobalKey switch
					{
						"gk_fpo_bossstone_eikthyr" => StatusEffect_FPO_Eikthyr_Passive,
						"gk_fpo_bossstone_theelder" => StatusEffect_FPO_TheElder_Passive,
						"gk_fpo_bossstone_bonemass" => StatusEffect_FPO_Bonemass_Passive,
						"gk_fpo_bossstone_moder" => StatusEffect_FPO_Moder_Passive,
						"gk_fpo_bossstone_yagluth" => StatusEffect_FPO_Yagluth_Passive,
						"gk_fpo_bossstone_queen" => StatusEffect_FPO_Queen_Passive,
						"gk_fpo_bossstone_fader" => StatusEffect_FPO_Fader_Passive,
						
						_ => null
					};
					
					if(New_StatusEffect_FPO != null)
					{
						StatusEffect_FPO_Passive.m_MaxHealth += New_StatusEffect_FPO.m_MaxHealth;
						StatusEffect_FPO_Passive.m_HealthRegen += New_StatusEffect_FPO.m_HealthRegen;
						StatusEffect_FPO_Passive.m_healthRegenMultiplier += (New_StatusEffect_FPO.m_healthRegenMultiplier - 1.00F);
						StatusEffect_FPO_Passive.m_MaxStamina += New_StatusEffect_FPO.m_MaxStamina;
						StatusEffect_FPO_Passive.m_staminaRegenMultiplier += (New_StatusEffect_FPO.m_staminaRegenMultiplier - 1.00F);
						StatusEffect_FPO_Passive.m_MaxEitr += New_StatusEffect_FPO.m_MaxEitr;
						StatusEffect_FPO_Passive.m_eitrRegenMultiplier += (New_StatusEffect_FPO.m_eitrRegenMultiplier - 1.00F);
						StatusEffect_FPO_Passive.m_addMaxCarryWeight += New_StatusEffect_FPO.m_addMaxCarryWeight;
						StatusEffect_FPO_Passive.m_speedModifier += New_StatusEffect_FPO.m_speedModifier;
						StatusEffect_FPO_Passive.m_jumpModifier = new Vector3((StatusEffect_FPO_Passive.m_jumpModifier.x + New_StatusEffect_FPO.m_jumpModifier.x), (StatusEffect_FPO_Passive.m_jumpModifier.y + New_StatusEffect_FPO.m_jumpModifier.y), (StatusEffect_FPO_Passive.m_jumpModifier.z + New_StatusEffect_FPO.m_jumpModifier.z));
						StatusEffect_FPO_Passive.m_maxMaxFallSpeed = (StatusEffect_FPO_Passive.m_maxMaxFallSpeed == 0.00F) ? New_StatusEffect_FPO.m_maxMaxFallSpeed : ((New_StatusEffect_FPO.m_maxMaxFallSpeed == 0.00F) ? StatusEffect_FPO_Passive.m_maxMaxFallSpeed : ((New_StatusEffect_FPO.m_maxMaxFallSpeed < StatusEffect_FPO_Passive.m_maxMaxFallSpeed) ? New_StatusEffect_FPO.m_maxMaxFallSpeed : StatusEffect_FPO_Passive.m_maxMaxFallSpeed));
						StatusEffect_FPO_Passive.m_EquipmentSpeedModifier += New_StatusEffect_FPO.m_EquipmentSpeedModifier;
						StatusEffect_FPO_Passive.m_WindSpeedModifier += New_StatusEffect_FPO.m_WindSpeedModifier;
						StatusEffect_FPO_Passive.m_runStaminaDrainModifier += New_StatusEffect_FPO.m_runStaminaDrainModifier;
						StatusEffect_FPO_Passive.m_jumpStaminaUseModifier += New_StatusEffect_FPO.m_jumpStaminaUseModifier;
						StatusEffect_FPO_Passive.m_stealthModifier += New_StatusEffect_FPO.m_stealthModifier;
						StatusEffect_FPO_Passive.m_noiseModifier += New_StatusEffect_FPO.m_noiseModifier;
						StatusEffect_FPO_Passive.m_StatusAttributes = StatusAttributes_Concat(StatusEffect_FPO_Passive.m_StatusAttributes.ToList(), New_StatusEffect_FPO.m_StatusAttributes.ToList());
						StatusEffect_FPO_Passive.m_OutgoingDamageTypes = OutgoingDamage_Concat(StatusEffect_FPO_Passive.m_OutgoingDamageTypes.ToList(), New_StatusEffect_FPO.m_OutgoingDamageTypes.ToList(), StatusEffect_FPO_Passive.m_OutgoingDamageModifiers.ToList(), New_StatusEffect_FPO.m_OutgoingDamageModifiers.ToList(), out List<float> m_OutgoingDamageModifiers);
						StatusEffect_FPO_Passive.m_OutgoingDamageModifiers = m_OutgoingDamageModifiers;
						StatusEffect_FPO_Passive.m_fallDamageModifier += New_StatusEffect_FPO.m_fallDamageModifier;
						StatusEffect_FPO_Passive.m_HeatDamageModifier += New_StatusEffect_FPO.m_HeatDamageModifier;
						StatusEffect_FPO_Passive.m_mods = IncomingDamage_Concat(StatusEffect_FPO_Passive.m_mods.ToList(), New_StatusEffect_FPO.m_mods.ToList());
					}
				}
			}
			
			StatusEffect_FPO_Passive.m_OutgoingDamageTypes = OutgoingDamage_Sort(StatusEffect_FPO_Passive.m_OutgoingDamageTypes.ToList(), StatusEffect_FPO_Passive.m_OutgoingDamageModifiers.ToList(), out List<float> New_List);
			StatusEffect_FPO_Passive.m_OutgoingDamageModifiers = New_List;
			StatusEffect_FPO_Passive.m_mods = IncomingDamage_Sort(StatusEffect_FPO_Passive.m_mods.ToList());
		}
		
		static void Update_Equipped_Stats()
		{
			foreach(StatusEffect_FPO New_StatusEffect_FPO in List_StatusEffect_FPO_Equipped)
			{
				New_StatusEffect_FPO.m_ttl = 0.00F;
				New_StatusEffect_FPO.m_icon = ObjectDB.instance.GetStatusEffect(GetHash(New_StatusEffect_FPO.name.Replace("SE_FPO_", "GP_").Replace("_Equipped", ""))).m_icon;
			}
		}
		
		static void Update_Active_Stats()
		{
			if(List_StatusEffect_FPO.Count > 0)
			{
				for(int Index = 0; Index < List_StatusEffect_FPO.Count; Index ++)
				{
					List_StatusEffect_FPO[Index].m_MaxHealth = List_StatusEffect_FPO_Active[Index].m_MaxHealth;
					List_StatusEffect_FPO[Index].m_HealthRegen = List_StatusEffect_FPO_Active[Index].m_HealthRegen;
					List_StatusEffect_FPO[Index].m_healthRegenMultiplier = List_StatusEffect_FPO_Active[Index].m_healthRegenMultiplier;
					List_StatusEffect_FPO[Index].m_MaxStamina = List_StatusEffect_FPO_Active[Index].m_MaxStamina;
					List_StatusEffect_FPO[Index].m_staminaRegenMultiplier = List_StatusEffect_FPO_Active[Index].m_staminaRegenMultiplier;
					List_StatusEffect_FPO[Index].m_MaxEitr = List_StatusEffect_FPO_Active[Index].m_MaxEitr;
					List_StatusEffect_FPO[Index].m_eitrRegenMultiplier = List_StatusEffect_FPO_Active[Index].m_eitrRegenMultiplier;
					List_StatusEffect_FPO[Index].m_addMaxCarryWeight = List_StatusEffect_FPO_Active[Index].m_addMaxCarryWeight;
					List_StatusEffect_FPO[Index].m_speedModifier = List_StatusEffect_FPO_Active[Index].m_speedModifier;
					List_StatusEffect_FPO[Index].m_jumpModifier = List_StatusEffect_FPO_Active[Index].m_jumpModifier;
					List_StatusEffect_FPO[Index].m_maxMaxFallSpeed = List_StatusEffect_FPO_Active[Index].m_maxMaxFallSpeed;
					List_StatusEffect_FPO[Index].m_EquipmentSpeedModifier = List_StatusEffect_FPO_Active[Index].m_EquipmentSpeedModifier;
					List_StatusEffect_FPO[Index].m_WindSpeedModifier = List_StatusEffect_FPO_Active[Index].m_WindSpeedModifier;
					List_StatusEffect_FPO[Index].m_runStaminaDrainModifier = List_StatusEffect_FPO_Active[Index].m_runStaminaDrainModifier;
					List_StatusEffect_FPO[Index].m_jumpStaminaUseModifier = List_StatusEffect_FPO_Active[Index].m_jumpStaminaUseModifier;
					List_StatusEffect_FPO[Index].m_stealthModifier = List_StatusEffect_FPO_Active[Index].m_stealthModifier;
					List_StatusEffect_FPO[Index].m_noiseModifier = List_StatusEffect_FPO_Active[Index].m_noiseModifier;
					List_StatusEffect_FPO[Index].m_StatusAttributes = List_StatusEffect_FPO_Active[Index].m_StatusAttributes.ToList();
					List_StatusEffect_FPO[Index].m_OutgoingDamageTypes = List_StatusEffect_FPO_Active[Index].m_OutgoingDamageTypes.ToList();
					List_StatusEffect_FPO[Index].m_OutgoingDamageModifiers = List_StatusEffect_FPO_Active[Index].m_OutgoingDamageModifiers.ToList();
					List_StatusEffect_FPO[Index].m_fallDamageModifier = List_StatusEffect_FPO_Active[Index].m_fallDamageModifier;
					List_StatusEffect_FPO[Index].m_HeatDamageModifier = List_StatusEffect_FPO_Active[Index].m_HeatDamageModifier;
					List_StatusEffect_FPO[Index].m_mods = List_StatusEffect_FPO_Active[Index].m_mods.ToList();
					
					List_StatusEffect_FPO[Index].m_ttl = ConfigEntry_General_GuardianPower_ActiveDuration.Value;
					
					List_StatusEffect_FPO[Index].m_startEffects.m_effectPrefabs = new EffectList.EffectData[1];
					List_StatusEffect_FPO[Index].m_startEffects.m_effectPrefabs[0] = new EffectList.EffectData{ m_prefab = PrefabManager.Instance.GetPrefab("fx_GP_Activation") };
					
					List_StatusEffect_FPO[Index].m_icon = ObjectDB.instance.GetStatusEffect(GetHash(List_StatusEffect_FPO[Index].name.Replace("SE_FPO_", "GP_"))).m_icon;
					
					foreach(string GlobalKey in ZoneSystem.instance.GetGlobalKeys())
					{
						if(GlobalKey.StartsWith("gk_fpo_bossstone_") && (GlobalKey != "gk_fpo_bossstone_passive"))
						{
							StatusEffect_FPO New_StatusEffect_FPO = GlobalKey switch
							{
								"gk_fpo_bossstone_eikthyr" => StatusEffect_FPO_Eikthyr_Shared,
								"gk_fpo_bossstone_theelder" => StatusEffect_FPO_TheElder_Shared,
								"gk_fpo_bossstone_bonemass" => StatusEffect_FPO_Bonemass_Shared,
								"gk_fpo_bossstone_moder" => StatusEffect_FPO_Moder_Shared,
								"gk_fpo_bossstone_yagluth" => StatusEffect_FPO_Yagluth_Shared,
								"gk_fpo_bossstone_queen" => StatusEffect_FPO_Queen_Shared,
								"gk_fpo_bossstone_fader" => StatusEffect_FPO_Fader_Shared,
								
								_ => null
							};
							
							if(New_StatusEffect_FPO != null)
							{
								List_StatusEffect_FPO[Index].m_MaxHealth += New_StatusEffect_FPO.m_MaxHealth;
								List_StatusEffect_FPO[Index].m_HealthRegen += New_StatusEffect_FPO.m_HealthRegen;
								List_StatusEffect_FPO[Index].m_healthRegenMultiplier += (New_StatusEffect_FPO.m_healthRegenMultiplier - 1.00F);
								List_StatusEffect_FPO[Index].m_MaxStamina += New_StatusEffect_FPO.m_MaxStamina;
								List_StatusEffect_FPO[Index].m_staminaRegenMultiplier += (New_StatusEffect_FPO.m_staminaRegenMultiplier - 1.00F);
								List_StatusEffect_FPO[Index].m_MaxEitr += New_StatusEffect_FPO.m_MaxEitr;
								List_StatusEffect_FPO[Index].m_eitrRegenMultiplier += (New_StatusEffect_FPO.m_eitrRegenMultiplier - 1.00F);
								List_StatusEffect_FPO[Index].m_addMaxCarryWeight += New_StatusEffect_FPO.m_addMaxCarryWeight;
								List_StatusEffect_FPO[Index].m_speedModifier += New_StatusEffect_FPO.m_speedModifier;
								List_StatusEffect_FPO[Index].m_jumpModifier = new Vector3((List_StatusEffect_FPO[Index].m_jumpModifier.x + New_StatusEffect_FPO.m_jumpModifier.x), (List_StatusEffect_FPO[Index].m_jumpModifier.y + New_StatusEffect_FPO.m_jumpModifier.y), (List_StatusEffect_FPO[Index].m_jumpModifier.z + New_StatusEffect_FPO.m_jumpModifier.z));
								List_StatusEffect_FPO[Index].m_maxMaxFallSpeed = (List_StatusEffect_FPO[Index].m_maxMaxFallSpeed == 0.00F) ? New_StatusEffect_FPO.m_maxMaxFallSpeed : ((New_StatusEffect_FPO.m_maxMaxFallSpeed == 0.00F) ? List_StatusEffect_FPO[Index].m_maxMaxFallSpeed : ((New_StatusEffect_FPO.m_maxMaxFallSpeed < List_StatusEffect_FPO[Index].m_maxMaxFallSpeed) ? New_StatusEffect_FPO.m_maxMaxFallSpeed : List_StatusEffect_FPO[Index].m_maxMaxFallSpeed));
								List_StatusEffect_FPO[Index].m_EquipmentSpeedModifier += New_StatusEffect_FPO.m_EquipmentSpeedModifier;
								List_StatusEffect_FPO[Index].m_WindSpeedModifier += New_StatusEffect_FPO.m_WindSpeedModifier;
								List_StatusEffect_FPO[Index].m_runStaminaDrainModifier += New_StatusEffect_FPO.m_runStaminaDrainModifier;
								List_StatusEffect_FPO[Index].m_jumpStaminaUseModifier += New_StatusEffect_FPO.m_jumpStaminaUseModifier;
								List_StatusEffect_FPO[Index].m_stealthModifier += New_StatusEffect_FPO.m_stealthModifier;
								List_StatusEffect_FPO[Index].m_noiseModifier += New_StatusEffect_FPO.m_noiseModifier;
								List_StatusEffect_FPO[Index].m_StatusAttributes = StatusAttributes_Concat(List_StatusEffect_FPO[Index].m_StatusAttributes.ToList(), New_StatusEffect_FPO.m_StatusAttributes.ToList());
								List_StatusEffect_FPO[Index].m_OutgoingDamageTypes = OutgoingDamage_Concat(List_StatusEffect_FPO[Index].m_OutgoingDamageTypes.ToList(), New_StatusEffect_FPO.m_OutgoingDamageTypes.ToList(), List_StatusEffect_FPO[Index].m_OutgoingDamageModifiers.ToList(), New_StatusEffect_FPO.m_OutgoingDamageModifiers.ToList(), out List<float> m_OutgoingDamageModifiers);
								List_StatusEffect_FPO[Index].m_OutgoingDamageModifiers = m_OutgoingDamageModifiers;
								List_StatusEffect_FPO[Index].m_fallDamageModifier += New_StatusEffect_FPO.m_fallDamageModifier;
								List_StatusEffect_FPO[Index].m_HeatDamageModifier += New_StatusEffect_FPO.m_HeatDamageModifier;
								List_StatusEffect_FPO[Index].m_mods = IncomingDamage_Concat(List_StatusEffect_FPO[Index].m_mods.ToList(), New_StatusEffect_FPO.m_mods.ToList());
							}
						}
					}
					
					List_StatusEffect_FPO[Index].m_OutgoingDamageTypes = OutgoingDamage_Sort(List_StatusEffect_FPO[Index].m_OutgoingDamageTypes.ToList(), List_StatusEffect_FPO[Index].m_OutgoingDamageModifiers.ToList(), out List<float> New_List);
					List_StatusEffect_FPO[Index].m_OutgoingDamageModifiers = New_List;
					List_StatusEffect_FPO[Index].m_mods = IncomingDamage_Sort(List_StatusEffect_FPO[Index].m_mods.ToList());
				}
			}
		}
		
		static List<StatusEffect.StatusAttribute> StatusAttributes_Concat(List<StatusEffect.StatusAttribute> List, List<StatusEffect.StatusAttribute> List_Add)
		{
			List<StatusEffect.StatusAttribute> New_List_Add = new List<StatusEffect.StatusAttribute>{ };
			
			foreach(StatusEffect.StatusAttribute New_StatusAttributeAdd in List_Add)
			{
				bool New_Bool = true;
				
				foreach(StatusEffect.StatusAttribute New_StatusAttribute in List)
				{
					if(New_StatusAttributeAdd == New_StatusAttribute)
					{ New_Bool = false; }
				}
				
				if(New_Bool)
				{ New_List_Add.Add(New_StatusAttributeAdd); }
			}
			
			return List.Concat(New_List_Add).ToList();
		}
		
		static List<Skills.SkillType> OutgoingDamage_Concat(List<Skills.SkillType> Type, List<Skills.SkillType> TypeAdd, List<float> Modifier, List<float> ModifierAdd, out List<float> m_OutgoingDamageModifiers)
		{
			List<Skills.SkillType> New_TypeAdd = new List<Skills.SkillType>{ };
			List<float> New_ModifierAdd = new List<float>{ };
			
			foreach(Skills.SkillType New_SkillTypeAdd in TypeAdd)
			{
				bool New_Bool = true;
				int Index = 0;
				
				foreach(Skills.SkillType New_SkillType in Type)
				{
					if(New_SkillTypeAdd == New_SkillType)
					{
						New_Bool = false;
						Index = Type.IndexOf(New_SkillType);
					}
				}
				
				if(New_Bool)
				{
					New_TypeAdd.Add(New_SkillTypeAdd);
					New_ModifierAdd.Add(ModifierAdd[TypeAdd.IndexOf(New_SkillTypeAdd)]);
				}
				else
				{ Modifier[Index] += ModifierAdd[TypeAdd.IndexOf(New_SkillTypeAdd)]; }
			}
			
			m_OutgoingDamageModifiers = Modifier.Concat(New_ModifierAdd).ToList();
			return Type.Concat(New_TypeAdd).ToList();
		}
		
		static List<HitData.DamageModPair> IncomingDamage_Concat(List<HitData.DamageModPair> List, List<HitData.DamageModPair> List_Add)
		{
			List<HitData.DamageModPair> New_List_Add = new List<HitData.DamageModPair>{ };
			
			foreach(HitData.DamageModPair New_DamageModPairAdd in List_Add)
			{
				bool New_Bool = true;
				int Index = 0;
				
				foreach(HitData.DamageModPair New_DamageModPair in List)
				{
					if(New_DamageModPairAdd.m_type == New_DamageModPair.m_type)
					{
						New_Bool = false;
						Index = List.IndexOf(New_DamageModPair);
					}
				}
				
				if(New_Bool)
				{ New_List_Add.Add(New_DamageModPairAdd); }
				else
				{ List[Index] = new HitData.DamageModPair{ m_type = New_DamageModPairAdd.m_type, m_modifier = ((GetToolTipIncomingDamageModifier(New_DamageModPairAdd.m_modifier) < GetToolTipIncomingDamageModifier(List[Index].m_modifier)) ? New_DamageModPairAdd.m_modifier : List[Index].m_modifier) }; }
			}
			
			return List.Concat(New_List_Add).ToList();
		}
		
		static List<Skills.SkillType> OutgoingDamage_Sort(List<Skills.SkillType> List_Type, List<float> List_Modifier, out List<float> List_Sort)
		{
			List<Skills.SkillType> New_List_Type = new List<Skills.SkillType>{ };
			List<float> New_List_Modifier = new List<float>{ };
			int Index;
			
			List<Skills.SkillType> New_SkillTypes = new List<Skills.SkillType>
			{
				Skills.SkillType.All,
				Skills.SkillType.Unarmed,
				Skills.SkillType.Clubs,
				Skills.SkillType.Axes,
				Skills.SkillType.Knives,
				Skills.SkillType.Spears,
				Skills.SkillType.Swords,
				Skills.SkillType.Polearms,
				Skills.SkillType.Bows,
				Skills.SkillType.Crossbows,
				Skills.SkillType.ElementalMagic,
				Skills.SkillType.BloodMagic,
				Skills.SkillType.WoodCutting,
				Skills.SkillType.Pickaxes,
			};
			
			foreach(Skills.SkillType New_SkillType in New_SkillTypes)
			{
				if(OutgoingDamage_Sort_Loop(List_Type.ToList(), New_SkillType, out Index))
				{
					New_List_Type.Add(List_Type[Index]);
					New_List_Modifier.Add(List_Modifier[Index]);
				}
			}
			
			List_Sort = New_List_Modifier.ToList();
			return New_List_Type.ToList();
		}
		
		static bool OutgoingDamage_Sort_Loop(List<Skills.SkillType> List, Skills.SkillType SkillType_Sort, out int Index)
		{
			foreach(Skills.SkillType New_SkillType in List)
			{
				if(New_SkillType == SkillType_Sort)
				{
					Index = List.IndexOf(New_SkillType);
					return true;
				}
			}
			
			Index = 0;
			return false;
		}
		
		static List<HitData.DamageModPair> IncomingDamage_Sort(List<HitData.DamageModPair> List)
		{
			List<HitData.DamageModPair> New_List = new List<HitData.DamageModPair>{ };
			
			List<HitData.DamageType> New_DamageTypes = new List<HitData.DamageType>
			{
				HitData.DamageType.Physical,
				HitData.DamageType.Pierce,
				HitData.DamageType.Slash,
				HitData.DamageType.Blunt,
				HitData.DamageType.Elemental,
				HitData.DamageType.Lightning,
				HitData.DamageType.Poison,
				HitData.DamageType.Frost,
				HitData.DamageType.Fire,
				HitData.DamageType.Spirit,
				HitData.DamageType.Chop,
				HitData.DamageType.Pickaxe
			};

			foreach(HitData.DamageType New_DamageType in New_DamageTypes)
			{ New_List.Add(IncomingDamage_Sort_Loop(List.ToList(), New_DamageType)); }
			
			return New_List.ToList();
		}
		
		static HitData.DamageModPair IncomingDamage_Sort_Loop(List<HitData.DamageModPair> List, HitData.DamageType DamageType_Sort)
		{
			foreach(HitData.DamageModPair New_DamageModPair in List)
			{
				if(New_DamageModPair.m_type == DamageType_Sort)
				{ return New_DamageModPair; }
			}
			
			return new HitData.DamageModPair{ };
		}
		
		static float Update_Player_BaseStats(string New_String, Player New_Player)
		{
			if(New_Player != null)
			{
				float MaxHealth = 0.00F;
				float HealthRegen = 0.00F;
				float MaxStamina = 0.00F;
				float MaxEitr = 0.00F;
				float EquipmentSpeedModifier = 0.00F;
				float WindSpeedModifier = 0.00F;
				float HeatDamageModifier = 0.00F;
				
				var playerSeman = GetSEMan(New_Player);
				foreach(StatusEffect New_StatusEffect in playerSeman.GetStatusEffects())
				{
					if(New_StatusEffect.name.StartsWith("SE_FPO_"))
					{
						StatusEffect_FPO New_StatusEffect_FPO = (StatusEffect_FPO)New_StatusEffect;
						MaxHealth += New_StatusEffect_FPO.m_MaxHealth;
						HealthRegen += New_StatusEffect_FPO.m_HealthRegen;
						MaxStamina += New_StatusEffect_FPO.m_MaxStamina;
						MaxEitr += New_StatusEffect_FPO.m_MaxEitr;
						EquipmentSpeedModifier += New_StatusEffect_FPO.m_EquipmentSpeedModifier;
						WindSpeedModifier += New_StatusEffect_FPO.m_WindSpeedModifier;
						HeatDamageModifier += New_StatusEffect_FPO.m_HeatDamageModifier;
					}
				}
				
				if(New_String == "MaxHealth")
				{ return MaxHealth; }
				
				if(New_String == "HealthRegen")
				{ return HealthRegen; }
				
				if(New_String == "MaxStamina")
				{ return MaxStamina; }
				
				if(New_String == "MaxEitr")
				{ return MaxEitr; }
				
				if(New_String == "EquipmentSpeedModifier")
				{ return EquipmentSpeedModifier; }
				
				if(New_String == "WindSpeedModifier")
				{ return WindSpeedModifier; }
				
				if(New_String == "HeatDamageModifier")
				{ return HeatDamageModifier; }
			}
			
			return 0.00F;
		}
		
		static void Power_Unlock(BossStone New_BossStone)
		{
			if(Player.m_localPlayer != null)
			{
				string New_String = GetBossName(New_BossStone.m_itemStand.m_currentItemName);
				
				if(New_String != "")
				{ Power_Set("GP_" + New_String, true); }
			}
		}
		
		static void Power_Unlock_Spawn(Player New_Player)
		{
			if(New_Player == Player.m_localPlayer)
			{
				if((!New_Player.GetGuardianPowerName().StartsWith("GP_")) && ZoneSystem.instance.GetGlobalKey("gk_fpo_bossstone_passive"))
				{
					List<string> New_List = new List<string>{ };
					
					var playerUniques = GetPlayerUniques(New_Player);
					foreach(string PlayerKey in playerUniques)
					{
						if(PlayerKey.StartsWith("GP_"))
						{ New_List.Add(PlayerKey); }
					}
					
					List<string> Powers = Power_Sort(New_List.ToList());
					Power_Set(Powers[0], true);
				}
			}
		}
		
		static void Power_Lock(Player New_Player)
		{
			if(New_Player == Player.m_localPlayer)
			{
				if(New_Player.GetGuardianPowerName().StartsWith("GP_"))
				{
					if(!ZoneSystem.instance.GetGlobalKey("gk_fpo_bossstone_passive"))
					{ New_Player.SetGuardianPower(""); }
					else
					{
						bool New_Bool = true;
						
						foreach(string GlobalKey in ZoneSystem.instance.GetGlobalKeys())
						{
							if(GetBossName(New_Player.GetGuardianPowerName()) == GetBossName(GlobalKey))
							{ New_Bool = false; }
						}
						
						if(New_Bool)
						{ New_Player.SetGuardianPower(""); }
					}
				}
			}
		}
		
		static void Power_Cycle()
		{
			List<string> PlayerKeys = new List<string>{ };
			
			PlayerKeys_Update();
			
			var localPlayerUniques = GetPlayerUniques(Player.m_localPlayer);
			foreach(string PlayerKey in localPlayerUniques)
			{
				foreach(string GlobalKey in ZoneSystem.instance.GetGlobalKeys())
				{
					if(GlobalKey.StartsWith("gk_fpo_bossstone_"))
					{
						if((GetBossName(PlayerKey) == GetBossName(GlobalKey)))
						{ PlayerKeys.Add(PlayerKey); }
					}
				}
			}
			
			List<string> Powers = Power_Sort(PlayerKeys.ToList());
			
			if(Powers.Count > 0)
			{
				int Index = 0;
				
				foreach(string Power in Powers)
				{
					if(Player.m_localPlayer.GetGuardianPowerName() == Power)
					{ Index = Powers.IndexOf(Power); }
				}
				
				if((Index + 1) == Powers.Count)
				{ Index = -1; }
				
				Power_Set(Powers[Index + 1], false);
			}
		}
		
		static void Power_Set(string Power, bool Unlock)
		{
			Player.m_localPlayer.SetGuardianPower(Power);
			
			EffectList New_EffectList = new EffectList{ m_effectPrefabs = new EffectList.EffectData[1] };
			
			Vector3 New_Vector3 = Player.m_localPlayer.transform.position;
			New_Vector3.y += 0.925F;
			
			if(Unlock)
			{
				New_EffectList.m_effectPrefabs[0] = new EffectList.EffectData{ m_prefab = PrefabManager.Instance.GetPrefab("GO_FPO_Effect_PowerUnlock"), m_scale = true };
				New_EffectList.Create(New_Vector3, Player.m_localPlayer.transform.rotation, Player.m_localPlayer.transform, 0.50F);
			}
			else
			{
				New_EffectList.m_effectPrefabs[0] = new EffectList.EffectData{ m_prefab = PrefabManager.Instance.GetPrefab("sfx_gui_button"), m_attach = true};
				New_EffectList.Create(New_Vector3, Player.m_localPlayer.transform.rotation, Player.m_localPlayer.transform);
			}
		}
		
		static List<string> Power_Sort(List<string> PlayerKeys)
		{
			List<string> Powers = new List<string>{ };
			
			foreach(string Boss in List_BossNames)
			{
				if(PlayerKeys.Contains("GP_" + Boss))
				{ Powers.Add("GP_" + Boss); }
			}
			
			return Powers.ToList();
		}
		
		static string GetBossName(string New_String)
		{
			New_String = New_String.ToLower();
			
			if(New_String.StartsWith("$item_trophy_") || New_String.StartsWith("gk_fpo_bossstone_") || New_String.StartsWith("gp_"))
			{
				if(New_String.Contains("_eikthyr"))
				{ return "Eikthyr"; }
				
				if(New_String.Contains("_theelder") || New_String.Contains("_elder"))
				{ return "TheElder"; }
				
				if(New_String.Contains("_bonemass"))
				{ return "Bonemass"; }
				
				if(New_String.Contains("_moder") || New_String.Contains("_dragonqueen"))
				{ return "Moder"; }
				
				if(New_String.Contains("_yagluth") || New_String.Contains("_goblinking"))
				{ return "Yagluth"; }
				
				if(New_String.Contains("_queen") || New_String.Contains("_seekerqueen"))
				{ return "Queen"; }
				
				if(New_String.Contains("_fader"))
				{ return "Fader"; }
			}
			
			return "";
		}
	}
}