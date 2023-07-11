using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace EgyptianCartouches.Boons
{
    // Armor
    public class SobekBoonItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out SobekBoon _);
        }
    }

    public class SobekBoon : BoonBase
    {
        public static int nbMode;
        public override void Activation(bool active)
        {
            if (active)
            {
                if (nbMode == 0)
                {
                    LeatherSkin(true);
                }
                nbMode++;
            }
            else
            {
                nbMode--;
                if (nbMode == 0)
                {
                    LeatherSkin(false);
                }
            }
        }

        private void LeatherSkin(bool active)
        {
            if (active)
            {
                EventManager.onCreatureHit += EventManager_onCreatureHit;
            }
            else
            {
                EventManager.onCreatureHit -= EventManager_onCreatureHit;
            }
        }

        private void EventManager_onCreatureHit(Creature creature, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart && creature.isPlayer)
            {
                collisionInstance.damageStruct.damage /= 2;
            }
        }

        public override void DebugName()
        {
            Debug.Log($"EgyptianCartouches : Name of boon : {this.GetType()}");
        }
    }
}
