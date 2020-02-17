using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TGS;
using  ControlFreak2.Demos.Cameras;

public class TGSCellSelect : TGSInterface
{
    public GameObject mapCursor;

    Camera mainCamera;
    InMapGUI inMapGUI;
    GameManager gameManager;
    PlayerEntity selectedPlayer;
    
    private Cell selectedCell;
    public Cell SelectedCell {
        get { return selectedCell; }
        set {
            selectedCell = value;

            Bounds cellBounds = tgs.CellGetRectWorldSpace(selectedCell.index);

            Vector3 cursorPosition = cellBounds.center;
            if (gameManager.currentPhase == "Player") {
                if (!mapCursor.activeSelf)
                    mapCursor.SetActive(true);

                cursorPosition.y = mapCursor.transform.position.y;
                mapCursor.transform.position = cursorPosition;
            }
        }
    }

    Cell startCell;

    bool movingPlayer = false;
    bool actionSelect = false;    
    List<int> selectedMovePath;
    List<int> movableCellIndices;
    List<int> actionableCellIndices;
    
    new void Start()
    {
        base.Start();
        OnFirstScan.AddListener(delegate{
            // maybe scan them every frame?
            ScanCells();

            // Set highlighted cell to first player
            if (gameManager.currentPhase == "Player")
                SetSelectedCellOnPlayer(selectedPlayer);
        });

        tgs.OnCellHighlight += (int cellIndex, ref bool cancelHighlight) => {
            cancelHighlight = true;
        };

        mainCamera = this.gameObject.GetComponent<Camera>();
        inMapGUI = mainCamera.GetComponent<InMapGUI>();
        gameManager = this.gameObject.GetComponent<GameManager>();
    }

    new void Update()
    {
        base.Update();

        // Only check for user input during Player Phase
        if (gameManager.currentPhase == "Player") {
            int lastCellIndex = SelectedCell.index;

            StartCoroutine("UserCellSelection", lastCellIndex);

            if(actionSelect)  // Draw attack range
                DrawPlayerAttackRange();

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X)) {
                UndoPlayerSelect();
                UndoPlayerMove();
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z)) {
                Bounds cellBounds = cellPositions[SelectedCell.index];

                List<PlayerEntity> players = gameManager.playerEntities;
                List<EnemyEntity> enemies = gameManager.enemyEntities;

                for (int i = 0; i < players.Count; i++) {   
                    if (withinCell(cellBounds, players[i].transform.position)) {
                        selectedPlayer = players[i];

                        if (!selectedPlayer.HasMoved) {
                            movingPlayer = true;
                            startCell = SelectedCell;

                            movableCellIndices = tgs.CellGetNeighbours(SelectedCell, selectedPlayer.moveRadius);

                            for(int idx = 0; idx < movableCellIndices.Count; idx++) { 
                                int movableIndex = movableCellIndices[idx];
                                Bounds movableBounds = cellPositions[movableIndex];

                                foreach (EnemyEntity enemy in enemies) {
                                    if (withinCell(movableBounds, enemy.transform.position)) {
                                        tgs.CellToggleRegionSurface(movableIndex, true, Color.red);
                                        movableCellIndices.RemoveAt(movableCellIndices.IndexOf(movableIndex));
                                    }
                                }
                            }

                            for(int idx = 0; idx < movableCellIndices.Count; idx++) {
                                int movableIndex = movableCellIndices[idx];

                                tgs.CellToggleRegionSurface(movableIndex, true, Color.blue);
                            }
                        } else if (SelectedCell != null && SelectedCell.index == startCell.index) {
                            movingPlayer = false;
                            tgs.ClearAll();
                        }
                    }
                }

                foreach(EnemyEntity enemy in enemies) {
                    if (withinCell(cellBounds, enemy.transform.position)) {
                        if (enemy.rangeDisplayed) {
                            ClearEnemyRanges(selectedCell.index, enemy.moveRadius);
                        } else {
                            DrawEnemyRanges(selectedCell.index, enemy.moveRadius);
                        }
                    }
                }

                // IDEA: out of game explore is telltale like, interactive cutscenes
                // IDEA Start with missing child, father infiltrated group of 12 to become 13 (they will sacrifice child)
                // IDEA: Church of Lucifer scene, reference Lost Souls 2001 movie (maybe they summon lucifer or somthing)
                // IDEA: Catacombs of the Vatican
                // IDEA: Secret Tunnels under White House, masonic city
                
                // IDEA: Hollywood: celebrity cloning and sacrifice center, faustian bargain signing center
                //  "Make us proud, then you too can join in on our sex orgies, ritual magic ceremoinies...
                //  "and if we like the cut of your jib, then you too can join our illustrious 'A LIST'"
                //  "We'll be watching"
                //  Sacrifice of uncooperative, rebellious or unproductive signees (celebrities)

                // IDEA: Bohemian Grove infiltration
                // IDEA: Bildeberg Group ritual
                // IDEA: Dawning of Maitreya in UN Headquarters

                // IDEA: UNIT TYPES
                // Astral Projector: spawn astral body to plane to fight, real body remains hidden further on grud
                // Invoker: Generally low combat skill, but summons powerful demons
                // Necromancer

                // TODO: Add XCOM like cover system
                // TODO: LOST ODDYSEY LIKE attack user input timer

                // TODO: DRAW USER ATTACK RANGES FOR LONG RANGE (magic, arrows, etc.)
                // TODO: DRAW ENEMY ATTACK RANGES (currently just movement range)
                if (movingPlayer) {
                    if (movableCellIndices.Contains(SelectedCell.index)) {
                        Cell targetCell = SelectedCell;
                        
                        List<Vector3> cellCenters = new List<Vector3>();

                        foreach(int cellIdx in selectedMovePath) {
                            cellCenters.Add(tgs.CellGetPosition(cellIdx));
                        }
                        movingPlayer = false;
                        

                        // Have the camera follow the moving player
                        SimpleFollowCam cameraFollow = mainCamera.GetComponent<SimpleFollowCam>();
                        cameraFollow.targetTransform = selectedPlayer.gameObject.transform;
                        
                        selectedPlayer.OnReachedDestination.AddListener(delegate{
                            // Show action GUI when Player is done moving
                            inMapGUI.isDisplayed = true;

                            // Back to following arrow cursor
                            cameraFollow.targetTransform = mapCursor.transform;
                        });

                        // Start moving.
                        selectedPlayer.MovePath = cellCenters;

                        tgs.ClearAll();
                    } 
                }
            }
        } else {
            tgs.ClearAll();
            mapCursor.SetActive(false);
        }
    }

    public void ClearActionSelectMode() {
        actionSelect = false;
    }

    public void SetActionSelectMode() {
        actionSelect = true;
        actionableCellIndices = new List<int> { selectedPlayer.currentCellIndex };
        List<Cell> neighbors = PlayerCellNeighbors();

        List<PlayerEntity> otherPlayers = new List<PlayerEntity>(gameManager.playerEntities);
        otherPlayers.Remove(selectedPlayer);    // All players, but the selected one.

        foreach(int cellIdx in AttackableCells()) {
            Bounds cellBounds = cellPositions[cellIdx];
            
            if (CanAttackCell(cellIdx))
                actionableCellIndices.Add(cellIdx);
        }

        foreach(Cell cell in neighbors) {
            foreach(PlayerEntity player in otherPlayers) {
                if (withinCell(cellPositions[cell.index], player.transform.position))
                    actionableCellIndices.Add(cell.index);
            }
        }
    }

    public bool CanAttackCell(int cellIndex) {
        return AttackableCells().Contains(cellIndex);
    }

    public bool CanAttack() {
        return AttackableCells().Contains(selectedCell.index);
    }

    public List<int> AttackableCells() {
        List<int> attackableCells = new List<int>();

        List<int> playerAtkRange = selectedPlayer.AttackRange();

        foreach(int cellIndex in playerAtkRange) {
            Bounds cellBounds = cellPositions[cellIndex];
            foreach(EnemyEntity enemy in gameManager.enemyEntities) {
                if (withinCell(cellBounds, enemy.transform.position))
                    attackableCells.Add(cellIndex);
            }
        }

        return attackableCells;
    }

    public void NextPlayer(PlayerEntity player) {
        selectedPlayer = player;
        SetSelectedCellOnPlayer(player);

        print ("UNMOVED PLAYERS: " + gameManager.UnmovedPlayers().Count);
    }

    public void SetSelectedCellOnPlayer(PlayerEntity player) {
        Vector3 playerPosition = player.gameObject.transform.position;

        for(int i = 0; i < cellPositions.Count; i++) {
            int cellIndex = i;
            Bounds cellBounds = cellPositions[cellIndex];

            if (withinCell(cellBounds, playerPosition)) {
                tgs.ClearAll();

                SelectedCell = tgs.cells[cellIndex];
                tgs.CellToggleRegionSurface(cellIndex, true, Color.yellow);
            }
        }
    }

    public PlayerEntity GetSelectedPlayer() {
        return selectedPlayer;
    }

    public EnemyEntity GetSelectedEnemy() {
        Bounds cellBounds = cellPositions[SelectedCell.index];
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach(GameObject enemy in enemies) {
            if (withinCell(cellBounds, enemy.transform.position))
                return enemy.GetComponent<EnemyEntity>();
        }

        throw new UnityException("No enemy selected on the grid.");        
    }

    public void Refresh() {
        selectedPlayer = gameManager.UnmovedPlayers()[0];

        startCell = null;
        actionSelect = false;
        movingPlayer = false;
        inMapGUI.isDisplayed = false;

        selectedMovePath = new List<int>();
        movableCellIndices = new List<int>();
        actionableCellIndices = new List<int>();
        
        tgs.ClearAll();

        SelectedCell = PlayerInhabitingCell();
        tgs.CellToggleRegionSurface(selectedCell.index, true, Color.yellow);

        print ("Unmoved players: " + gameManager.UnmovedPlayers().Count);
    }

    void UndoPlayerSelect() {
        if (movingPlayer) {
            startCell = null;
            movingPlayer = false;
            movableCellIndices = new List<int>();

            tgs.ClearAll();
            
            SelectedCell = PlayerInhabitingCell();
            tgs.CellToggleRegionSurface(selectedCell.index, true, Color.yellow);
        }
    }

    void UndoPlayerMove() {
        if (actionSelect) {
            actionSelect = false;
            movingPlayer = false;
            inMapGUI.isDisplayed = false;
            selectedPlayer.HasMoved = false;

            selectedMovePath = new List<int>();
            movableCellIndices = new List<int>();
            actionableCellIndices = new List<int>();
            
            selectedPlayer.transform.position = cellPositions[startCell.index].center;

            tgs.ClearAll();

            SelectedCell = startCell;
            startCell = null;
            tgs.CellToggleRegionSurface(selectedCell.index, true, Color.yellow);
        }
    }

    int NextActionableCell(int cellIndex) {
        if (actionableCellIndices.Contains(cellIndex)) {
            return cellIndex;
        } else if (actionableCellIndices.Count == 1) {
            return actionableCellIndices[0];
        }

        // LONG RANGE ACTION SELECT
        Vector3 selectedPosition = cellPositions[cellIndex].center;
        Vector3 currentPosition = cellPositions[selectedCell.index].center;
        string direction = ActionableCellDirection(currentPosition, selectedPosition);

        int nextCellIndex;
        switch (direction) {
            case "Up"    :
                nextCellIndex = ActionableCellAbove(cellIndex);

                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellToRight(cellIndex);
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellBelow(cellIndex);
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellToRight(cellIndex);
                    
                return nextCellIndex;
            case "Right" :
                nextCellIndex = ActionableCellToRight(cellIndex);
                
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellBelow(cellIndex);
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellToLeft(cellIndex);
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellAbove(cellIndex);
                
                return nextCellIndex;
            case "Down"  :
                nextCellIndex = ActionableCellBelow(cellIndex);
                
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellToLeft(cellIndex);
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellAbove(cellIndex);
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellToRight(cellIndex);
                
                return nextCellIndex;
            case "Left"  :
                nextCellIndex = ActionableCellToLeft(cellIndex);
                
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellAbove(cellIndex);
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellToRight(cellIndex);
                if (nextCellIndex == -1)
                    nextCellIndex = ActionableCellBelow(cellIndex);
                
                return nextCellIndex;
        }

        throw new UnityException("Cannot find an actionabel cell...");
    }

    int ActionableCellAbove(int cellIndex) {
        int targetCellIndex = -1;
        Vector3 selectedPosition = cellPositions[cellIndex].center;

        foreach (int cellIdx in actionableCellIndices) {
            Vector3 actionablePosition = cellPositions[cellIdx].center;

            if (actionablePosition.x < selectedPosition.x)
                return cellIdx;
        }

        return targetCellIndex;
    }

    
    int ActionableCellToRight(int cellIndex) {
        int targetCellIndex = -1;
        Vector3 selectedPosition = cellPositions[cellIndex].center;

        foreach (int cellIdx in actionableCellIndices) {
            Vector3 actionablePosition = cellPositions[cellIdx].center;

            if (actionablePosition.z > selectedPosition.z)
                return cellIdx;
        }

        return targetCellIndex;
    }

    
    int ActionableCellBelow(int cellIndex) {
        int targetCellIndex = -1;
        Vector3 selectedPosition = cellPositions[cellIndex].center;

        foreach (int cellIdx in actionableCellIndices) {
            Vector3 actionablePosition = cellPositions[cellIdx].center;

            if (actionablePosition.x > selectedPosition.x)
                return cellIdx;
        }

        return targetCellIndex;
    }

    
    int ActionableCellToLeft(int cellIndex) {
        int targetCellIndex = -1;
        Vector3 selectedPosition = cellPositions[cellIndex].center;

        foreach (int cellIdx in actionableCellIndices) {
            Vector3 actionablePosition = cellPositions[cellIdx].center;

            if (actionablePosition.z < selectedPosition.z)
                return cellIdx;
        }

        return targetCellIndex;
    }

    string ActionableCellDirection(Vector3 currentPosition, Vector3 selectedPosition) {
        if (selectedPosition.x > currentPosition.x)
            return "Down";
        if (selectedPosition.x < currentPosition.x)
            return "Up";
        if (selectedPosition.z < currentPosition.z)
            return "Left";
        if (selectedPosition.z > currentPosition.z)
            return "Right";

        throw new UnityException("Cannot calculate newly selected direction");
    }

    IEnumerator UserCellSelection(int lastCellIndex) {
        int cellIndex;

        float waitTime = 2f;
        float vertAxis = Input.GetAxis("Vertical");
        float horiAxis = Input.GetAxis("Horizontal");
                

        if (horiAxis != 0) {
            if (!movingPlayer)
                tgs.ClearAll();

            int newRow;
            
            if (horiAxis > 0) {
                newRow = SelectedCell.row + 1; 
            } else {
                newRow = SelectedCell.row - 1; 
            }

            if (newRow < 0)
                newRow = 0;

            cellIndex = tgs.CellGetIndex(newRow, SelectedCell.column);
            
            if (movingPlayer) {
                List<Cell> neighbors = tgs.CellGetNeighbours(startCell.index);
                
                if (movableCellIndices.Contains(cellIndex) || cellIndex == startCell.index) {
                    SelectedCell = tgs.cells[cellIndex];

                    if (cellIndex != startCell.index) {
                        if (cellWithinList(cellIndex, neighbors))
                            selectedMovePath = new List<int>();
                 
                        if (selectedMovePath.Contains(cellIndex)) {
                            int firstIndex = selectedMovePath.IndexOf(cellIndex);
                            
                            if (firstIndex <= selectedMovePath.Count - 2) {
                                int times = selectedMovePath.Count - (firstIndex + 1);
                                for(int i = 1; i <= times; i++)
                                    selectedMovePath.RemoveAt(firstIndex + 1);

                            } else if (firstIndex == selectedMovePath.Count - 1) {
                                selectedMovePath.RemoveAt(firstIndex);
                            }
                        } else {
                            selectedMovePath.Add(cellIndex);
                        }

                    }

                    tgs.CellToggleRegionSurface(lastCellIndex, true, Color.blue);
                    tgs.CellToggleRegionSurface(cellIndex, true, Color.yellow);

                    yield return new WaitForSecondsRealtime(waitTime);
                }
            } else if (actionSelect) {
                Cell playerCell = PlayerInhabitingCell();

                int actionableCellIdx = NextActionableCell(cellIndex);             

                if (playerCell.index != actionableCellIdx)  // Have Player rotate to face actions
                    selectedPlayer.LookAtCell(cellPositions[actionableCellIdx]);

                SelectedCell = tgs.cells[actionableCellIdx];
                tgs.CellToggleRegionSurface(actionableCellIdx, true, Color.yellow);

                yield return new WaitForSecondsRealtime(waitTime);
            } else {
                SelectedCell = tgs.cells[cellIndex];
                tgs.CellToggleRegionSurface(cellIndex, true, Color.yellow);

                yield return new WaitForSecondsRealtime(waitTime);
            }    

            lastCellIndex = SelectedCell.index;
        }

        if (vertAxis != 0) {
            if (!movingPlayer)
                tgs.ClearAll();

            int newCol;
            
            if (vertAxis > 0) {
                newCol = SelectedCell.column - 1; 
            } else {
                newCol = SelectedCell.column + 1;
            }

            if (newCol < 0) {
                newCol = 0;
            }

            cellIndex = tgs.CellGetIndex(SelectedCell.row, newCol);

            if (movingPlayer) {
                List<Cell> neighbors = tgs.CellGetNeighbours(startCell.index);
                if (movableCellIndices.Contains(cellIndex) || cellIndex == startCell.index) {

                    SelectedCell = tgs.cells[cellIndex];


                    if (cellIndex != startCell.index) {
                        if (cellWithinList(cellIndex, neighbors))
                            selectedMovePath = new List<int>();
                        if (selectedMovePath.Contains(cellIndex)) {
                            int firstIndex = selectedMovePath.IndexOf(cellIndex);
                            
                            if (firstIndex <= selectedMovePath.Count - 2) {
                                int times = selectedMovePath.Count - (firstIndex + 1);
                                for(int i = 1; i <= times; i++)
                                    selectedMovePath.RemoveAt(firstIndex + 1);

                            } else if (firstIndex == selectedMovePath.Count - 1) {
                                selectedMovePath.RemoveAt(firstIndex);
                            }
                        } else {
                            selectedMovePath.Add(cellIndex);
                        }

                    }

                    tgs.CellToggleRegionSurface(lastCellIndex, true, Color.blue);
                    tgs.CellToggleRegionSurface(cellIndex, true, Color.yellow);
                    
                    yield return new WaitForSecondsRealtime(waitTime);
                }
            } else if (actionSelect) {
                Cell playerCell = PlayerInhabitingCell();

                int actionableCellIdx = NextActionableCell(cellIndex);
                    
                if (playerCell.index != actionableCellIdx)  // Have Player rotate to face actions
                    selectedPlayer.LookAtCell(cellPositions[actionableCellIdx]);

                SelectedCell = tgs.cells[actionableCellIdx];
                tgs.CellToggleRegionSurface(actionableCellIdx, true, Color.yellow);
                
                yield return new WaitForSecondsRealtime(waitTime);
            } else {
                SelectedCell = tgs.cells[cellIndex];

                tgs.CellToggleRegionSurface(cellIndex, true, Color.yellow);
                
                yield return new WaitForSecondsRealtime(0.3f);
            }

            lastCellIndex = SelectedCell.index;
        }
    }

    // Likely have to rescan on each player selection
    // ie: flying character selected can move over thigns the horseback one cannot
    void ScanCells()
    {
        GameObject[] landObstacles = GameObject.FindGameObjectsWithTag("Land Obstacle");

        tgs.CellSetGroup(PlayerInhabitingCell().index, CELL_MASKS["PLAYER"]);

        for(int i = 0; i < cellPositions.Count; i++) {
            int cellIndex = i;
            Bounds cellBounds = cellPositions[cellIndex];

            foreach (GameObject obstacle in landObstacles) {
                List<Bounds> objBounds = new List<Bounds>();
                Collider[] colliders = obstacle.GetComponents<Collider>();
                
                foreach(Collider col in colliders)
                    objBounds.Add(col.bounds);

                if (withinCellAsColliders(cellBounds, objBounds)) {
                    // tgs.cells[cellIndex].canCross = selectedPlayer.canFly;
                    
                    tgs.CellSetGroup (cellIndex, CELL_MASKS["IMMOVABLE_ON_LAND"]);
                    
                    // Assign a crossing cost to barrier for path-finding purposes
                    tgs.CellSetSideCrossCost (cellIndex, CELL_SIDE.Top, barrierCost);
                    tgs.CellSetSideCrossCost (cellIndex, CELL_SIDE.Left, barrierCost);
                    tgs.CellSetSideCrossCost (cellIndex, CELL_SIDE.Right, barrierCost);
                    tgs.CellSetSideCrossCost (cellIndex, CELL_SIDE.Bottom, barrierCost);
                }
            }
        }
    }

    // TODO: Must be expanded for long range attackers
    void DrawPlayerAttackRange() {        
        foreach(int cellIndex in selectedPlayer.AttackRange()) {
            if (CanAttackCell(cellIndex) && cellIndex != selectedCell.index)
                tgs.CellToggleRegionSurface(cellIndex, true, Color.red);
        }
    }


    void ClearEnemyRanges(int cellIndex, int moveRadius) {
        List<int> moveRange = tgs.CellGetNeighbours(cellIndex, moveRadius);
        foreach(int cellIdx in moveRange)
            tgs.CellClear(cellIdx);
    }

    void DrawEnemyRanges(int cellIndex, int moveRadius) {
        List<int> moveRange = tgs.CellGetNeighbours(cellIndex, moveRadius);
        foreach(int cellIdx in moveRange)
            tgs.CellToggleRegionSurface(cellIdx, true, Color.red);
    }

    List<Cell> PlayerCellNeighbors() {
        return tgs.CellGetNeighbours(PlayerInhabitingCell().index);        
    }

    Cell PlayerInhabitingCell() {
        return GetCellAtPosition(selectedPlayer.transform.position);
    }
}
