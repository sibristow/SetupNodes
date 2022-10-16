namespace JsonTree
{
    /// <summary>
    /// Dev substitute for datamodel class
    /// </summary>
    public class SetupContext
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? ParentContextId { get; set; }

        public int ParentValueOrder { get; set; }


        public SetupContext(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            this.Id = id;
            this.Name = name;
        }
    }
}