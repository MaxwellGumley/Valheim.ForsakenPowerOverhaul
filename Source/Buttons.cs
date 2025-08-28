using BepInEx;
using Jotunn.Configs;
using Jotunn.Managers;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		static ButtonConfig ButtonConfig_PowerCycle;
		
		static void Add_Buttons()
		{
			ButtonConfig_PowerCycle = new ButtonConfig{ Name = "Button_PowerCycle", Config = ConfigEntry_Button_PowerCycle };
			InputManager.Instance.AddButton(PluginGUID, ButtonConfig_PowerCycle);
		}
	}
}