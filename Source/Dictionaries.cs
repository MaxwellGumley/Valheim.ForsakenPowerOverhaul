using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		static Dictionary<string, ConfigEntry<int>> Dictionary_ConfigEntry_Int = new Dictionary<string, ConfigEntry<int>>{ };
		static Dictionary<string, ConfigEntry<float>> Dictionary_ConfigEntry_Float = new Dictionary<string, ConfigEntry<float>>{ };
		static Dictionary<string, ConfigEntry<string>> Dictionary_ConfigEntry_String = new Dictionary<string, ConfigEntry<string>>{ };
		
	}
}