public class NodeGrid
{
    private const float gridXDist = 1.5f;
    private const float gridYDist = 1.5f;
    public int width { get; private set; } = 0;
    public int height { get; private set; } = 0;

    public NodeGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public (float x, float y) GetCoords(int x, int y)
    {
        return (x * gridXDist, y * gridYDist);
    }
}
