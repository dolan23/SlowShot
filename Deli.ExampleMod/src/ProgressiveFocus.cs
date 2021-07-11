using Deli.Immediate;
using Deli.Setup;
using UnityEngine;
using FistVR;
using UnityEngine.SceneManagement;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using Valve.VR;


namespace Deli.ProgressiveFocus
{
	public class ProgressiveFocus : DeliBehaviour
	{
		private static ConfigEntry<float> timeScale;
		public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;


		public ProgressiveFocus()
		{
			UnityEngine.Debug.Log("Loading ProgressiveFocus");
			SceneManager.sceneLoaded += SceneLoaded;
		}

		public void Awake() {
			Logger.LogInfo("ProgressiveFocus started!");

			timeScale = Config.Bind("General", "TimeScale", 0.5f, "Time");
			Harmony.CreateAndPatchAll(typeof(ProgressiveFocus));
		}

		private void SceneLoaded(Scene scene, LoadSceneMode mode) {
			UnityEngine.Debug.Log("Scene Loaded");
			Harmony.CreateAndPatchAll(typeof(ProgressiveFocus));
		}

		private static void ChangeTimeScale(float scale)
		{
			UnityEngine.Time.timeScale = Mathf.Clamp(scale, 0.4f, 1.0f);
			UnityEngine.Time.fixedDeltaTime = UnityEngine.Time.timeScale / SteamVR.instance.hmd_DisplayFrequency;
		}

		private static void ChangeTimeScaleExtreme(float scale)
		{
			UnityEngine.Time.timeScale = Mathf.Clamp(scale, 0.25f, 1.0f);
			UnityEngine.Time.fixedDeltaTime = UnityEngine.Time.timeScale / SteamVR.instance.hmd_DisplayFrequency;
		}

		[HarmonyPatch(typeof(AudioSource), "pitch", MethodType.Setter)]
		[HarmonyPrefix]
		public static void FixPitch(ref float value)
		{
			value *= Mathf.Clamp(Time.timeScale, 0.5f, 1.0f);
		}

		[HarmonyPatch(typeof(FVRViveHand), "Update")]
		[HarmonyPrefix]
		public static void HandUpdate(FVRViveHand __instance)
		{				
			var thisTriggerTravel = __instance.Input.TriggerFloat;
			var otherTriggerTravel = __instance.OtherHand.Input.TriggerFloat;
			
			var triggerTravelMax = Math.Max(thisTriggerTravel, otherTriggerTravel);
			var triggerTravelMin = Math.Min(thisTriggerTravel, otherTriggerTravel);

			if (triggerTravelMax > 0.98 && triggerTravelMin > 0.1) {
				ChangeTimeScaleExtreme(1.0f - triggerTravelMin);
			}
			else if(triggerTravelMax > 0.2f) {
				//ChangeTimeScale(Time.timeScale - 0.1f*Time.deltaTime);
			}
			else {
				if(Time.timeScale != 1f) { 
					ChangeTimeScale(Time.timeScale + 0.5f*Time.deltaTime);
				}
			}
			
		}
	}
}