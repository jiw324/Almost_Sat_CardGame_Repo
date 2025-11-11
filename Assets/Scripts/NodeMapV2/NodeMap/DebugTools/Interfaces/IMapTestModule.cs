public interface IMapTestModule
{
    string ModuleName { get; }
    void Run(NodeMap map);
    string GetReport();
    void Reset();
}