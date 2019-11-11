using System.Collections.Generic;
namespace Search.OnDiskDataStructure
{
    /// <summary>
    /// An interface for on-disk dictionary
    /// </summary>
    /// <typeparam name="TKey">Generic type of key</typeparam>
    /// <typeparam name="TValue">Generic type of value</typeparam>
    public interface IOnDiskDictionary<TKey, TValue>
    {
        void Save(SortedDictionary<TKey, TValue> valuePairs, string path, string fileName);
        TValue Get(TKey key, string path, string fileName);
        List<TValue> Get(List<TKey> key, string path, string fileName);
        TKey[] GetKeys(string path, string fileName);
        TValue[] GetValues(string path, string fileName);
    }
}