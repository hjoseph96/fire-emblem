using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherBattler : Battler
{
    Bow bow;
    Transform arrowSpawnPoint;
    Transform projectileTarget;
    Transform projectileLaunchPoint;

    Arrow arrowInstance;
    bool targetsLocked= false;
    bool windingUp = false;
    bool spawnedArrow = false;
    bool firedArrow = false;
    bool dodgedArrow = false;

    void Awake() {
        _battlerType = "Archer";
        transform.position = new Vector3(
            transform.position.x,
            0.91f,
            transform.position.z
        );
    }

    // Start is called before the first frame update
    void Start()
    {
        timesHit = 0;

        animator = this.gameObject.GetComponent<Animator>();
        bow = entity.equippedWeapon.GetComponent<Bow>();
        
        foreach(Transform child in GetComponentsInChildren<Transform>()) {
            if (child.tag == "Weapon Spawn Point") {
                bow.Spawn(child);
                bow = bow.WeaponInstance.GetComponent<Bow>();
            }

            if (child.tag == "Arrow Spawn Point")
                arrowSpawnPoint = child;

            if (child.tag == "Projectile Launch Point")
                projectileLaunchPoint = child;
        }

        OnAttackInitiated.AddListener(delegate{
            // Set Projectile Target
            foreach (Transform child in attackTarget.GetComponentsInChildren<Transform>()) {
                if (child.tag == "Projectile Target")
                    projectileTarget = child;
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
            if (!targetsLocked) {
                foreach (Transform child in opposingBattler.GetComponentsInChildren<Transform>()) 
                {
                    if (child.tag == "Projectile Target") {
                        projectileTarget = child;
                        targetsLocked = true;
                    }
                }
            }    

            opposingBattler.OnHitReactionComplete.AddListener(delegate{
                attacking = false;
                completedAttack = true;
                // enemyWeaponCollider.SetActive(false);
                
                OnAttackComplete.Invoke();
            });

            opposingBattler.OnDeathComplete.AddListener(delegate{
                attacking = false;
                completedAttack = true;

                OnAttackComplete.Invoke();
            });

            if (!attacked) {
                mainCamera.transform.position = opposingBattler.magicTargetCameraAngle.transform.position;
                mainCamera.transform.rotation = opposingBattler.magicTargetCameraAngle.transform.rotation;

                if (!windingUp) {
                    windingUp = true;

                    bool isAttackCritical = AttackIsCritical();
                    transform.LookAt(attackTarget.transform);

                    if (isAttackCritical) {
                        animator.Play("Critical Attack");
                    } else {
                        animator.Play("Attack");
                    }    
                }

        
                if (animator.onAnimationComplete("Attack", 0.19f)) {
                    if (!spawnedArrow){
                        arrowInstance = bow.SpawnArrow(arrowSpawnPoint);
                        spawnedArrow = true;
                    }
                }
                    
                if (animator.onAnimationComplete("Attack", 0.7f)) {
                    arrowInstance.OnArrowFired.AddListener(delegate{
                        attacked = true;
                    });

                    arrowInstance.HalfwayToTarget.AddListener(delegate{
                        SetToMagicTargetCameraAngle();
                        opposingBattler.OnDodgeComplete.AddListener(delegate{
                            arrowInstance.Reset();

                            attacking = false;
                            completedAttack = true;
                            
                            OnAttackComplete.Invoke();
                        });

                        // dodgedArrow = opposingBattler.AttemptDodgeArrow(arrowInstance);
                    });

                    arrowInstance.ReachedTarget.AddListener(delegate{
                        reachedEnemy = true;
                    });

                    if (!firedArrow) {
                        arrowInstance.Shoot(projectileTarget, projectileLaunchPoint);
                        firedArrow = true;
                    }
                }

                if (animator.onAnimationComplete("Critical Attack", 0.3f)) {
                    if (!spawnedArrow) {
                        arrowInstance = bow.SpawnArrow(arrowSpawnPoint);
                        spawnedArrow = true;
                    }
                }

                if (animator.onAnimationComplete("Shoot Arrow", 0.3f)) {
                    arrowInstance.OnArrowFired.AddListener(delegate{
                        attacked = true;
                    });

                    arrowInstance.HalfwayToTarget.AddListener(delegate{
                        SetToMagicTargetCameraAngle();
                        opposingBattler.OnDodgeComplete.AddListener(delegate{
                            arrowInstance.Reset();

                            attacking = false;
                            completedAttack = true;
                            
                            OnAttackComplete.Invoke();
                        });

                        // dodgedArrow = opposingBattler.AttemptDodgeArrow(arrowInstance);
                    });

                    arrowInstance.ReachedTarget.AddListener(delegate{
                        reachedEnemy = true;
                    });

                    if (!firedArrow) {
                        arrowInstance.Shoot(projectileTarget, projectileLaunchPoint);
                        firedArrow = true;
                    }
                }

                return;
            }

            if (!reachedEnemy) {
                bool finishedAttacking = 
                    animator.onAnimationComplete("Attack", 1f) ||
                    animator.onAnimationComplete("Shoot Arrow", 1f); 
                
                if (finishedAttacking)
                    animator.Play("Idle");
            } else {
                if (!dodgedArrow) {
                    SetCameraToDefault();
                }
            }
        }   
    }

     void OnTriggerEnter(Collider collider) {
        bool dodged = HasDodged();
     
        if (collider.tag == "Damage Collider") {
            if (dodged) {
                animator.Play("Dodge");
            } else {
                GetHit();
            }
        }

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
}
