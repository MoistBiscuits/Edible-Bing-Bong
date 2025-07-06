using BepInEx;
using BepInEx.Unity.Mono;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using Zorro.Core;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace EdibleBingBong;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private bool alreadyLoaded = false;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Object.DontDestroyOnLoad(base.gameObject);
        SceneManager.sceneLoaded += this.OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        Logger.LogInfo($"Loaded Scene: {scene.name}");;
        if (!this.alreadyLoaded)
        {
            Logger.LogInfo("Loaded the item Database");
            this.alreadyLoaded = true;
            ItemDatabase instance = SingletonAsset<ItemDatabase>.Instance;
            TweakFoodValues(instance);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= this.OnSceneLoaded;
    }

    private static void TweakFoodValues(ItemDatabase itemDatabase)
    {
        foreach (KeyValuePair<ushort, Item> keyValuePair in itemDatabase.itemLookup)
        { 
            Item item = keyValuePair.Value;
            Logger.LogInfo($"Item name: {item.name}");
            if (item.name == "BingBong") {
                Logger.LogInfo("Found BingBong");
                Action_Consume consumeAction = item.GetComponent<Action_Consume>();
                if (consumeAction == null)
                {
                    consumeAction = item.gameObject.AddComponent<Action_Consume>();
                }

                consumeAction.OnCastFinished = true;

                //Allow the item to be "used" and tell the UI the action
                item.usingTimePrimary = 1.2f;
                item.UIData.hasMainInteract = true;
                item.UIData.mainInteractPrompt = "Devour";

                ItemUseFeedback itemUseFeedback = item.GetComponent<ItemUseFeedback>();
                if (itemUseFeedback == null)
                {
                    itemUseFeedback = item.gameObject.AddComponent<ItemUseFeedback>();
                }

                itemUseFeedback.useAnimation = "Eat";

                Action_RestoreHunger hungerAction = item.GetComponent<Action_RestoreHunger>();
                
                if (hungerAction == null)
                {
                    hungerAction = item.gameObject.AddComponent<Action_RestoreHunger>();
                }

                hungerAction.restorationAmount = 0.1f;
                hungerAction.OnCastFinished = true;

                //Make it poison the player too cause why not

                Action_InflictPoison poisonAction = item.GetComponent<Action_InflictPoison>();

                if (poisonAction == null)
                {
                    poisonAction = item.gameObject.AddComponent<Action_InflictPoison>();
                }

                poisonAction.inflictionTime = 10f;
                poisonAction.poisonPerSecond = 0.075f;
                poisonAction.delay = 5f;

                poisonAction.enabled = true;
                poisonAction.OnConsumed = true;

                //And it also injures you too cause its a large plushie

                Action_ModifyStatus hurtAction = item.GetComponent<Action_ModifyStatus>();

                if (hurtAction == null)
                {
                    hurtAction = item.gameObject.AddComponent<Action_ModifyStatus>();
                }

                hurtAction.statusType = CharacterAfflictions.STATUSTYPE.Injury;
                hurtAction.changeAmount = 0.2f;

                hurtAction.enabled = true;
                hurtAction.OnConsumed = true;
            }
        }
    }
}
