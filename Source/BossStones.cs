using BepInEx;
using System.Text;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		class BossStone_FPO : MonoBehaviour
		{
			public void Update_BossStones()
			{
				GlobalKeys_Update(GetComponent<BossStone>());
				PlayerKeys_Update();
				
				Update_Passive_Stats();
				Update_Active_Stats();
				
				if(Player.m_localPlayer != null)
				{ Power_Unlock_Spawn(Player.m_localPlayer); }
			}
		}
		
		static string GetBossStoneHoverText(string New_String, bool New_Bool)
		{
			StringBuilder New_StringBuilder = new StringBuilder(256);
			
			if(New_String != "")
			{
				New_StringBuilder.Append("<size=24><smallcaps><color=orange>" + GetLocalization("$se_" + New_String.ToLower() + "_name") + "</color></smallcaps></size><size=20>\n");
				New_StringBuilder.Append("<smallcaps>" + GetLocalization("$ui_fpo_passive") + "</smallcaps></size><size=16>\n" + GetToolTipStringFormat((StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_" + New_String + "_Passive"))) + "</size><size=20>\n");
				
				if(Mod_EpicLoot && !New_Bool)
				{ New_StringBuilder.Append("	\n"); }
				
				New_StringBuilder.Append("<smallcaps>" + GetLocalization("$ui_fpo_equipped $ui_fpo_passive") + "</smallcaps></size><size=16>\n" + GetToolTipStringFormat((StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_" + New_String + "_Equipped"))) + "</size><size=20>\n");
				
				if(Mod_EpicLoot && !New_Bool)
				{ New_StringBuilder.Append("	\n"); }
				
				New_StringBuilder.Append("<smallcaps>" + GetLocalization("$ui_fpo_active") + "</smallcaps></size><size=16>\n" + GetToolTipStringFormat(((StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_" + New_String + "_Active")))) + "</size><size=20>\n");
				
				if(Mod_EpicLoot && !New_Bool)
				{ New_StringBuilder.Append("	\n"); }
				
				New_StringBuilder.Append("<smallcaps>" + GetLocalization("$ui_fpo_shared $ui_fpo_active") + "</smallcaps></size><size=16>\n" + GetToolTipStringFormat(((StatusEffect_FPO)ObjectDB.instance.GetStatusEffect(GetHash("SE_FPO_" + New_String + "_Shared")))) + "</size><size=20>\n\n");
				
				if(Mod_EpicLoot && !New_Bool)
				{ New_StringBuilder.Append("	\n	\n"); }
			}
			
			return New_StringBuilder.ToString();
		}
	}
}