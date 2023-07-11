using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace EgyptianCartouches.Boons
{
    // Kill restore health / canibalism corpses ?

    public class AmmitBoonItem : ItemModule
    {
        public float percentHealed = 20f;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out AmmitBoon _);
        }
    }

    public class AmmitBoon : BoonBase
    {
        public static ModOptionFloat[] PercentageHealed()
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

        [ModOption(name: "% Healed on NPC killed", tooltip: "% healed when an NPC is killed", valueSourceName: nameof(PercentageHealed), defaultValueIndex = 4, saveValue = true, category = "Ammit Boon")]
        private static float perCentHealed = 20f;

        public override void Activation(bool active)
        {
            if (active)
            {
                if (activationNB == 0)
                {
                    EventManager.onCreatureKill += EventManager_onCreatureKill;
                }
                activationNB++;
            }
            else
            {
                activationNB--;
                if (activationNB == 0)
                {
                    EventManager.onCreatureKill -= EventManager_onCreatureKill;
                }
            }
        }

        public override void DebugName()
        {
            Debug.Log($"EgyptianCartouches : Name of boon : {this.GetType()}");
        }

        protected override void ManagedOnEnable()
        {
            base.ManagedOnEnable();
            Debug.Log("IN ENABLE");
        }


        private void EventManager_onCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd || creature == this.creature || this.creature == null || !IsDoneByCreature(collisionInstance, this.creature))
                return;
            this.creature.Heal((perCentHealed / 100f) * this.creature.maxHealth, this.creature);
        }

        public bool IsDoneByCreature(CollisionInstance collisionInstance, Creature creature)
        {
            if ((bool)collisionInstance.sourceColliderGroup)
            {
                if ((bool)collisionInstance.sourceColliderGroup.collisionHandler.item?.lastHandler?.creature == creature)
                {
                    return true;
                }

                if ((bool)collisionInstance.sourceColliderGroup.collisionHandler.ragdollPart?.ragdoll.creature.lastInteractionCreature == creature)
                {
                    return true;
                }
            }
            else
            {
                if ((bool)collisionInstance.casterHand?.mana.creature == creature)
                {
                    return true;
                }

                if ((bool)collisionInstance.targetColliderGroup?.collisionHandler.ragdollPart?.ragdoll.creature.lastInteractionCreature == creature)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
