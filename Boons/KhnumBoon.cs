using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace EgyptianCartouches.Boons
{
    // Stronger

    public class KhnumBoonItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out KhnumBoon _);
        }
    }

    public class KhnumBoon : BoonBase
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
        [ModOption(name: "Power Ratio %", tooltip: "Strength used to add force on impact when striking an npc", valueSourceName: nameof(Percentages), defaultValueIndex = 20, order = 1, saveValue = true, category = "Khnum Boon")]
        private static float powerRatio = 100f;
        [ModOption(name: "% chances to Dismember", tooltip: "% chances to dismember an NPC on impact", valueSourceName: nameof(Percentages), defaultValueIndex = 20, order = 2, saveValue = true, category = "Khnum Boon")]
        private static float percentageOfDismember = 100f;
        private bool rightHandEquipped;
        private bool leftHandEquipped;
        private Item rightHandItem;
        private Item leftHandItem;

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
                    if (creature.handRight.grabbedHandle != null)
                    {
                        SetupItem(creature.handRight);
                    }
                    if (creature.handLeft.grabbedHandle != null)
                    {
                        SetupItem(creature.handLeft);
                    }
                }
                activationNB++;
            }
            else
            {
                activationNB--;
                if (activationNB == 0)
                {
                    if (rightHandEquipped)
                    {
                        RemoveItem(rightHandItem.mainHandler);
                    }
                    if (leftHandEquipped)
                    {
                        RemoveItem(leftHandItem.mainHandler);
                    }
                    creature.handLeft.OnGrabEvent -= OnItemGrabbed;
                    creature.handRight.OnGrabEvent -= OnItemGrabbed;
                    creature.handLeft.OnUnGrabEvent -= OnItemUngrabbed;
                    creature.handRight.OnUnGrabEvent -= OnItemUngrabbed;
                }
            }
        }

        private void OnItemGrabbed(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                if (side == Side.Right)
                {
                    if (!rightHandEquipped)
                    {
                        if (handle.item.mainHandler)
                            SetupItem(handle.item.mainHandler);
                    }
                }
                else
                {
                    if (!leftHandEquipped)
                    {
                        if (handle.item.mainHandler)
                            SetupItem(handle.item.mainHandler);
                    }
                }
            }
        }

        private void OnItemUngrabbed(Side side, Handle handle, bool throwing, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                if (side == Side.Right)
                {
                    if (rightHandEquipped)
                    {
                        RemoveItem(rightHandItem.mainHandler);
                    }
                }
                else
                {
                    if (leftHandEquipped)
                    {
                        RemoveItem(leftHandItem.mainHandler);
                    }
                }
            }
        }

        private void SetupItem(RagdollHand hand)
        {
            if (hand.side == Side.Right)
            {
                rightHandItem = hand.grabbedHandle.item;
                rightHandEquipped = true;
                rightHandItem.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            }
            else
            {
                leftHandItem = hand.grabbedHandle.item;
                leftHandEquipped = true;
                leftHandItem.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            }
        }

        private void RemoveItem(RagdollHand hand)
        {
            if (hand.side == Side.Right)
            {
                rightHandItem.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionStartEvent;
                rightHandItem = null;
                rightHandEquipped = false;
            }
            else
            {
                leftHandItem.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionStartEvent;
                leftHandItem = null;
                leftHandEquipped = false;
            }
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance.intensity > 0.35f)
            {
                if (collisionInstance.targetColliderGroup?.collisionHandler?.item is Item item)
                {
                    if (item?.mainHandler?.creature == creature)
                        return;
                    OnItemHit(item, collisionInstance);
                }
                else if (collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart part)
                {
                    if (part.ragdoll.creature == creature)
                        return;
                    OnRagdollHit(part, collisionInstance);
                }
                else
                {

                }
            }
        }

        private void OnItemHit(Item item, CollisionInstance hit)
        {
            if (item.mainHandler != null)
            {
                item.mainHandler.UnGrab(true);
            }
            item.physicBody.rigidBody.AddForce(hit.impactVelocity * (item.physicBody.rigidBody.mass < 1f ? item.physicBody.rigidBody.mass * 5f : item.physicBody.rigidBody.mass) * (1f + 3f * powerRatio / 100f), ForceMode.Impulse);
        }

        private void OnRagdollHit(RagdollPart ragdollPart, CollisionInstance hit)
        {
            ragdollPart.ragdoll.creature.TryPush(Creature.PushType.Magic, hit.impactVelocity.normalized, 3);
            if (percentageOfDismember >= UnityEngine.Random.Range(0f, 100f))
            {
                if (!ragdollPart.isSliced && ragdollPart.sliceAllowed)
                {
                    ragdollPart.ragdoll.TrySlice(ragdollPart);
                    ragdollPart.ragdoll.creature.Kill();
                }
            }
            if (ragdollPart.type == RagdollPart.Type.Head)
            {
                ragdollPart.ragdoll.headPart.TrySlice();
                ragdollPart.ragdoll.creature.Kill();
            }
            foreach (RagdollPart part in Snippet.RagdollPartsImportantList(ragdollPart.ragdoll.creature))
                part.physicBody.rigidBody.AddForce(hit.impactVelocity * (1f + 3f * powerRatio / 100f), ForceMode.Impulse);
            ragdollPart.physicBody.rigidBody.AddForce(hit.impactVelocity * (1f + 3f * powerRatio / 100f) * 2f, ForceMode.Impulse);
        }


        public override void DebugName()
        {
            Debug.Log($"EgyptianCartouches : Name of boon : {this.GetType()}");
        }
    }
}
