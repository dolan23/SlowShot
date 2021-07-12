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
		public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;


		public ProgressiveFocus()
		{
			UnityEngine.Debug.Log("Loading ProgressiveFocus");
		}

		public void Awake() {
			Logger.LogInfo("ProgressiveFocus started!");
			Harmony.CreateAndPatchAll(typeof(ProgressiveFocus));
		}

		private static void ChangeTimeScale(float scale)
		{
			UnityEngine.Time.timeScale = Mathf.Clamp(scale, 0.4f, 1.0f);
			UnityEngine.Time.fixedDeltaTime = UnityEngine.Time.timeScale / SteamVR.instance.hmd_DisplayFrequency;
		}

		private static void ChangeTimeScaleExtreme(float scale)
		{
			scale *= 0.25f;
			UnityEngine.Time.timeScale = Mathf.Clamp(scale, 0.05f, 1.0f);
			UnityEngine.Time.fixedDeltaTime = UnityEngine.Time.timeScale / SteamVR.instance.hmd_DisplayFrequency;
		}

		[HarmonyPatch(typeof(AudioSource), "pitch", MethodType.Setter)]
		[HarmonyPrefix]
		public static void FixPitch(ref float value)
		{
			if (Time.timeScale < 0.5f)
			{
				value *= 0.5f;
			}
			else { 
				value *= 1.0f;
			}
		}

		[HarmonyPatch(typeof(FVRViveHand), "Update")]
		[HarmonyPrefix]
		public static void HandUpdate(FVRViveHand __instance)
		{
			var thisTriggerTravel = __instance.Input.TriggerFloat;
			var otherTriggerTravel = __instance.OtherHand.Input.TriggerFloat;

			var thisGun = __instance.CurrentInteractable;
			var otherGun = __instance.OtherHand.CurrentInteractable;
			int thisGunID = 0;
			int otherGunID = 1;
			var sameGunNoAkimboFlag = false;

			if (thisGun != null) {
				thisGunID = thisGun.GetInstanceID();
			}

			if (otherGun != null) {
				otherGunID = otherGun.GetInstanceID();
			}
			//
			//(!(thisHasGun is FVRFireArm && otherTriggerGun is FVRFireArm) && thisHasGun.GetInstanceID() != otherTriggerGun.GetInstanceID());


			if	(
					(
						(thisGun is FVRFireArm || otherGun is FVRFireArm)
						&&
						!(thisGun is FVRFireArm && otherGun is FVRFireArm)
					) 
				|| 
					(
						(thisGun is FVRFireArm && otherGun is FVRFireArm) 
						&&
						thisGunID == otherGunID 
					) 
				)
			{

				var triggerTravelMax = Math.Max(thisTriggerTravel, otherTriggerTravel);
				var triggerTravelMin = Math.Min(thisTriggerTravel, otherTriggerTravel);

				if (triggerTravelMax > 0.98 && triggerTravelMin > 0.2)
				{
					ChangeTimeScaleExtreme(1.0f - triggerTravelMin);
				}
				//else if(triggerTravelMax > 0.2f) {
				//ChangeTimeScale(Time.timeScale - 0.1f*Time.deltaTime);
				//}
				else
				{
					if (Time.timeScale != 1f)
					{
						ChangeTimeScale(Time.timeScale + 0.5f * Time.deltaTime);
					}
				}
			}
		}
	}
}