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
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace Beastiary;

[BepInPlugin("Tjatja.powerfantasymods.Beastiary", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    static ConfigEntry<bool> modEnabled;
    internal static new ManualLogSource Logger;
    static Npc NPCHolder;
    static int posX = 0, posY = 1, AreaSelector = 1, MonsterSelector = 0, bossCount = 0;
    static List<Vector3> CurrentMonPos = new List<Vector3>();
    static Font TextFont;
    static GameObject MainCanvas, MainPanel, PreviewTitle, PreviewDescription, PreviewAnimator, SubPanel, MonSelector;
    static Text PreviewTitleText, PreviewDescriptionText;
    static Text Button1TitleText, Button2TitleText, Button3TitleText, Button4TitleText, Button5TitleText;
    static Animator PreviewAnimatorA;
    static List<GameObject> ObjectPool = new List<GameObject>();
    static List<GameObject> CurrentMonPool = new List<GameObject>();
    static List<GameObject> ControllerButtons = new List<GameObject>();
    static bool inUse = false;
    static String inputs = "Xbox";

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

    [HarmonyPostfix] //Create beastiary statue
    [HarmonyPatch(typeof(PlayerStats), "InitiateCharacter")]
    public static void SelectionInterface_EnterHub()
    {
        GameObject TafelUnlocks = GameObject.Find("TafelUnlocks");
        if (TafelUnlocks == null)
        {
            Console.WriteLine("TafelUnlocks is null exit");
            return;
        }
        if (TafelUnlocks != null)
        {
            TafelUnlocks.SetName("TafelMonsters");
            GameObject Props = GameObject.Find("Props");
            GameObject TafelMonsters = Instantiate(TafelUnlocks, new Vector3(-10f, -11f, 0f), new Quaternion()) as GameObject;
            TafelMonsters.transform.SetParent(Props.transform, true);
            foreach (Transform t in TafelMonsters.GetComponentInChildren<Transform>())
            {
                if(t.GetName() == "Monuments 1_95")
                {
                    SpriteRenderer buste = t.GetComponentInChildren<SpriteRenderer>();
                    Texture2D newText = new Texture2D(2, 2);
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "PredatoryStatue256.png"))
                    {
                        byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "PredatoryStatue256.png");
                        newText.LoadImage(array);
                    }
                    Sprite spRite = Sprite.Create(newText, new Rect(0, 0, newText.width, newText.height), new Vector2(0.5f,0.5f));
                    buste.sprite = spRite;
                }
            }
            NPCHolder = TafelMonsters.GetComponent<Npc>();
            TafelMonsters.SetActive(true);
            MainCanvas = UnityEngine.GameObject.Find("Canvas");
            if (MainCanvas == null) { Console.WriteLine("Error Canvas Not Found!");}
        }
    }

    public static void BuildMonSelector(Vector3 CurMonPos)
    {
        if (ControllerButtons != null && ControllerButtons.Count > 0)
        {
            float nulPosX = ((Screen.width - (Screen.width / 2)) - (Screen.width / 4));
            float nulPosY = Screen.height - (Screen.height / 12.5f);
            if (MonSelector == null)
            {
                MonSelector = new GameObject("MonSelector", typeof(RectTransform));
                MonSelector.AddComponent<CanvasRenderer>();
                var image = MonSelector.AddComponent<Image>();
                image.rectTransform.sizeDelta = new Vector2((Screen.width * 0.045f), (Screen.height * 0.09f));
                image.rectTransform.anchoredPosition = new Vector3(CurMonPos.x, CurMonPos.y + (Screen.width * 0.008f), 5f);

                Texture2D newText = new Texture2D(2, 2);
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Selection128.png"))
                {
                    byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Selection128.png");
                    newText.LoadImage(array);
                }
                image.sprite = Sprite.Create(newText, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f));
            }
            else
            {
                var image = MonSelector.GetComponent<Image>();
                Destroy(image);
                Destroy(MonSelector);

                MonSelector = new GameObject("MonSelector", typeof(RectTransform));
                MonSelector.AddComponent<CanvasRenderer>();
                image = MonSelector.AddComponent<Image>();
                image.rectTransform.sizeDelta = new Vector2((Screen.width * 0.045f), (Screen.height * 0.09f));
                image.rectTransform.anchoredPosition = new Vector3(CurMonPos.x, CurMonPos.y + (Screen.width * 0.008f), 5f);
                Texture2D newText = new Texture2D(2, 2);
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Selection128.png"))
                {
                    byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Selection128.png");
                    newText.LoadImage(array);
                }
                image.sprite = Sprite.Create(newText, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f));

            }
            MonSelector.transform.SetParent(MainPanel.transform);
        }
        else
        {
            if (MonSelector != null)
            {
                var image = MonSelector.GetComponent<Image>();
                Destroy(image);
                Destroy(MonSelector);

            }
        }
    }

    public static void BuildAEnemyBox(EnemyInfo EI)
    {
        if (EI == null) { return; }
        GameObject currentMonster = new GameObject(EI.enemyPrefab.GetName(), typeof(RectTransform));
        currentMonster.AddComponent<CanvasRenderer>();
        var button = currentMonster.AddComponent<Button>();
        var image = currentMonster.AddComponent<Image>();
        float nulPosX = ((Screen.width - (Screen.width / 2)) - (Screen.width / 4));
        float nulPosY = Screen.height - (Screen.height / 12.5f);
        image.rectTransform.sizeDelta = new Vector2((Screen.width / 20), (Screen.height / 10));
        image.rectTransform.anchoredPosition = new Vector3((nulPosX + ((Screen.width / 25) * posX) - ((Screen.width/100)*15)), nulPosY - ((Screen.height / 12.5f) * posY), 3);
        CurrentMonPos.Add(new Vector3((nulPosX + ((Screen.width / 25) * posX) - ((Screen.width / 100) * 15)), nulPosY - ((Screen.height / 12.5f) * posY), 3));
        CurrentMonPool.Add(currentMonster);
        posX++;
        if (posX > 10) {
            posX = 0;
            posY++;
        }
        image.sprite = EI.enemyPrefab.GetComponent<SpriteRenderer>().sprite;
        button.onClick.AddListener(new UnityAction(() => SelectAEnemy(EI)));
        currentMonster.transform.SetParent(MainPanel.transform);
        ObjectPool.Add(currentMonster.gameObject);
    }

    public static void BuildAEnemyBox(GameObject Prefab)
    {
        if (Prefab == null) { return; }
        GameObject currentMonster = new GameObject(Prefab.GetName(), typeof(RectTransform));
        currentMonster.AddComponent<CanvasRenderer>();
        var button = currentMonster.AddComponent<Button>();
        var image = currentMonster.AddComponent<Image>();
        float nulPosX = ((Screen.width - (Screen.width / 2)) - (Screen.width / 4));
        float nulPosY = Screen.height - (Screen.height / 12.5f);
        image.rectTransform.sizeDelta = new Vector2((Screen.width / 20), (Screen.height / 10));
        image.rectTransform.anchoredPosition = new Vector3((nulPosX + ((Screen.width / 25) * posX) - ((Screen.width / 100) * 15)), nulPosY - ((Screen.height / 12.5f) * posY), 3);
        CurrentMonPos.Add(new Vector3((nulPosX + ((Screen.width / 25) * posX) - ((Screen.width / 100) * 15)), nulPosY - ((Screen.height / 12.5f) * posY), 3));
        CurrentMonPool.Add(currentMonster);
        posX++;
        if (posX > 10)
        {
            posX = 0;
            posY++;
        }
        image.sprite = Prefab.GetComponent<SpriteRenderer>().sprite;
        button.onClick.AddListener(new UnityAction(() => SelectAEnemy(Prefab)));
        currentMonster.transform.SetParent(MainPanel.transform);
        ObjectPool.Add(currentMonster.gameObject);
    }

    public static void SelectAEnemy(EnemyInfo EI) 
    {
        
        PreviewTitleText.text = EI.enemyPrefab.GetName();
        GameObject EIEnemyPrefab = Instantiate(EI.enemyPrefab);

        Transform[] componentsInChildren1 = EIEnemyPrefab.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < componentsInChildren1.Length; i++)
        {
            componentsInChildren1[i].gameObject.layer = 2;
        }
        Collider2D[] componentsInChildren4 = EIEnemyPrefab.GetComponentsInChildren<Collider2D>(true);
        for (int l = 0; l < componentsInChildren4.Length; l++)
        {
            componentsInChildren4[l].enabled = false;
        }
        Collider[] componentsInChildren5 = EIEnemyPrefab.GetComponentsInChildren<Collider>(true);
        for (int m = 0; m < componentsInChildren5.Length; m++)
        {
            componentsInChildren5[m].enabled = false;
        }
        Rigidbody2D[] componentsInChildren6 = EIEnemyPrefab.GetComponentsInChildren<Rigidbody2D>(true);
        for (int n = 0; n < componentsInChildren6.Length; n++)
        {
            componentsInChildren6[n].simulated = false;
        }
        Rigidbody[] componentsInChildren7 = EIEnemyPrefab.GetComponentsInChildren<Rigidbody>(true);
        for (int num = 0; num < componentsInChildren7.Length; num++)
        {
            componentsInChildren7[num].isKinematic = true;
            componentsInChildren7[num].detectCollisions = false;
        }
        EnemyHealth[] componentsInChildren8 = EIEnemyPrefab.GetComponentsInChildren<EnemyHealth>(true);
        for (int num = 0; num < componentsInChildren8.Length; num++)
        {
            componentsInChildren8[num].enabled = false;
        }
        EnemyController [] componentsInChildren9 = EIEnemyPrefab.GetComponentsInChildren<EnemyController>(true);
        for (int num = 0; num < componentsInChildren9.Length; num++)
        {
            componentsInChildren9[num].enabled = false;
        }
        EnemyAttack[] componentsInChildren10 = EIEnemyPrefab.GetComponentsInChildren<EnemyAttack>(true);
        for (int num = 0; num < componentsInChildren10.Length; num++)
        {
            componentsInChildren10[num].enabled = false;
        }

        PreviewAnimator.GetComponent<Image>().sprite = EI.enemyPrefab.GetComponent<SpriteRenderer>().sprite;
        PreviewAnimator.GetComponent<Image>().GetComponentInChildren<RectTransform>().sizeDelta = new Vector2((Screen.width*0.15f), (Screen.height*0.3f));
        PreviewAnimatorA.runtimeAnimatorController = EIEnemyPrefab.GetComponent<Animator>().runtimeAnimatorController;
        PreviewAnimatorA.updateMode = AnimatorUpdateMode.UnscaledTime;
        PreviewAnimatorA.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        //PreviewAnimatorA.deltaPosition.Set((Screen.width*0.8f), (Screen.height*0.63f) ,21f);
        PreviewAnimatorA.Rebind();
        PreviewAnimatorA.Update(0f);
        Destroy(EIEnemyPrefab);

        var EnemyHealthScript = EI.enemyPrefab.GetComponent<EnemyHealth>();
        string builder = "Stat Info:\n";
        builder = builder + "Max Health             :" + EnemyHealthScript.maxHealth +"\n";
        builder = builder + "Health Modifier       :" + EnemyHealthScript.healthModifier + "\n";
        builder = builder + "Spawn Frequency  :" + EI.spawnFrequency + "\n";
        builder = builder + "Spawn After           :" + EI.spawnAfterSeconds + "s\n";
        builder = builder + "Stop Spawn After   :" + EI.stopSpawnsAfterSeconds + "s\n";
        builder = builder + "Pack Size                :" + EI.packSize + "\n";
        builder = builder + "Burst Spawn           :" + EI.burstSpawn + "\n";
        PreviewDescriptionText.text = builder;

    }

    public static void SelectAEnemy(GameObject GO)
    {
        PreviewTitleText.text = GO.GetName();
        GameObject EIEnemyPrefab = Instantiate(GO);

        Transform[] componentsInChildren1 = EIEnemyPrefab.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < componentsInChildren1.Length; i++)
        {
            componentsInChildren1[i].gameObject.layer = 2;
        }
        Collider2D[] componentsInChildren4 = EIEnemyPrefab.GetComponentsInChildren<Collider2D>(true);
        for (int l = 0; l < componentsInChildren4.Length; l++)
        {
            componentsInChildren4[l].enabled = false;
        }
        Collider[] componentsInChildren5 = EIEnemyPrefab.GetComponentsInChildren<Collider>(true);
        for (int m = 0; m < componentsInChildren5.Length; m++)
        {
            componentsInChildren5[m].enabled = false;
        }
        Rigidbody2D[] componentsInChildren6 = EIEnemyPrefab.GetComponentsInChildren<Rigidbody2D>(true);
        for (int n = 0; n < componentsInChildren6.Length; n++)
        {
            componentsInChildren6[n].simulated = false;
        }
        Rigidbody[] componentsInChildren7 = EIEnemyPrefab.GetComponentsInChildren<Rigidbody>(true);
        for (int num = 0; num < componentsInChildren7.Length; num++)
        {
            componentsInChildren7[num].isKinematic = true;
            componentsInChildren7[num].detectCollisions = false;
        }
        EnemyHealth[] componentsInChildren8 = EIEnemyPrefab.GetComponentsInChildren<EnemyHealth>(true);
        for (int num = 0; num < componentsInChildren8.Length; num++)
        {
            componentsInChildren8[num].enabled = false;
        }
        EnemyController[] componentsInChildren9 = EIEnemyPrefab.GetComponentsInChildren<EnemyController>(true);
        for (int num = 0; num < componentsInChildren9.Length; num++)
        {
            componentsInChildren9[num].enabled = false;
        }
        EnemyAttack[] componentsInChildren10 = EIEnemyPrefab.GetComponentsInChildren<EnemyAttack>(true);
        for (int num = 0; num < componentsInChildren10.Length; num++)
        {
            componentsInChildren10[num].enabled = false;
        }

        PreviewAnimator.GetComponent<Image>().sprite = GO.GetComponent<SpriteRenderer>().sprite;
        PreviewAnimator.GetComponent<Image>().GetComponentInChildren<RectTransform>().sizeDelta = new Vector2((Screen.width * 0.15f), (Screen.height * 0.3f));
        PreviewAnimatorA.runtimeAnimatorController = EIEnemyPrefab.GetComponent<Animator>().runtimeAnimatorController;
        PreviewAnimatorA.updateMode = AnimatorUpdateMode.UnscaledTime;
        PreviewAnimatorA.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        PreviewAnimatorA.Rebind();
        PreviewAnimatorA.Update(0f);
        Destroy(EIEnemyPrefab);

        var EnemyHealthScript = GO.GetComponent<EnemyHealth>();
        string builder = "Stat Info:\n";
        builder = builder + "Max Health             :" + EnemyHealthScript.maxHealth + "\n";
        builder = builder + "Health Modifier       :" + EnemyHealthScript.healthModifier + "\n";
        PreviewDescriptionText.text = builder;
    }

    public static void DrawControllerButtons()
    {
        GameObject BButton = new GameObject("BButton", typeof(RectTransform));
        BButton.AddComponent<CanvasRenderer>();
        var image = BButton.AddComponent<Image>();
        image.rectTransform.sizeDelta = new Vector2((Screen.width * 0.02f), (Screen.height * 0.04f));
        image.rectTransform.anchoredPosition = new Vector3((Screen.width - (Screen.width * 0.035f)), (Screen.height / 20), 0f);
        Texture2D newText = new Texture2D(2, 2);
        if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "bButton.png"))
        {
            byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "bButton.png");
            newText.LoadImage(array);
        }
        image.sprite = Sprite.Create(newText, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f));
        BButton.transform.SetParent(MainPanel.transform);
        ControllerButtons.Add(BButton);

        GameObject LBButton = new GameObject("LBButton", typeof(RectTransform));
        LBButton.AddComponent<CanvasRenderer>();
        var image1 = LBButton.AddComponent<Image>();
        image1.rectTransform.sizeDelta = new Vector2((Screen.width * 0.02f), (Screen.height * 0.04f));
        image1.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.035f), (Screen.height / 20), 0f);
        Texture2D newText1 = new Texture2D(2, 2);
        if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "lbButton.png"))
        {
            byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "lbButton.png");
            newText1.LoadImage(array);
        }
        image1.sprite = Sprite.Create(newText1, new Rect(0, 0, newText1.width, newText1.height), new Vector2(0f, 0f));
        LBButton.transform.SetParent(MainPanel.transform);
        ControllerButtons.Add(LBButton);

        GameObject RBButton = new GameObject("RBButton", typeof(RectTransform));
        RBButton.AddComponent<CanvasRenderer>();
        var image2 = RBButton.AddComponent<Image>();
        image2.rectTransform.sizeDelta = new Vector2((Screen.width * 0.02f), (Screen.height * 0.04f));
        image2.rectTransform.anchoredPosition = new Vector3((Screen.width - (Screen.width * 0.23f)), (Screen.height / 20), 0f);
        Texture2D newText2 = new Texture2D(2, 2);
        if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "rbButton.png"))
        {
            byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "rbButton.png");
            newText2.LoadImage(array);
        }
        image2.sprite = Sprite.Create(newText2, new Rect(0, 0, newText2.width, newText2.height), new Vector2(0f, 0f));
        RBButton.transform.SetParent(MainPanel.transform);
        ControllerButtons.Add(RBButton);

    }
    public static void DeleteControllerButtons()
    {
        if (ControllerButtons == null) { return; }
        if (ControllerButtons != null)
        {
            foreach(GameObject go in ControllerButtons)
            {
                if (go.GetComponentsInChildren<Image>() != null)
                {
                    foreach (Image img in go.GetComponentsInChildren<Image>())
                    {
                        if (img == null) continue;
                        if (img.sprite != null)
                            img.sprite = null;

                    }
                }
                Destroy(go.gameObject);
            }
            ControllerButtons = new List<GameObject>();
        }
    }
    



[HarmonyPrefix]//circle/escape
    [HarmonyPatch(typeof(PlayerController), "OnBack")]
    public static bool PlayerController_OnBack(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                SmallCall(MainPanel);
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]//start/esc
    [HarmonyPatch(typeof(PlayerController), "OnPause")]
    public static bool PlayerController_OnPause(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                SmallCall(MainPanel);
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    
    [HarmonyPrefix]//right trigger/space
    [HarmonyPatch(typeof(PlayerController), "OnRB")]
    public static bool PlayerController_OnRB(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                switch (AreaSelector)
                {
                    case 1:
                        LoadArea("orc kingdom");
                        break;
                    case 2:
                        LoadArea("forgotten crypt");
                        break;
                    case 3:
                        LoadArea("hell");
                        break;
                    case 4:
                        LoadArea("Bosses");
                        break;
                    case 5:
                        LoadArea("corrupted forest");
                        break;
                }
                AreaSelector++;
                if (AreaSelector > 5)
                {
                    AreaSelector = 1;
                }
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]//right trigger/space
    [HarmonyPatch(typeof(PlayerController), "OnDash")]
    public static bool PlayerController_OnDash(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                switch (AreaSelector)
                {
                    case 1:
                        LoadArea("orc kingdom");
                        break;
                    case 2:
                        LoadArea("forgotten crypt");
                        break;
                    case 3:
                        LoadArea("hell");
                        break;
                    case 4:
                        LoadArea("Bosses");
                        break;
                    case 5:
                        LoadArea("corrupted forest");
                        break;
                }
                AreaSelector++;
                if(AreaSelector > 5)
                {
                    AreaSelector = 1;
                }
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]//lefttrigger/right mouse button
    [HarmonyPatch(typeof(PlayerController), "OnToggleAutoAttack")]
    public static bool PlayerController_OnToggleAutoAttack(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                switch (AreaSelector)
                {
                    case 1:
                        LoadArea("Bosses");
                        break;
                    case 2:
                        LoadArea("corrupted forest");
                        break;
                    case 3:
                        LoadArea("orc kingdom");
                        break;
                    case 4:
                        LoadArea("forgotten crypt");
                        break;
                    case 5:
                        LoadArea("hell");
                        break;
                }
                AreaSelector--;
                if (AreaSelector < 1)
                {
                    AreaSelector = 5;
                }
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]//lefttrigger/right mouse button
    [HarmonyPatch(typeof(PlayerController), "OnLB")]
    public static bool PlayerController_OnLB(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                switch (AreaSelector)
                {
                    case 1:
                        LoadArea("Bosses");
                        break;
                    case 2:
                        LoadArea("corrupted forest");
                        break;
                    case 3:
                        LoadArea("orc kingdom");
                        break;
                    case 4:
                        LoadArea("forgotten crypt");
                        break;
                    case 5:
                        LoadArea("hell");
                        break;
                }
                AreaSelector--;
                if (AreaSelector < 1)
                {
                    AreaSelector = 5;
                }
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]//x/left mouse
    [HarmonyPatch(typeof(PlayerController), "OnConfirm")]
    public static bool PlayerController_OnConfirm(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                if (CurrentMonPool[MonsterSelector].GetComponent<Button>() != null)
                {
                    CurrentMonPool[MonsterSelector].GetComponent<Button>().onClick.Invoke();
                }
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerController), "OnNavigateLeft")]
    public static bool PlayerController_OnNavigateLeft(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                MonsterSelector--;
                if (MonsterSelector < 0)
                {
                    MonsterSelector = CurrentMonPos.Count - 1;
                }
                BuildMonSelector(CurrentMonPos[MonsterSelector]);
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerController), "OnNavigateRight")]
    public static bool PlayerController_OnNavigateRight(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                MonsterSelector++;
                if (MonsterSelector > CurrentMonPos.Count - 1)
                {
                    MonsterSelector = 0;
                }
                BuildMonSelector(CurrentMonPos[MonsterSelector]);
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerController), "OnNavigateUp")]
    public static bool PlayerController_OnNavigateUp(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                int temp = MonsterSelector;
                if (temp < bossCount)
                {
                    MonsterSelector = 0;
                }
                else if (temp >= bossCount && temp < bossCount + bossCount - 1)
                {
                    MonsterSelector -= bossCount;
                }
                else if (temp >= bossCount + bossCount - 1 && temp < bossCount + 10)
                {
                    MonsterSelector = bossCount-1;
                }
                else
                {
                    MonsterSelector -= 11;
                }
                if (MonsterSelector < 0)
                {
                   MonsterSelector = 0;
                }
                BuildMonSelector(CurrentMonPos[MonsterSelector]);
                //move up with selection
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerController), "OnNavigateDown")]
    public static bool PlayerController_OnNavigateDown(InputValue value)
    {
        if (inUse)
        {
            if (value.isPressed)
            {
                if (MonsterSelector < bossCount)
                {
                    MonsterSelector = MonsterSelector + bossCount;
                }
                else
                {
                    MonsterSelector += 11;
                }

                if (MonsterSelector > CurrentMonPos.Count - 1)
                {
                    MonsterSelector = CurrentMonPos.Count-1;
                }
                BuildMonSelector(CurrentMonPos[MonsterSelector]);
            }
            return false;//skip the function
        }
        return true;//don't skip the function
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerController), "OnControlsChanged")]
    public static bool PlayerController_OnControlsChanged(PlayerInput input)
    {
        if (inUse)
        {
            switch (input.currentControlScheme)
            {
                case "Keyboard":
                    DeleteControllerButtons();
                    break;
                case "Xbox":
                    DrawControllerButtons();
                    break;

            }
            BuildMonSelector(CurrentMonPos[MonsterSelector]);
        }
        return true;//don't skip the function
    }

    [HarmonyPrefix]//Create beastiary menu
    [HarmonyPatch(typeof(Npc), "Interact")]
    public static bool Npc_Interact(Npc __instance)
    {
        if (__instance == null) return true;
        if (!__instance.openUnlocks) return true;
        if (__instance.transform.position != new Vector3(-10f, -11f, 0f)) return true;
        inUse = true;
        PlayerStats.Instance.BeginPerkSelectionXpLock();
        AudioManager.Instance.TuneDownMusic();
        Time.timeScale = 0f;
        if (UIManager.Instance.mobile)
        {
            UIManager.Instance.DeactivateJoysticks();
        }

        if (NPCHolder.openUnlocks)
        {
            if (NPCHolder.transform.position == new Vector3(-10f, -11f, 0f))
            {
                Console.WriteLine("Opening Beastiary");
                GameObject canvas = MainCanvas;
                GameObject goHolder = canvas.GetComponentInChildren<LocalizedText>().gameObject;
                if (goHolder != null)
                {
                    TextFont = goHolder.GetComponentInChildren<LocalizedText>().font.Get();
                }
                else
                {
                    return true;
                }

                UnityEngine.Color color;
                MainPanel = new GameObject("MainPanel", typeof(RectTransform));
                MainPanel.AddComponent<CanvasRenderer>();
                var image = MainPanel.AddComponent<Image>();
                image.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
                image.rectTransform.anchoredPosition = new Vector3((Screen.width / 2), (Screen.height / 2), 0f);
                ColorUtility.TryParseHtmlString("#000000", out color);//black
                image.color = color;
                MainPanel.transform.SetParent(canvas.transform);

                SubPanel = new GameObject("SubPanel", typeof(RectTransform));
                SubPanel.AddComponent<CanvasRenderer>();
                var image1 = SubPanel.AddComponent<Image>();
                image1.rectTransform.sizeDelta = new Vector2(Screen.width-(Screen.width/2), Screen.height - (Screen.height / 4));
                image1.rectTransform.anchoredPosition = new Vector3((Screen.width - (Screen.width/2) - Screen.width/5), (Screen.height - (Screen.height/2)) , 0f);
                ColorUtility.TryParseHtmlString("#2d1d17", out color);//darkbrown menu tone
                image1.color = color;
                SubPanel.transform.SetParent(MainPanel.transform);

                GameObject PreviewPanel = new GameObject("PreviewPanel", typeof(RectTransform));
                PreviewPanel.AddComponent<CanvasRenderer>();
                var image2 = PreviewPanel.AddComponent<Image>();
                image2.rectTransform.sizeDelta = new Vector2(Screen.width - (Screen.width / 2) - (Screen.width /5), Screen.height - (Screen.height / 4));
                image2.rectTransform.anchoredPosition = new Vector3((Screen.width - (Screen.width / 5)), (Screen.height - (Screen.height / 2)), 0f);
                ColorUtility.TryParseHtmlString("#2d1d17", out color);//darkbrown menu tone
                image2.color = color;
                PreviewPanel.transform.SetParent(MainPanel.transform);

                
                GameObject TitlePanel = new GameObject("TitlePanel", typeof(RectTransform));
                TitlePanel.AddComponent<CanvasRenderer>();
                var TextBox = TitlePanel.AddComponent<Text>();
                TextBox.rectTransform.sizeDelta = new Vector2(Screen.width - (Screen.width / 2), (Screen.height / 5));
                TextBox.rectTransform.anchoredPosition = new Vector3((Screen.width - (Screen.width / 2)), Screen.height - (Screen.height/20), 0f);
                ColorUtility.TryParseHtmlString("#E2A216", out color);//font color Yellow
                TextBox.color= color;
                if (TextFont != null)
                {
                    TextBox.font = TextFont;
                }
                TextBox.text = "Beastiary";
                TextBox.fontSize = 48;
                TextBox.alignment = TextAnchor.MiddleCenter;
                TitlePanel.transform.SetParent(MainPanel.transform);

                GameObject BackButton = new GameObject("BackButton", typeof(RectTransform));
                BackButton.AddComponent<CanvasRenderer>();
                var button = BackButton.AddComponent<Button>();
                var image3 = BackButton.AddComponent<Image>();
                image3.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                image3.rectTransform.anchoredPosition = new Vector3((Screen.width - (Screen.width / 10)), (Screen.height / 20), 0f);
                Texture2D newText = new Texture2D(2, 2);
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" +  "BlankButton.png"))
                {
                    byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "BlankButton.png");
                    newText.LoadImage(array);
                }
                image3.sprite = Sprite.Create(newText, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f)); 
                button.onClick.AddListener(new UnityAction(() => SmallCall(MainPanel)));
                BackButton.transform.SetParent(MainPanel.transform);

                GameObject Button6Title = new GameObject("Button1Title", typeof(RectTransform));
                Button6Title.AddComponent<CanvasRenderer>();
                var button6 = Button6Title.AddComponent<Button>();
                var Button6TitleText = Button6Title.AddComponent<Text>();
                Button6TitleText.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                Button6TitleText.rectTransform.anchoredPosition = new Vector3((Screen.width - (Screen.width / 10)), (Screen.height / 20), 0f);
                ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
                Button6TitleText.color = color;
                if (TextFont != null)
                {
                    Button6TitleText.font = TextFont;
                }
                Button6TitleText.fontStyle = FontStyle.Bold;
                Button6TitleText.text = "Back";
                Button6TitleText.fontSize = 22;
                Button6TitleText.alignment = TextAnchor.MiddleCenter;
                button6.onClick.AddListener(new UnityAction(() => SmallCall(MainPanel)));
                Button6TitleText.transform.SetParent(MainPanel.transform);

                PreviewTitle = new GameObject("PreviewTitle", typeof(RectTransform));
                PreviewTitle.AddComponent<CanvasRenderer>();
                PreviewTitleText = PreviewTitle.AddComponent<Text>();
                PreviewTitleText.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
                PreviewTitleText.rectTransform.anchoredPosition = new Vector3((Screen.width*0.8f), (Screen.height * 0.78f), 0f);
                ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
                PreviewTitleText.color = color;
                if (TextFont != null)
                {
                    PreviewTitleText.font = TextFont;
                }
                PreviewTitleText.text = "Select a Monster.";
                PreviewTitleText.fontSize = 22;
                PreviewTitleText.alignment = TextAnchor.MiddleCenter;
                PreviewTitle.transform.SetParent(MainPanel.transform);

                PreviewDescription = new GameObject("PreviewDescription", typeof(RectTransform));
                PreviewDescription.AddComponent<CanvasRenderer>();
                PreviewDescriptionText = PreviewDescription.AddComponent<Text>();
                PreviewDescriptionText.rectTransform.sizeDelta = new Vector2(Screen.width/6, Screen.height/4);
                PreviewDescriptionText.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.83f), (Screen.height * 0.35f), 0f);
                ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
                PreviewDescriptionText.color = color;
                if (TextFont != null)
                {
                    PreviewDescriptionText.font = TextFont;
                }
                PreviewDescriptionText.text = "Enemy stats will show up here. After you select a monster on the left side.";
                PreviewDescriptionText.fontSize = 14;
                PreviewDescriptionText.alignment = TextAnchor.MiddleLeft;
                PreviewDescription.transform.SetParent(MainPanel.transform);

                PreviewAnimator = new GameObject("PreviewAnimator", typeof(RectTransform));
                PreviewAnimator.AddComponent<CanvasRenderer>();
                var image9 = PreviewAnimator.AddComponent<Image>();
                PreviewAnimatorA = PreviewAnimator.AddComponent<Animator>();
                PreviewAnimatorA.runtimeAnimatorController = null;
                image9.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(0f, 0f);
                image9.GetComponentInChildren<RectTransform>().anchoredPosition = new Vector3((Screen.width*0.8f), (Screen.height * 0.62f), 3f);
                PreviewAnimator.transform.SetParent(MainPanel.transform);


                GameObject CF = new GameObject("CorruptedForest", typeof(RectTransform));
                CF.AddComponent<CanvasRenderer>();
                var buttonCF = CF.AddComponent<Button>();
                var imageCF = CF.AddComponent<Image>();
                imageCF.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                imageCF.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.1f), (Screen.height / 20), 0f); 
                Texture2D newText1 = new Texture2D(2, 2);
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "BlankButton.png"))
                {
                    byte[] array = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "BlankButton.png");
                    newText1.LoadImage(array);
                }
                imageCF.sprite = Sprite.Create(newText1, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f));
                buttonCF.onClick.AddListener(new UnityAction(() => LoadArea("corrupted forest")));
                CF.transform.SetParent(MainPanel.transform);

                GameObject Button1Title = new GameObject("Button1Title", typeof(RectTransform));
                Button1Title.AddComponent<CanvasRenderer>();
                var button1 = Button1Title.AddComponent<Button>();
                Button1TitleText = Button1Title.AddComponent<Text>();
                Button1TitleText.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                Button1TitleText.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.1f), (Screen.height / 20), 0f);
                ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
                Button1TitleText.color = color;
                if (TextFont != null)
                {
                    Button1TitleText.font = TextFont;
                }
                Button1TitleText.fontStyle = FontStyle.Bold;
                Button1TitleText.text = "Stage I";
                Button1TitleText.fontSize = 22;
                Button1TitleText.alignment = TextAnchor.MiddleCenter;
                button1.onClick.AddListener(new UnityAction(() => LoadArea("corrupted forest")));
                Button1TitleText.transform.SetParent(MainPanel.transform);

                GameObject OK = new GameObject("OrcKingdom", typeof(RectTransform));
                OK.AddComponent<CanvasRenderer>();
                var buttonOK = OK.AddComponent<Button>();
                var imageOK = OK.AddComponent<Image>();
                imageOK.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                imageOK.rectTransform.anchoredPosition = new Vector3((Screen.width*0.25f), (Screen.height / 20), 0f);
                imageOK.sprite = Sprite.Create(newText1, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f));
                buttonOK.onClick.AddListener(new UnityAction(() => LoadArea("orc kingdom")));
                OK.transform.SetParent(MainPanel.transform);

                GameObject Button2Title = new GameObject("Button2Title", typeof(RectTransform));
                Button2Title.AddComponent<CanvasRenderer>();
                var button2 = Button2Title.AddComponent<Button>();
                Button2TitleText = Button2Title.AddComponent<Text>();
                Button2TitleText.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                Button2TitleText.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.25f), (Screen.height / 20), 0f);
                ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
                Button2TitleText.color = color;
                if (TextFont != null)
                {
                    Button2TitleText.font = TextFont;
                }
                Button2TitleText.fontStyle = FontStyle.Bold;
                Button2TitleText.text = "Stage II";
                Button2TitleText.fontSize = 22;
                Button2TitleText.alignment = TextAnchor.MiddleCenter;
                button2.onClick.AddListener(new UnityAction(() => LoadArea("orc kingdom")));
                Button2TitleText.transform.SetParent(MainPanel.transform);


                GameObject FC = new GameObject("ForgottenCrypt", typeof(RectTransform));
                FC.AddComponent<CanvasRenderer>();
                var buttonFC = FC.AddComponent<Button>();
                var imageFC = FC.AddComponent<Image>();
                imageFC.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                imageFC.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.4f), (Screen.height / 20), 0f);
                imageFC.sprite = Sprite.Create(newText1, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f));
                buttonFC.onClick.AddListener(new UnityAction(() => LoadArea("forgotten crypt")));
                FC.transform.SetParent(MainPanel.transform);

                GameObject Button3Title = new GameObject("Button3Title", typeof(RectTransform));
                Button3Title.AddComponent<CanvasRenderer>();
                var button3 = Button3Title.AddComponent<Button>();
                Button3TitleText = Button3Title.AddComponent<Text>();
                Button3TitleText.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                Button3TitleText.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.4f), (Screen.height / 20), 0f);
                ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
                Button3TitleText.color = color;
                if (TextFont != null)
                {
                    Button3TitleText.font = TextFont;
                }
                Button3TitleText.fontStyle = FontStyle.Bold;
                Button3TitleText.text = "Stage III";
                Button3TitleText.fontSize = 22;
                Button3TitleText.alignment = TextAnchor.MiddleCenter;
                button3.onClick.AddListener(new UnityAction(() => LoadArea("forgotten crypt")));
                Button3TitleText.transform.SetParent(MainPanel.transform);

                GameObject hell = new GameObject("hell", typeof(RectTransform));
                hell.AddComponent<CanvasRenderer>();
                var buttonhell = hell.AddComponent<Button>();
                var imagehell = hell.AddComponent<Image>();
                imagehell.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                imagehell.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.55f), (Screen.height / 20), 0f);
                imagehell.sprite = Sprite.Create(newText1, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f));
                buttonhell.onClick.AddListener(new UnityAction(() => LoadArea("hell")));
                hell.transform.SetParent(MainPanel.transform);

                GameObject Button4Title = new GameObject("Button4Title", typeof(RectTransform));
                Button4Title.AddComponent<CanvasRenderer>();
                var button4 = Button4Title.AddComponent<Button>();
                Button4TitleText = Button4Title.AddComponent<Text>();
                Button4TitleText.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                Button4TitleText.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.55f), (Screen.height / 20), 0f);
                ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
                Button4TitleText.color = color;
                if (TextFont != null)
                {
                    Button4TitleText.font = TextFont;
                }
                Button4TitleText.fontStyle = FontStyle.Bold;
                Button4TitleText.text = "Stage IV";
                Button4TitleText.fontSize = 22;
                Button4TitleText.alignment = TextAnchor.MiddleCenter;
                button4.onClick.AddListener(new UnityAction(() => LoadArea("hell")));
                Button4TitleText.transform.SetParent(MainPanel.transform);

                GameObject Bosses = new GameObject("Bosses", typeof(RectTransform));
                Bosses.AddComponent<CanvasRenderer>();
                var buttonBosses = Bosses.AddComponent<Button>();
                var imageBosses = Bosses.AddComponent<Image>();
                imageBosses.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                imageBosses.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.7f), (Screen.height / 20), 0f);
                imageBosses.sprite = Sprite.Create(newText1, new Rect(0, 0, newText.width, newText.height), new Vector2(0f, 0f));
                buttonBosses.onClick.AddListener(new UnityAction(() => LoadArea("Bosses")));
                Bosses.transform.SetParent(MainPanel.transform);


                GameObject Button5Title = new GameObject("Button5Title", typeof(RectTransform));
                Button5Title.AddComponent<CanvasRenderer>();
                var button5 = Button5Title.AddComponent<Button>();
                Button5TitleText = Button5Title.AddComponent<Text>();
                Button5TitleText.rectTransform.sizeDelta = new Vector2((Screen.width / 10), (Screen.height / 20));
                Button5TitleText.rectTransform.anchoredPosition = new Vector3((Screen.width * 0.7f), (Screen.height / 20), 0f);
                ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
                Button5TitleText.color = color;
                if (TextFont != null)
                {
                    Button5TitleText.font = TextFont;
                }
                Button5TitleText.fontStyle = FontStyle.Bold;
                Button5TitleText.text = "Bosses";
                Button5TitleText.fontSize = 22;
                Button5TitleText.alignment = TextAnchor.MiddleCenter;
                button5.onClick.AddListener(new UnityAction(() => LoadArea("Bosses")));
                Button5TitleText.transform.SetParent(MainPanel.transform);

                if (inputs == "Xbox")
                {
                    DrawControllerButtons();
                }
                LoadArea("corrupted forest");
                return false;//skip the function
            }
        }
        return true;//don't skip
    }

    public static void SetActiveAreaButton(String resources)
    {
        UnityEngine.Color color;
        ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
        UnityEngine.Color FCW = color;
        ColorUtility.TryParseHtmlString("#E2A216", out color);//font color Yellow
        UnityEngine.Color FCY = color;
        switch (resources)
        {
            case "corrupted forest":
                Button1TitleText.color = FCY;
                Button2TitleText.color = FCW;
                Button3TitleText.color = FCW;
                Button4TitleText.color = FCW;
                Button5TitleText.color = FCW;
                break;
            case "orc kingdom":
                Button1TitleText.color = FCW;
                Button2TitleText.color = FCY;
                Button3TitleText.color = FCW;
                Button4TitleText.color = FCW;
                Button5TitleText.color = FCW;
                break;
            case "forgotten crypt":
                Button1TitleText.color = FCW;
                Button2TitleText.color = FCW;
                Button3TitleText.color = FCY;
                Button4TitleText.color = FCW;
                Button5TitleText.color = FCW;
                break;
            case "hell":
                Button1TitleText.color = FCW;
                Button2TitleText.color = FCW;
                Button3TitleText.color = FCW;
                Button4TitleText.color = FCY;
                Button5TitleText.color = FCW;
                break;
            case "Bosses":
                Button1TitleText.color = FCW;
                Button2TitleText.color = FCW;
                Button3TitleText.color = FCW;
                Button4TitleText.color = FCW;
                Button5TitleText.color = FCY;
                break;
        }
    }

    public static void LoadArea(String resources)
    {
        cleanPool();
        SetActiveAreaButton(resources);
        UnityEngine.Color color;
        GameObject BossHeaderBackground = new GameObject("BossHeaderBackground", typeof(RectTransform));
        BossHeaderBackground.AddComponent<CanvasRenderer>();
        var image4 = BossHeaderBackground.AddComponent<Image>();
        float nulPosX = (Screen.width / 4) + (Screen.width / 20);
        float nulPosY = Screen.height - (Screen.height / 15f);
        image4.rectTransform.sizeDelta = new Vector2(Screen.width - (Screen.width / 2), (Screen.height / 13f));
        image4.rectTransform.anchoredPosition = new Vector3(nulPosX, (nulPosY - (Screen.height / 12.5f) * posY), 0f);
        ColorUtility.TryParseHtmlString("#1E1310", out color);//darkerbrown menu tone
        image4.color = color;
        BossHeaderBackground.transform.SetParent(SubPanel.transform);
        ObjectPool.Add(BossHeaderBackground.gameObject);

        GameObject BossHeader = new GameObject("BossHeader", typeof(RectTransform));
        BossHeader.AddComponent<CanvasRenderer>();
        var TextBox1 = BossHeader.AddComponent<Text>();
        TextBox1.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        TextBox1.rectTransform.anchoredPosition = new Vector3(nulPosX, (nulPosY - (Screen.height / 12.5f) * posY), 0f);
        ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
        TextBox1.color = color;
        if (TextFont != null)
        {
            TextBox1.font = TextFont;
        }
        if (resources != "Bosses")
        {
            TextBox1.text = resources + " - Bosses";
        }
        else
        {
            TextBox1.text = resources;
        }
        TextBox1.fontSize = 22;
        TextBox1.alignment = TextAnchor.MiddleCenter;
        BossHeader.transform.SetParent(BossHeaderBackground.transform);
        ObjectPool.Add(BossHeader.gameObject);
        posY++;

        if (resources != "Bosses")
        {
            bossCount = 0;
            foreach (EnemyInfo EI in Resources.LoadAll<EnemyInfo>("Enemies/" + resources))
            {
                if (EI.miniboss)
                {
                    BuildAEnemyBox(EI);
                    bossCount++;
                }
            }
        }
        else
        {
            Level level = Resources.Load<Level>("Levels/Biomes/Corrupted Forest");//Predatory Mushroom
            BuildAEnemyBox(level.boss);
            level = Resources.Load<Level>("Levels/Biomes/Orc Kingdom");//Goblin King
            BuildAEnemyBox(level.boss);
            level = Resources.Load<Level>("Levels/Biomes/Forgotten Crypt");//Angel of Death
            BuildAEnemyBox(level.boss);
            level = Resources.Load<Level>("Levels/Biomes/Hell");//Balrog
            BuildAEnemyBox(level.boss);
            bossCount = 4;
        }
        posX = 0;
        posY++;

        GameObject MonsterHeaderBackground = new GameObject("MonsterHeaderBackground", typeof(RectTransform));
        MonsterHeaderBackground.AddComponent<CanvasRenderer>();
        var image5 = MonsterHeaderBackground.AddComponent<Image>();
        float nulPosX1 = (Screen.width / 4) + (Screen.width / 20);
        float nulPosY1 = Screen.height - (Screen.height / 15f);
        image5.rectTransform.sizeDelta = new Vector2(Screen.width - (Screen.width / 2), (Screen.height / 13f));
        image5.rectTransform.anchoredPosition = new Vector3(nulPosX1, (nulPosY1 - (Screen.height / 12.5f) * posY), 0f);
        ColorUtility.TryParseHtmlString("#1E1310", out color);//darkerbrown menu tone
        image5.color = color;
        MonsterHeaderBackground.transform.SetParent(SubPanel.transform);
        ObjectPool.Add(MonsterHeaderBackground.gameObject);

        GameObject MonsterHeader = new GameObject("MonsterHeader", typeof(RectTransform));
        MonsterHeader.AddComponent<CanvasRenderer>();
        var TextBox2 = MonsterHeader.AddComponent<Text>();
        TextBox2.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        TextBox2.rectTransform.anchoredPosition = new Vector3(nulPosX1, (nulPosY1 - (Screen.height / 12.5f) * posY), 0f);
        ColorUtility.TryParseHtmlString("#EEE", out color);//font color white
        TextBox2.color = color;
        if (TextFont != null)
        {
            TextBox2.font = TextFont;
        }
        if (resources != "Bosses")
        {
            TextBox2.text = resources + " - Monsters";
        }
        else
        {
            TextBox2.text = "General - Monsters";
        }
        TextBox2.fontSize = 22;
        TextBox2.alignment = TextAnchor.MiddleCenter;
        MonsterHeader.transform.SetParent(MonsterHeaderBackground.transform);
        ObjectPool.Add(MonsterHeader.gameObject);
        posY++;
        if (resources != "Bosses")
        {
            foreach (EnemyInfo EI in Resources.LoadAll<EnemyInfo>("Enemies/" + resources))
            {
                if (!EI.miniboss)
                {
                    BuildAEnemyBox(EI);
                }
            }
        }
        else
        {
            SpawnManager SM = SpawnManager.Instance;
            EnemyInfo temp = Resources.Load<EnemyInfo>("Enemies/Challenger/Void");//challenger
            BuildAEnemyBox(temp);
            temp = Resources.Load<EnemyInfo>("Enemies/Void/Void");//Void
            BuildAEnemyBox(temp);
            temp = Resources.Load<EnemyInfo>("Enemies/Dragon/Undead Dragon");//Dragon
            BuildAEnemyBox(temp);
            BuildAEnemyBox(SM.mimicPrefab);//mimic
            temp = Resources.Load<EnemyInfo>("Enemies/Alien Sniper/Alien");//Bats mapmod
            BuildAEnemyBox(temp);
            temp = Resources.Load<EnemyInfo>("Enemies/Ambuster");//Void
            BuildAEnemyBox(temp);
            temp = Resources.Load<EnemyInfo>("Enemies/Ambuster/Ambuster");//Void
            BuildAEnemyBox(temp);

        }

        posY = 1;
        posX = 0;
        if (CurrentMonPos != null && CurrentMonPos.Count > 0)
        {
            BuildMonSelector(CurrentMonPos[0]);
        }
    }

    public static void cleanPool()
    {
        if (ObjectPool == null){ return; }
        if (ObjectPool != null)
        {
            MonsterSelector = 0;
            CurrentMonPos = new List<Vector3>();
            foreach (GameObject go in ObjectPool)
            {
                if (ObjectPool == null) continue;
                if (go.GetComponentsInChildren<Image>() != null)
                {
                    foreach (Image img in go.GetComponentsInChildren<Image>())
                    {
                        if (img == null) continue;
                        if (img.sprite != null)
                            img.sprite = null;

                    }
                }
                Destroy(go.gameObject);
            }
        }
        ObjectPool = new List<GameObject>(); 
        CurrentMonPool = new List<GameObject>();
    }

    public static void SmallCall(GameObject go)
    {
        Console.WriteLine("Closing Beastiary");
        posY = 1;
        posX = 0;
        ObjectPool = new List<GameObject>();
        inUse = false;
        MonsterSelector = 0;
        MonSelector = null; 
        CurrentMonPool = new List<GameObject>();
        CurrentMonPos = new List<Vector3>();
        Destroy(go);
        PlayerStats.Instance.CompletePerkSelectionXpLock();
        AudioManager.Instance.TuneBackMusic();
        Time.timeScale = 1f;
        if (UIManager.Instance.mobile)
        {
            UIManager.Instance.ActivateJoysticks();
        }
    }
}
