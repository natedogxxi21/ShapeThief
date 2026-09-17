using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public int dungeonwidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDungeon();

    }

    private void CreateDungeon()
    {
        DungeonGenerator generator = new DungeonGenerator(dungeonwidth,  dungeonLength);
        var listOfRooms = generator.CalculateRooms(maxIterations, roomWidthMin, roomLengthMin);

    }
    // Update is called once per frame
    void Update()
    {
          
    }
}
