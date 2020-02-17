using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagicianBattler : Battler
{
    
    Grimiore grimiore;
    GameObject castingCircleTarget;
    GameObject projectileLaunchPoint;
    GameObject projectileTarget;

    MagicEffect projectileInstance;
    bool circleSpawned = false;
    bool dodgedAttack = false;

    void Awake() {
        _battlerType = "Magician";
    }

    // Start is called before the first frame update
    void Start()
    {
        timesHit = 0;

        animator = this.gameObject.GetComponent<Animator>();
        grimiore = entity.equippedWeapon.GetComponent<Grimiore>();
        
        foreach(Transform child in GetComponentsInChildren<Transform>()) {
            if (child.tag == "Weapon Spawn Point") {
                grimiore.Spawn(child);
                grimiore = grimiore.WeaponInstance.GetComponent<Grimiore>();
            }

            if (child.tag == "Casting Circle Target")
                castingCircleTarget   = child.gameObject;
            
            if (child.tag == "Projectile Launch Point")
                projectileLaunchPoint = child.gameObject;
        }

        OnAttackInitiated.AddListener(delegate{
            // Set Proejctile Target
            foreach (Transform child in attackTarget.GetComponentsInChildren<Transform>()) {
                if (child.tag == "Projectile Target")
                    projectileTarget = child.gameObject;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        DetectHitReactions();
                
        if (!isAlive) {
            SetToMagicCameraAngle();
            
            if (animator.onAnimationComplete("Hit Reaction", 0.9f))
                animator.Play("Death");

            if (animator.onAnimationComplete("Critical Hit Reaction", 0.9f))
                animator.Play("Death");
            

            if (animator.onAnimationComplete("Death", 0.95f)) {
                this.gameObject.SetActive(false);
               
                OnAttackComplete.Invoke();
                OnDeathComplete.Invoke();
            }
            
        }

        if (attacking && isAlive) {
            Battler opposingBattler = attackTarget.GetComponent<Battler>();
            
            GameObject enemyWeaponCollider = null;
            if (opposingBattler.BattlerType == "Swordsman")
                enemyWeaponCollider = attackTarget.GetComponent<SwordsmanBattler>().weaponColliderObject;
            
            opposingBattler.OnHitReactionComplete.AddListener(delegate{
                attacking = false;
                completedAttack = true;

                if (enemyWeaponCollider != null)
                    enemyWeaponCollider.SetActive(false);
                
                OnAttackComplete.Invoke();
            });

            opposingBattler.OnDeathComplete.AddListener(delegate{
                attacking = false;
                completedAttack = true;

                OnAttackComplete.Invoke();
            });

            if (!attacked) {
                SetToMagicCameraAngle();

                transform.LookAt(attackTarget.transform);
                animator.Play("Standing 2H Magic Attack 3");
                
                if(animator.onAnimationComplete("Standing 2H Magic Attack 3", .3f) && !circleSpawned) 
                {
                    grimiore.OnCastingCircleSpawn.AddListener(delegate {
                        circleSpawned = true;
                    });
                    
                    grimiore.SpawnCastingCircle(castingCircleTarget);
                }

                if(animator.onAnimationComplete("Standing 2H Magic Attack 3", .8f))
                {
                    grimiore.OnParticleEffectSpawn.AddListener(delegate {
                        attacked = true;
                    });

                    grimiore.OnParticleEffectHalfway.AddListener(delegate {
                        if (!reachedEnemy) {
                            grimiore.DestroyCastingCircle();

                            SetToMagicTargetCameraAngle();
                            opposingBattler.OnDodgeComplete.AddListener(delegate{
                                projectileInstance.Reset();

                                attacking = false;
                                completedAttack = true;
                                entity.HasMoved = true;

                                if (enemyWeaponCollider != null)
                                    enemyWeaponCollider.SetActive(false);
                                
                                OnAttackComplete.Invoke();
                            });
                        
                            dodgedAttack = opposingBattler.AttemptMagicDodge(projectileInstance);

                            if (enemyWeaponCollider != null)
                                enemyWeaponCollider.SetActive(true);   // Prevent bouncing off weapon's mesh...
                        }
                    });

                    grimiore.OnParticleEffectLanded.AddListener(delegate{
                        reachedEnemy = true;
                    });

                    if (grimiore.magicEffect.particleType == "Magic Projectile")
                        LaunchMagicProjectile();
                }
                return;
            }

            if (!reachedEnemy) {
                if(animator.onAnimationComplete("Standing 2H Magic Attack 3", 1f))
                    animator.Play("Idle");
            } else {
                if (!dodgedAttack)
                    SetCameraToDefault();
            }
        }   
    }

    void OnTriggerEnter(Collider collider) {
        bool dodged = HasDodged();
        
        // Normal Damage
        if (collider.tag == "Damage Collider") {
            if (dodged) {
                animator.Play("Dodge");
            } else {
                GetHit();
            }
        }

        // Critical Damage
        if (collider.tag == "Critical Damage Collider") {
            if (dodged && HasDodgedCrit()) {
                animator.Play("Critical Hit Dodge");
            } else {
                GetCriticallyHit();
            }
        }
    }

    void DetectHitReactions() {
        if (animator.onAnimationComplete("Hit Reaction", 0.95f)) {
            timesHit += 1;
            OnHitReactionComplete.Invoke();
        }

        if (animator.onAnimationComplete("Critical Hit Reaction", 0.95f)) {
            timesHit += 1;
            OnHitReactionComplete.Invoke();
        }

        if (animator.onAnimationComplete("Dodge", 0.95f)) {
            OnHitReactionComplete.Invoke();
        }

        if (animator.onAnimationComplete("Critical Hit Dodge", 0.95f)) {
            OnHitReactionComplete.Invoke();
        }

    }

    void LaunchMagicProjectile() {
        MagicProjectile projectile = grimiore.magicEffect.GetComponent<MagicProjectile>();
        
        projectileInstance = projectile.Launch(
            grimiore,
            projectileLaunchPoint.transform.position,
            projectileTarget.transform
        );
    }
}
