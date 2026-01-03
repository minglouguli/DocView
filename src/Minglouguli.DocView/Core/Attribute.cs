namespace Minglouguli.DocView
{

#if NET8_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Property)]
    public class DocvForeignTableAttribute<T> : DocvForeignTableAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isMulti">对否多对一</param>
        public DocvForeignTableAttribute(bool isMulti = true) : base(typeof(T), isMulti)
        {

        }
    }

#endif

#if NET6_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Property)]
    public class DocvForeignTableAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isMulti">对否多对一</param>
        public DocvForeignTableAttribute(Type type, bool isMulti = true)
        {
            this.Type = type;
            this.IsMulti = isMulti;
        }

        public Type Type { get; }
        public bool IsMulti { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DocvTableAttribute : Attribute
    {
        public DocvTableAttribute(string name = null, string schema = null, string tagName = null)
        {
            this.Name = name;
            this.Schema = schema;
            this.TagName = tagName;
        }

        public string Name { get; }
        public string Schema { get; }
        public string TagName { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DocvPrimaryKeyAttribute : Attribute
    {

    }

#endif
}
