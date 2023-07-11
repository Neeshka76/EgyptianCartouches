using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace EgyptianCartouches.Boons
{
    // Imbue Fire and throw fireball on impact

    public class SekhmetBoonItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out SekhmetBoon _);
        }
    }

    public class SekhmetBoon : BoonBase
    {
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
                if (rightHandEquipped)
                {
                    RemoveItem(rightHandItem.mainHandler);
                }
                if (leftHandEquipped)
                {
                    RemoveItem(leftHandItem.mainHandler);
                }
                activationNB--;
                if (activationNB == 0)
                {
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
                        SetupItem(handle.item.mainHandler);
                    }
                }
                else
                {
                    if (!leftHandEquipped)
                    {
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
                StartCoroutine(ImbueWithFireRightHand());
            }
            else
            {
                leftHandItem = hand.grabbedHandle.item;
                leftHandEquipped = true;
                leftHandItem.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
                StartCoroutine(ImbueWithFireLeftHand());
            }
        }

        private void RemoveItem(RagdollHand hand)
        {
            if (hand.side == Side.Right)
            {
                rightHandItem.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionStartEvent;
                rightHandItem = null;
                rightHandEquipped = false;
                StopCoroutine(ImbueWithFireRightHand());
            }
            else
            {
                leftHandItem.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionStartEvent;
                leftHandItem = null;
                leftHandEquipped = false;
                StopCoroutine(ImbueWithFireLeftHand());
            }
        }
        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance.intensity > 0.35f)
            {
                if (collisionInstance.targetColliderGroup?.collisionHandler?.item is Item item && item?.mainHandler?.creature == creature)
                    return;
                if (collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart part && part?.ragdoll?.creature == creature)
                    return;
                Snippet.ThrowFireball(collisionInstance.contactPoint + collisionInstance.impactVelocity.normalized * 0.5f, (-collisionInstance.impactVelocity).normalized, 50f, 0f, creature);
            }
        }

        public IEnumerator ImbueWithFireRightHand()
        {
            while (true)
            {
                if (rightHandItem != null)
                {
                    if (Snippet.imbueBelowLevelItem(rightHandItem, 75f))
                    {
                        Snippet.ImbueItem(rightHandItem, "Fire");
                    }
                }
                yield return null;
            }
        }
        public IEnumerator ImbueWithFireLeftHand()
        {
            while (true)
            {
                if (leftHandItem != null)
                {
                    if (Snippet.imbueBelowLevelItem(leftHandItem, 75f))
                    {
                        Snippet.ImbueItem(leftHandItem, "Fire");
                    }
                }
                yield return null;
            }
        }

        public override void DebugName()
        {
            Debug.Log($"EgyptianCartouches : Name of boon : {this.GetType()}");
        }
    }
}
