using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		static ConfigEntry<KeyCode> ConfigEntry_Button_PowerCycle;
		
		static ConfigEntry<string> ConfigEntry_General_Preset;
		static ConfigEntry<int> ConfigEntry_General_GuardianPower_ActiveDuration;
		static ConfigEntry<int> ConfigEntry_General_GuardianPower_CooldownDuration;
		static ConfigEntry<bool> ConfigEntry_PowerCycle_Bool;
		
		//Eikthyr
		
		
		
		
		//TheElder
		
		
		
		
		//Bonemass
		
		
		
		
		//Moder
		
		
		
		
		//Yagluth
		
		
		
		
		//Queen
		
		
		
		
		//Fader
		
		
		
		
		//Extra
		static ConfigEntry<bool> Config_SE_CorpseRun_Bool;
		
		void Add_ConfigEntries()
		{
			string String_Key_MaxHealth = "MaxHealth";
			string String_Key_HealthRegen = "HealthRegen";
			string String_Key_HealthRegenModifier = "HealthRegenModifier";
			string String_Key_MaxStamina = "MaxStamina";
			string String_Key_StaminaRegenModifier = "StaminaRegenModifier";
			string String_Key_MaxEitr = "MaxEitr";
			string String_Key_EitrRegenModifier = "EitrRegenModifier";
			string String_Key_MaxWeight = "MaxWeight";
			string String_Key_MoveSpeedModifier = "MoveSpeedModifier";
			string String_Key_JumpForceModifier = "JumpForceModifier";
			string String_Key_MaxFallSpeed = "MaxFallSpeed";
			string String_Key_EquipmentSpeedModifier = "EquipmentSpeedModifier";
			string String_Key_WindSpeedModifier = "WindSpeedModifier";
			string String_Key_RunStaminaCostModifier = "RunStaminaCostModifier";
			string String_Key_JumpStaminaCostModifier = "JumpStaminaCostModifier";
			string String_Key_StealthModifier = "StealthModifier";
			string String_Key_NoiseModifier = "NoiseModifier";
			string String_Key_StatusAttributes = "StatusAttributes";
			string String_Key_OutgoingDamageTypes = "OutgoingDamageTypes";
			string String_Key_OutgoingDamageModifiers = "OutgoingDamageModifiers";
			string String_Key_FallDamageModifier = "FallDamageModifier";
			string String_Key_HeatDamageModifier = "HeatDamageModifier";
			string String_Key_IncomingDamageTypes = "IncomingDamageTypes";
			string String_Key_IncomingDamageModifiers = "IncomingDamageModifiers";
			string String_Key_IncomingDamageMultiplierTypes = "IncomingDamageMultiplierTypes";
			string String_Key_IncomingDamageMultiplierModifiers = "IncomingDamageMultiplierModifiers";
			string String_Key_OutgoingDamageMultiplierTypes = "OutgoingDamageMultiplierTypes";
			string String_Key_OutgoingDamageMultiplierModifiers = "OutgoingDamageMultiplierModifiers";
			
			string String_Value_FloatIncrease = "A value of \"12.34\" translates to a 12.34% Increase.";
			string String_Value_FloatDecrease = "A value of \"12.34\" translates to a 12.34% Decrease.";
			string String_Value_StatusAttribute = "Acceptable Values: [ColdResistance, DoubleImpactDamage, SailingPower].";
			string String_Value_OutgoingDamageTypes = "Acceptable Values: [All, Unarmed, Clubs, Axes, Knives, Spears, Swords, Polearms, Bows, Crossbows, ElementalMagic, BloodMagic, WoodCutting, Pickaxes].";
			string String_Value_IncomingDamageTypes = "Acceptable Values: [Physical, Pierce, Slash, Blunt, Elemental, Lightning, Poison, Frost, Fire, Spirit, Chop, Pickaxe].";
			string String_Value_IncomingDamageModifiers = "Acceptable Values: [0, 25, 50, 100, 150, 200] (0 = Immune, 25 = Very Resistant, 50 = Resistant, 100 = Neutral, 150 = Weak, 200 = Very Weak).";
			string String_Value_IncomingDamageMultiplierTypes = "Acceptable Values: [Physical, Pierce, Slash, Blunt, Elemental, Lightning, Poison, Frost, Fire, Spirit, Chop, Pickaxe, All].";
			string String_Value_IncomingDamageMultiplierModifiers = "Acceptable Values: Any percentage (e.g., 90.0 = 10% damage reduction, 110.0 = 10% damage increase).";
			string String_Value_OutgoingDamageMultiplierTypes = "Acceptable Values: [All, Unarmed, Clubs, Axes, Knives, Spears, Swords, Polearms, Bows, Crossbows, ElementalMagic, BloodMagic, WoodCutting, Pickaxes].";
			string String_Value_OutgoingDamageMultiplierModifiers = "Acceptable Values: Any percentage (e.g., 110.0 = 10% damage increase, 90.0 = 10% damage reduction).";
			string String_Value_Multiple = "Multiple Values Acceptable with Format: \"ValueA,ValueB,ValueC\" or ValueA,ValueB,ValueC or ValueA|ValueB|ValueC (pipes recommended - avoids comment conflicts and LibreCalc auto-conversion).";
			
			string String_ModPriority = "This Config may take priority over most mods.";
			string String_GameBalance = "This Config is for Game Balance.";
			
			ConfigurationManagerAttributes Config_AdminOnly = new ConfigurationManagerAttributes{ IsAdminOnly = true };
			
			ConfigDescription ConfigDescription_PowerCycle = new ConfigDescription("Input to Cycle through unlocked Forsaken Powers.");
			
			ConfigDescription ConfigDescription_Preset = new ConfigDescription("Config Preset.\nAcceptable Values: [Custom, Vanilla, Vanilla+, Passive Powers].", new AcceptableValueList<string>("Custom", "Vanilla", "Vanilla+", "Passive Powers"), Config_AdminOnly);
			ConfigDescription ConfigDescription_ActiveDuration = new ConfigDescription(("Forsaken Power's Active Duration in Seconds.\n" + String_ModPriority), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_CooldownDuration = new ConfigDescription(("Forsaken Power's Cooldown Duration in Seconds.\n" + String_ModPriority), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_PowerCycle_Bool = new ConfigDescription("Enable Power Cycling.", null, Config_AdminOnly);
			
			ConfigDescription ConfigDescription_MaxHealth = new ConfigDescription("Increase Max Health by Value.", null, Config_AdminOnly);
			ConfigDescription ConfigDescription_HealthRegen = new ConfigDescription(("Increase Health Regeneration by Value."), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_HealthRegenModifier = new ConfigDescription(("Increase Health Regeneration by Value Percent.\n" + String_Value_FloatIncrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_MaxStamina = new ConfigDescription("Increase Max Stamina by Value.", null, Config_AdminOnly);
			ConfigDescription ConfigDescription_StaminaRegenModifier = new ConfigDescription(("Increase Stamina Regeneration by Value Percent.\n" + String_Value_FloatIncrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_MaxEitr = new ConfigDescription(("Increase Max Eitr by Value."), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_EitrRegenModifier = new ConfigDescription(("Increase Eitr Regeneration by Value Percent.\n" + String_Value_FloatIncrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_MaxWeight = new ConfigDescription(("Increase Max Weight Capacity by Value."), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_MoveSpeedModifier = new ConfigDescription(("Increase Movement Speed by Value Percent.\n" + String_Value_FloatIncrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_JumpForceModifier = new ConfigDescription(("Increase Jump Force by Value Percent.\n" + String_Value_FloatIncrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_MaxFallSpeed = new ConfigDescription(("Limits Max Fall Speed to Value."), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_EquipmentSpeedModifier = new ConfigDescription(("Decrease Movement Speed Penalty from Equipment by Value Percent.\n" + String_Value_FloatDecrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_WindSpeedModifier = new ConfigDescription(("Increase Wind Speed when Sailing.\n" + String_Value_FloatIncrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_RunStaminaCostModifier = new ConfigDescription(("Decrease Run Stamina Cost by Value Percent.\n" + String_Value_FloatDecrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_JumpStaminaCostModifier = new ConfigDescription(("Decrease Jump Stamina Cost by Value Percent.\n" + String_Value_FloatDecrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_StealthModifier = new ConfigDescription(("Increase Stealth by Value Percent.\n" + String_Value_FloatIncrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_NoiseModifier = new ConfigDescription(("Decrease Noise by Value Percent.\n" + String_Value_FloatDecrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_StatusAttributes = new ConfigDescription(("Applies Attributes.\n" + String_Value_StatusAttribute + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_OutgoingDamageTypes = new ConfigDescription(("Modify Outgoing Damage Types.\n" + String_Value_OutgoingDamageTypes + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_OutgoingDamageModifiers = new ConfigDescription(("Modify Outgoing Damage Types by Value Percent.\n" + String_Value_FloatIncrease + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_FallDamageModifier = new ConfigDescription(("Decrease Fall Damage by Value Percent.\n" + String_Value_FloatDecrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_HeatDamageModifier = new ConfigDescription(("Decrease Heat Damage by Value Percent.\n" + String_Value_FloatDecrease), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_IncomingDamageTypes = new ConfigDescription(("Modify Incoming Damage Types.\n" + String_Value_IncomingDamageTypes + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_IncomingDamageModifiers = new ConfigDescription(("Modify Incoming Damage Types by Value Percent.\n" + String_Value_IncomingDamageModifiers + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_IncomingDamageMultiplierTypes = new ConfigDescription(("Apply Additional Damage Multipliers to Incoming Damage Types.\n" + String_Value_IncomingDamageMultiplierTypes + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_IncomingDamageMultiplierModifiers = new ConfigDescription(("Apply Additional Damage Multipliers by Percentage.\n" + String_Value_IncomingDamageMultiplierModifiers + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_OutgoingDamageMultiplierTypes = new ConfigDescription(("Apply Additional Damage Multipliers to Outgoing Damage Types.\n" + String_Value_OutgoingDamageMultiplierTypes + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			ConfigDescription ConfigDescription_OutgoingDamageMultiplierModifiers = new ConfigDescription(("Apply Additional Damage Multipliers by Percentage.\n" + String_Value_OutgoingDamageMultiplierModifiers + "\n" + String_Value_Multiple), null, Config_AdminOnly);
			
			ConfigDescription ConfigDescription_ExtraCorpseRun =  new ConfigDescription(("Corpse Run from Tombstones.\n" + String_GameBalance), null, Config_AdminOnly);
			
			//Input
			ConfigEntry_Button_PowerCycle = Config.Bind("C00 Client", "PowerCycle", KeyCode.G, ConfigDescription_PowerCycle);
			
			//General
			ConfigEntry_General_Preset = Config.Bind("S00 Server: General", "Preset", "Vanilla+", ConfigDescription_Preset);
			ConfigEntry_General_GuardianPower_ActiveDuration = Config.Bind("S00 Server: General", "PowerActiveDuration", 60, ConfigDescription_ActiveDuration);
			ConfigEntry_General_GuardianPower_CooldownDuration = Config.Bind("S00 Server: General", "PowerCooldownDuration", 240, ConfigDescription_CooldownDuration);
			ConfigEntry_PowerCycle_Bool = Config.Bind("S00 Server: General", "Enable Power Cycling", true, ConfigDescription_PowerCycle_Bool);
			
			//Extra
			Config_SE_CorpseRun_Bool = Config.Bind("SX1 Server: Corpse Run", "Enabled", true, ConfigDescription_ExtraCorpseRun);
			
			//Loop
			foreach(string Power in List_BossNames)
			{
				foreach(string Component in List_Components)
				{
					string String_Section = "S" + (List_BossNames.IndexOf(Power) + 1).ToString() + (List_Components.IndexOf(Component) + 1).ToString() + " Server: " + Power + " (" + Component + ")";
					
					Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxHealth"] = Config.Bind(String_Section, String_Key_MaxHealth, 0, ConfigDescription_MaxHealth);
					Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxHealth"] = Config.Bind(String_Section, String_Key_MaxHealth, 0, ConfigDescription_MaxHealth);
					Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_HealthRegen"] = Config.Bind(String_Section, String_Key_HealthRegen, 0, ConfigDescription_HealthRegen);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_HealthRegenModifier"] = Config.Bind(String_Section, String_Key_HealthRegenModifier, 0.00F, ConfigDescription_HealthRegenModifier);
					Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxStamina"] = Config.Bind(String_Section, String_Key_MaxStamina, 0, ConfigDescription_MaxStamina);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_StaminaRegenModifier"] = Config.Bind(String_Section, String_Key_StaminaRegenModifier, 0.00F, ConfigDescription_StaminaRegenModifier);
					Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxEitr"] = Config.Bind(String_Section, String_Key_MaxEitr, 0, ConfigDescription_MaxEitr);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_EitrRegenModifier"] = Config.Bind(String_Section, String_Key_EitrRegenModifier, 0.00F, ConfigDescription_EitrRegenModifier);
					Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxWeight"] = Config.Bind(String_Section, String_Key_MaxWeight, 0, ConfigDescription_MaxWeight);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_MoveSpeedModifier"] = Config.Bind(String_Section, String_Key_MoveSpeedModifier, 0.00F, ConfigDescription_MoveSpeedModifier);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_JumpForceModifier"] = Config.Bind(String_Section, String_Key_JumpForceModifier, 0.00F, ConfigDescription_JumpForceModifier);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_MaxFallSpeed"] = Config.Bind(String_Section, String_Key_MaxFallSpeed, 0.00F, ConfigDescription_MaxFallSpeed);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_EquipmentSpeedModifier"] = Config.Bind(String_Section, String_Key_EquipmentSpeedModifier, 0.00F, ConfigDescription_EquipmentSpeedModifier);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_WindSpeedModifier"] = Config.Bind(String_Section, String_Key_WindSpeedModifier, 0.00F, ConfigDescription_WindSpeedModifier);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_RunStaminaCostModifier"] = Config.Bind(String_Section, String_Key_RunStaminaCostModifier, 0.00F, ConfigDescription_RunStaminaCostModifier);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_JumpStaminaCostModifier"] = Config.Bind(String_Section, String_Key_JumpStaminaCostModifier, 0.00F, ConfigDescription_JumpStaminaCostModifier);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_StealthModifier"] = Config.Bind(String_Section, String_Key_StealthModifier, 0.00F, ConfigDescription_StealthModifier);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_NoiseModifier"] = Config.Bind(String_Section, String_Key_NoiseModifier, 0.00F, ConfigDescription_NoiseModifier);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_StatusAttributes"] = Config.Bind(String_Section, String_Key_StatusAttributes, "", ConfigDescription_StatusAttributes);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_OutgoingDamageTypes"] = Config.Bind(String_Section, String_Key_OutgoingDamageTypes, "", ConfigDescription_OutgoingDamageTypes);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_OutgoingDamageModifiers"] = Config.Bind(String_Section, String_Key_OutgoingDamageModifiers, "", ConfigDescription_OutgoingDamageModifiers);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_FallDamageModifier"] = Config.Bind(String_Section, String_Key_FallDamageModifier, 0.00F, ConfigDescription_FallDamageModifier);
					Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_HeatDamageModifier"] = Config.Bind(String_Section, String_Key_HeatDamageModifier, 0.00F, ConfigDescription_HeatDamageModifier);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_IncomingDamageTypes"] = Config.Bind(String_Section, String_Key_IncomingDamageTypes, "", ConfigDescription_IncomingDamageTypes);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_IncomingDamageModifiers"] = Config.Bind(String_Section, String_Key_IncomingDamageModifiers, "", ConfigDescription_IncomingDamageModifiers);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_IncomingDamageMultiplierTypes"] = Config.Bind(String_Section, String_Key_IncomingDamageMultiplierTypes, "", ConfigDescription_IncomingDamageMultiplierTypes);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_IncomingDamageMultiplierModifiers"] = Config.Bind(String_Section, String_Key_IncomingDamageMultiplierModifiers, "", ConfigDescription_IncomingDamageMultiplierModifiers);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_OutgoingDamageMultiplierTypes"] = Config.Bind(String_Section, String_Key_OutgoingDamageMultiplierTypes, "", ConfigDescription_OutgoingDamageMultiplierTypes);
					Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_OutgoingDamageMultiplierModifiers"] = Config.Bind(String_Section, String_Key_OutgoingDamageMultiplierModifiers, "", ConfigDescription_OutgoingDamageMultiplierModifiers);
				}
			}
			
			Config.SettingChanged += Config_SettingChanged;
			Config.SaveOnConfigSet = true;
			Config.Save();
		}
		
		static void Config_SettingChanged(object New_Object, EventArgs New_EventArgs)
		{
			Config_SettingRead();
		}
		
		static void Config_SettingRead()
		{
			if(ConfigEntry_General_Preset.Value == "Custom")
			{
				foreach(string Boss in List_BossNames)
				{
					int Index = Boss switch
					{
						"Eikthyr" => 0,
						"TheElder" => 1,
						"Bonemass" => 2,
						"Moder" => 3,
						"Yagluth" => 4,
						"Queen" => 5,
						"Fader" => 6,
					
						_ => 0
					};
				
					foreach(string Component in List_Components)
					{
						StatusEffect_FPO New_StatusEffect_FPO = Component switch
						{
							"Passive" => List_StatusEffect_FPO_Passive[Index],
							"Equipped" => List_StatusEffect_FPO_Equipped[Index],
							"Active" => List_StatusEffect_FPO_Active[Index],
							"Shared" => List_StatusEffect_FPO_Shared[Index],
						
							_ => null
						};
					
						if(New_StatusEffect_FPO != null)
						{
							New_StatusEffect_FPO.m_MaxHealth = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Boss + "_" + Component + "_MaxHealth"].Value;
							New_StatusEffect_FPO.m_HealthRegen = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Boss + "_" + Component + "_HealthRegen"].Value;
							New_StatusEffect_FPO.m_healthRegenMultiplier = 1.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_HealthRegenModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_MaxStamina = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Boss + "_" + Component + "_MaxStamina"].Value;
							New_StatusEffect_FPO.m_staminaRegenMultiplier = 1.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_StaminaRegenModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_MaxEitr = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Boss + "_" + Component + "_MaxEitr"].Value;
							New_StatusEffect_FPO.m_eitrRegenMultiplier = 1.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_EitrRegenModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_addMaxCarryWeight = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Boss + "_" + Component + "_MaxWeight"].Value;;
							New_StatusEffect_FPO.m_speedModifier = 0.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_MoveSpeedModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_jumpModifier = (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_JumpForceModifier"].Value != 0.00F) ? (Vector3.one * (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_JumpForceModifier"].Value / 200.00F)) : Vector3.zero;
							New_StatusEffect_FPO.m_maxMaxFallSpeed = 0.00F + Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_MaxFallSpeed"].Value;
							New_StatusEffect_FPO.m_EquipmentSpeedModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_EquipmentSpeedModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_WindSpeedModifier = 0.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_WindSpeedModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_runStaminaDrainModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_RunStaminaCostModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_jumpStaminaUseModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_JumpStaminaCostModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_stealthModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_StealthModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_noiseModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_NoiseModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_StatusAttributes = GetStatusAttributes(Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_StatusAttributes"].Value);
							New_StatusEffect_FPO.m_OutgoingDamageTypes = GetOutgoingDamages(Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_OutgoingDamageTypes"].Value, Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_OutgoingDamageModifiers"].Value, out List<float> Out_OutgoingDamageModifiers);
							New_StatusEffect_FPO.m_OutgoingDamageModifiers = Out_OutgoingDamageModifiers;
							New_StatusEffect_FPO.m_fallDamageModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_FallDamageModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_HeatDamageModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Boss + "_" + Component + "_HeatDamageModifier"].Value / 100.00F);
							New_StatusEffect_FPO.m_mods = GetIncomingDamages(Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_IncomingDamageTypes"].Value, Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_IncomingDamageModifiers"].Value);
							New_StatusEffect_FPO.m_IncomingDamageMultiplierTypes = GetIncomingDamageMultiplierTypes(Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_IncomingDamageMultiplierTypes"].Value, Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_IncomingDamageMultiplierModifiers"].Value, out List<float> IncomingMultiplierMods);
							New_StatusEffect_FPO.m_IncomingDamageMultiplierModifiers = IncomingMultiplierMods;
							New_StatusEffect_FPO.m_OutgoingDamageMultiplierTypes = GetOutgoingDamageMultiplierTypes(Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_OutgoingDamageMultiplierTypes"].Value, Dictionary_ConfigEntry_String["ConfigEntry_" + Boss + "_" + Component + "_OutgoingDamageMultiplierModifiers"].Value, out List<float> OutgoingMultiplierMods);
							New_StatusEffect_FPO.m_OutgoingDamageMultiplierModifiers = OutgoingMultiplierMods;
						}
					}
				}
			}
			
			if(ConfigEntry_General_Preset.Value == "Vanilla")
			{
				Config_SettingReset();

				StatusEffect_FPO_Eikthyr_Active.m_runStaminaDrainModifier = -0.60F;
				StatusEffect_FPO_Eikthyr_Active.m_jumpStaminaUseModifier = -0.60F;
				
				StatusEffect_FPO_TheElder_Active.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.WoodCutting };
				StatusEffect_FPO_TheElder_Active.m_OutgoingDamageModifiers = new List<float>{ 0.60F };
				
				StatusEffect_FPO_Bonemass_Active.m_mods = new List<HitData.DamageModPair>
				{
					new HitData.DamageModPair{ m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant }
				};
				
				StatusEffect_FPO_Moder_Active.m_StatusAttributes = new List<StatusEffect.StatusAttribute>{ StatusEffect.StatusAttribute.SailingPower };
				
				StatusEffect_FPO_Yagluth_Active.m_mods = new List<HitData.DamageModPair>
				{
					new HitData.DamageModPair{ m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant }
				};
				
				StatusEffect_FPO_Queen_Active.m_eitrRegenMultiplier = 2.00F;
				StatusEffect_FPO_Queen_Active.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.Pickaxes };
				StatusEffect_FPO_Queen_Active.m_OutgoingDamageModifiers = new List<float>{ 0.60F };
				
				StatusEffect_FPO_Fader_Active.m_addMaxCarryWeight = 300;
				StatusEffect_FPO_Fader_Active.m_speedModifier = 0.10F;
			}
			
			if(ConfigEntry_General_Preset.Value == "Vanilla+")
			{
				Config_SettingReset();
				
				StatusEffect_FPO_Eikthyr_Passive.m_MaxStamina = 25;
				StatusEffect_FPO_Eikthyr_Passive.m_EquipmentSpeedModifier = -0.50F;
				
				StatusEffect_FPO_Eikthyr_Equipped.m_staminaRegenMultiplier = 2.00F;
				StatusEffect_FPO_Eikthyr_Equipped.m_speedModifier = 0.10F;
				StatusEffect_FPO_Eikthyr_Equipped.m_jumpModifier = Vector3.one * 0.25F;

				StatusEffect_FPO_Eikthyr_Active.m_EquipmentSpeedModifier = -0.50F;
				StatusEffect_FPO_Eikthyr_Active.m_runStaminaDrainModifier = -0.50F;
				StatusEffect_FPO_Eikthyr_Active.m_jumpStaminaUseModifier = -0.50F;
				
				StatusEffect_FPO_Eikthyr_Shared.m_staminaRegenMultiplier = 1.50F;
				
				StatusEffect_FPO_TheElder_Passive.m_MaxHealth = 10;
				StatusEffect_FPO_TheElder_Passive.m_HealthRegen = 1;
				StatusEffect_FPO_TheElder_Passive.m_addMaxCarryWeight = 50;
				
				StatusEffect_FPO_TheElder_Equipped.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.WoodCutting };
				StatusEffect_FPO_TheElder_Equipped.m_OutgoingDamageModifiers = new List<float>{ 3.00F };
				
				StatusEffect_FPO_TheElder_Active.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.Axes, Skills.SkillType.Knives, Skills.SkillType.Swords };
				StatusEffect_FPO_TheElder_Active.m_OutgoingDamageModifiers = new List<float>{ 1.00F, 1.00F, 1.00F };
				
				StatusEffect_FPO_TheElder_Shared.m_healthRegenMultiplier = 1.50F;
				
				StatusEffect_FPO_Bonemass_Passive.m_MaxHealth = 15;
				
				StatusEffect_FPO_Bonemass_Equipped.m_healthRegenMultiplier = 2.00F;
				
				StatusEffect_FPO_Bonemass_Active.m_mods = new List<HitData.DamageModPair>
				{
					new HitData.DamageModPair{ m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant }
				};
				
				StatusEffect_FPO_Bonemass_Shared.m_healthRegenMultiplier = 1.50F;
				
				StatusEffect_FPO_Moder_Passive.m_fallDamageModifier = -0.50F;
				
				StatusEffect_FPO_Moder_Equipped.m_WindSpeedModifier = 1.00F;
				StatusEffect_FPO_Moder_Equipped.m_StatusAttributes = new List<StatusEffect.StatusAttribute>{ StatusEffect.StatusAttribute.SailingPower };
				
				StatusEffect_FPO_Moder_Active.m_maxMaxFallSpeed = 5.00F;
				StatusEffect_FPO_Moder_Active.m_fallDamageModifier = -1.00F;
				
				StatusEffect_FPO_Moder_Shared.m_speedModifier = 0.10F;
				
				StatusEffect_FPO_Yagluth_Passive.m_MaxEitr = 20;
				
				StatusEffect_FPO_Yagluth_Equipped.m_eitrRegenMultiplier = 2.00F;
				
				StatusEffect_FPO_Yagluth_Active.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.ElementalMagic };
				StatusEffect_FPO_Yagluth_Active.m_OutgoingDamageModifiers = new List<float>{ 1.00F };
				StatusEffect_FPO_Yagluth_Active.m_mods = new List<HitData.DamageModPair>
				{
					new HitData.DamageModPair{ m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant }
				};
				
				StatusEffect_FPO_Yagluth_Shared.m_eitrRegenMultiplier = 1.50F;
				
				StatusEffect_FPO_Queen_Passive.m_MaxEitr = 30;
				StatusEffect_FPO_Queen_Passive.m_addMaxCarryWeight = 50;
				
				StatusEffect_FPO_Queen_Equipped.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.Pickaxes };
				StatusEffect_FPO_Queen_Equipped.m_OutgoingDamageModifiers = new List<float>{ 3.00F };
				
				StatusEffect_FPO_Queen_Active.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.BloodMagic };
				StatusEffect_FPO_Queen_Active.m_OutgoingDamageModifiers = new List<float>{ 1.00F };
				
				StatusEffect_FPO_Queen_Shared.m_eitrRegenMultiplier = 1.50F;
				
				StatusEffect_FPO_Fader_Passive.m_MaxStamina = 50;
				StatusEffect_FPO_Fader_Passive.m_addMaxCarryWeight = 200;
				
				StatusEffect_FPO_Fader_Equipped.m_HeatDamageModifier = -0.60F;
				
				StatusEffect_FPO_Fader_Active.m_HeatDamageModifier = -1.00F;
				
				StatusEffect_FPO_Fader_Shared.m_speedModifier = 0.10F;
				StatusEffect_FPO_Fader_Shared.m_staminaRegenMultiplier = 1.50F;
			}
			
			if(ConfigEntry_General_Preset.Value == "Passive Powers")
			{
				Config_SettingReset();
				
				StatusEffect_FPO_Eikthyr_Passive.m_runStaminaDrainModifier = -0.60F;
				StatusEffect_FPO_Eikthyr_Passive.m_jumpStaminaUseModifier = -0.60F;
				
				StatusEffect_FPO_TheElder_Passive.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.WoodCutting };
				StatusEffect_FPO_TheElder_Passive.m_OutgoingDamageModifiers = new List<float>{ 0.60F };
				
				StatusEffect_FPO_Bonemass_Passive.m_mods = new List<HitData.DamageModPair>
				{
					new HitData.DamageModPair{ m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant }
				};
				
				StatusEffect_FPO_Moder_Passive.m_StatusAttributes = new List<StatusEffect.StatusAttribute>{ StatusEffect.StatusAttribute.SailingPower };
				
				StatusEffect_FPO_Yagluth_Passive.m_mods = new List<HitData.DamageModPair>
				{
					new HitData.DamageModPair{ m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant },
					new HitData.DamageModPair{ m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant }
				};
				
				StatusEffect_FPO_Queen_Passive.m_eitrRegenMultiplier = 2.00F;
				StatusEffect_FPO_Queen_Passive.m_OutgoingDamageTypes = new List<Skills.SkillType>{ Skills.SkillType.Pickaxes };
				StatusEffect_FPO_Queen_Passive.m_OutgoingDamageModifiers = new List<float>{ 0.60F };
				
				StatusEffect_FPO_Fader_Passive.m_addMaxCarryWeight = 300;
				StatusEffect_FPO_Fader_Passive.m_speedModifier = 0.10F;
			}
			
			Update_Passive_Stats();
			Update_Active_Stats();
		}
		
		static void Config_SettingReset()
		{
			// Assert that arrays are initialized
			if (List_StatusEffect_FPO_Passive == null || List_StatusEffect_FPO_Equipped == null || 
				List_StatusEffect_FPO_Active == null || List_StatusEffect_FPO_Shared == null)
			{
				ForsakenPowerOverhaul.log?.LogError("ASSERTION FAILED: StatusEffect arrays are null in Config_SettingReset");
				throw new System.Exception("ForsakenPowerOverhaul assertion failed: StatusEffect arrays are null in Config_SettingReset");
			}
			
			foreach(string Boss in List_BossNames)
			{
				int Index = Boss switch
				{
					"Eikthyr" => 0,
					"TheElder" => 1,
					"Bonemass" => 2,
					"Moder" => 3,
					"Yagluth" => 4,
					"Queen" => 5,
					"Fader" => 6,
					
					_ => 0
				};
				
				// Assert list bounds
				if (Index < 0 || Index >= List_StatusEffect_FPO_Passive.Count || 
					Index >= List_StatusEffect_FPO_Equipped.Count ||
					Index >= List_StatusEffect_FPO_Active.Count ||
					Index >= List_StatusEffect_FPO_Shared.Count)
				{
					ForsakenPowerOverhaul.log?.LogError($"ASSERTION FAILED: Index {Index} is out of bounds for boss {Boss}");
					throw new System.Exception($"ForsakenPowerOverhaul assertion failed: Index {Index} out of bounds for boss {Boss}");
				}
				
				foreach(string Component in List_Components)
				{
					StatusEffect_FPO New_StatusEffect_FPO = Component switch
					{
						"Passive" => List_StatusEffect_FPO_Passive[Index],
						"Equipped" => List_StatusEffect_FPO_Equipped[Index],
						"Active" => List_StatusEffect_FPO_Active[Index],
						"Shared" => List_StatusEffect_FPO_Shared[Index],
					
						_ => null
					};
				
					if(New_StatusEffect_FPO != null)
					{
						New_StatusEffect_FPO.m_MaxHealth = 0;
						New_StatusEffect_FPO.m_HealthRegen = 0;
						New_StatusEffect_FPO.m_healthRegenMultiplier = 1.00F;
						New_StatusEffect_FPO.m_MaxStamina = 0;
						New_StatusEffect_FPO.m_staminaRegenMultiplier = 1.00F;
						New_StatusEffect_FPO.m_MaxEitr = 0;
						New_StatusEffect_FPO.m_eitrRegenMultiplier = 1.00F;
						New_StatusEffect_FPO.m_addMaxCarryWeight = 0;
						New_StatusEffect_FPO.m_speedModifier = 0.00F;
						New_StatusEffect_FPO.m_jumpModifier = Vector3.zero;
						New_StatusEffect_FPO.m_maxMaxFallSpeed = 0.00F;
						New_StatusEffect_FPO.m_EquipmentSpeedModifier = 0.00F;
						New_StatusEffect_FPO.m_WindSpeedModifier = 0.00F;
						New_StatusEffect_FPO.m_runStaminaDrainModifier = 0.00F;
						New_StatusEffect_FPO.m_jumpStaminaUseModifier = 0.00F;
						New_StatusEffect_FPO.m_stealthModifier = 0.00F;
						New_StatusEffect_FPO.m_noiseModifier = 0.00F;
						New_StatusEffect_FPO.m_StatusAttributes = new List<StatusEffect.StatusAttribute>{ };
						New_StatusEffect_FPO.m_OutgoingDamageTypes = new List<Skills.SkillType>{ };
						New_StatusEffect_FPO.m_OutgoingDamageModifiers = new List<float>{ };
						New_StatusEffect_FPO.m_fallDamageModifier = 0.00F;
						New_StatusEffect_FPO.m_HeatDamageModifier = 0.00F;
						New_StatusEffect_FPO.m_mods = new List<HitData.DamageModPair>{ };
						New_StatusEffect_FPO.m_IncomingDamageMultiplierTypes = new List<HitData.DamageType>{ };
						New_StatusEffect_FPO.m_IncomingDamageMultiplierModifiers = new List<float>{ };
						New_StatusEffect_FPO.m_OutgoingDamageMultiplierTypes = new List<Skills.SkillType>{ };
						New_StatusEffect_FPO.m_OutgoingDamageMultiplierModifiers = new List<float>{ };
					}
				}
			}
		}
	}
}