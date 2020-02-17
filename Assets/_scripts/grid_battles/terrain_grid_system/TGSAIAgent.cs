using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;

public class TGSAIAgent : TGSInterface
{
    GameManager  gameManager;
    Entity       entity;
    EnemyEntity  enemy;
    OtherEntity  otherEntity;

    int lastTurn = 0;
    bool targetsLocked = false;
    List<Entity> targets = new List<Entity>();
    List<Entity> comrades = new List<Entity>();
    
    List<int>    targetCells = new List<int>();
    List<int>    closeRangeAttackPoints = new List<int>();
    List<int>    longRangeAttackPoints = new List<int>();
    List<int>    comradeCells = new List<int>();

    List<int>    moveableCells = new List<int>();
    List<int>    actionableCells = new List<int>();


    // TODO: AI behavior for equipping weapons

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        OnFirstScan.AddListener(ScanCells);

        gameManager = GameObject.Find("Grid Camera").GetComponent<GameManager>();
        
        entity = this.gameObject.GetComponent<Entity>();
        if (this.gameObject.tag == "Enemy") {
            enemy = this.gameObject.GetComponent<EnemyEntity>();
        } else if (this.gameObject.tag == "Other") {
            otherEntity = this.gameObject.GetComponent<OtherEntity>();
        }
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // don't operate on prefabs? I get ghost attacks.
        if (this.gameObject.scene.name == null)
            return;

        if (gameManager.currentPhase == OperationPhase()) {
            if (lastTurn < gameManager.turn) {
                targetsLocked = false;
            }

            if (!targetsLocked){
                ScanForTargets();
                targetsLocked = true;
            }

            if (entity.turnToMove) {
                if (!entity.HasMoved && !entity.isMoving)
                    MoveToNearestTarget();
            }

            lastTurn = gameManager.turn;
        }
    }


    void SortAttackPointsByDistance() {
        int startIndex = EntityInhabitingCell().index;
        
        closeRangeAttackPoints.Sort(delegate(int cellIndex1, int cellIndex2) {
            int distance1 = tgs.CellGetBoxDistance(startIndex, cellIndex1);
            int distance2 = tgs.CellGetBoxDistance(startIndex, cellIndex2);
            
            return  distance1.CompareTo(distance2);
        });

        longRangeAttackPoints.Sort(delegate(int cellIndex1, int cellIndex2) {
            int distance1 = tgs.CellGetBoxDistance(startIndex, cellIndex1);
            int distance2 = tgs.CellGetBoxDistance(startIndex, cellIndex2);
            
            return  distance1.CompareTo(distance2);
        });
    }

    void MoveToNearestTarget() {
        SortAttackPointsByDistance();
        
        if (canAttack() && !entity.isMoving) {
            bool isCloseRange = closeRangeAttackPoints.Contains(entity.currentCellIndex);
            bool canAttackLongRange = longRangeAttackPoints.Count > 0;
            bool withinLongRange = false; 

            if (canAttackLongRange) {
                foreach (int cellIndex in longRangeAttackPoints) {
                    if (entity.AttackRange().Contains(cellIndex))
                        withinLongRange = true;
                }
            }

            if (canAttackLongRange && withinLongRange) {
                InitiateMovement();
            } else {
                InitiateAttack();
                entity.HasMoved = true;
                entity.hasAttacked = true;
            }
        } else {
            InitiateMovement();
        }
    }

    void InitiateMovement() {
        RejectOccupiedCells();

        List<Vector3> path = BuildMovePath();

        entity.OnReachedDestination.AddListener(delegate{
            entity.MovePath = new List<Vector3>();

            targetsLocked = false;
            entity.turnToMove = false;

            // Should be an acc1ess priority here: Attack, Heal self or comrade, etc.
            if (canAttack()) {
                InitiateAttack();
                entity.HasMoved = true;
                entity.hasAttacked = true;
            } else {
                entity.HasMoved = true;
            }
        });

        if (entity.EntityType == "enemy")
            enemy.MovePath = path;

        if (entity.EntityType == "player")
            otherEntity.MovePath = path;
    }

    string OperationPhase() {
        if (entity.EntityType == "enemy")
            return "Enemy";
        
        if (entity.EntityType == "other")
            return "Other";

        throw new UnityException("Supporting only EnemyEntity or OtherEntity. Given: " + entity.EntityType);
    }


    bool canAttack() {
        print(entity.gameObject.name + ": " + entity.hasAttacked);
        if (!entity.hasAttacked) {
            if (AllAttackPoints().Contains(EntityInhabitingCell().index))
                return true;
        }

        return false;
    }

    void SetMoveableCells() {
        moveableCells = new List<int>();
        moveableCells = tgs.CellGetNeighbours(EntityInhabitingCell().index, entity.moveRadius);
    }

    List<Vector3> BuildMovePath() {
        SetMoveableCells();

        int cellIndex;
        if (longRangeAttackPoints.Count > 0) {
            cellIndex = longRangeAttackPoints[0];
        } else {
            cellIndex = closeRangeAttackPoints[0];
        }

        int currentCellIndex = EntityInhabitingCell().index;
        List<Vector3> path = new List<Vector3>();
        List<int> pathToTarget = tgs.FindPath(currentCellIndex, cellIndex, 0, entity.moveRadius);

        if (pathToTarget.Count == 0) {  // Target out of range... Move to closest square to target
            Dictionary<int, int> cellDistances = new Dictionary<int, int>();
            
            foreach(int cellIdx in moveableCells) {
                int boxDistance = tgs.CellGetBoxDistance(cellIdx, cellIndex);      
                cellDistances[cellIdx] = boxDistance;
            }

            List<KeyValuePair<int, int>> cellDist = cellDistances.ToList();
            cellDist.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            int closestCellIndex = cellDist[0].Key;
            pathToTarget = tgs.FindPath(currentCellIndex, closestCellIndex, 0, entity.moveRadius);
        }

        foreach(int cellIdx in pathToTarget) {
            Bounds cellBounds = cellPositions[cellIdx];

            path.Add(cellBounds.center);
        }

        return path;
    }

    void InitiateAttack() {
        List<int> attackRange = entity.AttackRange();
        Dictionary<Entity, int> attackableTargets = new Dictionary<Entity, int>();

        // Sort by targets you'd do the most damage to.
        targets.Sort(delegate(Entity target1, Entity target2) {
            int atkDMG_1 = entity.PreviewAttack(target1)["ATK_DMG"];
            int atkDMG_2 = entity.PreviewAttack(target2)["ATK_DMG"];

            return atkDMG_1.CompareTo(atkDMG_2);
        });

        foreach(int cellIndex in attackRange) {
            foreach(Entity target in targets) {
                if (withinCell(cellPositions[cellIndex], target.transform.position))
                    attackableTargets[target] = cellIndex;
            }
        }

        print("Attackable Targets: " + attackableTargets);

        var targetPicker = attackableTargets.GetEnumerator();
        targetPicker.MoveNext();

        var targetData = targetPicker.Current;
        PlayerEntity targetPlayer = targetData.Key.GetComponent<PlayerEntity>();
        
        gameManager.LoadBattleScene(enemy, targetPlayer);
    }

    Cell EntityInhabitingCell() {
        return GetCellAtPosition(transform.position);
    }

    void RejectOccupiedCells() {
        if (closeRangeAttackPoints.Count == 0) {

            foreach(int cellIndex in closeRangeAttackPoints) {
                Bounds cellBounds = cellPositions[cellIndex];

                
                foreach(Entity entity in gameManager.entities) {
                    if (withinCell(cellBounds, entity.transform.position))
                        closeRangeAttackPoints.Remove(cellIndex);
                }
            }
        }

        if (longRangeAttackPoints.Count > 0) {
            foreach(int cellIndex in longRangeAttackPoints) {
                Bounds cellBounds = cellPositions[cellIndex];
                
                foreach(Entity entity in gameManager.entities) {
                    if (withinCell(cellBounds, entity.transform.position))
                        longRangeAttackPoints.Remove(cellIndex);
                }
            }
        }
    }

    void ScanCells() {
        GameObject[] landObstacles = GameObject.FindGameObjectsWithTag("Land Obstacle");
        tgs.CellSetGroup(EntityInhabitingCell().index, CELL_MASKS[entity.EntityType.ToUpper()]);
        
        for(int i = 0; i < cellPositions.Count; i++) {
            int cellIndex = i;
            Bounds cellBounds = cellPositions[cellIndex];

            foreach (GameObject obstacle in landObstacles) {
                List<Bounds> objBounds  = new List<Bounds>();
                Collider[] colliders    = obstacle.GetComponents<Collider>();
                
                foreach(Collider col in colliders)
                    objBounds.Add(col.bounds);

                if (withinCellAsColliders(cellBounds, objBounds)) {
                        tgs.cells[cellIndex].canCross = entity.canFly;
                        
                        tgs.CellSetGroup(cellIndex, CELL_MASKS["IMMOVABLE_ON_LAND"]);
                        
                        // Assign a crossing cost to barrier for path-finding purposes
                        tgs.CellSetSideCrossCost (cellIndex, CELL_SIDE.Top, barrierCost);
                        tgs.CellSetSideCrossCost (cellIndex, CELL_SIDE.TopLeft, barrierCost);
                        tgs.CellSetSideCrossCost (cellIndex, CELL_SIDE.TopRight, barrierCost);
                }
            }
        }
    }


    void ScanForTargets() {
        switch (entity.EntityType) {
            case "enemy" :
                AddTargetsAsEnemy();
                AddComradesAsEnemy();

                SetTargetCells();
                SetComradeCells();

                return;
            case "other" :
                AddTargetsAsOther();
                AddComradesAsOther();

                SetTargetCells();
                SetComradeCells();

                return;
        }
    }

    void AddTargetsAsEnemy() {
        if (gameManager.otherEntities.Count > 0) {
            foreach(OtherEntity other in gameManager.otherEntities) {
                Entity entity = other.GetComponent<Entity>();
                if (!targets.Contains(entity)) {
                    targets.Add(entity);

                    entity.OnDeathEvent.AddListener(delegate{
                        targets.Remove(entity);
                    });
                }
            }
        }

        if (gameManager.playerEntities.Count > 0) {
            foreach(PlayerEntity player in gameManager.playerEntities) {
                Entity entity = player.GetComponent<Entity>();
                if (!targets.Contains(entity)) {
                    targets.Add(entity);

                    entity.OnDeathEvent.AddListener(delegate{
                        targets.Remove(entity);
                    });
                }
            }
        }
    }

    void AddTargetsAsOther() {
        if(gameManager.enemyEntities.Count > 0) {
            foreach(EnemyEntity enemy in gameManager.enemyEntities) {
                Entity entity = enemy.GetComponent<Entity>();
                if (!targets.Contains(entity)) {
                    targets.Add(entity);

                    entity.OnDeathEvent.AddListener(delegate{
                        targets.Remove(entity);
                    });
                }
            }
        }
    }

    List<int> AllAttackPoints() {
        List<int> allAttackPoints = new List<int>(closeRangeAttackPoints);
        
        allAttackPoints.AddRange(longRangeAttackPoints);
        
        return allAttackPoints;
    }

    void SetTargetCells() {
        targetCells = new List<int>();
        closeRangeAttackPoints = new List<int>();
        longRangeAttackPoints = new List<int>();

        foreach(Cell cell in tgs.cells) {
            foreach (Entity target in targets) {
                // Check every cell for a target entity
                if (cell.canCross && withinCell(cellPositions[cell.index], target.transform.position)) {
                    targetCells.Add(cell.index);

                    List<Cell> neighbors = tgs.CellGetNeighbours(cell.index); 
                    // Add attack points to get within a target's range
                    foreach(int attackPoint in entity.AttackRange(cell.index)) {
                        if (cell.canCross) { // Rule out impassable cells
                            if (neighbors.Contains(tgs.cells[attackPoint])) {
                                closeRangeAttackPoints.Add(attackPoint);
                            } else {
                                longRangeAttackPoints.Add(attackPoint);
                            }
                        }
                    }
                }
            }
        }
    }

    void AddComradesAsEnemy() {
        List<EnemyEntity> friends = new List<EnemyEntity>(gameManager.enemyEntities);
        friends.Remove(enemy);
        
        foreach(EnemyEntity friend in friends) {
            Entity entity = friend.GetComponent<Entity>();
            
            if (!comrades.Contains(entity)) {
                comrades.Add(entity);
                
                entity.OnDeathEvent.AddListener(delegate{
                    comrades.Remove(entity);
                });
            }
        }
    }

    void AddComradesAsOther() {
        List<OtherEntity> friends = new List<OtherEntity>(gameManager.otherEntities);
        friends.Remove(otherEntity);
        
        foreach(OtherEntity friend in friends) {
            Entity entity = friend.GetComponent<Entity>();
            if (!comrades.Contains(entity)) {
                comrades.Add(entity);
                
                entity.OnDeathEvent.AddListener(delegate{
                    comrades.Remove(entity);
                });
            }
        }

        List<PlayerEntity> players = new List<PlayerEntity>(gameManager.playerEntities);
        foreach(PlayerEntity player in players) {
            Entity entity = player.GetComponent<Entity>();
            if (!comrades.Contains(entity)) {
                comrades.Add(entity);

            }
        }
    }

    void SetComradeCells() {
        comradeCells = new List<int>();

        foreach(Cell cell in tgs.cells) {
            foreach (Entity comrade in comrades) {
                if (withinCell(cellPositions[cell.index], comrade.transform.position))
                    comradeCells.Add(cell.index);
            }
        }
    }
}
