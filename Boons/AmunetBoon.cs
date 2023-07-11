using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace EgyptianCartouches.Boons
{
    // Throw knife on parry
    public class AmunetBoonItem : ItemModule
    {
        public List<string> projectileAddress;
        public List<string> bigProjectileAddress;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.TryGetOrAddComponent(out AmunetBoon amunetBoon);
            amunetBoon.Init(projectileAddress, bigProjectileAddress);
        }
    }

    public class AmunetBoon : BoonBase
    {
        private List<string> projectileAddress;
        private List<string> bigProjectileAddress;
        public static ModOptionFloat[] ChancesOfBigProjectile()
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
        [ModOption(name: "Spawning Big Projectile chances %", tooltip: "Chance to spawn a big projectile instead of a small projectile", valueSourceName: nameof(ChancesOfBigProjectile), defaultValueIndex = 2, saveValue = true, category = "Ammunet Boon")]
        private static float chancesOfBigProjectile = 10f;
        public void Init(List<string> projectileAddress, List<string> bigProjectileAddress)
        {
            this.projectileAddress = projectileAddress;
            this.bigProjectileAddress = bigProjectileAddress;
        }
        public override void Activation(bool active)
        {
            if (active)
            {
                if (activationNB == 0)
                {
                    EventManager.onCreatureParry += EventManager_onCreatureParry;
                }
                activationNB++;
            }
            else
            {
                activationNB--;
                if (activationNB == 0)
                {
                    EventManager.onCreatureParry -= EventManager_onCreatureParry;
                }
            }
        }

        private void EventManager_onCreatureParry(Creature creature, CollisionInstance collisionInstance)
        {
            if (IsDoneByCreature(collisionInstance, creature) && collisionInstance.intensity > 0.1f)
            {
                if (collisionInstance.targetColliderGroup.collisionHandler.isItem)
                {
                    if (collisionInstance.sourceColliderGroup.collisionHandler.item?.mainHandler?.creature is Creature target && collisionInstance.targetColliderGroup.collisionHandler.item?.mainHandler?.creature is Creature target1)
                    {
                        if (target != this.creature && target1 == this.creature)
                            InitProjectile(target);
                    }
                }
            }
        }

        public bool IsDoneByCreature(CollisionInstance collisionInstance, Creature creature)
        {
            if (collisionInstance.sourceColliderGroup)
            {
                if (collisionInstance.sourceColliderGroup.collisionHandler.item?.lastHandler?.creature == creature)
                {
                    return true;
                }

                if (collisionInstance.sourceColliderGroup.collisionHandler.ragdollPart?.ragdoll.creature.lastInteractionCreature == creature)
                {
                    return true;
                }
            }
            else
            {
                if (collisionInstance.casterHand?.mana.creature == creature)
                {
                    return true;
                }

                if (collisionInstance.targetColliderGroup?.collisionHandler.ragdollPart?.ragdoll.creature.lastInteractionCreature == creature)
                {
                    return true;
                }
            }
            return false;
        }

        private void InitProjectile(Creature target)
        {
            if (chancesOfBigProjectile <= UnityEngine.Random.Range(0f, 100f))
            {
                ThrowProjectile(target, projectileAddress[UnityEngine.Random.Range(0, projectileAddress.Count)], false);
            }
            else
            {
                ThrowProjectile(target, bigProjectileAddress[UnityEngine.Random.Range(0, bigProjectileAddress.Count)], true);
            }
        }

        private void ThrowProjectile(Creature target, string Id, bool isBigProjectile)
        {
            Catalog.GetData<ItemData>(Id).SpawnAsync(projectile =>
            {
                projectile.transform.position = target.transform.position + (target.transform.position - creature.transform.position).normalized;
                projectile.transform.position = RandomPositionAroundAPointWithADirection(projectile.transform.position, (creature.transform.position - target.transform.position).normalized, Vector3.up, 0.75f) + Vector3.up * (isBigProjectile ? 2f : 1f);
                Vector3 targetToHit = target.GetRandomRagdollPart(0b00000000000101).transform.position;
                projectile.transform.rotation = Quaternion.LookRotation((targetToHit - projectile.transform.position).normalized, Vector3.up);
                projectile.physicBody.rigidBody.AddForce((targetToHit - projectile.transform.position).normalized * 10f, ForceMode.VelocityChange);
                projectile.Throw();
                if (!isBigProjectile)
                    projectile.transform.localScale = Vector3.one * 0.75f;
            }, target.transform.position + (target.transform.position - creature.transform.position).normalized, Quaternion.identity);
        }

        private Vector3 RandomPositionAroundAPointWithADirection(Vector3 position, Vector3 forwardDirection, Vector3 upDirection, float radiusMax)
        {
            return position + Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), forwardDirection) * upDirection * UnityEngine.Random.Range(0f, radiusMax);
        }

        public override void DebugName()
        {
            Debug.Log($"EgyptianCartouches : Name of boon : {this.GetType()}");
        }
    }
}
