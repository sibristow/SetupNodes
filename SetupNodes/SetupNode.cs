using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;

namespace JsonTree
{
    /// <summary>
    /// replacement class for ContextWrapper
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class SetupNode : ISetupNode
    {
        public Guid Id { get; private set; }

        public SetupContext SetupContext { get; set; }

        public SetupNode? ParentNode { get; set; }

        public List<SetupNode> Children { get; private set; }

        public string Name
        {
            get
            {
                return this.SetupContext.Name;
            }
        }

        public int Count
        {
            get
            {
                return ((ICollection<SetupNode>)this.Children).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((ICollection<SetupNode>)this.Children).IsReadOnly;
            }
        }



        public SetupNode(SetupContext setupContext)
        {
            this.Id = Guid.NewGuid();
            this.Children = new List<SetupNode>();
            this.SetupContext = setupContext ?? throw new ArgumentNullException(nameof(setupContext));
        }



        public SetupNode? this[Guid nodeId]
        {
            get
            {
                return this.Children.FirstOrDefault(c => Guid.Equals(c.Id, nodeId));
            }
        }

        public SetupNode? this[string path]
        {
            get
            {
                return this.FindByPath(path);
            }
        }

        public SetupNode this[int index]
        {
            get
            {
                return ((IList<SetupNode>)this.Children)[index];
            }

            set
            {
                ((IList<SetupNode>)this.Children)[index] = value;
            }
        }



        public SetupNode? FindByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return this;

            var parts = path.Split(new char[] { '.', '[', ']' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var matchedNode = this.MatchNodeByPathPart(parts, out int skip);
            if (matchedNode == null)
                return null;

            // pass remaining parts of path on to node
            var nextPath = string.Join('.', parts.Skip(skip).ToArray());
            return matchedNode[nextPath];
        }

        private SetupNode? MatchNodeByPathPart(string[] parts, out int skip)
        {
            if (int.TryParse(parts[0], out var index))
            {
                // first path item is an index
                skip = 2;
                return this.Children.FirstOrDefault(v => string.Equals(parts[1], v.SetupContext.Name) && v.SetupContext.ParentValueOrder == index);
            }

            // first path item is a property
            skip = 1;
            return this.Children.FirstOrDefault(v => string.Equals(parts[0], v.SetupContext.Name));
        }

        private SetupNode? MatchNodeByPathPart(string pathPart)
        {
            var hasIndex = System.Text.RegularExpressions.Regex.Match(pathPart, "^(.*)\\[([0-9]+)\\]$");
            if (hasIndex.Success)
            {
                var name = hasIndex.Groups[1].Value;
                var index = int.Parse(hasIndex.Groups[2].Value);
                return this.Children.FirstOrDefault(v => string.Equals(name, v.SetupContext.Name) && v.SetupContext.ParentValueOrder == index);
            }

            // find the node for the first part of the path
            return this.Children.FirstOrDefault(v => string.Equals(pathPart, v.SetupContext.Name));
        }


        #region IList implementation
        public int IndexOf(SetupNode item)
        {
            return ((IList<SetupNode>)this.Children).IndexOf(item);
        }

        public void Insert(int index, SetupNode item)
        {
            ((IList<SetupNode>)this.Children).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<SetupNode>)this.Children).RemoveAt(index);
        }

        public void Add(SetupNode item)
        {
            ((ICollection<SetupNode>)this.Children).Add(item);
        }

        public void Clear()
        {
            ((ICollection<SetupNode>)this.Children).Clear();
        }

        public bool Contains(SetupNode item)
        {
            return ((ICollection<SetupNode>)this.Children).Contains(item);
        }

        public void CopyTo(SetupNode[] array, int arrayIndex)
        {
            ((ICollection<SetupNode>)this.Children).CopyTo(array, arrayIndex);
        }

        public bool Remove(SetupNode item)
        {
            return ((ICollection<SetupNode>)this.Children).Remove(item);
        }

        public IEnumerator<SetupNode> GetEnumerator()
        {
            return ((IEnumerable<SetupNode>)this.Children).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Children).GetEnumerator();
        }
        #endregion
    }
}