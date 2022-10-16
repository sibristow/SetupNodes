using System.Collections;

namespace JsonTree
{
    public class SetupNodeCollection : IEnumerable<SetupNode>
    {
        private Dictionary<Guid, SetupNode> dictionary;

        public SetupNode this[Guid key]
        {
            get
            {
                return ((IDictionary<Guid, SetupNode>)this.dictionary)[key];
            }

            set
            {
                ((IDictionary<Guid, SetupNode>)this.dictionary)[key] = value;
            }
        }

        public SetupNode? this[string jsonPath]
        {
            get
            {
                return this.FindByPath(jsonPath);
            }
        }


        public SetupNodeCollection()
        {
            this.dictionary = new Dictionary<Guid, SetupNode>();
        }


        /// <summary>
        /// Add a root <see cref="SetupNode"/> to the collection
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(SetupNode node)
        {
            this.dictionary.Add(node.Id, node);
        }

        /// <summary>
        /// Add a parented <see cref="SetupNode"/> to the collection
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentNodeId"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public void AddNode(SetupNode node, Guid parentNodeId)
        {
            if (!dictionary.ContainsKey(parentNodeId))
                throw new KeyNotFoundException($"Parent node with id {parentNodeId} was not found");

            // ensure node's parent is set
            var parentNode = this.dictionary[parentNodeId];

            this.AddNode(node, parentNode);
        }

        public void AddNode(SetupNode node, SetupNode parentNode)
        {
            node.ParentNode = parentNode;

            // add to parent's children
            if (!parentNode.Contains(node))
                parentNode.Add(node);

            // add to main dictionary
            this.dictionary.Add(node.Id, node);
        }

        /// <summary>
        /// Adds a parented <see cref="SetupContext"/> to the collection
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentContextId"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public void AddNodeToParentContext(SetupNode node, Guid parentContextId)
        {
            var parentNode = this.dictionary.Values.FirstOrDefault(v => Guid.Equals(parentContextId, v.SetupContext.Id));
            if (parentNode == null)
                throw new KeyNotFoundException($"No node for context {parentContextId} found");

            this.AddNode(node, parentNode.Id);
        }

        /// <summary>
        /// Finds the <see cref="SetupNode"/> for the SetupContext Id
        /// </summary>
        /// <param name="contextId"></param>
        /// <returns></returns>
        public SetupNode? FindByContextId(Guid contextId)
        {
            return this.dictionary.Values.FirstOrDefault(v => Guid.Equals(contextId, v.SetupContext.Id));
        }

        /// <summary>
        /// Finds the <see cref="SetupNode"/> following the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SetupNode? FindByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Empty path");

            var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // find the node for the first part of the path
            var node = this.dictionary.Values.FirstOrDefault(v => v.ParentNode == null && string.Equals(parts[0], v.SetupContext.Name));
            if (node == null)
                return null;

            // pass remaining parts of path on to node
            var nextPath = string.Join('.', parts.Skip(1).ToArray());
            return node[nextPath];
        }

        /// <summary>
        /// Ensures the parent/children references are consistent between <see cref="SetupNode"/> and <see cref="SetupContext"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int EnsureConsistency()
        {
            var count = 0;

            foreach(var node in this.dictionary.Values)
            {
                if (node.SetupContext.ParentContextId == null && node.ParentNode != null)
                {
                    // setupcontext is not parented, but node has parent
                    // ensure node is not parented
                    node.ParentNode.Remove(node);
                    node.ParentNode = null;
                    count++;
                    continue;
                }

                if (node.SetupContext.ParentContextId != null && node.ParentNode == null)
                {
                    // setupcontext is parented, but node does not have parent
                    // ensure node has parent
                    var parent = this.FindByContextId(node.SetupContext.ParentContextId.Value);
                    if (parent == null)
                    {
                        throw new Exception($"Node {node.Name} ({node.Id}) has context with parent {node.SetupContext.ParentContextId}, but this context was not found in the collection.");
                    }

                    if (!parent.Contains(node))
                    {
                        parent.Add(node);
                    }
                    node.ParentNode = parent;
                    count++;
                    continue;
                }

                if (node.ParentNode != null && node.SetupContext.ParentContextId != null && node.SetupContext.ParentContextId != node.ParentNode.SetupContext.Id)
                {
                    // node has a different (i.e. wrong) parent than its setupcontext
                    // ensure node has same parent
                    var parent = this.FindByContextId(node.SetupContext.ParentContextId.Value);
                    if (parent == null)
                    {
                        throw new Exception($"Node {node.Name} ({node.Id}) has context with parent {node.SetupContext.ParentContextId}, but this context was not found in the collection.");
                    }

                    node.ParentNode.Remove(node);
                    node.ParentNode = parent;
                    parent.Add(node);
                }
            }

            return count;
        }


        #region Interface implementations
        public IEnumerator<SetupNode> GetEnumerator()
        {
            return this.dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}