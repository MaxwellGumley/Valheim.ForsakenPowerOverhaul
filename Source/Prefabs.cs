using BepInEx;
using Jotunn.Managers;
using UnityEngine;

namespace ForsakenPowerOverhaul
{
	partial class ForsakenPowerOverhaul : BaseUnityPlugin
	{
		static void Add_Prefabs()
		{
			GameObject New_GameObject = PrefabManager.Instance.CreateClonedPrefab("GO_FPO_Effect_PowerUnlock", "fx_GP_Stone");
			foreach(Transform New_Transform in New_GameObject.transform)
			{
				if(New_Transform.GetComponent<ParticleSystem>())
				{
					var Main = New_Transform.GetComponent<ParticleSystem>().main;
					Main.scalingMode = ParticleSystemScalingMode.Hierarchy;
				}
			}
			PrefabManager.Instance.AddPrefab(New_GameObject);
			
			PrefabManager.OnVanillaPrefabsAvailable -= Add_Prefabs;
		}
	}
}