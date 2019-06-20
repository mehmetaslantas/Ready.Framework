using System;

namespace Ready.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CachePrefixAttribute : Attribute
    {
        public CachePrefixAttribute(int order, string text)
        {
            Order = order;
            Text = text;
        }

        internal int Order { get; }

        internal string Text { get; }
    }
}