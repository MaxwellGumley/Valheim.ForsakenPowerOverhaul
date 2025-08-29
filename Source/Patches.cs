using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		// Additional cached field accessors for Patches.cs
		private static FieldInfo EquipmentMovementField;
		private static FieldInfo EquipmentFireField;

		// Initialize additional field accessors for Patches.cs
		private static void InitializePatchFieldAccessors()
		{
			// Equipment modifier fields need to be found dynamically based on the equipment type
			var equipmentModType = PlayerEquipmentModifierValuesField?.FieldType;
			if (equipmentModType != null)
			{
				EquipmentMovementField = AccessTools.Field(equipmentModType, "m_movement");
				EquipmentFireField = AccessTools.Field(equipmentModType, "m_fire");
			}
		}
		[HarmonyPatch(typeof(BossStone), "Start")]
		class Patch_BossStone_Start
		{
			static void Postfix(ref BossStone __instance)
			{
				if(__instance != null)
				{
					if(!__instance.GetComponent<BossStone_FPO>())
					{ __instance.gameObject.AddComponent<BossStone_FPO>(); }
					
					__instance.GetComponent<BossStone_FPO>().Invoke("Update_BossStones", 1.00F);
				}
			}
		}
		
		[HarmonyPatch(typeof(BossStone), "DelayedAttachEffects_Step3")]
		class Patch_BossStone_DelayedAttachEffects_Step3
		{
			static void Postfix(ref BossStone __instance)
			{
				if(__instance != null)
				{
					GlobalKeys_Update(__instance);
					PlayerKeys_Update();
					
					Update_Passive_Stats();
					Update_Active_Stats();
					
					Power_Unlock(__instance);
				}
			}
		}
		
		[HarmonyPatch(typeof(ItemStand), nameof(ItemStand.GetHoverText))]
		class Patch_Item_Stand_GetHoverText
		{
			static void Postfix(ref ItemStand __instance, ref string __result)
			{
				if(__instance != null)
				{
					if(__instance.HaveAttachment())
					{
						if(!__instance.m_canBeRemoved)
						{
							if(ZoneSystem.instance.GetGlobalKey(__instance.m_guardianPower.m_name.Replace("$se_", "GK_FPO_BossStone_").Replace("_name", "")))
							{
								__result = GetLocalization(GetBossStoneHoverText(GetBossName(__instance.m_currentItemName), true));
							
								if(!ConfigEntry_PowerCycle_Bool.Value)
								{
									if(Player.m_localPlayer.GetGuardianPowerName() != __instance.m_guardianPower.m_name)
									{ __result += GetLocalization("[<color=yellow><b>$KEY_Use</b></color>] $guardianstone_hook_activate"); }
								}
							}
							else
							{ __result = ""; }
						}
					}
				}
			}
		}
		
		[HarmonyPatch(typeof(ItemStand), "IsGuardianPowerActive")]
		class Patch_ItemStand_IsGuardianPowerActive
		{
			static void Postfix(ref ItemStand __instance, ref bool __result)
			{
				if(__instance != null)
				{
					if(ConfigEntry_PowerCycle_Bool.Value)
					{ __result = true; }
				}
			}
		}
		
		[HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
		class Patch_Player_OnSpawned
		{
			static void Postfix(ref Player __instance)
			{
				if(__instance != null)
				{
					Config_SettingRead();
					
					PlayerKeys_Update();
					
					Update_Passive_Stats();
					Update_Active_Stats();
					Update_Equipped_Stats();
					
					Power_Lock(__instance);
					Power_Unlock_Spawn(__instance);
				}
			}
		}
		
		[HarmonyPatch(typeof(Player),nameof(Player.ActivateGuardianPower))]
		class Patch_Player_ActivateGuardianPower
		{
			static bool Prefix(ref Player __instance, ref bool __result)
			{
				if(__instance != null)
				{
					if(__instance.m_guardianPowerCooldown > 0.00F)
					{ __result = false; }
					
					var guardianSE = GetGuardianSE(__instance);
					if(guardianSE == null)
					{ __result = false; }
					
					var seman = GetSEMan(__instance);
					if(seman.GetStatusEffect(GetHash(__instance.GetGuardianPowerName().Replace("GP_", "SE_FPO_"))))
					{ seman.RemoveStatusEffect(GetHash(__instance.GetGuardianPowerName().Replace("GP_", "SE_FPO_"))); }
					
					seman.AddStatusEffect(GetHash(__instance.GetGuardianPowerName().Replace("GP_", "SE_FPO_")), true);
					__instance.m_guardianPowerCooldown = ConfigEntry_General_GuardianPower_CooldownDuration.Value;
					
					foreach(StatusEffect New_StatusEffect in seman.GetStatusEffects())
					{
						if(New_StatusEffect.name.StartsWith("SE_FPO_") && (New_StatusEffect.name != "SE_FPO_Passive") && (New_StatusEffect.name != __instance.GetGuardianPowerName().Replace("GP_", "SE_FPO_")))
						{
							seman.RemoveStatusEffect(New_StatusEffect);
							break;
						}
					}
					
					__result = false;
				}
				
				return false;
			}
		}
		
		[HarmonyPatch(typeof(Player), nameof(Player.SetMaxHealth))]
		class Patch_Player_SetMaxHealth
		{
			static bool Prefix(ref Player __instance, ref float health)
			{
				if(__instance != null)
				{
					float MaxHealth = 0.00F;
					MaxHealth += Update_Player_BaseStats("MaxHealth", __instance);
					
					health += MaxHealth;
				}
				
				return true;
			}
		}
		
		[HarmonyPatch(typeof(Player), "UpdateFood")]
		class Patch_Player_UpdateFood
		{
			static void Postfix(ref Player __instance)
			{
				if(__instance != null)
				{
					var foodRegenTimer = GetFoodRegenTimer(__instance);
					if(foodRegenTimer == 0.00F)
					{
						float HealthRegen = 0.00F;
						HealthRegen += Update_Player_BaseStats("HealthRegen", __instance);
						
						float regenMultiplier = 1.00F;
						var seman = GetSEMan(__instance);
						seman.ModifyHealthRegen(ref regenMultiplier);
						
						__instance.Heal(HealthRegen * regenMultiplier);
					}
				}
			}
		}
		
		[HarmonyPatch(typeof(Player), nameof(Player.SetMaxStamina))]
		class Patch_Player_SetMaxStamina
		{
			static bool Prefix(ref Player __instance, ref float stamina)
			{
				if(__instance != null)
				{
					float MaxStamina = 0.00F;
					MaxStamina += Update_Player_BaseStats("MaxStamina", __instance);
					
					stamina += MaxStamina;
				}
				
				return true;
			}
		}
		
		[HarmonyPatch(typeof(Player), "SetMaxEitr")]
		class Patch_Player_SetMaxEitr
		{
			static bool Prefix(ref Player __instance, ref float eitr)
			{
				if(__instance != null)
				{
					float MaxEitr = 0.00F;
					MaxEitr += Update_Player_BaseStats("MaxEitr", __instance);
					
					eitr += MaxEitr;
				}
				
				return true;
			}
		}
		
		[HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentMovementModifier))]
		class Patch_Player_GetEquipmentMovementModifier
		{
			static void Postfix(ref Player __instance)
			{
				if(__instance != null)
				{
					float EquipmentSpeedModifier = 0.00F;
					EquipmentSpeedModifier += Update_Player_BaseStats("EquipmentSpeedModifier", __instance);
					
					if(EquipmentSpeedModifier != 0.00F)
					{
						EquipmentSpeedModifier = (EquipmentSpeedModifier < -1.00F) ? -1.00F : EquipmentSpeedModifier;
						EquipmentSpeedModifier = 1.00F + EquipmentSpeedModifier;
						
						var equipmentModValues = GetEquipmentModifierValues(__instance);
						var rightItem = GetRightItem(__instance);
						var leftItem = GetLeftItem(__instance);
						var chestItem = GetChestItem(__instance);
						var legItem = GetLegItem(__instance);
						var helmetItem = GetHelmetItem(__instance);
						var shoulderItem = GetShoulderItem(__instance);
						var utilityItem = GetUtilityItem(__instance);
						
						// Reset equipment modifier value
						EquipmentMovementField?.SetValue(equipmentModValues, 0.00F);
						
						if(rightItem != null)
						{ 
							float currentVal = (float)(EquipmentMovementField?.GetValue(equipmentModValues) ?? 0f);
							float modifier = (rightItem.m_shared.m_movementModifier < 0.00F) ? (rightItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : rightItem.m_shared.m_movementModifier;
							EquipmentMovementField?.SetValue(equipmentModValues, currentVal + modifier);
						}
						
						if(leftItem != null)
						{ 
							float currentVal = (float)(EquipmentMovementField?.GetValue(equipmentModValues) ?? 0f);
							float modifier = (leftItem.m_shared.m_movementModifier < 0.00F) ? (leftItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : leftItem.m_shared.m_movementModifier;
							EquipmentMovementField?.SetValue(equipmentModValues, currentVal + modifier);
						}
						
						if(chestItem != null)
						{ 
							float currentVal = (float)(EquipmentMovementField?.GetValue(equipmentModValues) ?? 0f);
							float modifier = (chestItem.m_shared.m_movementModifier < 0.00F) ? (chestItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : chestItem.m_shared.m_movementModifier;
							EquipmentMovementField?.SetValue(equipmentModValues, currentVal + modifier);
						}
						
						if(legItem != null)
						{ 
							float currentVal = (float)(EquipmentMovementField?.GetValue(equipmentModValues) ?? 0f);
							float modifier = (legItem.m_shared.m_movementModifier < 0.00F) ? (legItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : legItem.m_shared.m_movementModifier;
							EquipmentMovementField?.SetValue(equipmentModValues, currentVal + modifier);
						}
						
						if(helmetItem != null)
						{ 
							float currentVal = (float)(EquipmentMovementField?.GetValue(equipmentModValues) ?? 0f);
							float modifier = (helmetItem.m_shared.m_movementModifier < 0.00F) ? (helmetItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : helmetItem.m_shared.m_movementModifier;
							EquipmentMovementField?.SetValue(equipmentModValues, currentVal + modifier);
						}
						
						if(shoulderItem != null)
						{ 
							float currentVal = (float)(EquipmentMovementField?.GetValue(equipmentModValues) ?? 0f);
							float modifier = (shoulderItem.m_shared.m_movementModifier < 0.00F) ? (shoulderItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : shoulderItem.m_shared.m_movementModifier;
							EquipmentMovementField?.SetValue(equipmentModValues, currentVal + modifier);
						}
						
						if(utilityItem != null)
						{ 
							float currentVal = (float)(EquipmentMovementField?.GetValue(equipmentModValues) ?? 0f);
							float modifier = (utilityItem.m_shared.m_movementModifier < 0.00F) ? (utilityItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : utilityItem.m_shared.m_movementModifier;
							EquipmentMovementField?.SetValue(equipmentModValues, currentVal + modifier);
						}
					}
				}
			}
		}
		
		[HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentHeatResistanceModifier))]
		class Patch_Player_GetEquipmentHeatResistanceModifier
		{
			static void Postfix(ref Player __instance)
			{
				if(__instance != null)
				{
					float HeatDamageModifier = 0.00F;
					HeatDamageModifier += Update_Player_BaseStats("HeatDamageModifier", __instance);
					
					// Access equipment modifier values through cached field accessor
					var equipmentModValues = GetEquipmentModifierValues(__instance);
					if (equipmentModValues != null)
					{
						float currentFire = (float)(EquipmentFireField?.GetValue(equipmentModValues) ?? 0f);
						EquipmentFireField?.SetValue(equipmentModValues, currentFire - HeatDamageModifier);
						
						// Clamp to max 1.0
						float newValue = (float)(EquipmentFireField?.GetValue(equipmentModValues) ?? 0f);
						if (newValue > 1.00F) EquipmentFireField?.SetValue(equipmentModValues, 1.00F);
					}
				}
			}
		}
		
		[HarmonyPatch(typeof(SEMan), nameof(SEMan.HaveStatusAttribute))]
		class Patch_SEMan_HaveStatusAttribute
		{
			static void Postfix(ref SEMan __instance, ref StatusEffect.StatusAttribute value, ref bool __result)
			{
				if(__instance != null)
				{
					bool New_Bool = __result;
					
					foreach(StatusEffect New_StatusEffect in __instance.GetStatusEffects())
					{
						if(New_StatusEffect.name.StartsWith("SE_FPO_"))
						{
							StatusEffect_FPO New_StatusEffect_FPO = (StatusEffect_FPO)New_StatusEffect;
							
							if(New_StatusEffect_FPO.HaveAttribute(value))
							{ New_Bool = true; }
						}
					}
					
					__result = New_Bool;
				}
			}
		}
		
		[HarmonyPatch(typeof(Ship), "GetSailForce")]
		class Patch_Ship_SailForce
		{
			static void Postfix(ref Ship __instance, ref Vector3 __result)
			{
				if(__instance != null)
				{
					if(__instance.IsWindControllActive())
					{
						float WindSpeedModifier = 0.00F;
						
						var shipPlayers = GetShipPlayers(__instance);
						foreach(Player New_Player in shipPlayers)
						{ WindSpeedModifier += Update_Player_BaseStats("WindSpeedModifier", New_Player); }
						
						__result *= 1.00F + WindSpeedModifier;
					}
				}
			}
		}
		
		[HarmonyPatch(typeof(TextsDialog), "AddActiveEffects")]
		class Patch_TextsDialog_AddActiveEffects
		{
			static void Postfix(ref TextsDialog __instance)
			{
				if(__instance != null)
				{
					StringBuilder New_StringBuilder = new StringBuilder(256);
					StringBuilder New_StringBuilder_FPO = new StringBuilder(256);
					
					List<StatusEffect> New_List = new List<StatusEffect>{ };
					Player.m_localPlayer.GetSEMan().GetHUDStatusEffects(New_List);
					
					foreach(StatusEffect New_StatusEffect in New_List)
					{
						if(New_StatusEffect.name.StartsWith("SE_FPO_") && !New_StatusEffect.name.Contains("_Equipped") && !New_StatusEffect.name.Contains("_Passive"))
						{ New_StringBuilder.Append(GetToolTipFormat(New_StatusEffect)); }
					}
					
					foreach(StatusEffect New_StatusEffect in New_List)
					{
						if(New_StatusEffect.name.StartsWith("SE_FPO_") && New_StatusEffect.name.Contains("_Equipped"))
						{ New_StringBuilder.Append(GetToolTipFormat(New_StatusEffect)); }
					}
					
					foreach(StatusEffect New_StatusEffect in New_List)
					{
						if(New_StatusEffect.name.StartsWith("SE_FPO_") && New_StatusEffect.name.Contains("_Passive"))
						{ New_StringBuilder.Append(GetToolTipFormat(New_StatusEffect)); }
					}
					
					foreach(StatusEffect New_StatusEffect in New_List)
					{
						if(!New_StatusEffect.name.StartsWith("SE_FPO_"))
						{ New_StringBuilder.Append(GetToolTipFormat(New_StatusEffect)); }
					}
					
					foreach(string GlobalKey in GlobalKeys_Sort())
					{
						string New_String = GetBossName(GlobalKey);
						
						if(New_String != "")
						{ New_StringBuilder_FPO.Append(GetBossStoneHoverText(New_String, false)); }
					}
					
					var textsList = GetTextsDialogTexts(__instance);
					textsList.RemoveAt(0);
					textsList.Insert(0, new TextsDialog.TextInfo(GetLocalization("$inventory_activeeffects"), GetLocalization(New_StringBuilder)));
					textsList.Insert(1, new TextsDialog.TextInfo(GetLocalization("$ui_fpo"), GetLocalization(New_StringBuilder_FPO)));
				}
			}
		}
		
		/// <summary>
		/// Patch to apply incoming damage multipliers after Epic Loot and built-in resistance systems
		/// </summary>
		[HarmonyPatch(typeof(Character), "RPC_Damage")]
		[HarmonyAfter("randyknapp.mods.epicloot")]
		class Patch_Character_RPC_Damage_IncomingMultipliers
		{
			static void Prefix(Character __instance, HitData hit)
			{
				if (__instance is Player player)
				{
					// Apply incoming damage multipliers to each damage type
					hit.m_damage.m_fire *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Fire);
					hit.m_damage.m_frost *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Frost);
					hit.m_damage.m_lightning *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Lightning);
					hit.m_damage.m_poison *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Poison);
					hit.m_damage.m_spirit *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Spirit);
					hit.m_damage.m_blunt *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Blunt);
					hit.m_damage.m_slash *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Slash);
					hit.m_damage.m_pierce *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Pierce);
					hit.m_damage.m_chop *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Chop);
					hit.m_damage.m_pickaxe *= DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Pickaxe);
					
					// Apply physical damage multiplier to all physical types if specified
					float physicalMultiplier = DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Physical);
					if (physicalMultiplier != 1.0f)
					{
						hit.m_damage.m_blunt *= physicalMultiplier;
						hit.m_damage.m_slash *= physicalMultiplier;
						hit.m_damage.m_pierce *= physicalMultiplier;
					}
					
					// Apply elemental damage multiplier to all elemental types if specified
					float elementalMultiplier = DamageMultiplierCache.GetIncomingDamageMultiplier(player, HitData.DamageType.Elemental);
					if (elementalMultiplier != 1.0f)
					{
						hit.m_damage.m_fire *= elementalMultiplier;
						hit.m_damage.m_frost *= elementalMultiplier;
						hit.m_damage.m_lightning *= elementalMultiplier;
						hit.m_damage.m_poison *= elementalMultiplier;
						hit.m_damage.m_spirit *= elementalMultiplier;
					}
				}
			}
		}
		
		/// <summary>
		/// Patch to apply outgoing damage multipliers when player attacks
		/// </summary>
		[HarmonyPatch(typeof(Character), "Damage")]
		[HarmonyAfter("randyknapp.mods.epicloot")]
		class Patch_Character_Damage_OutgoingMultipliers
		{
			static void Prefix(Character __instance, HitData hit)
			{
				if (__instance is Player player && hit != null)
				{
					// Try to determine the skill type from the hit data or equipped weapon
					Skills.SkillType skillType = GetSkillTypeFromHit(player, hit);
					
					if (skillType != Skills.SkillType.None)
					{
						// Apply outgoing damage multiplier
						float multiplier = DamageMultiplierCache.GetOutgoingDamageMultiplier(player, skillType);
						if (multiplier != 1.0f)
						{
							hit.m_damage.Modify(multiplier);
						}
						
						// Also check for "All" skill type multiplier
						float allMultiplier = DamageMultiplierCache.GetOutgoingDamageMultiplier(player, Skills.SkillType.All);
						if (allMultiplier != 1.0f)
						{
							hit.m_damage.Modify(allMultiplier);
						}
					}
				}
			}
			
			/// <summary>
			/// Determine skill type from hit data or equipped weapon
			/// </summary>
			private static Skills.SkillType GetSkillTypeFromHit(Player player, HitData hit)
			{
				// Try to get skill type from hit data
				if (hit.m_skill != Skills.SkillType.None)
				{
					return hit.m_skill;
				}
				
				// Fall back to equipped weapon skill type
				var rightItem = GetRightItem(player);
				if (rightItem?.m_shared?.m_skillType != null)
				{
					return rightItem.m_shared.m_skillType;
				}
				
				// Default to unarmed if no weapon
				return Skills.SkillType.Unarmed;
			}
		}
		
		/// <summary>
		/// Patch to invalidate cache when status effects change
		/// </summary>
		[HarmonyPatch(typeof(SEMan), "AddStatusEffect", new System.Type[] { typeof(StatusEffect), typeof(bool), typeof(int), typeof(float) })]
		class Patch_SEMan_AddStatusEffect_InvalidateCache
		{
			static void Postfix(SEMan __instance, StatusEffect statusEffect)
			{
				// Use reflection to get the character field from SEMan
				var characterField = AccessTools.Field(typeof(SEMan), "m_character");
				if (characterField?.GetValue(__instance) is Player player && statusEffect is StatusEffect_FPO)
				{
					DamageMultiplierCache.Reset(player);
				}
			}
		}
		
		/// <summary>
		/// Patch to invalidate cache when status effects are removed
		/// </summary>
		[HarmonyPatch(typeof(SEMan), "RemoveStatusEffect", new System.Type[] { typeof(StatusEffect), typeof(bool) })]
		class Patch_SEMan_RemoveStatusEffect_InvalidateCache
		{
			static void Postfix(SEMan __instance, StatusEffect statusEffect)
			{
				// Use reflection to get the character field from SEMan
				var characterField = AccessTools.Field(typeof(SEMan), "m_character");
				if (characterField?.GetValue(__instance) is Player player && statusEffect is StatusEffect_FPO)
				{
					DamageMultiplierCache.Reset(player);
				}
			}
		}
	}
}