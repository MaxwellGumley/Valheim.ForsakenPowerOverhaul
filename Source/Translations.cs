using BepInEx;
using Jotunn.Managers;
using System.Collections.Generic;
using System.Text;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		static void Add_Translations()
		{
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
			return Localization.m_instance.Localize(New_String);
		}
		
		static string GetLocalization(StringBuilder New_StringBuilder)
		{
			return Localization.m_instance.Localize(New_StringBuilder.ToString());
		}
	}
}