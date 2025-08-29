using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		/// <summary>
		/// Cache system for damage multiplier calculations, similar to Epic Loot's EquipmentEffectCache.
		/// Caches calculated multiplier values per player to avoid expensive recalculations on every damage event.
		/// Cache is invalidated when status effects change.
		/// </summary>
		public static class DamageMultiplierCache
		{
			// Cache structure: Player -> (EffectType -> Multiplier Value)
			public static ConditionalWeakTable<Player, Dictionary<string, float?>> CachedMultipliers = new ConditionalWeakTable<Player, Dictionary<string, float?>>();
			
			/// <summary>
			/// Reset cache for a specific player (called when status effects change)
			/// </summary>
			public static void Reset(Player player)
			{
				if (player != null)
				{
					CachedMultipliers.Remove(player);
				}
			}
			
			/// <summary>
			/// Get cached multiplier value or calculate if not cached
			/// </summary>
			/// <param name="player">Player to get multiplier for</param>
			/// <param name="effectKey">Unique key identifying the effect type (e.g., "incoming_fire", "outgoing_bows")</param>
			/// <param name="calculate">Function to calculate the value if not cached</param>
			/// <returns>Cached or calculated multiplier value</returns>
			public static float Get(Player player, string effectKey, Func<float> calculate)
			{
				if (player == null || string.IsNullOrEmpty(effectKey))
				{
					return 1.0f; // Default multiplier (no change)
				}
				
				Dictionary<string, float?> playerCache = CachedMultipliers.GetOrCreateValue(player);
				
				if (playerCache.TryGetValue(effectKey, out var cachedValue))
				{
					return cachedValue ?? 1.0f;
				}
				
				// Calculate and cache the value
				float calculatedValue = calculate();
				playerCache[effectKey] = calculatedValue;
				return calculatedValue;
			}
			
			/// <summary>
			/// Get incoming damage multiplier for a specific damage type
			/// </summary>
			public static float GetIncomingDamageMultiplier(Player player, HitData.DamageType damageType)
			{
				string effectKey = $"incoming_{damageType}";
				return Get(player, effectKey, () => CalculateIncomingDamageMultiplier(player, damageType));
			}
			
			/// <summary>
			/// Get outgoing damage multiplier for a specific skill type
			/// </summary>
			public static float GetOutgoingDamageMultiplier(Player player, Skills.SkillType skillType)
			{
				string effectKey = $"outgoing_{skillType}";
				return Get(player, effectKey, () => CalculateOutgoingDamageMultiplier(player, skillType));
			}
			
			/// <summary>
			/// Calculate incoming damage multiplier by examining all active FPO status effects
			/// </summary>
			private static float CalculateIncomingDamageMultiplier(Player player, HitData.DamageType damageType)
			{
				float finalMultiplier = 1.0f; // Start with no change
				
				var seman = GetSEMan(player);
				foreach (StatusEffect statusEffect in seman.GetStatusEffects())
				{
					if (statusEffect is StatusEffect_FPO fpoEffect)
					{
						// Find multipliers for this damage type
						for (int i = 0; i < fpoEffect.m_IncomingDamageMultiplierTypes.Count && i < fpoEffect.m_IncomingDamageMultiplierModifiers.Count; i++)
						{
							if (fpoEffect.m_IncomingDamageMultiplierTypes[i] == damageType)
							{
								// Convert percentage to multiplier (0% = 0.0, 100% = 1.0, 200% = 2.0)
								float multiplier = fpoEffect.m_IncomingDamageMultiplierModifiers[i] / 100.0f;
								finalMultiplier *= multiplier;
							}
						}
					}
				}
				
				return finalMultiplier;
			}
			
			/// <summary>
			/// Calculate outgoing damage multiplier by examining all active FPO status effects
			/// </summary>
			private static float CalculateOutgoingDamageMultiplier(Player player, Skills.SkillType skillType)
			{
				float finalMultiplier = 1.0f; // Start with no change
				
				var seman = GetSEMan(player);
				foreach (StatusEffect statusEffect in seman.GetStatusEffects())
				{
					if (statusEffect is StatusEffect_FPO fpoEffect)
					{
						// Find multipliers for this skill type
						for (int i = 0; i < fpoEffect.m_OutgoingDamageMultiplierTypes.Count && i < fpoEffect.m_OutgoingDamageMultiplierModifiers.Count; i++)
						{
							if (fpoEffect.m_OutgoingDamageMultiplierTypes[i] == skillType || fpoEffect.m_OutgoingDamageMultiplierTypes[i] == Skills.SkillType.All)
							{
								// Convert percentage to multiplier (50% = 0.5, 100% = 1.0, 150% = 1.5)
								float multiplier = fpoEffect.m_OutgoingDamageMultiplierModifiers[i] / 100.0f;
								finalMultiplier *= multiplier;
							}
						}
					}
				}
				
				return finalMultiplier;
			}
		}
	}
}
