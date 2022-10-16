namespace JsonTree
{
    public interface ISetupNode : IList<SetupNode>
    {
        SetupNode? this[Guid nodeId] { get; }

        SetupNode? this[string path] { get; }

        List<SetupNode> Children { get; }

        Guid Id { get; }

        string Name { get; }

        SetupNode? ParentNode { get; set; }

        SetupContext SetupContext { get; set; }

        SetupNode? FindByPath(string path);
    }
}