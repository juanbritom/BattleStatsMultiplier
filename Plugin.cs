using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;

namespace BattleStatsMultiplier
{
    [BepInPlugin("zuk.digimonno.battlestatsmultiplier", "Battle Stats Multiplier", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public static ConfigEntry<double> rateConfig;
        public static ConfigEntry<string> rateMode;
        public override void Load()
        {
            //configfile
            rateConfig = Config.Bind("General", "rate", 5.0, "Rate in which battle status rewards are multiplied (or added) by. Use a positive float. Rounded down when on add mode.");
            rateMode = Config.Bind("General", "mode", "multiply", "Mode to apply the status up. Acceptable values: multiply, add");
            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loading!");
            if(rateConfig.Value<0 || (rateMode.Value != "multiply" && rateMode.Value != "add"))
            {
                Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} plugins found problems within its configuration file, please check it!");
                return;
            }
            else
            {
                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            }
            Awake();
        }

        public void Awake()
        {
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(uResultPanelDigimonBase), "SetRiseData")]
        public static class ModPatch
        {
            [HarmonyPrefix]
            public static bool SetRiseData_Patch(ref int hpRiseValue,ref int mpRiseValue,
                ref int forcefulnessRiseValue,ref int robustnessRiseValue,ref int clevernessRiseValue,
                ref int rapidityRiseValue, int fatigueRiseValue)
            {
                if (rateMode.Value == "multiply")
                {
                    hpRiseValue = (int)System.Math.Floor(hpRiseValue * rateConfig.Value);
                    mpRiseValue = (int)System.Math.Floor(mpRiseValue * rateConfig.Value);
                    forcefulnessRiseValue = (int)System.Math.Floor(forcefulnessRiseValue * rateConfig.Value);
                    robustnessRiseValue = (int)System.Math.Floor(robustnessRiseValue * rateConfig.Value);
                    clevernessRiseValue = (int)System.Math.Floor(clevernessRiseValue * rateConfig.Value);
                    rapidityRiseValue = (int)System.Math.Floor(rapidityRiseValue * rateConfig.Value);
                }
                else
                {
                    hpRiseValue+=(int)Math.Round(rateConfig.Value*10);
                    mpRiseValue+=(int)Math.Round(rateConfig.Value*10);
                    forcefulnessRiseValue+=(int)Math.Round(rateConfig.Value);
                    robustnessRiseValue+=(int)Math.Round(rateConfig.Value);
                    clevernessRiseValue += (int)Math.Round(rateConfig.Value);
                    rapidityRiseValue += (int)Math.Round(rateConfig.Value);
                }

                return true;
            }
        }
    }
}

