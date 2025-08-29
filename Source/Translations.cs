using BepInEx;
using Jotunn.Managers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		// Cache for localization results to improve performance
		private static Dictionary<string, string> localizationCache = new Dictionary<string, string>();
		
		static void Add_Translations()
		{
			ForsakenPowerOverhaul.log?.LogInfo("[FPO] Adding translations");
			
			LocalizationManager.Instance.GetLocalization().AddTranslation("English", new Dictionary<string, string>
			{
				{ "ui_fpo", "Forsaken Power Overhaul" },
				
				{ "ui_fpo_passive", "Passive" },
				{ "ui_fpo_equipped", "Equipped" },
				{ "ui_fpo_active", "Active" },
				{ "ui_fpo_shared", "Shared" },
				
				{ "ui_fpo_equipmentspeedmodifier", "Movement Speed Penalty from Equipment" },
				{ "ui_fpo_windspeedmodifier", "Wind Speed when Sailing" },

				{ "ui_fpo_stealth", "Visibility when Sneaking" },
				
				{ "ui_fpo_coldresistance", "Cold Resistance" },
				{ "ui_fpo_doubleimpactdamage", "Double Impact Damage" },
				{ "ui_fpo_sailingpower", "Tailwind when Sailing" },
				
				{ "ui_fpo_outgoing", "Outgoing Damage with" },
				{ "ui_fpo_outgoing_all", "Outgoing Damage" },
				{ "ui_fpo_outgoing_tool", "Efficiency with" },
				{ "ui_fpo_incoming", "Incoming Damage from" },
				
				{ "ui_fpo_heatdamage", "Heat Damage" }
			});
		}
		
		static string GetLocalization(string New_String)
		{
			// Check cache first
			if (localizationCache.TryGetValue(New_String, out string cachedResult))
			{
				return cachedResult;
			}
			
			// Get the localized string from Jotunn's LocalizationManager
			// These should never be null if the mod is properly initialized
			Trace.Assert(LocalizationManager.Instance != null, "[FPO] LocalizationManager.Instance is null - mod initialization failed");
			Trace.Assert(LocalizationManager.Instance.GetLocalization() != null, "[FPO] GetLocalization() returned null - localization system not initialized");
			
			var localization = LocalizationManager.Instance.GetLocalization();
			
			string result;
			
			// Use regex to find and replace all $tokens in the string
			if (New_String.Contains("$"))
			{
				// Pattern to match $followed by word characters (letters, numbers, underscores)
				string pattern = @"\$([a-zA-Z_][a-zA-Z0-9_]*)";
				bool allTranslated = true;
				
				result = Regex.Replace(New_String, pattern, match =>
				{
					string token = match.Value; // The full $token
					string translated = localization.TryTranslate(token);
					
					if (string.IsNullOrEmpty(translated) || translated.Equals(token))
					{
						ForsakenPowerOverhaul.log?.LogWarning($"[FPO] Translation not found for key: {token}");
						allTranslated = false;
						return token; // Return original token if translation failed
					}
					else
					{
						return translated;
					}
				});
				
				if (allTranslated)
				{
					ForsakenPowerOverhaul.log?.LogInfo($"[FPO] Successfully translated '{New_String}' to '{result}'");
				}
				else
				{
					ForsakenPowerOverhaul.log?.LogWarning($"[FPO] Partially translated '{New_String}' to '{result}'");
				}
			}
			else
			{
				// If no $ tokens, return as-is (might be plain text)
				ForsakenPowerOverhaul.log?.LogInfo($"[FPO] No translation tokens found in: {New_String}");
				result = New_String;
			}
			
			// Cache the result
			localizationCache[New_String] = result;
			return result;
		}
		
		static string GetLocalization(StringBuilder New_StringBuilder)
		{
			// Convert StringBuilder to string and get localization
			return GetLocalization(New_StringBuilder.ToString());
		}
	}
}