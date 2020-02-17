using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TGS;

public class TGSInterface : MonoBehaviour
{
    public int barrierCost = 10000;
    public static Dictionary<string, int> CELL_MASKS = new Dictionary<string, int>();
    protected TerrainGridSystem tgs;
    protected UnityEvent OnFirstScan = new UnityEvent();
    protected Dictionary<int, Bounds> cellPositions = new Dictionary<int, Bounds>();


    bool firstScan = false;
    // Start is called before the first frame update
    protected void Start()
    {
        tgs = TerrainGridSystem.instance;

        CELL_MASKS["PASSABLE"] = 1;
        CELL_MASKS["PLAYER"] = 2;
        CELL_MASKS["ENEMY"] = 3;
        CELL_MASKS["OTHER"] = 4;
        CELL_MASKS["IMMOVABLE_ON_LAND"] = 5;
        CELL_MASKS["IMMOVABLE_IN_AIR"] = 6;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (!firstScan) {
            tgs = TerrainGridSystem.instance;
            tgs.enableRectangleSelection = false;
            
            foreach(Cell cell in tgs.cells) {
                cellPositions[cell.index] = tgs.CellGetRectWorldSpace(cell.index);
            }
            
            OnFirstScan.Invoke();

            firstScan = true;
        }
    }
    public static Cell CellAtPosition(Vector3 position) {
        TerrainGridSystem tgs = TerrainGridSystem.instance;
        Dictionary<int, Bounds> cellPositions = new Dictionary<int, Bounds>();


        foreach(Cell cell in tgs.cells)
            cellPositions[cell.index] = tgs.CellGetRectWorldSpace(cell.index);
        
        foreach (Cell cell in tgs.cells) {
            Bounds cellBounds = cellPositions[cell.index];
            if (IsWithinCell(cellBounds, position))
                return cell;
        }

        throw new UnityException("No Cell at given postion: " + position);
    }
    protected Cell GetCellAtPosition(Vector3 position) {
        foreach (Cell cell in tgs.cells) {
            Bounds cellBounds = cellPositions[cell.index];
            if (withinCell(cellBounds, position))
                return cell;
        }

        throw new UnityException("No Cell at given postion: " + position);
    }

    protected bool withinCell(Bounds cellBounds, Vector3 position) {
        Vector3 center = cellBounds.center;
        Vector3 extents = cellBounds.extents;

        float xMin = center.x - extents.z;
        float xMax = center.x + extents.z;
        float zMin = center.z - extents.z;
        float zMax = center.z + extents.z;

        bool withinZ = position.z > zMin && position.z < zMax;
        bool withinX = position.x > xMin && position.x < xMax;

        return withinX && withinZ;
    }

    
    public static bool IsWithinCell(Bounds cellBounds, Vector3 position) {
        Vector3 center = cellBounds.center;
        Vector3 extents = cellBounds.extents;

        float xMin = center.x - extents.z;
        float xMax = center.x + extents.z;
        float zMin = center.z - extents.z;
        float zMax = center.z + extents.z;

        bool withinZ = position.z > zMin && position.z < zMax;
        bool withinX = position.x > xMin && position.x < xMax;

        return withinX && withinZ;
    }
    protected bool withinCellAsBounds(Bounds cellBounds, Bounds meshBounds) {
        return cellBounds.Intersects(meshBounds);
    }

    protected bool withinCellAsColliders(Bounds cellBounds, List<Bounds> colliderBounds) {
        bool anyWithin = false;

        foreach(Bounds colliderBoundary in colliderBounds) {
            if (colliderBoundary.Intersects(cellBounds))
                anyWithin = true;
        }

        return anyWithin;
    }

    protected bool cellWithinList(int cellIndex, List<Cell> cellList) {
        bool within = false;

        foreach(Cell cell in cellList) {
            if (cell.index == cellIndex)
                within = true;
        }

        return within;
    }
}
