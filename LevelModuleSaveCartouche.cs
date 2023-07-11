using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using EgyptianCartouches.Items;

namespace EgyptianCartouches
{
    public class LevelModuleSaveCartouche : ThunderScript
    {
        private static List<CartoucheBoonsSaveData> cartouchesBoonsDatas;
        private static List<Item> listOfCartouches;
        public override void ScriptEnable()
        {
            EventManager.onPossess += EventManager_onPossess;
            EventManager.onUnpossess += EventManager_onUnpossess;
            base.ScriptEnable();
        }

        public override void ScriptDisable()
        {
            EventManager.onPossess -= EventManager_onPossess;
            EventManager.onUnpossess -= EventManager_onUnpossess;
            base.ScriptDisable();
        }


        private void EventManager_onUnpossess(Creature creature, EventTime eventTime)
        {
            if (EventTime.OnStart == eventTime)
            {
                GetNbCartouches();
                if (listOfCartouches.Count <= 0)
                {
                    Debug.Log($"EgyptianCartouches : No cartouches to save");
                    return;
                }
                else
                {
                    SaveDatas();
                }
            }
        }

        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if (EventTime.OnEnd == eventTime)
            {
                Player.local.StartCoroutine(LoadDatasRoutine());
            }
        }

        private IEnumerator LoadDatasRoutine()
        {
            yield return new WaitForSeconds(2f);
            GetNbCartouches();
            if (listOfCartouches.Count <= 0)
            {
                Debug.Log($"EgyptianCartouches : No cartouches to load");
            }
            else
            {
                LoadDatas();
            }
            yield return null;
        }
        private void GetNbCartouches()
        {
            listOfCartouches = new List<Item>();
            cartouchesBoonsDatas = new List<CartoucheBoonsSaveData>();

            foreach (Item item in Player.local.creature.equipment.GetAllHolsteredItems())
            {
                if (item.itemId.Contains("EgyptianCartouche"))
                {
                    listOfCartouches.Add(item);
                }
            }
        }

        public void SaveDatas()
        {
            string pathToFile = Path.Combine(Application.streamingAssetsPath, "Mods", "EgyptianCartouches", "Saves.json");
            foreach (Item item in listOfCartouches)
            {
                List<Item> cartoucheBaseItems = item.GetComponent<CartoucheBase>().ReturnItems();
                List<string> listItems = new List<string>();
                for (int i = 0; i < cartoucheBaseItems.Count; i++)
                {
                    listItems.Add(cartoucheBaseItems[i].itemId);
                }
                CartoucheBoonsSaveData data = new CartoucheBoonsSaveData
                {
                    cartoucheId = item.itemId,
                    itemsId = listItems
                };
                if (data == null)
                {
                    Debug.Log($"EgyptianCartouches : Save data null");
                    return;
                }
                cartouchesBoonsDatas.Add(data);
                string jsonWrite = JsonConvert.SerializeObject(cartouchesBoonsDatas, Formatting.Indented);
                File.WriteAllText(pathToFile, jsonWrite);
            }
            Debug.Log($"EgyptianCartouches : Saved {cartouchesBoonsDatas.Count} cartouches at : Saves.json");
        }

        public static void LoadDatas()
        {
            string pathToFile = Path.Combine(Application.streamingAssetsPath, "Mods", "EgyptianCartouches", "Saves.json");
            if (!File.Exists(pathToFile))
            {
                Debug.Log($"EgyptianCartouches : No saves for Saves.json");
                return;
            }
            cartouchesBoonsDatas = JsonConvert.DeserializeObject<List<CartoucheBoonsSaveData>>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Mods", "EgyptianCartouches", "Saves.json")));
            if (cartouchesBoonsDatas == null)
            {
                Debug.Log($"EgyptianCartouches : No datas to load from Saves.json");
                return;
            }
            for (int i = 0; i < listOfCartouches.Count; i++)
            {
                for (int j = 0; j < cartouchesBoonsDatas[i].itemsId.Count; j++)
                {
                    Holder holder = listOfCartouches[i].childHolders[j];
                    Catalog.GetData<ItemData>(cartouchesBoonsDatas[i].itemsId[j]).SpawnAsync(item1 =>
                    {
                        holder.Snap(item1, true);
                    }, listOfCartouches[i].childHolders[j].transform.position);
                }
            }
            Debug.Log($"EgyptianCartouches : Loaded {listOfCartouches.Count} cartouche from : Saves.json");
        }
    }
}
