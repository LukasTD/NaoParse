using System.Collections.Generic;

namespace NaoParse.Util
{
    public class SafeDictionary<K, V> : Dictionary<K, V>
    {
        public void SafeSet(K key, V value)
        {
            if (ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                this.Add(key, value);
            }
        }

        public V SafeGet(K key)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                return default(V);
            }
        }
    }
}