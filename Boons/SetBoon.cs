using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using ThunderRoad.Pools;

namespace EgyptianCartouches.Boons
{
    // Imbue weapons held and make them arc to nearby items/NPC on impact
    public class SetBoonItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out SetBoon _);
        }
    }

    public class SetBoon : BoonBase
    {
        private bool rightHandEquipped;
        private bool leftHandEquipped;
        private Item rightHandItem;
        private Item leftHandItem;
        private EffectData boltEffectData = Catalog.GetData<EffectData>("SpellLightningBolt", true);
        EffectData imbueHitRagdollEffectData = Catalog.GetData<EffectData>("ImbueLightningRagdoll", true);
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
                rightHandItem.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionRightStartEvent;
                StartCoroutine(ImbueWithLightningRightHand());
            }
            else
            {
                leftHandItem = hand.grabbedHandle.item;
                leftHandEquipped = true;
                leftHandItem.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionLeftStartEvent;
                StartCoroutine(ImbueWithLightningLeftHand());
            }
        }

        private void RemoveItem(RagdollHand hand)
        {
            if (hand.side == Side.Right)
            {
                rightHandItem.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionRightStartEvent;
                rightHandItem = null;
                rightHandEquipped = false;
                StopCoroutine(ImbueWithLightningRightHand());
            }
            else
            {
                leftHandItem.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionLeftStartEvent;
                leftHandItem = null;
                leftHandEquipped = false;
                StopCoroutine(ImbueWithLightningLeftHand());
            }
        }
        private void MainCollisionHandler_OnCollisionRightStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance.intensity > 0.35f)
            {
                //if (collisionInstance.targetColliderGroup?.collisionHandler?.item is Item item && item?.mainHandler?.creature == creature
                //    && collisionInstance.sourceColliderGroup?.collisionHandler?.item is Item item1 && item1?.mainHandler?.creature == creature)
                //    return;
                //if (collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart part && part?.ragdoll?.creature == creature
                //    && collisionInstance.sourceColliderGroup?.collisionHandler?.ragdollPart is RagdollPart part1 && part1?.ragdoll?.creature == creature)
                //    return;
                if (collisionInstance.targetColliderGroup?.collisionHandler?.item is Item item && item?.mainHandler?.creature == creature)
                    return;
                if (collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart part && part?.ragdoll?.creature == creature)
                    return;
                CollisionBehaviour(collisionInstance, rightHandItem);
            }
        }
        private void MainCollisionHandler_OnCollisionLeftStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance.intensity > 0.35f)
            {
                if (collisionInstance.targetColliderGroup?.collisionHandler?.item is Item item && item?.mainHandler?.creature == creature)
                    return;
                if (collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart part && part?.ragdoll?.creature == creature)
                    return;
                CollisionBehaviour(collisionInstance, leftHandItem);
            }
        }

        private void CollisionBehaviour(CollisionInstance collisionInstance, Item itemExcluded)
        {
            List<Creature> creatures = Snippet.CreaturesInRadius(collisionInstance.contactPoint, 3f, true, false, true).ToList();
            List<Creature> creatureSelection;
            List<Item> items = Snippet.ItemsInRadius(collisionInstance.contactPoint, 3f, itemToExclude: itemExcluded).ToList();
            List<Item> itemSelection;
            for (int i = creatures.Count - 1; i > 0; i--)
            {
                creatures.Remove(creature);
            }
            if (creatures.Count > 3)
                creatureSelection = creatures.GetRange(UnityEngine.Random.Range(0, creatures.Count - 2), 3);
            else
            {
                creatureSelection = creatures;
            }
            for (int i = creatureSelection.Count - 1; i > 0; i--)
            {
                PlayBolt(collisionInstance?.targetCollider?.transform, collisionInstance.contactPoint.ClosestRagdollPart(creatureSelection[i]).transform);
                collisionInstance = new CollisionInstance(new DamageStruct(DamageType.Energy, 1f)
                {
                    hitRagdollPart = collisionInstance.contactPoint.ClosestRagdollPart(creatureSelection[i]),
                    hitBack = (Vector3.Dot((collisionInstance.contactPoint - collisionInstance.contactPoint.ClosestRagdollPart(creatureSelection[i]).transform.position).normalized, collisionInstance.contactPoint.ClosestRagdollPart(creatureSelection[i]).forwardDirection) < 0f),
                    pushLevel = 1,
                    damageType = DamageType.Energy
                }, null, null)
                {
                    impactVelocity = collisionInstance.impactVelocity,
                    contactPoint = collisionInstance.contactPoint,
                    contactNormal = collisionInstance.contactNormal,
                    targetColliderGroup = collisionInstance.targetColliderGroup,
                };
                collisionInstance.contactPoint.ClosestRagdollPart(creatureSelection[i]).ragdoll.creature.Damage(collisionInstance);
                creatureSelection[i].TryElectrocute(1f, 5f, true, true, imbueHitRagdollEffectData);
            }
            if (items.Count > 7)
                itemSelection = items.GetRange(UnityEngine.Random.Range(0, items.Count - 6), 7);
            else
            {
                itemSelection = items;
            }
            for (int i = itemSelection.Count - 1; i > 0; i--)
            {
                if (itemSelection[i] != null)
                {
                    if (itemExcluded != null)
                        PlayBolt(collisionInstance?.targetCollider?.transform, itemSelection[i].transform);
                    Snippet.ImbueItem(itemSelection[i], "Lightning");
                }
            }
        }

        private void PlayBolt(Transform source = null, Transform target = null, Vector3? sourcePos = null, Vector3? targetPos = null)
        {
            EffectInstance effectInstance = boltEffectData.Spawn(Vector3.zero, Quaternion.identity, null, null, true, null, false, Array.Empty<Type>());
            Transform transform = PoolUtils.GetTransformPoolManager().Get();
            Transform transform2 = PoolUtils.GetTransformPoolManager().Get();
            effectInstance.SetSource(transform);
            effectInstance.SetTarget(transform2);
            if (source != null)
            {
                transform.SetParent(source);
            }
            if (target != null)
            {
                transform2.SetParent(target);
            }
            transform.position = (sourcePos ?? ((source != null) ? source.position : Vector3.zero));
            transform2.position = (targetPos ?? ((target != null) ? target.position : Vector3.zero));
            effectInstance.Play(0, false);
            effectInstance.onEffectFinished += this.OnBoltEffectFinishEvent;
        }
        private void OnBoltEffectFinishEvent(EffectInstance instance)
        {
            if (instance.sourceTransform != null)
            {
                PoolUtils.GetTransformPoolManager().Release(instance.sourceTransform);
            }
            if (instance.targetTransform != null)
            {
                PoolUtils.GetTransformPoolManager().Release(instance.targetTransform);
            }
            instance.onEffectFinished -= this.OnBoltEffectFinishEvent;
        }

        private IEnumerator ImbueWithLightningRightHand()
        {
            while (true)
            {
                if (rightHandItem != null)
                {
                    if (Snippet.imbueBelowLevelItem(rightHandItem, 75f))
                    {
                        Snippet.ImbueItem(rightHandItem, "Lightning");
                    }
                }
                yield return null;
            }
        }
        private IEnumerator ImbueWithLightningLeftHand()
        {
            while (true)
            {
                if (leftHandItem != null)
                {
                    if (Snippet.imbueBelowLevelItem(leftHandItem, 75f))
                    {
                        Snippet.ImbueItem(leftHandItem, "Lightning");
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
