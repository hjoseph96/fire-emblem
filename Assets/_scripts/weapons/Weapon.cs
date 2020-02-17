using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// In Fire Emblem: Three Houses, 
// for every 5 points of strength a character has, 
// the unit ignores 1 point of weight. 
// The resulting value is then subtracted from the unit’s speed 
// to get Attack Speed. 

// Steel is actually less dense than iron, and weighs less. 
// However, the steel weapons in-game are actually heavier than iron ones, 
// and are the heaviest of all generic weapons types.

// weapon experience is gained whenever a unit attacks or counterattacks. 
// Unless a unit is recruited with some weapon experience beforehand, 
// by default all weapon experience begins at an empty E rank.


// WEAPON EXP: https://fireemblem.fandom.com/wiki/Weapon_Experience
public class Weapon : MonoBehaviour
{
    // OPTS: SWORD, AXE, LANCE, BOW, GRIMIORE, STAFF
    public string WeaponType {
        get { return _weaponType; }
    }
    protected string _weaponType;
    public string DamageType {
        get { return _damageType; }
    }
    protected string _damageType;
    
    public int mightStat;
    public int criticalRateStat;
    public int hitPercentageStat;
    public int maxUsageStat;
    public int weightStat;
    public int minRangeStat;
    public int maxRangeStat;

    public int buyPrice;
    public int costPerUse;
    public int usesLeft;
    public List<Dictionary<string, int>> effectiveAgainst;
    public Dictionary<string, Stat> STATS;

    public static Dictionary<string, int> RANKS = new Dictionary<string, int>{
        { "E", 0 }, { "D", 1 }, { "C", 2 }, 
        { "B", 3 }, { "A", 4 }, { "S", 5 }
    };

    static Dictionary<string, int> defaultWeaponRanking = new Dictionary<string, int>{ 
        { "E", 20 },
        { "D", 30 },
        { "C", 45 },
        { "B", 65 },
        { "A", 90 }
    };
    public static Dictionary<string, Dictionary<string, int>> EXP_TO_LVL_WEAPON = new Dictionary<string, Dictionary<string, int>>{
        { 
            "SWORD", new Dictionary<string, int>(defaultWeaponRanking)
        },
        { 
            "AXE", new Dictionary<string, int>(defaultWeaponRanking)   
        },
        { 
            "LANCE", new Dictionary<string, int>(defaultWeaponRanking)   
        },
        { 
            "BOW", new Dictionary<string, int>(defaultWeaponRanking)   
        },
        { 
            "GRIMIORE", new Dictionary<string, int>(defaultWeaponRanking)   
        },
        { 
            "STAFF", new Dictionary<string, int>(defaultWeaponRanking)   
        },
    };
    public static Dictionary<string, int> DEFAULT_EXP_PER_USE = new Dictionary<string, int>{
        { "SWORD", 1 }, { "AXE", 1 }, { "LANCE", 1 },
        { "BOW", 2 }, { "GRIMIORE", 2 }, { "STAFF", 10 }
    };

    protected Weapon _weaponInstance;
    public Weapon WeaponInstance {
        get {return _weaponInstance; }
    }

    public virtual void Exclusive() {
    }

    public string Name() {
        return _weaponInstance.name;
    }

    public void SetStats() {
        STATS = new Dictionary<string, Stat>();
        STATS["MIGHT"]      = new Stat(mightStat);
        STATS["CRIT_RATE"]  = new Stat(criticalRateStat);
        STATS["HIT"]        = new Stat(hitPercentageStat);
        STATS["MAX_USAGE"]  = new Stat(maxUsageStat);
        STATS["WEIGHT"]     = new Stat(weightStat);
        STATS["MIN_RANGE"]  = new Stat(minRangeStat);
        STATS["MAX_RANGE"]  = new Stat(maxRangeStat);
        STATS["CRIT_MULTIPLIER"] = new Stat(3);
    }

    protected virtual void Awake() {
        SetStats();
    }

    public virtual void Spawn(Transform spawnPoint) {
        _weaponInstance = Instantiate(this, spawnPoint.position, spawnPoint.rotation, spawnPoint);
           
    }
}
