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
    public abstract class CartoucheBase : ThunderBehaviour
    {
        protected Item item;
        protected List<BoonBase> boons;
        protected Creature target;
        protected BoonsDisplayer boonsDisplayer;
        protected override void ManagedOnEnable()
        {
            base.ManagedOnEnable();
            item = GetComponent<Item>();
            for (int i = 0; i < item.childHolders.Count; i++)
            {
                item.childHolders[i].Snapped += CartoucheBase_Snapped;
                item.childHolders[i].UnSnapped += CartoucheBase_UnSnapped;
            }
            boons = new List<BoonBase>();
            item.gameObject.TryGetOrAddComponent(out BoonsDisplayer boonsDisplayer);
            this.boonsDisplayer = boonsDisplayer;
        }

        protected virtual void CartoucheBase_Snapped(Item item)
        {
            BoonBase boonBase = item.GetComponent<BoonBase>();
            if (boonBase == null)
                return;
            boons.Add(boonBase);
            boonsDisplayer.AddBoon(boonBase);
            if (target == null)
                return;
            boonBase.SetCreature(target);
        }

        protected virtual void CartoucheBase_UnSnapped(Item item)
        {
            BoonBase boonBase = item.GetComponent<BoonBase>();
            if (boonBase == null)
                return;
            if (target != null)
                boonBase.SetCreature(null);
            if (boons.Contains(boonBase))
            {
                boons.Remove(boonBase);
                boonsDisplayer.RemoveBoon(boonBase);
            }
        }
        protected override void ManagedOnDisable()
        {
            base.ManagedOnDisable();
            for (int i = 0; i < item.childHolders.Count; i++)
            {
                item.childHolders[i].Snapped -= CartoucheBase_Snapped;
                item.childHolders[i].UnSnapped -= CartoucheBase_UnSnapped;
            }
            if (boonsDisplayer != null)
            {
                boonsDisplayer.Dispose();
            }
        }

        public List<BoonBase> ReturnBoonList()
        {
            return boons;
        }

        public List<Item> ReturnItems()
        {
            List<Item> item = new List<Item>();
            foreach(BoonBase boonBase in boons)
                item.Add(boonBase.ReturnItem());
            return item;
        }

    }
}
