using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Grapevine
{
    public class ListenerPrefixCollection : IListenerPrefixCollection
    {
        public int Count => PrefixCollection.Count;

        public bool IsReadOnly => PrefixCollection.IsReadOnly;

        protected ICollection<string> PrefixCollection
        {
            get;
        }

        public ListenerPrefixCollection(ICollection<string> prefixCollection)
        {
            PrefixCollection = prefixCollection;
        }

        public void Add(string item) => PrefixCollection.Add(item);

        public void Clear() => PrefixCollection.Clear();

        public bool Contains(string item) => PrefixCollection.Contains(item);

        public void CopyTo(string[] array, int arrayIndex) => PrefixCollection.CopyTo(array, arrayIndex);

        public IEnumerator<string> GetEnumerator() => PrefixCollection.GetEnumerator();

        public bool Remove(string item) => PrefixCollection.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => PrefixCollection.GetEnumerator();
    }
}
