using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using UnityEngine.UI;

namespace EgyptianCartouches.Boons
{
    // Powerful Throw
    public class BastetBoonItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out BastetBoon _);
        }
    }

    public class BastetBoon : BoonBase
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
        private bool rightHandEquipped;
        private bool leftHandEquipped;
        private Item rightHandItem;
        private Item leftHandItem;
        [ModOption(name: "Item Speed Added on Throw in %", tooltip: "Additionnal speed added to the item on throw", valueSourceName: nameof(Percentages), defaultValueIndex = 7, order = 1, saveValue = true, category = "Bastet Boon")]
        private static float itemPercentageOfSpeed = 35f;
        [ModOption(name: "% chances to Dismember", tooltip: "% chances to dismember an NPC on item throwed impact", valueSourceName: nameof(Percentages), defaultValueIndex = 20, order = 2, saveValue = true, category = "Bastet Boon")]
        private static float itemPercentageOfDismember = 100f;
        private BastetItemThrowBehaviour bastetItemThrowBehaviourRightHand;
        private BastetItemThrowBehaviour bastetItemThrowBehaviourLeftHand;
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
                    if (handle.item)
                    {
                        if (handle.item.itemId.Contains("Arrow"))
                            return;
                    }
                    if (!rightHandEquipped)
                    {
                        SetupItem(handle.item.mainHandler);
                    }
                }
                else
                {
                    if (!leftHandEquipped)
                    {
                        if (handle.item)
                        {
                            if (handle.item.itemId.Contains("Arrow"))
                                return;
                        }
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
                bastetItemThrowBehaviourRightHand = item.gameObject.GetComponent<BastetItemThrowBehaviour>();
                if (bastetItemThrowBehaviourRightHand == null)
                    rightHandItem.gameObject.AddComponent<BastetItemThrowBehaviour>().Init(itemPercentageOfSpeed, itemPercentageOfDismember, creature);
            }
            else
            {
                leftHandItem = hand.grabbedHandle.item;
                leftHandEquipped = true;
                bastetItemThrowBehaviourLeftHand = item.gameObject.GetComponent<BastetItemThrowBehaviour>();
                if (bastetItemThrowBehaviourLeftHand == null)
                    leftHandItem.gameObject.AddComponent<BastetItemThrowBehaviour>().Init(itemPercentageOfSpeed, itemPercentageOfDismember, creature);
            }
        }

        private void RemoveItem(RagdollHand hand)
        {
            if (hand.side == Side.Right)
            {
                rightHandItem = null;
                rightHandEquipped = false;
            }
            else
            {
                leftHandItem = null;
                leftHandEquipped = false;
            }
        }

        public override void DebugName()
        {
            Debug.Log($"EgyptianCartouches : Name of boon : {this.GetType()}");
        }
    }

    public class BastetItemThrowBehaviour : ThunderBehaviour
    {
        public Item item;
        private float dismemberChance;
        private float speedItem;
        private bool isFlying;
        private float dragOri;
        private bool flyFromThrowOri;
        private float flyRotationSpeedOri;
        private float flyThrowAngleOri;
        private float massOri;
        private Creature creature;
        private bool noFlyDir;
        protected override void ManagedOnEnable()
        {
            base.ManagedOnEnable();
            item = GetComponent<Item>();
        }

        private void Item_OnDespawnEvent(EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                Dispose();
            }
        }

        public void Init(float speedItem, float dismemberChance, Creature creature)
        {
            this.speedItem = speedItem;
            this.dismemberChance = dismemberChance;
            massOri = item.physicBody.rigidBody.mass;
            dragOri = item.physicBody.rigidBody.drag;
            flyFromThrowOri = item.flyFromThrow;
            flyRotationSpeedOri = item.flyRotationSpeed;
            flyThrowAngleOri = item.flyThrowAngle;
            item.flyFromThrow = true;
            item.flyRotationSpeed = 10f;
            item.flyThrowAngle = 360f;
            item.OnFlyStartEvent += Item_OnFlyStartEvent;
            if (item.flyDirRef == null)
            {
                Damager damager = ReturnBetterDamager();
                if (damager != null)
                    item.flyDirRef = damager.transform;
                else
                    item.flyDirRef = item.transform;
                noFlyDir = true;
            }
            this.creature = creature;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            item.OnDespawnEvent += Item_OnDespawnEvent;
        }

        private Damager ReturnBetterDamager()
        {
            Damager pierceDamager = null;
            Damager slashDamager = null;
            Damager bluntDamager = null;
            foreach (CollisionHandler collisionHandler in item.collisionHandlers)
            {
                foreach (Damager damager in collisionHandler.damagers)
                {
                    if (damager.direction == Damager.Direction.Forward)
                        pierceDamager = damager;
                    if (damager.direction == Damager.Direction.ForwardAndBackward)
                        slashDamager = damager;
                    if (damager.direction == Damager.Direction.All)
                        bluntDamager = damager;
                }
            }
            if (pierceDamager != null)
                return pierceDamager;
            else if (slashDamager != null)
                return slashDamager;
            else if (bluntDamager != null)
                return bluntDamager;
            else
                return null;
        }

        private void Item_OnFlyStartEvent()
        {
            isFlying = true;
            item.physicBody.rigidBody.drag /= 4f;
            item.physicBody.rigidBody.mass /= 4f;
            item.physicBody.rigidBody.AddForce(item.physicBody.rigidBody.velocity * speedItem / 100f * item.physicBody.rigidBody.mass, ForceMode.Impulse);
            item.IgnoreRagdollCollision(creature.ragdoll);
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance.intensity > 0.25f && isFlying)
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
            item.physicBody.rigidBody.mass = massOri;
            item.physicBody.rigidBody.drag = dragOri;
            item.flyFromThrow = flyFromThrowOri;
            item.flyRotationSpeed = flyRotationSpeedOri;
            item.flyThrowAngle = flyThrowAngleOri;
            if (noFlyDir)
            {
                item.flyDirRef = null;
            }
            item.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandler_OnCollisionStartEvent;
            item.OnFlyStartEvent -= Item_OnFlyStartEvent;
            item.OnDespawnEvent -= Item_OnDespawnEvent;
            item.ResetRagdollCollision();
            Destroy(this);
        }
    }

}
