using UnityEngine;

public class SpawnFootPrint : MonoBehaviour
{
    [Min(1)]
    public int width = 1;

    [Min(1)]
    public int depth = 1;

    public Vector2Int Size => new Vector2Int(width, depth);

}
