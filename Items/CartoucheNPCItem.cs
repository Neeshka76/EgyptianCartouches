using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using EgyptianCartouches;
using System.Collections;

namespace EgyptianCartouches.Items
{
    public class CartoucheNPCItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out CartoucheNPC _);
        }
    }

    public class CartoucheNPC : CartoucheBase
    {
        private bool selectMode = false;
        protected override void ManagedOnEnable()
        {
            base.ManagedOnEnable();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            EventManager.onCreatureKill += EventManager_onCreatureKill;
        }

        private void EventManager_onCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (target == null || eventTime == EventTime.OnStart)
                return;
            if (target == creature)
            {
                if (selectMode)
                {
                    target = SetNewTarget(creature);
                }
                else
                {
                    target = null;
                }
                for (int i = 0; i < boons.Count; i++)
                {
                    boons[i].SetCreature(target);
                }
                boonsDisplayer.SetTarget(target);
            }
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                selectMode ^= true;
                Player.local.StartCoroutine(HapticPulse(selectMode, ragdollHand.playerHand.controlHand));
                if (selectMode)
                {
                    target = SetNewTarget();
                    if (target != null)
                        Debug.Log($"EgyptianCartouches : Cartouche NPC target: {target.name}");
                }
                else
                {
                    if (target != null)
                    {
                        target = null;
                    }
                }
                for (int i = 0; i < boons.Count; i++)
                {
                    boons[i].SetCreature(target);
                }
                boonsDisplayer.SetTarget(target);
            }
        }

        private Creature SetNewTarget(Creature creatureToExclude = null)
        {
            List<Creature> creatures = Creature.allActive;
            creatures.Remove(Player.currentCreature);
            if (creatureToExclude != null)
                creatures.Remove(creatureToExclude);
            for (int i = creatures.Count - 1; i >= 0; i--)
            {
                if (creatures[i].state == Creature.State.Dead)
                    creatures.RemoveAt(i);
            }
            if (creatures.Count == 0)
            {
                return null;
            }
            return creatures[UnityEngine.Random.Range(0, creatures.Count)];
        }

        IEnumerator HapticPulse(bool active, PlayerControl.Hand hand)
        {
            if (active)
            {
                hand.HapticShort(2.5f);
                yield return new WaitForSeconds(0.2f);
                hand.HapticShort(1.5f);
                yield break;
            }
            else
            {
                hand.HapticShort(2.5f);
                yield return new WaitForSeconds(0.4f);
                hand.HapticShort(1.5f);
                yield break;
            }
        }

        protected override void ManagedOnDisable()
        {
            base.ManagedOnDisable();
            item.OnHeldActionEvent -= Item_OnHeldActionEvent;
            EventManager.onCreatureKill -= EventManager_onCreatureKill;
        }

        protected override void CartoucheBase_Snapped(Item item)
        {
            base.CartoucheBase_Snapped(item);
        }

        protected override void CartoucheBase_UnSnapped(Item item)
        {
            base.CartoucheBase_UnSnapped(item);
        }
    }
}
