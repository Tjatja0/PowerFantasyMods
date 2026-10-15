using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;


namespace MonsterSpawnRate;

[BepInPlugin("Tjatja.powerfantasymods.MonsterSpawnRate", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    static ConfigEntry<bool> modEnabled;
    internal static new ManualLogSource Logger;
    static float OptionEnemySpawnRate = 0.1f;
    static float OptionEnemyMultiplier = 0.1f;
    static Slider SliderEnemySpawnRate;
    static Slider SliderEnemyMultiplier;
    static Text TextEnemySpawnRate;
    static Text TextEnemyMultiplier;
    static bool loaded = false;

    private void Awake()
    {
        modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        if (modEnabled.Value)
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} enabled!");
        }
    }

    /* for version 1.3+
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpawnManager), "IsWebsiteSubmissionAllowed")]
    public static bool SpawnManager_IsWebsiteSubmissionAllowed(ref bool __result)
    {
        __result = false;
        return false;
    }
    */

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AchievementManager), "IsAlphaBuildActive")]
    public static bool AchievementManager_IsAlphaBuildActive(ref bool __result)
    {
        __result = true;
        return false;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamLeaderboardManager), "IsAlphaBuildActive")]
    public static bool SteamLeaderboardManager_IsAlphaBuildActive(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionManager), "Start")]
    public static void OptionManager_Start()
    {
        if (!Plugin.loaded)
        {
            GameObject newOption = new GameObject();
            GameObject newOption1 = new GameObject();
            GameObject OptionCompanionOpacity = new GameObject();
            GameObject ParentPanel = new GameObject();
            GameObject RealOptionPanel = new GameObject();

            if(UIManager.Instance !=null && UIManager.Instance.optionsWindow != null)
            {
                RealOptionPanel = UIManager.Instance.optionsWindow;
                if (RealOptionPanel == null)
                {
                    Console.WriteLine("OptionManager is null exit");
                    return;
                }
            }
            foreach (Transform t in RealOptionPanel.transform.GetComponentsInChildren<Transform>(false))
            {
                switch (t.name)
                {
                    case "OptionCompanionOpacity":
                        newOption = t.gameObject;
                        newOption1 = t.gameObject;
                        OptionCompanionOpacity = t.gameObject;
                        break;
                    case "Panel":
                        ParentPanel = t.gameObject;
                        break;
                }
            }
            if(newOption == new GameObject() || newOption == null)
            {
                Console.WriteLine("failed to get optionCompanionOpacity");
            }
            if (ParentPanel == new GameObject() || ParentPanel == null)
            {
                Console.WriteLine("failed to get Panel");
            }

            newOption.SetName("OptionEnemySpawnRate");
            GameObject GamOptionCompanionClone = Instantiate(newOption) as GameObject;
            GamOptionCompanionClone.transform.SetParent(ParentPanel.transform, false);
            GamOptionCompanionClone.SetActive(true);
            float float7 = OptionEnemySpawnRate;
            Transform OptionEnemySpawnrateSlider = GamOptionCompanionClone.transform.Find("Slider");
            Slider CustomSlide = OptionEnemySpawnrateSlider.GetComponent<Slider>();
            CustomSlide.onValueChanged.AddListener(delegate { SpawnManager_ChangeSpawnRate(); });
            SliderEnemySpawnRate = CustomSlide;
            Text CustomText = GamOptionCompanionClone.GetComponentInChildren<Text>();
            TextEnemySpawnRate = CustomText;
            CustomText.text = "Spawn Multiplier:";
            CustomSlide.value = 0.1f;
            GamOptionCompanionClone.SetActive(true);


            newOption1.SetName("OptionEnemySpawnRate");
            GameObject GamOptionCompanionClone1 = Instantiate(newOption1) as GameObject;
            GamOptionCompanionClone1.transform.SetParent(ParentPanel.transform, false);
            GamOptionCompanionClone1.SetActive(true);
            float float9 = OptionEnemyMultiplier;
            Transform OptionEnemySpawnrateSlider1 = GamOptionCompanionClone1.transform.Find("Slider");
            Slider CustomSlide1 = OptionEnemySpawnrateSlider1.GetComponent<Slider>();
            CustomSlide1.onValueChanged.AddListener(delegate { SpawnManager_ChangeMultiplier(); });
            SliderEnemyMultiplier = CustomSlide1; 
            Text CustomText1 = GamOptionCompanionClone1.GetComponentInChildren<Text>();
            TextEnemyMultiplier = CustomText1;
            CustomText1.text = "Monster Multiplier:";
            CustomSlide1.value = 0.1f;
            GamOptionCompanionClone1.SetActive(true);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(OptionManager.Instance.generaloptionsContent);
        }
    }

    public static void SpawnManager_ChangeSpawnRate()
    {
        Slider slider = SliderEnemySpawnRate;
        string val = slider.value + "";
        float sliderValue = float.Parse(val);
        sliderValue = ConvertFromSliderToValue(true, sliderValue);
        if (sliderValue != OptionEnemySpawnRate)
        {
            OptionEnemySpawnRate = sliderValue;
            TextEnemySpawnRate.text = "Spawn Multiplier : x" + OptionEnemySpawnRate; 
        }
    }

    public static void SpawnManager_ChangeMultiplier()
    {
        Slider slider1 = SliderEnemyMultiplier;
        string val1 = slider1.value + "";
        float sliderValue1 = float.Parse(val1);
        sliderValue1 = ConvertFromSliderToValue(false, sliderValue1);
        if (sliderValue1 != OptionEnemyMultiplier)
        {
            OptionEnemyMultiplier = sliderValue1;
            TextEnemyMultiplier.text = "Monster Multiplier : x" + OptionEnemyMultiplier;
        }
    }

    public static float ConvertFromSliderToValue(bool spawns, float value)
    {
        if(value < 0.01f)
        {
            if (spawns)
            {
                return 1f;
            }
            else
            {
                return 1f;
            }
        }
        if (value > 0.009f && value < 0.11f)
        {
            if (spawns)
            {
                return 0.5f;
            }
            else
            {
                return 2f;
            }
        }
        if (value > 0.1f && value < 0.21f)
        {
            if (spawns)
            {
                return 0.3f;
            }
            else
            {
                return 4f;
            }
        }
        if (value > 0.2f && value < 0.31f)
        {
            if (spawns)
            {
                return 0.2f;
            }
            else
            {
                return 5f;
            }
        }
        if (value > 0.3f && value < 0.41f)
        {
            if (spawns)
            {
                return 0.1f;
            }
            else
            {
                return 6f;
            }
        }
        if (value > 0.4f && value < 0.51f)
        {
            if (spawns)
            {
                return 0.07f;
            }
            else
            {
                return 7f;
            }
        }
        if (value > 0.5f && value < 0.61f)
        {
            if (spawns)
            {
                return 0.05f;
            }
            else
            {
                return 8f;
            }
        }
        if (value > 0.6f && value < 0.71f)
        {
            if (spawns)
            {
                return 0.01f;
            }
            else
            {
                return 9f;
            }
        }
        if (value > 0.7f && value < 0.81f)
        {
            if (spawns)
            {
                return 0.001f;
            }
            else
            {
                return 10f;
            }
        }

        return 0f;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpawnManager), "StartSpawns")]
    public static bool SpawnManager_ChangeLevel(ref Level _levelToLoad, SpawnManager __instance)
    {
        Level temp = _levelToLoad;
        int oldLimit = temp.fillerLimit;
        int sliderValue = oldLimit * (int)OptionEnemyMultiplier;
        temp.fillerLimit = sliderValue;
        SpawnManager.Instance.IncreaseFillerLimit(temp.fillerLimit-oldLimit);
        return true;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpawnManager), "SpawnEnemyWorker")]
    public static void SpawnManager_SpawnEnemy(SpawnManager __instance,
        bool isFiller, 
        ref float spawnFrequency)
    {
        if (isFiller && OptionEnemySpawnRate > 0f)
        {
            spawnFrequency = spawnFrequency * OptionEnemySpawnRate;
            Console.WriteLine("SpawnFrequency : " + spawnFrequency);
        }
    }

}
