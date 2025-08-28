using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		[HarmonyPatch(typeof(BossStone), nameof(BossStone.Start))]
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
		
		[HarmonyPatch(typeof(BossStone), nameof(BossStone.DelayedAttachEffects_Step3))]
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
							if(ZoneSystem.m_instance.GetGlobalKey(__instance.m_guardianPower.m_name.Replace("$se_", "GK_FPO_BossStone_").Replace("_name", "")))
							{
								__result = GetLocalization(GetBossStoneHoverText(GetBossName(__instance.m_currentItemName), true));
							
								if(!ConfigEntry_PowerCycle_Bool.Value)
								{
									if(!__instance.IsGuardianPowerActive(Player.m_localPlayer))
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
		
		[HarmonyPatch(typeof(ItemStand), nameof(ItemStand.IsGuardianPowerActive))]
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
					
					if(__instance.m_guardianSE == null)
					{ __result = false; }
					
					if(__instance.m_seman.GetStatusEffect(GetHash(__instance.GetGuardianPowerName().Replace("GP_", "SE_FPO_"))))
					{ __instance.m_seman.RemoveStatusEffect(GetHash(__instance.GetGuardianPowerName().Replace("GP_", "SE_FPO_"))); }
					
					__instance.m_seman.AddStatusEffect(GetHash(__instance.GetGuardianPowerName().Replace("GP_", "SE_FPO_")), true);
					__instance.m_guardianPowerCooldown = ConfigEntry_General_GuardianPower_CooldownDuration.Value;
					
					foreach(StatusEffect New_StatusEffect in __instance.m_seman.GetStatusEffects())
					{
						if(New_StatusEffect.name.StartsWith("SE_FPO_") && (New_StatusEffect.name != "SE_FPO_Passive") && (New_StatusEffect.name != __instance.GetGuardianPowerName().Replace("GP_", "SE_FPO_")))
						{
							__instance.m_seman.RemoveStatusEffect(New_StatusEffect);
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
		
		[HarmonyPatch(typeof(Player), nameof(Player.UpdateFood))]
		class Patch_Player_UpdateFood
		{
			static void Postfix(ref Player __instance)
			{
				if(__instance != null)
				{
					if(__instance.m_foodRegenTimer == 0.00F)
					{
						float HealthRegen = 0.00F;
						HealthRegen += Update_Player_BaseStats("HealthRegen", __instance);
						
						float regenMultiplier = 1.00F;
						__instance.m_seman.ModifyHealthRegen(ref regenMultiplier);
						
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
		
		[HarmonyPatch(typeof(Player), nameof(Player.SetMaxEitr))]
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
						
						__instance.m_equipmentModifierValues[0] = 0.00F;
						
						if(__instance.m_rightItem != null)
						{ __instance.m_equipmentModifierValues[0] += (__instance.m_rightItem.m_shared.m_movementModifier < 0.00F) ? (__instance.m_rightItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : __instance.m_rightItem.m_shared.m_movementModifier; }
						
						if(__instance.m_leftItem != null)
						{ __instance.m_equipmentModifierValues[0] += (__instance.m_leftItem.m_shared.m_movementModifier < 0.00F) ? (__instance.m_leftItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : __instance.m_leftItem.m_shared.m_movementModifier; }
						
						if(__instance.m_chestItem != null)
						{ __instance.m_equipmentModifierValues[0] += (__instance.m_chestItem.m_shared.m_movementModifier < 0.00F) ? (__instance.m_chestItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : __instance.m_chestItem.m_shared.m_movementModifier; }
						
						if(__instance.m_legItem != null)
						{ __instance.m_equipmentModifierValues[0] += (__instance.m_legItem.m_shared.m_movementModifier < 0.00F) ? (__instance.m_legItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : __instance.m_legItem.m_shared.m_movementModifier; }
						
						if(__instance.m_helmetItem != null)
						{ __instance.m_equipmentModifierValues[0] += (__instance.m_helmetItem.m_shared.m_movementModifier < 0.00F) ? (__instance.m_helmetItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : __instance.m_helmetItem.m_shared.m_movementModifier; }
						
						if(__instance.m_shoulderItem != null)
						{ __instance.m_equipmentModifierValues[0] += (__instance.m_shoulderItem.m_shared.m_movementModifier < 0.00F) ? (__instance.m_shoulderItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : __instance.m_shoulderItem.m_shared.m_movementModifier; }
						
						if(__instance.m_utilityItem != null)
						{ __instance.m_equipmentModifierValues[0] += (__instance.m_utilityItem.m_shared.m_movementModifier < 0.00F) ? (__instance.m_utilityItem.m_shared.m_movementModifier * EquipmentSpeedModifier) : __instance.m_utilityItem.m_shared.m_movementModifier; }
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
					
					__instance.m_equipmentModifierValues[2] -= HeatDamageModifier;
					__instance.m_equipmentModifierValues[2] = (__instance.m_equipmentModifierValues[2] > 1.00F) ? 1.00F : __instance.m_equipmentModifierValues[2];
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
		
		[HarmonyPatch(typeof(Ship), nameof(Ship.GetSailForce))]
		class Patch_Ship_SailForce
		{
			static void Postfix(ref Ship __instance, ref Vector3 __result)
			{
				if(__instance != null)
				{
					if(__instance.IsWindControllActive())
					{
						float WindSpeedModifier = 0.00F;
						
						foreach(Player New_Player in __instance.m_players)
						{ WindSpeedModifier += Update_Player_BaseStats("WindSpeedModifier", New_Player); }
						
						__result *= 1.00F + WindSpeedModifier;
					}
				}
			}
		}
		
		[HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.AddActiveEffects))]
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
					
					__instance.m_texts.RemoveAt(0);
					__instance.m_texts.Insert(0, new TextsDialog.TextInfo(GetLocalization("$inventory_activeeffects"), GetLocalization(New_StringBuilder)));
					__instance.m_texts.Insert(1, new TextsDialog.TextInfo(GetLocalization("$ui_fpo"), GetLocalization(New_StringBuilder_FPO)));
				}
			}
		}
	}
}