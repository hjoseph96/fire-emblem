using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordsmanBattler : Battler
{
    public GameObject weaponColliderObject;
    Sword sword;

    void Awake() {
        _battlerType = "Swordsman";
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        sword = entity.equippedWeapon.GetComponent<Sword>();

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren) {
            if (child.tag == "Weapon Spawn Point") {
                sword.Spawn(child);
                sword = sword.WeaponInstance.GetComponent<Sword>();
            }
        }

        
        foreach (Transform child in sword.GetComponentsInChildren<Transform>()) {
            if (child.tag == "Damage Collider")
                weaponColliderObject = child.gameObject; 
        }
    }

    // Update is called once per frame
    void Update()
    {
        DetectHitReactions();

        if (attacking) {

            if (!reachedEnemy) {
                
                if (Vector3.Distance(transform.position, attackTarget.transform.position) < 3.2f) {
                    reachedEnemy = true;

                    SetCameraToBattler();
                } else {
                    animator.Play("Run");
            
                    transform.LookAt(attackTarget.transform, Vector3.up);
                    transform.position = Vector3.MoveTowards(transform.position, attackTarget.transform.position, moveSpeed * Time.deltaTime);
                }

                return;
            }

            
            if (!attacked) {
                weaponColliderObject.SetActive(true);
                animator.Play("Great Sword Slash");
                
                if (animator.onAnimationComplete("Great Sword Slash", .35f)) 
                {
                    weaponColliderObject.SetActive(false);
                    attacked = true;
                }

                return;
            }

            if (!turned ) {
                animator.Play("Great Sword 180 Turn");

                if(animator.onAnimationComplete("Great Sword 180 Turn", .25f))
                {
                    transform.LookAt(standTarget.transform);
                    turned = true;
                    animator.Play("Run");
                }
            } else {
                MoveToStandTarget();

                if (Vector3.Distance(transform.position, standTarget.transform.position) < 3.0f) {
                    SetToStandTarget();

                    SetCameraToDefault();

                    attacking = false;
                    completedAttack = true;
                    transform.LookAt(attackTarget.transform);
                    animator.Play("Idle");

                    OnAttackComplete.Invoke();
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
            return;
        }

        if (animator.onAnimationComplete("Hit Reaction", 0.95f)) {
            timesHit += 1;

            OnHitReactionComplete.Invoke();
        }

        if (animator.onAnimationComplete("Critical Hit Reaction", 0.95f)) {
            timesHit += 1;
            
            OnHitReactionComplete.Invoke();
        }

        if (animator.onAnimationComplete("Dodge", 0.95f))
            OnHitReactionComplete.Invoke();

        if (animator.onAnimationComplete("Critical Hit Dodge", 0.95f))
            OnHitReactionComplete.Invoke();
    }
}
