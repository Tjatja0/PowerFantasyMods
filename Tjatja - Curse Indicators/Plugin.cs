using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

namespace CouchCoopMode;


[BepInPlugin("Tjatja.powerfantasymods.CurseIndicators", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    static ConfigEntry<bool> modEnabled;
    internal static new ManualLogSource Logger;
    static GameObject ActiveCurses, AsteroidsHolder, BladeTrapsHolder, ObserversHolder, AmbusterHolder, SniperHolder, WormsHolder, SerpentsHolder;
    //static GameObject buffIndicatorUI;
    static Slider buffTimer;
    static int buffCounter;
    static VisualCurseCounter VSC;

    private void Awake()
    {
        // Plugin startup logic
        modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        if (modEnabled.Value)
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} enabled!");
        }
    }

    [HarmonyPostfix] //create ActiveCurses UI
    [HarmonyPatch(typeof(MapModController), "SelectMapMods")]
    public static void PlayerController_OnConfirm()
    {

        GameObject ActiveBuffs = UIManager.Instance.buffIndicatorParent.gameObject;// GameObject.Find("ActiveBuffs");
        if (ActiveBuffs == null)
        {
            Console.WriteLine("Error ActiveBuffs is null!");
            return;
        }
        GameObject Canvas = GameObject.Find("Canvas");
        if (Canvas == null)
        {
            Console.WriteLine("Error Canvas Not Found!");
            return;
        }
        CurseSpawnDirector CSD = CurseSpawnDirector.Instance;
        if (CSD == null)
        {
            Console.WriteLine("Error CurseSpawnDirector Not Found!");
            return;
        }
        MapMods Mapmods = MapMods.Instance;
        if (Mapmods == null)
        {
            Console.WriteLine("Error MapMods Not Found!");
            return;
        }
        if (ActiveBuffs != null)
        {
            ActiveCurses = Instantiate(ActiveBuffs, new Vector3(Screen.width - ActiveBuffs.transform.position.x ,ActiveBuffs.transform.position.y, ActiveBuffs.transform.position.z) , new Quaternion()) as GameObject;
            RectTransform ActiveRect = ActiveCurses.GetComponent<RectTransform>();
            ActiveRect.anchorMin = new Vector2(1, 1);
            ActiveRect.anchorMax = new Vector2(1, 1);
            ActiveRect.pivot = new Vector2(1, 1);

            ActiveRect.localScale = new Vector3(
                ActiveRect.localScale.x * (Screen.dpi / 18),
                ActiveRect.localScale.y * (Screen.dpi / 18),
                ActiveRect.localScale.z);
            if (ActiveCurses.GetComponent<GridLayoutGroup>() != null)
            {
                ActiveCurses.GetComponent<GridLayoutGroup>().padding.top = 5;
            }
            ActiveCurses.transform.SetParent(ActiveBuffs.transform.parent.transform);
            ActiveCurses.transform.SetSiblingIndex(26);

        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CurseSpawnDirector), "SpawnAsteroids")]
    static IEnumerator<WaitForSeconds> CurseSpawnDirector_SpawnAsteroids(IEnumerator<WaitForSeconds> values)
    {
        WaitForSeconds interval = new WaitForSeconds(10f);
        WaitForSeconds stagger = new WaitForSeconds(0.05f);
        for (;;)
        {
            //Console.WriteLine("set 10f counter - Asteroids Ienumerator called!");
            if (AsteroidsHolder != null)
            {
                if (AsteroidsHolder.GetComponent<Plugin.VisualCurseCounter>() != null)
                {
                    AsteroidsHolder.GetComponent<Plugin.VisualCurseCounter>().Deactivate(1f);
                }
            }
            AsteroidsHolder = new GameObject("AsteroidsHolder", typeof(RectTransform));
            AsteroidsHolder.AddComponent<Plugin.VisualCurseCounter>();
            AsteroidsHolder.transform.SetParent(ActiveCurses.transform);
            AsteroidsHolder.GetComponent<Plugin.VisualCurseCounter>().ShowCurse("Asteroids", 9.5f);
            yield return interval;
            int count = (MapMods.Instance != null) ? MapMods.Instance.asteroids : 0;
            int num;
            for (int i = 0; i < count; i = num + 1)
            {
                if (SpawnManager.Instance.iceCubePrefab != null && PlayerController.Instance != null)
                {
                    using (new ProfilerMarker("CurseSpawnDirector.SpawnHazard").Auto())
                    {
                        Vector3 newPosition = new Vector3(PlayerController.Instance.transform.position.x + (UnityEngine.Random.insideUnitCircle *3.5f).x, PlayerController.Instance.transform.position.y + (UnityEngine.Random.insideUnitCircle * 3.5f).y, PlayerController.Instance.transform.position.z);
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(SpawnManager.Instance.iceCubePrefab, newPosition, Quaternion.identity);
                        gameObject.name = "Asteroid";
                        CurseSpawnDirector.Instance.TrackCurseObject(gameObject);
                    }
                }
                yield return stagger;
                num = i;
            }
        }
        yield break;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CurseSpawnDirector), "SpawnBladeTraps")]
    static IEnumerator<WaitForSeconds> CurseSpawnDirector_SpawnBladeTraps(IEnumerator<WaitForSeconds> values)
    {
        WaitForSeconds interval = new WaitForSeconds(10f);
        WaitForSeconds stagger = new WaitForSeconds(0.05f);
        for (;;)
        {
            //Console.WriteLine("set 10f counter - BladeTraps Ienumerator called!");
            if (BladeTrapsHolder != null)
            {
                if (BladeTrapsHolder.GetComponent<Plugin.VisualCurseCounter>() != null)
                {
                    BladeTrapsHolder.GetComponent<Plugin.VisualCurseCounter>().Deactivate(1f);
                }
            }
            BladeTrapsHolder = new GameObject("BladeTrapsHolder", typeof(RectTransform));
            BladeTrapsHolder.AddComponent<Plugin.VisualCurseCounter>();
            BladeTrapsHolder.transform.SetParent(ActiveCurses.transform);
            BladeTrapsHolder.GetComponent<Plugin.VisualCurseCounter>().ShowCurse("BladeTraps", 9.5f);
            yield return interval;
            int count = (MapMods.Instance != null) ? MapMods.Instance.bladeTraps : 0;
            int num;
            for (int i = 0; i < count; i = num + 1)
            {
                ObjectPooler objectPooler = (PoolHub.Instance != null) ? PoolHub.Instance.bladeTrapPool : null;
                if (objectPooler != null)
                {
                    using (new ProfilerMarker("CurseSpawnDirector.SpawnHazard").Auto())
                    {
                        PoolableObject poolableObject = objectPooler.Spawn(SpawnManager.Instance.GetCurseOffscreenSpawnPosition(), Quaternion.identity);
                        if (poolableObject != null)
                        {
                            poolableObject.name = "Blade Trap";
                            CurseSpawnDirector.Instance.TrackCurseObject(poolableObject.gameObject);
                        }
                    }
                }
                yield return stagger;
                num = i;
            }
        }
        yield break;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CurseSpawnDirector), "SpawnObservers")]
    static IEnumerator<WaitForSeconds> CurseSpawnDirector_SpawnObservers(IEnumerator<WaitForSeconds> values)
    { 
		for (;;)
		{
            float randomRange = UnityEngine.Random.Range(40f, 80f);
            //Console.WriteLine("set "+ randomRange + "f counter - Observers Ienumerator called!");
            if (ObserversHolder != null)
            {
                if (ObserversHolder.GetComponent<Plugin.VisualCurseCounter>() != null)
                {
                    ObserversHolder.GetComponent<Plugin.VisualCurseCounter>().Deactivate(1f);
                }
            }
            ObserversHolder = new GameObject("ObserversHolder", typeof(RectTransform));
            ObserversHolder.AddComponent<Plugin.VisualCurseCounter>();
            ObserversHolder.transform.SetParent(ActiveCurses.transform);
            ObserversHolder.GetComponent<Plugin.VisualCurseCounter>().ShowCurse("Observers", randomRange - 0.5f);
            yield return new WaitForSeconds(randomRange);
			int count = (MapMods.Instance != null) ? MapMods.Instance.observers : 0;
            int num;
			for (int i = 0; i<count; i = num + 1)
			{
				if (SpawnManager.Instance.observerPrefab != null && PlayerController.Instance != null)
                {
                    using (new ProfilerMarker("CurseSpawnDirector.SpawnHazard").Auto())
                    {
                        Vector3 position = new Vector3(PlayerController.Instance.transform.position.x + (UnityEngine.Random.insideUnitCircle.normalized * 25f).x, PlayerController.Instance.transform.position.y + (UnityEngine.Random.insideUnitCircle.normalized * 25f).y, PlayerController.Instance.transform.position.z);
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(SpawnManager.Instance.observerPrefab, position, Quaternion.identity);
                        gameObject.name = "Observer";
                        CurseSpawnDirector.Instance.TrackCurseObject(gameObject);
                    }
                }
                yield return new WaitForSeconds(1f);
                num = i;
			}
		}
		yield break;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CurseSpawnDirector), "SpawnSerpents")]
    static IEnumerator<WaitForSeconds> CurseSpawnDirector_SpawnSerpents(IEnumerator<WaitForSeconds> values)
    {
        for (;;)
        {
            //Console.WriteLine("set " + SpawnManager.Instance.SerpentSpawnInterval + "f counter - Serpents Ienumerator called!");
            if (SerpentsHolder != null)
            {
                if (SerpentsHolder.GetComponent<Plugin.VisualCurseCounter>() != null)
                {
                    SerpentsHolder.GetComponent<Plugin.VisualCurseCounter>().Deactivate(1f);
                }
            }
            SerpentsHolder = new GameObject("SerpentsHolder", typeof(RectTransform));
            SerpentsHolder.AddComponent<Plugin.VisualCurseCounter>();
            SerpentsHolder.transform.SetParent(ActiveCurses.transform);
            SerpentsHolder.GetComponent<Plugin.VisualCurseCounter>().ShowCurse("Serpents", SpawnManager.Instance.SerpentSpawnInterval - 0.5f);
            yield return new WaitForSeconds(SpawnManager.Instance.SerpentSpawnInterval);
            int count = (MapMods.Instance != null) ? MapMods.Instance.serpents : 0;
            int num;
            for (int i = 0; i < count; i = num + 1)
            {
                if (SpawnManager.Instance.serpentPrefab != null)
                {
                    using (new ProfilerMarker("CurseSpawnDirector.SpawnHazard").Auto())
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(SpawnManager.Instance.serpentPrefab, SpawnManager.Instance.GetSerpentSpawnPosition(), Quaternion.identity);
                        gameObject.name = "Serpent";
                        CurseSpawnDirector.Instance.TrackCurseObject(gameObject);
                        if (SpawnManager.Instance.enemyTransform != null)
                        {
                            gameObject.transform.SetParent(SpawnManager.Instance.enemyTransform);
                        }
                    }
                }
                if (SpawnManager.Instance.SerpentSpawnStagger > 0f)
                {
                    yield return new WaitForSeconds(SpawnManager.Instance.SerpentSpawnStagger);
                }
                num = i;
            }
        }
        yield break;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CurseSpawnDirector), "SpawnWorms")]
    static IEnumerator<WaitForSeconds> CurseSpawnDirector_SpawnWorms(IEnumerator<WaitForSeconds> values)
    {
        for (;;)
        {
            float randomRange = UnityEngine.Random.Range(10f, 20f);
            //Console.WriteLine("set " + randomRange + "f counter - Worms Ienumerator called!");
            if (WormsHolder != null)
            {
                if (WormsHolder.GetComponent<Plugin.VisualCurseCounter>() != null)
                {
                    WormsHolder.GetComponent<Plugin.VisualCurseCounter>().Deactivate(1f);
                }
            }
            WormsHolder = new GameObject("WormsHolder", typeof(RectTransform));
            WormsHolder.AddComponent<Plugin.VisualCurseCounter>();
            WormsHolder.transform.SetParent(ActiveCurses.transform);
            WormsHolder.GetComponent<Plugin.VisualCurseCounter>().ShowCurse("Worms", randomRange - 0.5f);
            yield return new WaitForSeconds(randomRange);
            int count = (MapMods.Instance != null) ? MapMods.Instance.nightmareWorms : 0;
            int num;
            for (int i = 0; i < count; i = num + 1)
            {
                if (SpawnManager.Instance.zombieBearPrefab != null)
                {
                    using (new ProfilerMarker("CurseSpawnDirector.SpawnHazard").Auto())
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(SpawnManager.Instance.zombieBearPrefab, SpawnManager.Instance.GetCurseOffscreenSpawnPosition(), Quaternion.identity);
                        gameObject.name = "Nightmare Worm";
                        CurseSpawnDirector.Instance.TrackCurseObject(gameObject);
                        BallTrap component = gameObject.GetComponent<BallTrap>();
                        if (component != null)
                        {
                            component.totalCount = count;
                            component.index = i;
                        }
                    }
                }
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.3f, 1.2f));
                num = i;
            }
        }
        yield break;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpawnManager), "StartStackedAdditionalSpawns")]
    static void SpawnManager_StartStackedAddtionalSpawns(string name)
    {
        if (name == null) return;
        if(name == "Ambuster")
        {
            if (MapMods.Instance.ambusters > 0)
            {
                EnemyInfo enemyInfo = SpawnManager.Instance.LoadEnemies("Ambuster")[0];
                //Console.WriteLine("set "+ enemyInfo.spawnFrequency + " counter - Ambuster spawner called!");
                if (AmbusterHolder != null)
                {
                    if (AmbusterHolder.GetComponent<Plugin.VisualCurseCounter>() != null)
                    {
                        AmbusterHolder.GetComponent<Plugin.VisualCurseCounter>().Deactivate(1f);
                    }
                }
                AmbusterHolder = new GameObject("AmbusterHolder", typeof(RectTransform));
                AmbusterHolder.AddComponent<Plugin.VisualCurseCounter>();
                AmbusterHolder.transform.SetParent(ActiveCurses.transform);
                AmbusterHolder.GetComponent<Plugin.VisualCurseCounter>().ShowCurse("Ambusters", enemyInfo.spawnFrequency);
            }
        }
        if (name == "Alien Sniper")
        {
            if(MapMods.Instance.alienSnipers > 0)
            {
                EnemyInfo enemyInfo = SpawnManager.Instance.LoadEnemies("Alien Sniper")[0];
                float snipetime = Mathf.Clamp(enemyInfo.spawnFrequency - 0.1f, 0.02f, enemyInfo.spawnFrequency);
                //Console.WriteLine("set "+ snipetime + " counter - Winged Sniper spawner called!");
                if (SniperHolder != null)
                {
                    if(SniperHolder.GetComponent<Plugin.VisualCurseCounter>() != null)
                    {
                        SniperHolder.GetComponent<Plugin.VisualCurseCounter>().Deactivate(1f);
                    }
                }
                SniperHolder = new GameObject("SniperHolder", typeof(RectTransform));
                SniperHolder.AddComponent<Plugin.VisualCurseCounter>();
                SniperHolder.transform.SetParent(ActiveCurses.transform);
                SniperHolder.GetComponent<Plugin.VisualCurseCounter>().ShowCurse("Snipers", snipetime);
            }
        }
    }

    public class VisualCurseCounter : MonoBehaviour
    {
        public GameObject buffIndicatorUI;
        public Slider buffTimer;
        public void ShowCurse(string name, float duration)
        {
            Color32 color = UnityEngine.Color.red;
            Sprite buffIcon;
            Texture2D newText = new Texture2D(2, 2);
            switch (name)
            {
                case "Asteroids":
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Asteroid.png"))
                    {
                        byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Asteroid.png");
                        newText.LoadImage(array);
                    }
                    break;
                case "BladeTraps":
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "BladeTraps.png"))
                    {
                        byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "BladeTraps.png");
                        newText.LoadImage(array);
                    }
                    break;
                case "Observers":
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Observer.png"))
                    {
                        byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Observer.png");
                        newText.LoadImage(array);
                    }
                    break;
                case "Serpents":
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Serpents.png"))
                    {
                        byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Serpents.png");
                        newText.LoadImage(array);
                    }
                    break;
                case "Worms":
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Worms.png"))
                    {
                        byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Worms.png");
                        newText.LoadImage(array);
                    }
                    break;
                case "Ambusters":
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Ambuster.png"))
                    {
                        byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Ambuster.png");
                        newText.LoadImage(array);
                    }
                    break;
                case "Snipers":
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "WingedSniper.png"))
                    {
                        byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "WingedSniper.png");
                        newText.LoadImage(array);
                    }
                    break;
            }
            Sprite spRite = Sprite.Create(newText, new Rect(0, 0, newText.width, newText.height), new Vector2(0.5f, 0.5f));
            buffIcon = spRite;


            buffIndicatorUI = UnityEngine.Object.Instantiate(UIManager.Instance.buffIndicatorPrefab, base.gameObject.transform);
            
            buffIndicatorUI.transform.GetComponent<RectTransform>().localScale = new Vector3(
                buffIndicatorUI.transform.GetComponent<RectTransform>().localScale.x * (Screen.dpi/18),
                buffIndicatorUI.transform.GetComponent<RectTransform>().localScale.y * (Screen.dpi/18),
                buffIndicatorUI.transform.GetComponent<RectTransform>().localScale.z);
            
            Image tempImage = buffIndicatorUI.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            tempImage.sprite = buffIcon;

            Image tempBorder = buffIndicatorUI.transform.GetChild(0).GetComponent<Image>();
            
            tempBorder.rectTransform.anchoredPosition = new Vector3(
                tempBorder.rectTransform.anchoredPosition.x - (Screen.width*0.01f),
                tempBorder.rectTransform.anchoredPosition.y,
                0f);
            
            buffTimer = buffIndicatorUI.transform.GetChild(1).GetComponent<Slider>();

            buffIndicatorUI.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector3(
                buffIndicatorUI.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.x + (Screen.width * 0.0025f), 
                buffIndicatorUI.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y, 
                0f);
           
            buffTimer.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = color;
            Transform transform = buffTimer.transform.Find("Title");
            if (transform != null && transform.TryGetComponent<Text>(out var component))
            {
                component.text = name.ToUpper();
                component.fontStyle = FontStyle.Bold;
            }

            ActiveCurses.GetComponent<GridLayoutGroup>().CalculateLayoutInputHorizontal();
            ActiveCurses.GetComponent<GridLayoutGroup>().CalculateLayoutInputVertical();
            switch (name)
            {
                case "Asteroids":
                case "BladeTraps":
                case "Observers":
                case "Serpents":
                case "Worms":
                    SetUpTimer(duration);
                    break;
                case "Ambusters":
                case "Snipers":
                    SetUpRepeatTimer(duration);
                    break;
            }
        }
        private void SetUpRepeatTimer(float duration)
        {
            buffTimer.maxValue = duration;
            buffTimer.value = duration;
            StartCoroutine(RepeatBuffTimer(duration, duration));
        }

        public IEnumerator RepeatBuffTimer(float duration, float reset)
        {
            yield return new WaitForSeconds(1f);
            duration--;
            buffTimer.value = duration;
            if (duration <= 0f)
            {
                duration = reset;
                StartCoroutine(RepeatBuffTimer(duration, reset));
            }
            else
            {
                StartCoroutine(RepeatBuffTimer(duration, reset));
            }
        }

        private void SetUpTimer(float duration)
        {
            buffTimer.maxValue = duration;
            buffTimer.value = duration;
            StartCoroutine(BuffTimer(duration));
        }

        public IEnumerator BuffTimer(float duration)
        {
            yield return new WaitForSeconds(1f);
            duration--;
            buffTimer.value = duration;
            if (duration <= 0f)
            {
                Deactivate(duration);
            }
            else
            {
                StartCoroutine(BuffTimer(duration));
            }
        }
        public void Deactivate(float duration)
        {
            StopCoroutine(BuffTimer(duration));
            UnityEngine.Object.Destroy(buffTimer);
            UnityEngine.Object.Destroy(buffIndicatorUI);
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }



}
