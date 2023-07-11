using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using EgyptianCartouches.Boons;

namespace EgyptianCartouches.Items
{
    public class CartouchePlayerItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out CartouchePlayer _);
        }
    }

    public class CartouchePlayer : CartoucheBase
    {
        public CartouchePlayerItem ItemModule { get; internal set; }
        protected override void ManagedOnEnable()
        {
            base.ManagedOnEnable();
            EventManager.onPossess += EventManager_onPossess;
            EventManager.onUnpossess += EventManager_onUnpossess;
            item.OnSpawnEvent += Item_OnSpawnEvent;
        }

        private void Item_OnSpawnEvent(EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd && Player.currentCreature != null)
            {
                target = Player.currentCreature;
                boonsDisplayer.SetTarget(target);
                foreach (BoonBase boonBase in boons)
                {
                    boonBase.SetCreature(target);
                }
            }
        }

        private void EventManager_onUnpossess(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                target = null;
                foreach (BoonBase boonBase in boons)
                {
                    boonBase.SetCreature(target);
                }
                boonsDisplayer.SetTarget(target);
            }
        }

        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                target = Player.currentCreature;
                boonsDisplayer.SetTarget(target);
                foreach (BoonBase boonBase in boons)
                {
                    boonBase.SetCreature(target);
                }
            }
        }

        protected override void ManagedOnDisable()
        {
            base.ManagedOnDisable();
            EventManager.onPossess -= EventManager_onPossess;
            EventManager.onUnpossess -= EventManager_onUnpossess;
            item.OnSpawnEvent -= Item_OnSpawnEvent;
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
