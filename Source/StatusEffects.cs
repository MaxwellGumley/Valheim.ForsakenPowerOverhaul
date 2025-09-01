using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		static StatusEffect_FPO StatusEffect_FPO_Passive;
		static StatusEffect_FPO StatusEffect_FPO_Eikthyr;
		static StatusEffect_FPO StatusEffect_FPO_TheElder;
		static StatusEffect_FPO StatusEffect_FPO_Bonemass;
		static StatusEffect_FPO StatusEffect_FPO_Moder;
		static StatusEffect_FPO StatusEffect_FPO_Yagluth;
		static StatusEffect_FPO StatusEffect_FPO_Queen;
		static StatusEffect_FPO StatusEffect_FPO_Fader;
		
		static StatusEffect_FPO StatusEffect_FPO_Eikthyr_Passive;
		static StatusEffect_FPO StatusEffect_FPO_TheElder_Passive;
		static StatusEffect_FPO StatusEffect_FPO_Bonemass_Passive;
		static StatusEffect_FPO StatusEffect_FPO_Moder_Passive;
		static StatusEffect_FPO StatusEffect_FPO_Yagluth_Passive;
		static StatusEffect_FPO StatusEffect_FPO_Queen_Passive;
		static StatusEffect_FPO StatusEffect_FPO_Fader_Passive;
		
		static StatusEffect_FPO StatusEffect_FPO_Eikthyr_Active;
		static StatusEffect_FPO StatusEffect_FPO_TheElder_Active;
		static StatusEffect_FPO StatusEffect_FPO_Bonemass_Active;
		static StatusEffect_FPO StatusEffect_FPO_Moder_Active;
		static StatusEffect_FPO StatusEffect_FPO_Yagluth_Active;
		static StatusEffect_FPO StatusEffect_FPO_Queen_Active;
		static StatusEffect_FPO StatusEffect_FPO_Fader_Active;
		
		static StatusEffect_FPO StatusEffect_FPO_Eikthyr_Shared;
		static StatusEffect_FPO StatusEffect_FPO_TheElder_Shared;
		static StatusEffect_FPO StatusEffect_FPO_Bonemass_Shared;
		static StatusEffect_FPO StatusEffect_FPO_Moder_Shared;
		static StatusEffect_FPO StatusEffect_FPO_Yagluth_Shared;
		static StatusEffect_FPO StatusEffect_FPO_Queen_Shared;
		static StatusEffect_FPO StatusEffect_FPO_Fader_Shared;
		
		static StatusEffect_FPO StatusEffect_FPO_Eikthyr_Equipped;
		static StatusEffect_FPO StatusEffect_FPO_TheElder_Equipped;
		static StatusEffect_FPO StatusEffect_FPO_Bonemass_Equipped;
		static StatusEffect_FPO StatusEffect_FPO_Moder_Equipped;
		static StatusEffect_FPO StatusEffect_FPO_Yagluth_Equipped;
		static StatusEffect_FPO StatusEffect_FPO_Queen_Equipped;
		static StatusEffect_FPO StatusEffect_FPO_Fader_Equipped;
		
		class StatusEffect_FPO : SE_Stats
		{
			public float m_MaxHealth = 0.00F;
			public float m_HealthRegen = 0.00F;
			public float m_MaxStamina = 0.00F;
			public float m_MaxEitr = 0.00F;
			public float m_EquipmentSpeedModifier = 0.00F;
			public List<StatusEffect.StatusAttribute> m_StatusAttributes = new List<StatusEffect.StatusAttribute>{ };
			public float m_WindSpeedModifier = 0.00F;
			public List<Skills.SkillType> m_OutgoingDamageTypes = new List<Skills.SkillType>{ };
			public List<float> m_OutgoingDamageModifiers = new List<float>{ };
			public float m_HeatDamageModifier = 0.00F;
			
			// New multiplier fields for arbitrary percentage damage modification
			public List<HitData.DamageType> m_IncomingDamageMultiplierTypes = new List<HitData.DamageType>{ };
			public List<float> m_IncomingDamageMultiplierModifiers = new List<float>{ };
			public List<Skills.SkillType> m_OutgoingDamageMultiplierTypes = new List<Skills.SkillType>{ };
			public List<float> m_OutgoingDamageMultiplierModifiers = new List<float>{ };
			
			public new bool HaveAttribute(StatusEffect.StatusAttribute New_StatusAttributeA)
			{
				bool New_Bool = false;
				
				foreach(StatusEffect.StatusAttribute New_StatusAttributeB in m_StatusAttributes)
				{
					if(New_StatusAttributeA == New_StatusAttributeB)
					{ New_Bool = true; }
				}
				
				return New_Bool;
			}
			
			public override void ModifyAttack(Skills.SkillType New_SkillTypeA, ref HitData New_HitData)
			{
				// Apply regular outgoing damage modifiers (additive)
				foreach(Skills.SkillType New_SkillTypeB in m_OutgoingDamageTypes)
				{
					if((New_SkillTypeB == New_SkillTypeA) || New_SkillTypeB == Skills.SkillType.All)
					{ New_HitData.m_damage.Modify((1.00F + m_OutgoingDamageModifiers[m_OutgoingDamageTypes.IndexOf(New_SkillTypeB)])); }
				}
				
				// Apply outgoing damage multipliers (multiplicative)
				foreach(Skills.SkillType skillType in m_OutgoingDamageMultiplierTypes)
				{
					if((skillType == New_SkillTypeA) || skillType == Skills.SkillType.All)
					{
						float multiplier = m_OutgoingDamageMultiplierModifiers[m_OutgoingDamageMultiplierTypes.IndexOf(skillType)];
						if(multiplier != 100.00F) // Only apply if different from 100% (no change)
						{
							New_HitData.m_damage.Modify(multiplier / 100.00F);
						}
					}
				}
			}
			
			public override string GetTooltipString()
			{
				StringBuilder New_StringBuilder = new StringBuilder(256);
				
				New_StringBuilder.Append(GetTooltipStats(this));
				
				string New_String = New_StringBuilder.ToString();
				
				// Pass the tooltip string through the translation system
				New_String = ForsakenPowerOverhaul.GetLocalization(New_String);
				
				//if(Mod_Auga)
				//{ New_String = " \n<size=12>" + New_String + "</size>"; }
				
				return New_String;
			}
		}
		
		static void Add_StatusEffects()
		{
			StatusEffect_FPO New_StatusEffect_FPO_Passive = ScriptableObject.CreateInstance<StatusEffect_FPO>();
			New_StatusEffect_FPO_Passive.name = "SE_FPO_Passive";
			New_StatusEffect_FPO_Passive.m_name = "$ui_fpo_passive";
			New_StatusEffect_FPO_Passive.m_tooltip = "$ui_fpo_passive";
			ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(New_StatusEffect_FPO_Passive, false));
			
			foreach(string Power in List_BossNames)
			{
				StatusEffect_FPO New_StatusEffect_FPO = ScriptableObject.CreateInstance<StatusEffect_FPO>();
				New_StatusEffect_FPO.name = ("SE_FPO_" + Power);
				New_StatusEffect_FPO.m_name = ("$se_" + Power.ToLower() + "_name");
				New_StatusEffect_FPO.m_tooltip = ("$se_" + Power.ToLower() + "_name");
				ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(New_StatusEffect_FPO, false));
			}
			
			foreach(string Power in List_BossNames)
			{
				foreach(string Component in List_Components)
				{
					StatusEffect_FPO New_StatusEffect_FPO = ScriptableObject.CreateInstance<StatusEffect_FPO>();
					New_StatusEffect_FPO.name = ("SE_FPO_" + Power + "_" + Component);
					New_StatusEffect_FPO.m_name = (Component == "Equipped") ? ("$ui_fpo_equipped $ui_fpo_passive") : ("$se_" + Power.ToLower() + "_name");
					New_StatusEffect_FPO.m_MaxHealth = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxHealth"].Value;
					New_StatusEffect_FPO.m_HealthRegen = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_HealthRegen"].Value;
					New_StatusEffect_FPO.m_healthRegenMultiplier = 1.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_HealthRegenModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_MaxStamina = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxStamina"].Value;
					New_StatusEffect_FPO.m_staminaRegenMultiplier = 1.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_StaminaRegenModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_MaxEitr = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxEitr"].Value;
					New_StatusEffect_FPO.m_eitrRegenMultiplier = 1.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_EitrRegenModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_addMaxCarryWeight = 0 + Dictionary_ConfigEntry_Int["ConfigEntry_" + Power + "_" + Component + "_MaxWeight"].Value;;
					New_StatusEffect_FPO.m_speedModifier = 0.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_MoveSpeedModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_jumpModifier = (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_JumpForceModifier"].Value != 0.00F) ? (Vector3.one * (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_JumpForceModifier"].Value / 200.00F)) : Vector3.zero;
					New_StatusEffect_FPO.m_maxMaxFallSpeed = 0.00F + Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_MaxFallSpeed"].Value;
					New_StatusEffect_FPO.m_EquipmentSpeedModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_EquipmentSpeedModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_WindSpeedModifier = 0.00F + (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_WindSpeedModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_runStaminaDrainModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_RunStaminaCostModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_jumpStaminaUseModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_JumpStaminaCostModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_stealthModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_StealthModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_noiseModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_NoiseModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_StatusAttributes = GetStatusAttributes(Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_StatusAttributes"].Value);
					New_StatusEffect_FPO.m_OutgoingDamageTypes = GetOutgoingDamages(Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_OutgoingDamageTypes"].Value, Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_OutgoingDamageModifiers"].Value, out List<float> Out_OutgoingDamageModifiers);
					New_StatusEffect_FPO.m_OutgoingDamageModifiers = Out_OutgoingDamageModifiers;
					New_StatusEffect_FPO.m_fallDamageModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_FallDamageModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_HeatDamageModifier = 0.00F - (Dictionary_ConfigEntry_Float["ConfigEntry_" + Power + "_" + Component + "_HeatDamageModifier"].Value / 100.00F);
					New_StatusEffect_FPO.m_mods = GetIncomingDamages(Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_IncomingDamageTypes"].Value, Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_IncomingDamageModifiers"].Value);
					
					// Add new multiplier fields
					New_StatusEffect_FPO.m_IncomingDamageMultiplierTypes = GetIncomingDamageMultiplierTypes(Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_IncomingDamageMultiplierTypes"].Value, Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_IncomingDamageMultiplierModifiers"].Value, out List<float> Out_IncomingDamageMultiplierModifiers);
					New_StatusEffect_FPO.m_IncomingDamageMultiplierModifiers = Out_IncomingDamageMultiplierModifiers;
					New_StatusEffect_FPO.m_OutgoingDamageMultiplierTypes = GetOutgoingDamageMultiplierTypes(Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_OutgoingDamageMultiplierTypes"].Value, Dictionary_ConfigEntry_String["ConfigEntry_" + Power + "_" + Component + "_OutgoingDamageMultiplierModifiers"].Value, out List<float> Out_OutgoingDamageMultiplierModifiers);
					New_StatusEffect_FPO.m_OutgoingDamageMultiplierModifiers = Out_OutgoingDamageMultiplierModifiers;
					
					ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(New_StatusEffect_FPO, false));
				}
			}
		}
		
		static List<StatusEffect.StatusAttribute> GetStatusAttributes(string List)
		{
			List<string> New_List = ParseCommaSeparatedValues(List);
			
			List<StatusEffect.StatusAttribute> m_StatusAttributes = new List<StatusEffect.StatusAttribute>{ };
			
			for(int Index = 0; Index < New_List.Count; Index ++)
			{
				if(GetStatusAttributeValid(New_List[Index].ToLower()))
				{
					bool New_Bool = true;
					
					foreach(StatusEffect.StatusAttribute New_StatusAttribute in m_StatusAttributes)
					{
						if(GetStatusAttribute(New_List[Index].ToLower()) == New_StatusAttribute)
						{ New_Bool = false; }
					}
					
					if(New_Bool)
					{ m_StatusAttributes.Add(GetStatusAttribute(New_List[Index].ToLower())); }
				}
			}
			
			return m_StatusAttributes;
		}
		
		static bool GetStatusAttributeValid(string New_String)
		{
			return New_String switch
			{
				"coldresistance" => true,
				"doubleimpactdamage" => true,
				"sailingpower" => true,
				
				_ => false
			};
		}
		
		static StatusEffect.StatusAttribute GetStatusAttribute(string New_String)
		{
			return New_String switch
			{
				"coldresistance" => StatusEffect.StatusAttribute.ColdResistance,
				"doubleimpactdamage" => StatusEffect.StatusAttribute.DoubleImpactDamage,
				"sailingpower" => StatusEffect.StatusAttribute.SailingPower,
				
				_ => StatusEffect.StatusAttribute.None
			};
		}
		
		static List<Skills.SkillType> GetOutgoingDamages(string String_Type, string String_Modifier, out List<float> Out_OutgoingDamageModifiers)
		{
			List<string> List_Type = ParseCommaSeparatedValues(String_Type);
			List<string> List_Modifier = ParseCommaSeparatedValues(String_Modifier);
			
			List<Skills.SkillType> m_OutgoingDamageTypes = new List<Skills.SkillType>{ };
			List<float> m_OutgoingDamageModifiers = new List<float>{ };
			
			for(int Index = 0; ((Index < List_Type.Count) && (Index < List_Modifier.Count)); Index ++)
			{
				if(GetOutgoingDamageTypeValid(List_Type[Index].ToLower()))
				{
					bool New_Bool = true;
					
					foreach(Skills.SkillType New_SkillType in m_OutgoingDamageTypes)
					{
						if(GetOutgoingDamageType(List_Type[Index].ToLower()) == New_SkillType)
						{ New_Bool = false; }
					}
					
					if(New_Bool)
					{
						m_OutgoingDamageTypes.Add(GetOutgoingDamageType(List_Type[Index].ToLower()));
						
						if(float.TryParse(List_Modifier[Index], out float New_Float))
						{ m_OutgoingDamageModifiers.Add((New_Float / 100.00F)); }
						else
						{ m_OutgoingDamageModifiers.Add(New_Float); }
					}
				}
			}
			
			Out_OutgoingDamageModifiers = m_OutgoingDamageModifiers;
			return m_OutgoingDamageTypes;
		}
		
		static bool GetOutgoingDamageTypeValid(string New_String)
		{
			return New_String switch
			{
				"all" => true,
				"unarmed" => true,
				"clubs" => true,
				"axes" => true,
				"knives" => true,
				"spears" => true,
				"swords" => true,
				"polearms" => true,
				"bows" => true,
				"crossbows" => true,
				"elementalmagic" => true,
				"bloodmagic" => true,
				"woodcutting" => true,
				"pickaxes" => true,
				
				_ => false
			};
		}
		
		static Skills.SkillType GetOutgoingDamageType(string New_String)
		{
			return New_String switch
			{
				"all" => Skills.SkillType.All,
				"unarmed" => Skills.SkillType.Unarmed,
				"clubs" => Skills.SkillType.Clubs,
				"axes" => Skills.SkillType.Axes,
				"knives" => Skills.SkillType.Knives,
				"spears" => Skills.SkillType.Spears,
				"swords" => Skills.SkillType.Swords,
				"polearms" => Skills.SkillType.Polearms,
				"bows" => Skills.SkillType.Bows,
				"crossbows" => Skills.SkillType.Crossbows,
				"elementalmagic" => Skills.SkillType.ElementalMagic,
				"bloodmagic" => Skills.SkillType.BloodMagic,
				"woodcutting" => Skills.SkillType.WoodCutting,
				"pickaxes" => Skills.SkillType.Pickaxes,
				
				_ => Skills.SkillType.None
			};
		}
		
		static List<HitData.DamageModPair> GetIncomingDamages(string String_Type, string String_Modifier)
		{
			List<string> List_Type = ParseCommaSeparatedValues(String_Type);
			List<string> List_Modifier = ParseCommaSeparatedValues(String_Modifier);
			
			List<HitData.DamageModPair> m_Mods = new List<HitData.DamageModPair>{ };
			
			for(int Index = 0; ((Index < List_Type.Count) && (Index < List_Modifier.Count)); Index ++)
			{
				if(GetIncomingDamageTypeValid(List_Type[Index].ToLower()))
				{
					bool New_Bool = true;
					
					foreach(HitData.DamageModPair New_DamageModPair in m_Mods)
					{
						if(GetIncomingDamageType(List_Type[Index].ToLower()) == New_DamageModPair.m_type)
						{ New_Bool = false; }
					}
					
					if(New_Bool)
					{
						if(int.TryParse(List_Modifier[Index], out int New_Int))
						{ m_Mods.Add(new HitData.DamageModPair{ m_type = GetIncomingDamageType(List_Type[Index].ToLower()), m_modifier = GetIncomingDamageModifier(New_Int) }); }
						else
						{ m_Mods.Add(new HitData.DamageModPair{ m_type = GetIncomingDamageType(List_Type[Index].ToLower()), m_modifier = HitData.DamageModifier.Normal }); }
					}
				}
			}
			
			return m_Mods;
		}
		
		static bool GetIncomingDamageTypeValid(string New_String)
		{
			return New_String switch
			{
				"physical" => true,
				"pierce" => true,
				"slash" => true,
				"blunt" => true,
				"elemental" => true,
				"lightning" => true,
				"poison" => true,
				"frost" => true,
				"fire" => true,
				"spirit" => true,
				"chop" => true,
				"pickaxe" => true,
				
				_ => false
			};
		}
		
		static HitData.DamageType GetIncomingDamageType(string New_String)
		{
			return New_String switch
			{
				"physical" => HitData.DamageType.Physical,
				"pierce" => HitData.DamageType.Pierce,
				"slash" => HitData.DamageType.Slash,
				"blunt" => HitData.DamageType.Blunt,
				"elemental" => HitData.DamageType.Elemental,
				"lightning" => HitData.DamageType.Lightning,
				"poison" => HitData.DamageType.Poison,
				"frost" => HitData.DamageType.Frost,
				"fire" => HitData.DamageType.Fire,
				"spirit" => HitData.DamageType.Spirit,
				"chop" => HitData.DamageType.Chop,
				"pickaxe" => HitData.DamageType.Pickaxe,
				
				_ => HitData.DamageType.Physical
			};
		}
		
		static HitData.DamageModifier GetIncomingDamageModifier(int New_Int)
		{
			return New_Int switch
			{
				200 => HitData.DamageModifier.VeryWeak,
				150 => HitData.DamageModifier.Weak,
				100 => HitData.DamageModifier.Normal,
				50 => HitData.DamageModifier.Resistant,
				25 => HitData.DamageModifier.VeryResistant,
				0 => HitData.DamageModifier.Immune,
				
				_ => HitData.DamageModifier.Normal
			};
		}
		
		/// <summary>
		/// Parse comma-separated values, handling both quoted and unquoted formats
		/// Also handles semicolon-separated values for LibreCalc compatibility
		/// </summary>
		static List<string> ParseCommaSeparatedValues(string input)
		{
			if (string.IsNullOrEmpty(input))
				return new List<string>();
			
			// Trim the input
			input = input.Trim();
			
			// Check if trimmed input is empty
			if (string.IsNullOrEmpty(input))
				return new List<string>();
			
			// Handle escaped quotes from BepInEx config system
			if (input.StartsWith("\\\"") && input.EndsWith("\\\"") && input.Length >= 4)
			{
				// Remove escaped quotes and split by comma
				string unquoted = input.Substring(2, input.Length - 4);
				return unquoted.Split(',').Select(s => s.Trim()).ToList();
			}
			// Check if the entire string is quoted (regular quotes)
			else if (input.StartsWith("\"") && input.EndsWith("\"") && input.Length >= 2)
			{
				// Remove outer quotes and split by comma
				string unquoted = input.Substring(1, input.Length - 2);
				return unquoted.Split(',').Select(s => s.Trim()).ToList();
			}
			// Check if using pipe delimiter (recommended - avoids comment conflicts)
			else if (input.Contains("|"))
			{
				// Split by pipe and filter out empty entries
				return input.Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
			}
			else
			{
				// Normal comma-separated values
				return input.Split(',').Select(s => s.Trim()).ToList();
			}
		}

		/// <summary>
		/// Parse incoming damage multiplier types and modifiers from config strings
		/// </summary>
		static List<HitData.DamageType> GetIncomingDamageMultiplierTypes(string String_Type, string String_Modifier, out List<float> Out_IncomingDamageMultiplierModifiers)
		{
			List<string> List_Type = ParseCommaSeparatedValues(String_Type);
			List<string> List_Modifier = ParseCommaSeparatedValues(String_Modifier);
			
			List<HitData.DamageType> m_Types = new List<HitData.DamageType>{ };
			List<float> m_Modifiers = new List<float>{ };
			
			for(int Index = 0; ((Index < List_Type.Count) && (Index < List_Modifier.Count)); Index ++)
			{
				if(GetIncomingDamageTypeValid(List_Type[Index].ToLower()))
				{
					bool New_Bool = true;
					
					// Check for duplicates
					foreach(HitData.DamageType existingType in m_Types)
					{
						if(GetIncomingDamageType(List_Type[Index].ToLower()) == existingType)
						{ New_Bool = false; }
					}
					
					if(New_Bool)
					{
						if(float.TryParse(List_Modifier[Index], out float New_Float))
						{
							m_Types.Add(GetIncomingDamageType(List_Type[Index].ToLower()));
							m_Modifiers.Add(New_Float); // Store as percentage (0-200+)
						}
						else
						{
							m_Types.Add(GetIncomingDamageType(List_Type[Index].ToLower()));
							m_Modifiers.Add(100.0f); // Default to 100% (no change)
						}
					}
				}
			}
			
			Out_IncomingDamageMultiplierModifiers = m_Modifiers;
			return m_Types;
		}
		
		/// <summary>
		/// Parse outgoing damage multiplier types and modifiers from config strings
		/// </summary>
		static List<Skills.SkillType> GetOutgoingDamageMultiplierTypes(string String_Type, string String_Modifier, out List<float> Out_OutgoingDamageMultiplierModifiers)
		{
			List<string> List_Type = ParseCommaSeparatedValues(String_Type);
			List<string> List_Modifier = ParseCommaSeparatedValues(String_Modifier);
			
			List<Skills.SkillType> m_Types = new List<Skills.SkillType>{ };
			List<float> m_Modifiers = new List<float>{ };
			
			for(int Index = 0; ((Index < List_Type.Count) && (Index < List_Modifier.Count)); Index ++)
			{
				if(GetOutgoingDamageTypeValid(List_Type[Index].ToLower()))
				{
					bool New_Bool = true;
					
					// Check for duplicates
					foreach(Skills.SkillType existingType in m_Types)
					{
						if(GetOutgoingDamageType(List_Type[Index].ToLower()) == existingType)
						{ New_Bool = false; }
					}
					
					if(New_Bool)
					{
						if(float.TryParse(List_Modifier[Index], out float New_Float))
						{
							m_Types.Add(GetOutgoingDamageType(List_Type[Index].ToLower()));
							m_Modifiers.Add(New_Float); // Store as percentage (0-200+)
						}
						else
						{
							m_Types.Add(GetOutgoingDamageType(List_Type[Index].ToLower()));
							m_Modifiers.Add(100.0f); // Default to 100% (no change)
						}
					}
				}
			}
			
			Out_OutgoingDamageMultiplierModifiers = m_Modifiers;
			return m_Types;
		}
		
		static string GetTooltipStats(StatusEffect_FPO New_StatusEffect_FPO)
		{
			StringBuilder New_StringBuilder = new StringBuilder(256);
			
			if(New_StatusEffect_FPO.m_MaxHealth != 0.00F)
			{ New_StringBuilder.Append("$se_health: [HP]" + New_StatusEffect_FPO.m_MaxHealth.ToString("+0;-0") + "</color>\n"); }
			
			if(New_StatusEffect_FPO.m_HealthRegen != 0)
			{ New_StringBuilder.Append("$se_healthregen: [HP]" + New_StatusEffect_FPO.m_HealthRegen.ToString("+0;-0") + "</color> HP/10s\n"); }
			
			if(New_StatusEffect_FPO.m_healthRegenMultiplier != 1.00F)
			{ New_StringBuilder.Append("$se_healthregen: [HP]" + ((New_StatusEffect_FPO.m_healthRegenMultiplier -1.00F) * 100.00F).ToString("+0;-0") + "%</color>\n"); }
			
			if(New_StatusEffect_FPO.m_MaxStamina != 0.00F)
			{ New_StringBuilder.Append("$se_stamina: [Stamina]" + New_StatusEffect_FPO.m_MaxStamina.ToString("+0;-0") + "</color>\n"); }
			
			if(New_StatusEffect_FPO.m_staminaRegenMultiplier != 1.00F)
			{ New_StringBuilder.Append("$se_staminaregen: [Stamina]" + ((New_StatusEffect_FPO.m_staminaRegenMultiplier - 1.00F) * 100.00F).ToString("+0;-0") + "%</color>\n"); }
			
			if(New_StatusEffect_FPO.m_MaxEitr != 0.00F)
			{ New_StringBuilder.Append("$se_eitr: [Eitr]" +  New_StatusEffect_FPO.m_MaxEitr.ToString("+0;-0") + "</color>\n"); }
			
			if(New_StatusEffect_FPO.m_eitrRegenMultiplier != 1.00F)
			{ New_StringBuilder.Append("$se_eitrregen: [Eitr]" + ((New_StatusEffect_FPO.m_eitrRegenMultiplier - 1.00F) * 100.00F).ToString("+0;-0") + "%</color>\n"); }
			
			if(New_StatusEffect_FPO.m_addMaxCarryWeight != 0.00F)
			{ New_StringBuilder.Append("$se_max_carryweight: [General]" + New_StatusEffect_FPO.m_addMaxCarryWeight.ToString("+0;-0") + "</color>\n"); }
			
			if(New_StatusEffect_FPO.m_speedModifier != 0.00F)
			{ New_StringBuilder.Append("$item_movement_modifier: [General]" + (New_StatusEffect_FPO.m_speedModifier * 100.00F).ToString("+0;-0") + "%</color>\n"); }
			
			if(New_StatusEffect_FPO.m_jumpModifier != Vector3.zero)
			{ New_StringBuilder.Append("$skill_jump: [General]" + (New_StatusEffect_FPO.m_jumpModifier.y * 200.00F).ToString("+0;-0") + "%</color>\n"); }
			
			if(New_StatusEffect_FPO.m_maxMaxFallSpeed != 0.00F)
			{ New_StringBuilder.Append("$item_limitfallspeed: [General]" + New_StatusEffect_FPO.m_maxMaxFallSpeed.ToString("0") + "</color> m/s\n"); }
			
			if(New_StatusEffect_FPO.m_EquipmentSpeedModifier != 0.00F)
			{
				float New_Float = ((New_StatusEffect_FPO.m_EquipmentSpeedModifier * 100.00F) < -100.00F) ? -100.00F : (New_StatusEffect_FPO.m_EquipmentSpeedModifier * 100.00F);
				New_StringBuilder.Append("$ui_fpo_equipmentspeedmodifier: [General]" + New_Float.ToString("+0;-0") + "%</color>\n");
			}
			
			if(New_StatusEffect_FPO.m_WindSpeedModifier != 0.00F)
			{ New_StringBuilder.Append("$ui_fpo_windspeedmodifier: [General]" + (New_StatusEffect_FPO.m_WindSpeedModifier * 100.00F).ToString("+0;-0") + "%</color>\n"); }
			
			if(New_StatusEffect_FPO.m_runStaminaDrainModifier != 0.00F)
			{
				float New_Float = ((New_StatusEffect_FPO.m_runStaminaDrainModifier * 100.00F) < -100.00F) ? -100.00F : (New_StatusEffect_FPO.m_runStaminaDrainModifier * 100.00F);
				New_StringBuilder.Append("$se_runstamina: [General]" + New_Float.ToString("+0;-0") + "%</color>\n");
			}
			
			if(New_StatusEffect_FPO.m_jumpStaminaUseModifier != 0.00F)
			{
				float New_Float = ((New_StatusEffect_FPO.m_jumpStaminaUseModifier * 100.00F) < -100.00F) ? -100.00F : (New_StatusEffect_FPO.m_jumpStaminaUseModifier * 100.00F);
				New_StringBuilder.Append("$se_jumpstamina: [General]" + New_Float.ToString("+0;-0") + "%</color>\n");
			}
			
			if(New_StatusEffect_FPO.m_stealthModifier != 0.00F)
			{
				float New_Float = ((New_StatusEffect_FPO.m_stealthModifier * 100.00F) < -100.00F) ? -100.00F : (New_StatusEffect_FPO.m_stealthModifier * 100.00F);
				New_StringBuilder.Append("$ui_fpo_stealth: [General]" + New_Float.ToString("+0;-0") + "%</color>\n");
			}
			
			if(New_StatusEffect_FPO.m_noiseModifier != 0.00F)
			{
				float New_Float = ((New_StatusEffect_FPO.m_noiseModifier * 100.00F) < -100.00F) ? -100.00F : (New_StatusEffect_FPO.m_noiseModifier * 100.00F);
				New_StringBuilder.Append("$se_noisemod: [General]" + New_Float.ToString("+0;-0") + "%</color>\n");
			}
			
			foreach(StatusEffect.StatusAttribute New_StatusAttribute in New_StatusEffect_FPO.m_StatusAttributes)
			{
				string New_StatusAttributeString = New_StatusAttribute switch
				{
					StatusEffect.StatusAttribute.ColdResistance => "$ui_fpo_coldresistance",
					StatusEffect.StatusAttribute.DoubleImpactDamage => "$ui_fpo_doubleimpactdamage",
					StatusEffect.StatusAttribute.SailingPower => "$ui_fpo_sailingpower",
					
					_ => ""
				};
				
				New_StringBuilder.Append(New_StatusAttributeString + "\n");
			}
			
			foreach(Skills.SkillType New_SkillType in New_StatusEffect_FPO.m_OutgoingDamageTypes)
			{
				if(New_StatusEffect_FPO.m_OutgoingDamageModifiers[New_StatusEffect_FPO.m_OutgoingDamageTypes.IndexOf(New_SkillType)] != 0.00F)
				{
					if(New_SkillType == Skills.SkillType.WoodCutting || New_SkillType == Skills.SkillType.Pickaxes)
					{ New_StringBuilder.Append("$ui_fpo_outgoing_tool " + GetToolTipOutgoingDamageType(New_SkillType) + ": [General]" + (New_StatusEffect_FPO.m_OutgoingDamageModifiers[New_StatusEffect_FPO.m_OutgoingDamageTypes.IndexOf(New_SkillType)] * 100.00F).ToString("+0;-0") + "%</color>\n"); }
					else if(New_SkillType == Skills.SkillType.All)
					{ New_StringBuilder.Append("$ui_fpo_outgoing_all: [General]" + (New_StatusEffect_FPO.m_OutgoingDamageModifiers[New_StatusEffect_FPO.m_OutgoingDamageTypes.IndexOf(New_SkillType)] * 100.00F).ToString("+0;-0") + "%</color>\n"); }
					else
					{ New_StringBuilder.Append("$ui_fpo_outgoing " + GetToolTipOutgoingDamageType(New_SkillType) + ": [General]" + (New_StatusEffect_FPO.m_OutgoingDamageModifiers[New_StatusEffect_FPO.m_OutgoingDamageTypes.IndexOf(New_SkillType)] * 100.00F).ToString("+0;-0") + "%</color>\n"); }
				}
			}
			
			if(New_StatusEffect_FPO.m_fallDamageModifier != 0.00F)
			{
				float New_Float = ((New_StatusEffect_FPO.m_fallDamageModifier * 100.00F) < -100.00F) ? -100.00F : (New_StatusEffect_FPO.m_fallDamageModifier * 100.00F);
				New_StringBuilder.Append("$item_falldamage: [General]" + New_Float.ToString("+0;-0") + "%</color>\n");
			}
			
			if(New_StatusEffect_FPO.m_HeatDamageModifier != 0.00F)
			{
				float New_Float = ((New_StatusEffect_FPO.m_HeatDamageModifier * 100.00F) < -100.00F) ? -100.00F : (New_StatusEffect_FPO.m_HeatDamageModifier * 100.00F);
				New_StringBuilder.Append("$ui_fpo_heatdamage: [General]" + New_Float.ToString("+0;-0") + "%</color>\n");
			}
			
			foreach(HitData.DamageModPair New_DamageModPair in New_StatusEffect_FPO.m_mods)
			{
				if(New_DamageModPair.m_modifier != HitData.DamageModifier.Normal)
				{ New_StringBuilder.Append("$ui_fpo_incoming " + GetToolTipIncomingDamageType(New_DamageModPair.m_type) + ": [General]" + (GetToolTipIncomingDamageModifier(New_DamageModPair.m_modifier) * 100.00F).ToString("+0;-0") + "%</color>\n"); }
			}
			
			// Add multiplier fields to tooltip
			foreach(HitData.DamageType damageType in New_StatusEffect_FPO.m_IncomingDamageMultiplierTypes)
			{
				float multiplier = New_StatusEffect_FPO.m_IncomingDamageMultiplierModifiers[New_StatusEffect_FPO.m_IncomingDamageMultiplierTypes.IndexOf(damageType)];
				if(multiplier != 100.00F) // Only show if different from 100% (no change)
				{ 
					New_StringBuilder.Append("$ui_fpo_incoming_multiplier " + GetToolTipIncomingDamageType(damageType) + ": [General]" + multiplier.ToString("0") + "%</color>\n");
				}
			}
			
			foreach(Skills.SkillType skillType in New_StatusEffect_FPO.m_OutgoingDamageMultiplierTypes)
			{
				float multiplier = New_StatusEffect_FPO.m_OutgoingDamageMultiplierModifiers[New_StatusEffect_FPO.m_OutgoingDamageMultiplierTypes.IndexOf(skillType)];
				if(multiplier != 100.00F) // Only show if different from 100% (no change)
				{
					if(skillType == Skills.SkillType.WoodCutting || skillType == Skills.SkillType.Pickaxes)
					{ New_StringBuilder.Append("$ui_fpo_outgoing_tool_multiplier " + GetToolTipOutgoingDamageType(skillType) + ": [General]" + multiplier.ToString("0") + "%</color>\n"); }
					else if(skillType == Skills.SkillType.All)
					{ New_StringBuilder.Append("$ui_fpo_outgoing_all_multiplier: [General]" + multiplier.ToString("0") + "%</color>\n"); }
					else
					{ New_StringBuilder.Append("$ui_fpo_outgoing_multiplier " + GetToolTipOutgoingDamageType(skillType) + ": [General]" + multiplier.ToString("0") + "%</color>\n"); }
				}
			}
			
			string New_String = New_StringBuilder.ToString();
			
			New_String = New_String.Replace("[HP]+", "+[HP]");
			New_String = New_String.Replace("[HP]-", "-[HP]");
			New_String = New_String.Replace("[HP]", "<color=#FF8080FF>");
			
			New_String = New_String.Replace("[Stamina]+", "+[Stamina]");
			New_String = New_String.Replace("[Stamina]-", "-[Stamina]");
			New_String = New_String.Replace("[Stamina]", "<color=#FFFF80FF>");
			
			New_String = New_String.Replace("[Eitr]+", "+[Eitr]");
			New_String = New_String.Replace("[Eitr]-", "-[Eitr]");
			New_String = New_String.Replace("[Eitr]", "<color=#8080FFFF>");
			
			New_String = New_String.Replace("[General]", "<color=orange>");
			
			if(New_String == "")
			{ New_String = "None\n"; }
			
			return New_String;
		}
		
		static string GetToolTipOutgoingDamageType(Skills.SkillType New_SkillType)
		{
			return New_SkillType switch
			{
				Skills.SkillType.Unarmed => "$skill_unarmed",
				
				Skills.SkillType.Clubs => "$skill_clubs",
				Skills.SkillType.Axes => "$skill_axes",
				Skills.SkillType.Knives => "$skill_knives",
				Skills.SkillType.Spears => "$skill_spears",
				Skills.SkillType.Swords => "$skill_swords",
				Skills.SkillType.Polearms => "$skill_polearms",
				
				Skills.SkillType.Bows => "$skill_bows",
				Skills.SkillType.Crossbows => "$skill_crowssbows",
				
				Skills.SkillType.ElementalMagic => "$skill_elementalmagic",
				Skills.SkillType.BloodMagic => "$skill_bloodmagic",
				
				Skills.SkillType.Blocking => "$skill_blocking",
				
				Skills.SkillType.Run => "$skill_run",
				Skills.SkillType.Jump => "$skill_jump",
				Skills.SkillType.Sneak => "$skill_sneak",
				Skills.SkillType.Swim => "$skill_swim",
				Skills.SkillType.Ride => "$skill_ride",
				
				Skills.SkillType.WoodCutting => "$skill_woodcutting",
				Skills.SkillType.Pickaxes => "$skill_pickaxes",
				Skills.SkillType.Fishing => "$skill_fishing",
				
				_ => ""
			};
		}
		
		static string GetToolTipIncomingDamageType(HitData.DamageType New_DamageType)
		{
			return New_DamageType switch
			{
				HitData.DamageType.Physical => "$ui_fpo_physical",
				HitData.DamageType.Pierce => "$inventory_pierce",
				HitData.DamageType.Slash => "$inventory_slash",
				HitData.DamageType.Blunt => "$inventory_blunt",
				
				HitData.DamageType.Chop => "$inventory_chop",
				HitData.DamageType.Pickaxe => "$inventory_pickaxe",
				
				HitData.DamageType.Elemental => "$ui_fpo_elemental",
				HitData.DamageType.Lightning => "$inventory_lightning",
				HitData.DamageType.Frost => "$inventory_frost",
				HitData.DamageType.Fire => "$inventory_fire",
				
				HitData.DamageType.Poison => "$inventory_poison",
				HitData.DamageType.Spirit => "$inventory_spirit",
				
				_ => ""
			};
		}
		
		static float GetToolTipIncomingDamageModifier(HitData.DamageModifier New_DamageModifier)
		{
			return New_DamageModifier switch
			{
				HitData.DamageModifier.VeryWeak => 1.00F,
				HitData.DamageModifier.Weak => 0.50F,
				HitData.DamageModifier.Normal => 0.00F,
				HitData.DamageModifier.Resistant => -0.50F,
				HitData.DamageModifier.VeryResistant => -0.75F,
				HitData.DamageModifier.Immune => -1.00F,
				
				_ => 0.00F
			};
		}
		
		static string GetToolTipFormat(StatusEffect New_StatusEffect)
		{
			StringBuilder New_StringBuilder = new StringBuilder(256);
			
			New_StringBuilder.Append("<color=orange>" + New_StatusEffect.m_name + "</color>\n");
			New_StringBuilder.Append(GetToolTipStringFormat(New_StatusEffect) + "\n");
			
			if(Mod_EpicLoot)
			{ New_StringBuilder.Append("	\n"); }
			
			return New_StringBuilder.ToString();
		}
		
		static string GetToolTipStringFormat(StatusEffect New_StatusEffect)
		{
			StringBuilder New_StringBuilder = new StringBuilder(256);
			
			foreach(string New_String in New_StatusEffect.GetTooltipString().Split(new char[]{ '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
			{ New_StringBuilder.Append("<size=16>" + New_String + "</size>\n"); }
			
			return New_StringBuilder.ToString();
		}
		
		static int GetHash(string New_String)
		{
			return New_String.GetHashCode();
		}
	}
}