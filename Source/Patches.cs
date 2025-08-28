using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
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
					
					var guardianSE = Traverse.Create(__instance).Field("m_guardianSE").GetValue();
					if(guardianSE == null)
					{ __result = false; }
					
					var seman = Traverse.Create(__instance).Field("m_seman").GetValue<SEMan>();
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
					var foodRegenTimer = Traverse.Create(__instance).Field("m_foodRegenTimer").GetValue<float>();
					if(foodRegenTimer == 0.00F)
					{
						float HealthRegen = 0.00F;
						HealthRegen += Update_Player_BaseStats("HealthRegen", __instance);
						
						float regenMultiplier = 1.00F;
						var seman = Traverse.Create(__instance).Field("m_seman").GetValue<SEMan>();
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
						
						var equipmentModValues = Traverse.Create(__instance).Field("m_equipmentModifierValues").GetValue();
						var rightItem = Traverse.Create(__instance).Field("m_rightItem").GetValue<ItemDrop.ItemData>();
						var leftItem = Traverse.Create(__instance).Field("m_leftItem").GetValue<ItemDrop.ItemData>();
						var chestItem = Traverse.Create(__instance).Field("m_chestItem").GetValue<ItemDrop.ItemData>();
						var legItem = Traverse.Create(__instance).Field("m_legItem").GetValue<ItemDrop.ItemData>();
						var helmetItem = Traverse.Create(__instance).Field("m_helmetItem").GetValue<ItemDrop.ItemData>();
						var shoulderItem = Traverse.Create(__instance).Field("m_shoulderItem").GetValue<ItemDrop.ItemData>();
						var utilityItem = Traverse.Create(__instance).Field("m_utilityItem").GetValue<ItemDrop.ItemData>();
						
						// Reset equipment modifier value
						var movementField = Traverse.Create(equipmentModValues).Field("m_movement");
						movementField.SetValue(0.00F);
						
						if(rightItem != null)
						{ 
							float currentVal = movementField.GetValue<float>();
							float modifier = (rightItem.m_shared.m_movementModifier < 0.00F) ? (rightItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : rightItem.m_shared.m_movementModifier;
							movementField.SetValue(currentVal + modifier);
						}
						
						if(leftItem != null)
						{ 
							float currentVal = movementField.GetValue<float>();
							float modifier = (leftItem.m_shared.m_movementModifier < 0.00F) ? (leftItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : leftItem.m_shared.m_movementModifier;
							movementField.SetValue(currentVal + modifier);
						}
						
						if(chestItem != null)
						{ 
							float currentVal = movementField.GetValue<float>();
							float modifier = (chestItem.m_shared.m_movementModifier < 0.00F) ? (chestItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : chestItem.m_shared.m_movementModifier;
							movementField.SetValue(currentVal + modifier);
						}
						
						if(legItem != null)
						{ 
							float currentVal = movementField.GetValue<float>();
							float modifier = (legItem.m_shared.m_movementModifier < 0.00F) ? (legItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : legItem.m_shared.m_movementModifier;
							movementField.SetValue(currentVal + modifier);
						}
						
						if(helmetItem != null)
						{ 
							float currentVal = movementField.GetValue<float>();
							float modifier = (helmetItem.m_shared.m_movementModifier < 0.00F) ? (helmetItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : helmetItem.m_shared.m_movementModifier;
							movementField.SetValue(currentVal + modifier);
						}
						
						if(shoulderItem != null)
						{ 
							float currentVal = movementField.GetValue<float>();
							float modifier = (shoulderItem.m_shared.m_movementModifier < 0.00F) ? (shoulderItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : shoulderItem.m_shared.m_movementModifier;
							movementField.SetValue(currentVal + modifier);
						}
						
						if(utilityItem != null)
						{ 
							float currentVal = movementField.GetValue<float>();
							float modifier = (utilityItem.m_shared.m_movementModifier < 0.00F) ? (utilityItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : utilityItem.m_shared.m_movementModifier;
							movementField.SetValue(currentVal + modifier);
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
					
					// Access equipment modifier values through Traverse
					var equipmentModValues = Traverse.Create(__instance).Field("m_equipmentModifierValues").GetValue();
					if (equipmentModValues != null)
					{
						var modifiersField = Traverse.Create(equipmentModValues).Field("m_fire");
						float currentFire = modifiersField.GetValue<float>();
						modifiersField.SetValue(currentFire - HeatDamageModifier);
						
						// Clamp to max 1.0
						float newValue = modifiersField.GetValue<float>();
						if (newValue > 1.00F) modifiersField.SetValue(1.00F);
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
						
						var shipPlayers = Traverse.Create(__instance).Field("m_players").GetValue<List<Player>>();
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
					
					var textsList = Traverse.Create(__instance).Field("m_texts").GetValue<List<TextsDialog.TextInfo>>();
					textsList.RemoveAt(0);
					textsList.Insert(0, new TextsDialog.TextInfo(GetLocalization("$inventory_activeeffects"), GetLocalization(New_StringBuilder)));
					textsList.Insert(1, new TextsDialog.TextInfo(GetLocalization("$ui_fpo"), GetLocalization(New_StringBuilder_FPO)));
				}
			}
		}
	}
}