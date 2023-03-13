using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MessagePack.Decoders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace BattleStatsMultiplier
{
    [BepInPlugin("zuk.digimonno.battlestatsmultiplier", "Battle Stats Multiplier", "1.1.0")]
    public class Plugin : BasePlugin
    {
        public static ConfigEntry<double> battleRateConfig;
        public static ConfigEntry<string> battleRateMode;
        public static ConfigEntry<double> trainingRateConfig;
        public static ConfigEntry<string> trainingRateMode;
        public override void Load()
        {
            //configfile
            battleRateConfig = Config.Bind("Battle Configuration", "rate", 5.0, "Rate in which battle status rewards are multiplied (or added) by. Use a positive float. Rounded down when on add mode.");
            battleRateMode = Config.Bind("Battle Configuration", "mode", "multiply", "Mode to apply the status up. Acceptable values: multiply, add");
            trainingRateConfig = Config.Bind("Training Configuration", "rate", 2.0, "Rate in which training status rewards are multiplied (or added) by. Use a positive float. Rounded down when on add mode.");
            trainingRateMode = Config.Bind("Training Configuration", "mode", "multiply", "Mode to apply the status up. Acceptable values: multiply, add");
            // Plugin startup logic
            if (battleRateConfig.Value<0 || (battleRateMode.Value != "multiply" && battleRateMode.Value != "add"))
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
            public static bool SetRiseData_Patch(uResultPanelDigimonBase __instance,ref int hpRiseValue,ref int mpRiseValue,
                ref int forcefulnessRiseValue,ref int robustnessRiseValue,ref int clevernessRiseValue,
                ref int rapidityRiseValue, int fatigueRiseValue)
            {

                if (__instance.m_partnerCtrl.m_data.m_commonData.m_weight>3000) //se ta aqui veio do training
                {
                    __instance.m_partnerCtrl.m_data.m_commonData.AddWeight(-3000); //dieta braba
                    if (trainingRateMode.Value == "multiply")
                    {
                        hpRiseValue = (int)System.Math.Floor(hpRiseValue * trainingRateConfig.Value);
                        mpRiseValue = (int)System.Math.Floor(mpRiseValue * trainingRateConfig.Value);
                        forcefulnessRiseValue = (int)System.Math.Floor(forcefulnessRiseValue * trainingRateConfig.Value);
                        robustnessRiseValue = (int)System.Math.Floor(robustnessRiseValue * trainingRateConfig.Value);
                        clevernessRiseValue = (int)System.Math.Floor(clevernessRiseValue * trainingRateConfig.Value);
                        rapidityRiseValue = (int)System.Math.Floor(rapidityRiseValue * trainingRateConfig.Value);
                    }
                    else
                    {
                        hpRiseValue+=(int)Math.Round(trainingRateConfig.Value*10);
                        mpRiseValue+=(int)Math.Round(trainingRateConfig.Value*10);
                        forcefulnessRiseValue+=(int)Math.Round(trainingRateConfig.Value);
                        robustnessRiseValue+=(int)Math.Round(trainingRateConfig.Value);
                        clevernessRiseValue += (int)Math.Round(trainingRateConfig.Value);
                        rapidityRiseValue += (int)Math.Round(trainingRateConfig.Value);
                    }
                    return true;
                }else{
                    if (battleRateMode.Value == "multiply")
                    {
                        hpRiseValue = (int)System.Math.Floor(hpRiseValue * battleRateConfig.Value);
                        mpRiseValue = (int)System.Math.Floor(mpRiseValue * battleRateConfig.Value);
                        forcefulnessRiseValue = (int)System.Math.Floor(forcefulnessRiseValue * battleRateConfig.Value);
                        robustnessRiseValue = (int)System.Math.Floor(robustnessRiseValue * battleRateConfig.Value);
                        clevernessRiseValue = (int)System.Math.Floor(clevernessRiseValue * battleRateConfig.Value);
                        rapidityRiseValue = (int)System.Math.Floor(rapidityRiseValue * battleRateConfig.Value);
                    }
                    else
                    {
                        hpRiseValue+=(int)Math.Round(battleRateConfig.Value*10);
                        mpRiseValue+=(int)Math.Round(battleRateConfig.Value*10);
                        forcefulnessRiseValue+=(int)Math.Round(battleRateConfig.Value);
                        robustnessRiseValue+=(int)Math.Round(battleRateConfig.Value);
                        clevernessRiseValue += (int)Math.Round(battleRateConfig.Value);
                        rapidityRiseValue += (int)Math.Round(battleRateConfig.Value);
                    }

                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(uTrainingResultPanelDigimon))]
        [HarmonyPatch("SetTrainingResultData")]
        public static class ModPatchTrain
        {
            [HarmonyPrefix]
            public static bool SetTrainingResultData_Patch(ref uTrainingResultPanelDigimon __instance, TrainingResultData trainingResultData){
                __instance.m_partnerCtrl.m_data.m_commonData.AddWeight(3000);
                return true;
            }

        }
        // test with transpiler
        // [HarmonyPatch(typeof(uTrainingResultPanelDigimon))]
        // [HarmonyPatch("SetTrainingResultData")]
        // //transpiler is monkas
        // public class uTrainingResultPanelDigimon_SetTrainingResultData_Patch
        // {
        //     static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        //     {
        //         // buscar onde ele chama o fella
        //         var riseIndex = -1;
        //         var codes = new List<CodeInstruction>(instructions);
        //         Label riseLoadLabel =  il.DefineLabel();
        //         for(int i = 0; i < codes.Count; i++)
        //         {
        //             FileLog.Log($"teste i = {i}, opcode = {codes[i].opcode}");
        //             if (codes[i].opcode == OpCodes.Ret) { 

        //                 FileLog.Log("entrooo......");
        //                 riseIndex = i - 9;//9 é o ret, 8 antes do rise (loadarg + 7 arg)
        //                 codes[riseIndex].labels.Add(riseLoadLabel);
        //                 FileLog.Log("vai saiiiii.....");
        //                 break;
        //             }   
        //         }
        //         var instructionsToInsert = new List<CodeInstruction>();
        //         instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4, 80));//push 80
        //         instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, (sbyte)8));//push var8
        //         instructionsToInsert.Add(new CodeInstruction(OpCodes.Add)); //add both then push value to pile
        //         instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, (sbyte)8)); //pop to v8
        //         FileLog.Log("antes de lançar o jump");
        //         instructionsToInsert.Add(new CodeInstruction(OpCodes.Jmp, riseLoadLabel));
        //         //my brain is mush now

        //         if (riseIndex != -1)
        //         {
        //             FileLog.Log("entrou naquele if la......");
        //             //codes.InsertRange(riseIndex, instructionsToInsert);
        //             FileLog.Log("uai sô, te liga:");
        //             foreach (var code in codes) {
        //                 FileLog.Log($"opcode op: {code.opcode}   {code.operand}");
        //             }
        //         }

        //         return codes;
        //     }
        // }
    }
}

