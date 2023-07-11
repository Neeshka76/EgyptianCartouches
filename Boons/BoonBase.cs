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
    public abstract class BoonBase : ThunderBehaviour
    {
        protected Item item;
        protected Creature creature;
        protected bool isActive = false;
        protected Holder holder;
        protected int nbOfHolder;
        public Sprite sprite;
        protected int activationNB = 0;
        protected override void ManagedOnEnable()
        {
            base.ManagedOnEnable();
            item = GetComponent<Item>();
            item.OnSnapEvent += Item_OnSnapEvent;
            item.OnUnSnapEvent += Item_OnUnSnapEvent;
            sprite = item.GetCustomReference<Image>("BoonImage").sprite;
        }

        private void Item_OnSnapEvent(Holder holder)
        {
            isActive = true;
            this.holder = holder;
        }

        private void Item_OnUnSnapEvent(Holder holder)
        {
            isActive = false;
            this.holder = null;
        }

        public abstract void DebugName();

        public void SetCreature(Creature creature)
        {
            if (creature != this.creature && creature != null && this.creature != null)
            {
                Activation(false);
            }
            if (creature != null)
            {
                this.creature = creature;
                Activation(true);
            }
            else
            {
                Activation(false);
                this.creature = creature;
            }
        }

        public abstract void Activation(bool active);

        public int ReturnHolderNumber()
        {
            if (holder != null)
            {
                return int.Parse(holder.data.highlightDefaultTitle.Substring(0, this.holder.data.highlightDefaultTitle.Length - 1));
            }
            else
            {
                return 0;
            }
        }

        public Item ReturnItem()
        {
            return item;
        }

        protected override void ManagedOnDisable()
        {
            item.OnSnapEvent -= Item_OnSnapEvent;
            item.OnUnSnapEvent -= Item_OnUnSnapEvent;
            base.ManagedOnDisable();
        }
    }
}
