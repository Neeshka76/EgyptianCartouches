using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace EgyptianCartouches.Boons
{
    // Powerful arrow

    public class NeithBoonItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out NeithBoon _);
        }
    }

    public class NeithBoon : BoonBase
    {
        public static ModOptionFloat[] Percentages()
        {
            List<ModOptionFloat> options = new List<ModOptionFloat>();
            float val = 0f;
            while (val <= 100f)
            {
                options.Add(new ModOptionFloat(val.ToString("0"), val));
                val += 5f;
            }
            return options.ToArray();
        }
        [ModOption(name: "Arrow Speed %", tooltip: "Additionnal speed added to the arrow on released", valueSourceName: nameof(Percentages), defaultValueIndex = 7, order = 1, saveValue = true, category = "Neith Boon")]
        private static float arrowPercentageOfSpeed = 35f;
        [ModOption(name: "% chances to Dismember", tooltip: "% chances to dismember an NPC on arrow impact", valueSourceName: nameof(Percentages), defaultValueIndex = 20, order = 2, saveValue = true, category = "Neith Boon")]
        private static float arrowPercentageOfDismember = 100f;
        private bool bowEquipped;
        private Item bowEquip;
        private BowString bowString;
        private int nbHandsOnBow = 0;
        
        public override void Activation(bool active)
        {
            if (active)
            {
                if (activationNB == 0)
                {
                    creature.handLeft.OnGrabEvent += OnItemGrabbed;
                    creature.handRight.OnGrabEvent += OnItemGrabbed;
                    creature.handLeft.OnUnGrabEvent += OnItemUngrabbed;
                    creature.handRight.OnUnGrabEvent += OnItemUngrabbed;
                    if (creature.handLeft.grabbedHandle != null)
                    {
                        SetupBow(creature.handLeft.grabbedHandle);
                    }
                    if (creature.handRight.grabbedHandle != null)
                    {
                        SetupBow(creature.handRight.grabbedHandle);
                    }
                }
                activationNB++;
            }
            else
            {
                activationNB--;
                if (activationNB == 0)
                {
                    creature.handLeft.OnGrabEvent -= OnItemGrabbed;
                    creature.handRight.OnGrabEvent -= OnItemGrabbed;
                    creature.handLeft.OnUnGrabEvent -= OnItemUngrabbed;
                    creature.handRight.OnUnGrabEvent -= OnItemUngrabbed;
                }
                if (bowEquipped)
                    RemoveBow();
                nbHandsOnBow = 0;
            }
        }

        private void SetupBow(Handle handle)
        {
            if (handle.item.itemId.Contains("Bow"))
            {
                nbHandsOnBow++;
            }
            if (nbHandsOnBow == 1 && !bowEquipped)
            {
                bowEquip = handle.item;
                bowString = bowEquip.GetComponentInChildren<BowString>();
                bowString.onArrowRemoved += BowString_onArrowRemoved;
                bowEquipped = true;
            }
        }

        private void RemoveBow()
        {
            bowEquip = null;
            bowString.onArrowRemoved -= BowString_onArrowRemoved;
            bowString = null;
            bowEquipped = false;
            nbHandsOnBow--;
        }

        private void OnItemGrabbed(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                if (!handle?.item)
                {
                    return;
                }
                if (handle.item.itemId.Contains("Bow"))
                {
                    SetupBow(handle);
                }
            }
        }

        private void OnItemUngrabbed(Side side, Handle handle, bool throwing, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                if (!handle?.item)
                {
                    return;
                }
                if (handle.item.itemId.Contains("Bow") && bowEquipped)
                {
                    if (nbHandsOnBow == 1)
                    {
                        RemoveBow();
                    }
                    if (nbHandsOnBow > 1)
                    {
                        nbHandsOnBow--;
                    }
                }
            }
        }

        private void BowString_onArrowRemoved(Item arrow, bool fired)
        {
            if (fired)
            {
                arrow.physicBody.rigidBody.AddForce(arrow.physicBody.rigidBody.velocity * arrowPercentageOfSpeed / 100f, ForceMode.Impulse);
                arrow.gameObject.AddComponent<NeithArrowBehaviour>().Init(arrowPercentageOfDismember);
            }
        }

        public override void DebugName()
        {
            Debug.Log($"EgyptianCartouches : Name of boon : {this.GetType()}");
        }
    }

    public class NeithArrowBehaviour : ThunderBehaviour
    {
        public Item item;
        private float dismemberChance;
        private bool isFlying;
        protected override void ManagedOnEnable()
        {
            base.ManagedOnEnable();
            item = GetComponent<Item>();
        }

        public void Init(float dismemberChance)
        {
            this.dismemberChance = dismemberChance;
            isFlying = true;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance.intensity > 0.25f && isFlying)
            {
                if (collisionInstance.targetColliderGroup?.collisionHandler?.item is Item item)
                {
                    OnItemHit(item, collisionInstance);
                }
                else if (collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart part)
                {
                    OnRagdollHit(part, collisionInstance);
                }
                else
                {

                }
                Dispose();
            }
        }

        private void OnItemHit(Item item, CollisionInstance hit)
        {
            if (item.mainHandler != null)
            {
                item.mainHandler.UnGrab(true);
            }
            item.physicBody.rigidBody.AddForce(hit.impactVelocity * (item.physicBody.rigidBody.mass < 1f ? item.physicBody.rigidBody.mass * 5f : item.physicBody.rigidBody.mass), ForceMode.Impulse);
        }

        private void OnRagdollHit(RagdollPart ragdollPart, CollisionInstance hit)
        {
            ragdollPart.ragdoll.creature.TryPush(Creature.PushType.Magic, hit.impactVelocity.normalized, 3);
            if (dismemberChance >= UnityEngine.Random.Range(0f, 100f))
            {
                if (!ragdollPart.isSliced && ragdollPart.sliceAllowed)
                {
                    ragdollPart.ragdoll.TrySlice(ragdollPart);
                    ragdollPart.ragdoll.creature.Kill();
                }
            }
            if (ragdollPart.type == RagdollPart.Type.Torso)
            {
                List<RagdollPart> parts = Snippet.RagdollPartsImportantList(ragdollPart.ragdoll.creature);
                foreach (RagdollPart part in parts)
                    part.physicBody.rigidBody.AddForce(hit.impactVelocity * 2, ForceMode.Impulse);
            }
            if (ragdollPart.type == RagdollPart.Type.Head)
            {
                ragdollPart.ragdoll.headPart.TrySlice();
                ragdollPart.ragdoll.creature.Kill();
            }
            ragdollPart.physicBody.rigidBody.AddForce(hit.impactVelocity * 4, ForceMode.Impulse);
        }

        public void Dispose()
        {
            item.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionStartEvent;
            Destroy(this);
        }
    }

}
