using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using EgyptianCartouches.Boons;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace EgyptianCartouches
{
    public class BoonsDisplayer : ThunderBehaviour
    {
        private Creature target;
        private List<BoonBase> boons = new List<BoonBase>();
        private bool createGOs;
        private List<GameObject> gameObjects = new List<GameObject>();
        private Dictionary<BoonBase, GameObject> gameObjectsDict = new Dictionary<BoonBase, GameObject>();
        public void Dispose()
        {
            Destroy(this);
        }
        public void SetTarget(Creature creature)
        {
            if (target == null)
            {
                createGOs = true;
            }
            target = creature;
            if(target != null)
            {
                RefreshTarget();
            }
            if (creature == null)
            {
                for(int i = boons.Count - 1; i >= 0; i--)
                {
                    DestroyGOs(boons[i]);
                }
            }
            if (createGOs)
            {
                createGOs = false;
                if (boons.Count <= 0)
                {
                    return;
                }
                foreach (BoonBase boon in boons)
                {
                    CreateGOs(boon);
                }
                RefreshPositions();
            }

        }
        public void AddBoon(BoonBase boon)
        {
            boons.Add(boon);
            if (target != null)
            {
                CreateGOs(boon);
                if (boons.Count > 0)
                    RefreshPositions();
            }
        }
        public void RemoveBoon(BoonBase boon)
        {
            boons.Remove(boon);
            if(target != null)
            {
                DestroyGOs(boon);
                if (boons.Count > 0)
                    RefreshPositions();
            }
        }
        

        private void CreateGOs(BoonBase boon)
        {
            GameObject imageParentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject imageGO = new GameObject();
            Image image = imageGO.gameObject.AddComponent<Image>();
            image.sprite = boon.sprite;
            Destroy(imageParentGO.GetComponent<Collider>());
            Destroy(imageParentGO.GetComponent<MeshRenderer>());
            imageParentGO.gameObject.AddComponent<Canvas>();
            imageParentGO.gameObject.AddComponent<CanvasRenderer>();
            imageParentGO.transform.SetParent(target.transform, false);
            imageGO.transform.SetParent(imageParentGO.transform, false);
            imageParentGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
            imageGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
            imageParentGO.transform.rotation *= Quaternion.LookRotation(-target.transform.right, Vector3.up);
            imageParentGO.transform.localScale = Vector3.one * .3f;
            imageParentGO.transform.localPosition = Vector3.up * 2.5f;
            imageParentGO.name = "Image" + image.sprite.name;
            imageGO.name = image.sprite.name;
            gameObjects.Add(imageParentGO);
            gameObjectsDict.Add(boon, imageParentGO);
        }

        private void DestroyGOs(BoonBase boon)
        {
            gameObjects.Remove(gameObjectsDict[boon]);
            Destroy(gameObjectsDict[boon]);
            gameObjectsDict.Remove(boon);
            if (boons.Count > 0)
                RefreshPositions();
        }

        private void RefreshTarget()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].transform.SetParent(target.transform, false);
                gameObjects[i].transform.rotation *= Quaternion.LookRotation(-target.transform.right, Vector3.up);
            }
        }

        private void RefreshPositions()
        {
            if (gameObjects.Count <= 0) 
                return;
            float step = 2f / gameObjects.Count;
            float length = 1f;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].transform.localPosition = Vector3.up * 2.5f + Vector3.right * ((i + length * 0.5f) * step - length);
            }
        }
    }
}
