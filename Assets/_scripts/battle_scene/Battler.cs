using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// Calculate Critical Attack on the attacker:
// Roll the dice
// Set Damage Colldier for nomal
// Set Critical Damage Collider for Criticals
// OnTriggerEnter, use differing tags to discern behavior

public class Battler : MonoBehaviour
{
    public Entity entity;
    public Weapon weapon;
    public int HealthPoints {
        get { return entity.CurrentHealthPoints; }
        set {
            if (value <= 0) {
                entity.CurrentHealthPoints = 0;

                isAlive = false;
            } else {
                entity.CurrentHealthPoints = value;
                isAlive = true;
            }

            HealthChanged.Invoke();
        }
    }

    protected string _battlerType;
    public string BattlerType {
        get { return _battlerType; }
    }
    public GameObject standTarget;
    public Camera mainCamera;
    public GameObject magicCameraAngle;
    public GameObject battlerCameraAngle;
    public GameObject defaultCameraAngle;
    public GameObject magicDodgeCameraAngle;
    public GameObject magicTargetCameraAngle;
    public float moveSpeed = 6.0f;    
    public Dictionary<string, int> defencePreview;
    public Dictionary<string, int> attackPreview;

    
    public UnityEvent HealthChanged;

    public UnityEvent OnDodgeComplete;
    public UnityEvent OnHitReactionComplete;
    public UnityEvent OnAttackInitiated;
    public UnityEvent OnAttackComplete;
    public UnityEvent OnDeathComplete;

    protected Battler attackTarget;
    protected Animator animator;
    protected bool completedAttack = false;
    protected bool reachedEnemy = false;
    protected bool attacking = false;
    protected bool attacked = false;
    protected bool turned = false;
    protected bool isAlive = true;
    protected bool dodgedMagic = false;
    protected MagicEffect _effectToDodge;

    public int TimesHit {
        get { return timesHit; }
    }
    protected int timesHit;

    public void Attack(Battler target) {
        if (isAlive && entity.WithinAttackRange(target.entity.currentCellIndex)) {
            attackTarget = target;
            attacking = true;

            SetCameraToDefault();

            OnAttackInitiated.Invoke();
        } else {
            attacking = false;
            completedAttack = false;

            OnAttackComplete.Invoke();
        }
    }

    public bool AttemptMagicDodge(MagicEffect particleEffect) {
        dodgedMagic = HasDodged();
    
        if (dodgedMagic) {
            if (particleEffect.particleType == "Magic Projectile") {
                DodgeMagicProjectile();
            }

            StartCoroutine("DodgeDelay");
        }

        return dodgedMagic;
    }
    
    IEnumerator DodgeDelay() {
        yield return new WaitForSecondsRealtime(2f);
        OnDodgeComplete.Invoke();
    }

    void DodgeMagicProjectile() {
        SetToMagicDodgeCameraAngle();
        animator.Play("Corkscrew Evade");
    }

    
    protected bool AttackIsCritical() {
        System.Random R = new System.Random();
        int C = R.Next(1, 101);
        if (C <= defencePreview["CRIT_RATE"]) return true;

        return false;
    }
    protected bool HasDodged() {
        System.Random R = new System.Random();
        int C = R.Next(1, 101);
        if (C >= defencePreview["ACCURACY"]) return true;

        return false;
    }

    protected bool HasDodgedCrit() {
        System.Random R = new System.Random();
        int C = R.Next(1, 101);
        if (C <= 35) return true;

        return false;
    }


    protected void GetHit() {
        animator.Play("Hit Reaction");

        HealthPoints -= defencePreview["ATK_DMG"];
    }

    protected void GetCriticallyHit() {
        animator.Play("Critical Hit Reaction");

        HealthPoints -= (defencePreview["ATK_DMG"] * defencePreview["CRIT_MULTIPLIER"]); 
    }

    protected void MoveToStandTarget() {
        float step =  moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, standTarget.transform.position, step);
    }

    protected void SetToStandTarget() {
        transform.position = standTarget.transform.position;
        transform.rotation = standTarget.transform.rotation;
    }

    protected void SetCameraToDefault() {
        // Set Camera Angle
        mainCamera.transform.position = defaultCameraAngle.transform.position;
        mainCamera.transform.rotation = defaultCameraAngle.transform.rotation;
    }

    protected void SetCameraToBattler() {
        // Set Camera Angle
        mainCamera.transform.position = battlerCameraAngle.transform.position;
        mainCamera.transform.rotation = battlerCameraAngle.transform.rotation;
    }

    protected void SetToMagicCameraAngle() {
        mainCamera.transform.position = magicCameraAngle.transform.position;
        mainCamera.transform.rotation = magicCameraAngle.transform.rotation;
    }
    
    protected void SetToMagicTargetCameraAngle() {
        mainCamera.transform.position = magicTargetCameraAngle.transform.position;
        mainCamera.transform.rotation = magicTargetCameraAngle.transform.rotation;
    }

    protected void SetToMagicDodgeCameraAngle() {
        mainCamera.transform.position = magicDodgeCameraAngle.transform.position;
        mainCamera.transform.rotation = magicDodgeCameraAngle.transform.rotation;
    }
}
