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
			//SceneManager.sceneLoaded += SceneLoaded;
		}

		public void Awake() {
			Logger.LogInfo("ProgressiveFocus started!");

			timeScale = Config.Bind("General", "TimeScale", 0.5f, "Time");

			Harmony.CreateAndPatchAll(typeof(ProgressiveFocus));
			//ChangeTimeScale(0.1f);
		}

		private void SceneLoaded(Scene scene, LoadSceneMode mode) {
			UnityEngine.Debug.Log("Scene Loaded");
			//ChangeTimeScale(0.1f);
		}

	
		public void Update()
		{
			//UnityEngine.Debug.Log("FrameUpdate");
			//ChangeTimeScale(0.1f);

		}

		private static void ChangeTimeScale(float time)
		{
			UnityEngine.Time.timeScale = time;
			UnityEngine.Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
			UnityEngine.Time.fixedDeltaTime = UnityEngine.Time.timeScale / SteamVR.instance.hmd_DisplayFrequency;
			UnityEngine.Debug.Log("Changed Time:" + Convert.ToString(UnityEngine.Time.timeScale));

		}

		[HarmonyPatch(typeof(AudioSource), "pitch", MethodType.Setter)]
		[HarmonyPrefix]
		public static void FixPitch(ref float value)
		{
			value *= Time.timeScale;
		}

		[HarmonyPatch(typeof(FVRViveHand), nameof(FVRViveHand.Update))]
		[HarmonyPrefix]
		public static void HandUpdate(FVRViveHand __instance)
		{
			var triggerTravel = __instance.Input.TriggerFloat;

			if (triggerTravel > 0.5f && triggerTravel < 0.7f)
			{
				UnityEngine.Debug.Log("TriggerDown");
				ChangeTimeScale(0.7f);
			}
			if (triggerTravel > 0.7f && triggerTravel < 0.9f)
			{
				ChangeTimeScale(0.5f);
			}
			if (triggerTravel > 0.9f)
			{
				ChangeTimeScale(0.1f);
			}
			else {
				ChangeTimeScale(1f);
			}

		}
	}
}