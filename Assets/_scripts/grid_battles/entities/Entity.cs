using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

using TGS;

public class Entity : MonoBehaviour
{
    public string EntityType {
        get { return entityType; }
    }
    protected string entityType;

    public int level;

    public bool canLead = false;
    public bool canFly = false;
    public int healthStat;
    public int strengthStat;
    public int speedStat;
    public int skillStat;
    public int magicStat;
    public int resistanceStat;
    public int defenseStat;
    public int luckStat;
    public int followupCritMultiplierStat;
    public int constitutionStat;
    public int moveRadius;
    public int leadershipStat;
    public int biorhythmStat;
    

    public string axeRank;
    public string swordRank;
    public string lanceRank;
    public string staffRank;
    public string grimioreRank;
    
    
    public Dictionary<string, int> WEAPON_RANKS = new Dictionary<string, int>();
    public Dictionary<string, Stat> BASE_STATS = new Dictionary<string, Stat>();
    public Dictionary<string, DependantStat> DERIVED_STATS = new Dictionary<string, DependantStat>();
    public List<Weapon> carriedWeapons;
    public Weapon equippedWeapon;

    public int CurrentHealthPoints {
        get { return healthPoints; }
        set {
            if (value <= 0) {
                healthPoints = 0;
                healthBar.SetValueCurrent(0);
                
                OnDeathEvent.Invoke();

                Destroy(this.gameObject);
            } else {
                healthPoints = value;
                healthBar.SetValueCurrent(value);
                
                HealthChanged.Invoke();
            }
        }
    }
    protected int healthPoints;
    public bool HasMoved {
        get { return _hasMoved; }
        set { 
            if (entityType == "player" && value == true)
                print("debug");

            _hasMoved = value;
        }
    }

    protected bool _hasMoved = false;

    public UnityEvent HealthChanged;
    public UnityEvent OnDeathEvent;
    public UnityEvent OnReachedDestination;

    public int currentCellIndex;

    public bool hasAttacked = false;
    public bool isMoving = false;
    public bool turnToMove = false;

    public float moveSpeed;
    public float rightRotation = 0;
    public float downRotation = 90;
    public float leftRotation = 180;
    public float upRotation = 270;

    public List<Vector3> MovePath {
        get { return movePath;}
        set {
            if (!isMoving) 
                movePath = value;
        }
    }

    protected int cellIndex = 0;
    protected TerrainGridSystem tgs;
    protected Animator animator;
    protected EnergyBar healthBar;
    protected bool rotationSet = false;
    protected List<Vector3> movePath = new List<Vector3>();

    protected virtual void Awake() {        
        BASE_STATS["HEALTH"]        = new Stat(healthStat);
        BASE_STATS["STRENGTH"]      = new Stat(strengthStat);
        BASE_STATS["SPEED"]         = new Stat(speedStat);
        BASE_STATS["SKILL"]         = new Stat(skillStat);
        BASE_STATS["MAGIC"]         = new Stat(magicStat);
        BASE_STATS["LUCK"]          = new Stat(luckStat);
        BASE_STATS["RESISTANCE"]    = new Stat(resistanceStat);
        BASE_STATS["DEFENSE"]       = new Stat(defenseStat);
        BASE_STATS["FOLLOW_UP"]     = new Stat(followupCritMultiplierStat);
        BASE_STATS["CONSTITUTION"]  = new Stat(constitutionStat);
        BASE_STATS["MOVE"]          = new Stat(moveRadius);
        BASE_STATS["BIORHYTHM"]     = new Stat(biorhythmStat);
        
        if (canLead)
            BASE_STATS["LEAD"] = new Stat(leadershipStat);
        
        if (axeRank != "")
            WEAPON_RANKS["AXE"]      = Weapon.RANKS[axeRank];
        if (swordRank != "")
            WEAPON_RANKS["SWORD"]    = Weapon.RANKS[swordRank];    
        if (lanceRank != "")
            WEAPON_RANKS["LANCE"]    = Weapon.RANKS[lanceRank];
        if (grimioreRank != "")
            WEAPON_RANKS["GRIMIORE"] = Weapon.RANKS[grimioreRank];
        if (staffRank != "")
            WEAPON_RANKS["STAFF"]    = Weapon.RANKS[staffRank];

        equippedWeapon.SetStats();
        currentCellIndex = TGSInterface.CellAtPosition(transform.position).index;
    }

    public bool CanUseSwords() {
        return WEAPON_RANKS.Keys.Contains("SWORD");
    }

    public bool CanUseAxes() {
        return WEAPON_RANKS.Keys.Contains("AXE");
    }

    public bool CanUseLances() {
        return WEAPON_RANKS.Keys.Contains("LANCE");
    }

    public bool CanUseGrimiores() {
        return WEAPON_RANKS.Keys.Contains("GRIMIORE");
    }

    public bool CanUseStaffs() {
        return WEAPON_RANKS.Keys.Contains("STAFF");
    }

    public int AttackDamage() {
        int weaponMight = equippedWeapon.STATS["MIGHT"].CalculateValue();
        
        if (equippedWeapon.WeaponType == "GRIMIORE") {
            int magicStat = BASE_STATS["MAGIC"].CalculateValue();
            
            return magicStat + weaponMight;
        } else {
            int strengthStat = BASE_STATS["STRENGTH"].CalculateValue();

            return strengthStat + weaponMight;
        }
    }

    public int AttackSpeed() {
        int weaponWeight = equippedWeapon.STATS["WEIGHT"].CalculateValue();
        int strengthStat = BASE_STATS["STRENGTH"].CalculateValue();
        int burden = weaponWeight - strengthStat;

        if (burden < 0)
            burden = 0;
        
        return BASE_STATS["SPEED"].CalculateValue() - burden;
    }

    public Dictionary<string, int> PreviewAttack(Entity defender) {
        Dictionary<string, int> battleStats = new Dictionary<string, int>();

        int atkDmg;
        if (equippedWeapon.WeaponType == "GRIMIORE") {
            atkDmg = AttackDamage() - defender.BASE_STATS["RESISTANCE"].CalculateValue();
        } else {
            atkDmg = AttackDamage() - defender.BASE_STATS["DEFENSE"].CalculateValue();
        }

        battleStats["ATK_DMG"] = atkDmg;
        battleStats["ACCURACY"] = Accuracy(defender);
        battleStats["CRIT_RATE"] = CriticalHitRate(defender);
        battleStats["CRIT_MULTIPLIER"] = equippedWeapon.STATS["CRIT_MULTIPLIER"].CalculateValue();
        return battleStats;
    }

    public bool CanDoubleAttack(Entity target) {
        int minDoubleAttackBuffer = 5;

        if ((this.AttackSpeed() - target.AttackSpeed()) > minDoubleAttackBuffer)
            return true;

        return false;
    }

    public int CriticalHitRate(Entity target) {
        int skill = BASE_STATS["SKILL"].CalculateValue();
        int weaponCritChance = equippedWeapon.STATS["CRIT_RATE"].CalculateValue();

        int critRate = ((skill / 2) + weaponCritChance) - target.BASE_STATS["LUCK"].CalculateValue();

        // if (WEAPON_RANKS[equippedWeapon.WeaponType] == Weapon.RANKS["S"])
        //     critRate += 5;

        return critRate;
    }

    public int DodgeChance() {
        int luckStat = BASE_STATS["LUCK"].CalculateValue();
        int biorhythmStat = BASE_STATS["BIORHYTHM"].CalculateValue();

        return (AttackSpeed() * 2) + luckStat + biorhythmStat;
    }

    public int HitRate(int cellIndex) {
        int boxDistance = tgs.CellGetBoxDistance(currentCellIndex, cellIndex);
        int luckStat = BASE_STATS["LUCK"].CalculateValue();
        int skillStat = BASE_STATS["SKILL"].CalculateValue();
        int biorhythmStat = BASE_STATS["BIORHYTHM"].CalculateValue();
        int weaponHitStat = equippedWeapon.STATS["HIT"].CalculateValue();

        int hitRate = (skillStat * 2) + luckStat + biorhythmStat + weaponHitStat;
        if (boxDistance >= 2)
            hitRate -= 15;
        
        return hitRate;
    }

    public int Accuracy(Entity target) {
        int boxDistance = tgs.CellGetBoxDistance(currentCellIndex, target.currentCellIndex);

        if (equippedWeapon.WeaponType != "STAFF") {
            // Add weapon W△ Weapon triangle effects
            return HitRate(target.currentCellIndex) - target.DodgeChance();
        } else {
            int magicStat = BASE_STATS["MAGIC"].CalculateValue();
            int resistanceStat = BASE_STATS["RESISTANCE"].CalculateValue();
            int skillStat = BASE_STATS["SKILL"].CalculateValue();
            
            return (magicStat - resistanceStat) + skillStat + 30 - (boxDistance * 2);
        }
    }

    public void LookAtCell(Bounds cellBounds) {
        Quaternion newRotation = Quaternion.Euler(transform.eulerAngles.x, directionToLook(cellBounds.center), transform.eulerAngles.z);
        transform.rotation = newRotation;
    }
    public void DestroyHealthBar() {
        Destroy(healthBar.gameObject);
    }
    public void DisableHealthBar() {
        healthBar.gameObject.SetActive(false);
    }

    public void EnableHealthBar() {
        healthBar.gameObject.SetActive(true);
    }

    public void SetHealthBar(EnergyBar bar) {
        healthBar = bar;
        healthBar.valueMax = BASE_STATS["HEALTH"].CalculateValue();
        healthBar.SetValueCurrent(healthPoints);
    }


    public List<int> AttackRange() {
        List<int> attackRange = new List<int>();

        int maxRange = equippedWeapon.STATS["MAX_RANGE"].CalculateValue();
        attackRange = tgs.CellGetNeighbours(currentCellIndex, maxRange);

        // If equipped weapon cannot be used at close range...
        if (equippedWeapon.STATS["MIN_RANGE"].CalculateValue() > 1) {
            List<Cell> neighbors = tgs.CellGetNeighbours(currentCellIndex);
            foreach(Cell unattackableCell in neighbors)
                attackRange.Remove(unattackableCell.index);
        }
        
        return attackRange;
    }

    public List<int> AttackRange(int cellIndex) {
        List<int> attackRange = new List<int>();
        Cell givenCell = tgs.cells[cellIndex];

        int maxRange = equippedWeapon.STATS["MAX_RANGE"].CalculateValue();
        attackRange = tgs.CellGetNeighbours(givenCell, maxRange);

        // If cannot use equipped weapon at close range
        if (equippedWeapon.STATS["MIN_RANGE"].CalculateValue() > 1) {
            List<Cell> neighbors = tgs.CellGetNeighbours(cellIndex);
            foreach(Cell unattackableCell in neighbors)
                attackRange.Remove(unattackableCell.index);
        }
        
        return attackRange;
    }

    public bool WithinAttackRange(int cellIndex) {
        return AttackRange().Contains(cellIndex);
    }

    protected void TraverseCells() {
        isMoving = true;
        
        animator.Play("Run");

        var cellCenter = movePath[cellIndex];
        if (!rotationSet) {
            Quaternion newRotation = Quaternion.Euler(transform.eulerAngles.x, directionToLook(cellCenter), transform.eulerAngles.z);
            transform.rotation = newRotation;
            rotationSet = true;
        }

        Vector3 currentPosition = this.transform.position;

        if(Vector3.Distance(currentPosition, cellCenter) > .1f) {
            string direction = currentlyLooking();
            float step =  moveSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, cellCenter, step);
        } else {
            rotationSet = false;
            
            if (cellIndex < movePath.Count - 1) {
                cellIndex += 1;
            } else {
                animator.Play("Idle");
                
                cellIndex = 0;
                movePath = new List<Vector3>();
                transform.position = Vector3.Lerp (transform.position, cellCenter, 0.5f);

                Cell occupyingCell = TGSInterface.CellAtPosition(cellCenter);
                currentCellIndex = occupyingCell.index;
                
                int mask = TGSInterface.CELL_MASKS[EntityType.ToUpper()];
                tgs.CellSetGroup(occupyingCell.index, mask);

                isMoving = false;
                OnReachedDestination.Invoke();
            }
        }

    }

    protected float directionToLook(Vector3 targetPosition) {
        if (transform.position.x < targetPosition.x) {
            if (currentlyLooking() == "Down") {
                return transform.eulerAngles.y;
            } else { 
                return MapLookDirection("Down");
            } 
        }

        if (transform.position.x > targetPosition.x) {
            if (currentlyLooking() == "Up") {
                return transform.eulerAngles.y;
            } else { 
                return MapLookDirection("Up");
            } 
        }
        
        if (transform.position.z < targetPosition.z) {
            if (currentlyLooking() == "Right") {
                return transform.eulerAngles.y;
            } else { 
                return MapLookDirection("Right");
            } 
        }
        
        if (transform.position.z > targetPosition.z) {
            if (currentlyLooking() == "Left") {
                return transform.eulerAngles.y;
            } else { 
                return MapLookDirection("Left");
            } 
        }

        throw new System.Exception();
    }

    protected float MapLookDirection(string direction) {
        switch(direction) {
            case "Right"  :
                return rightRotation;
            case "Left"  :
                return leftRotation;
            case "Down"  :
                return downRotation;
            case "Up"  :
                return upRotation;
        }
        
        throw new System.Exception();
    }

    protected string currentlyLooking() {
        if (transform.eulerAngles.y == rightRotation)
            return "Right";
        
        if (transform.eulerAngles.y == leftRotation)
            return "Left";

        if (transform.eulerAngles.y == upRotation)
            return "Up";

        if (transform.eulerAngles.y == downRotation)
            return "Down";
        
        return "Unknown";
    }
}
