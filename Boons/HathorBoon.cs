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
    // Regen Life
    public class HathorBoonItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out HathorBoon _);

        }
    }

    public class HathorBoon : BoonBase
    {
        public static ModOptionFloat[] Percentages()
        {
            List<ModOptionFloat> options = new List<ModOptionFloat>();
            float val = 0f;
            while (val <= 100f)
            {
                options.Add(new ModOptionFloat(val.ToString("0"), val));
                val += 2f;
            }
            return options.ToArray();
        }
        [ModOption(name: "% Passive Health Regen", tooltip: "Passive Health Regen % per seconds", valueSourceName: nameof(Percentages), defaultValueIndex = 2, order = 1, saveValue = true, category = "Hathor Boon")]
        private static float healthRegenPercentPerSec = 4f;
        [ModOption(name: "% Passive Mana Regen", tooltip: "Passive Mana Regen % per seconds", valueSourceName: nameof(Percentages), defaultValueIndex = 1, order = 2, saveValue = true, category = "Hathor Boon")]
        private static float manaRegenPercentPerSec = 2f;
        [ModOption(name: "% Passive Focus Regen", tooltip: "Passive Focus Regen % per seconds", valueSourceName: nameof(Percentages), defaultValueIndex = 2, order = 3, saveValue = true, category = "Hathor Boon")]
        private static float focusRegenPercentPerSec = 4f;
        
        public override void Activation(bool active)
        {
            if (active)
            {
                if (activationNB == 0)
                {
                    StartCoroutine(HathorRegen());
                }
                activationNB++;
            }
            else
            {
                activationNB--;
                if (activationNB == 0)
                {
                    StopCoroutine(HathorRegen());
                }
            }
        }

        private IEnumerator HathorRegen()
        {
            while (true)
            {
                if (creature != null)
                {
                    if (creature.currentHealth < creature.maxHealth)
                        creature.currentHealth += (healthRegenPercentPerSec / 100f) * creature.maxHealth * Time.deltaTime;
                    if (creature.currentHealth > creature.maxHealth)
                        creature.currentHealth = creature.maxHealth;
                    if (creature.mana.currentMana < creature.mana.maxMana)
                        creature.mana.currentMana += (manaRegenPercentPerSec / 100f) * creature.mana.maxMana * Time.deltaTime;
                    if (creature.mana.currentMana > creature.mana.maxMana)
                        creature.mana.currentMana = creature.mana.maxMana;
                    if (creature.isPlayer)
                    {
                        if (creature.mana.currentFocus < creature.mana.maxFocus)
                            creature.mana.currentFocus += (focusRegenPercentPerSec / 100f) * creature.mana.maxFocus * Time.deltaTime;
                        if (creature.mana.currentFocus > creature.mana.maxFocus)
                            creature.mana.currentFocus = creature.mana.maxFocus;
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
