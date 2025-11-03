using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ChainingHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    private const double LoadFactor = 0.75;

    private LinkedList<KeyValuePair<TKey, TValue>>[] buckets;
    private int size;   // buckets 길이
    private int count;  // 실제 요소 수
    private readonly IEqualityComparer<TKey> comparer;

    public ChainingHashTable(int capacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
    {
        if (capacity < 1) capacity = DefaultCapacity;
        size = NextPowerOfTwo(capacity);
        buckets = new LinkedList<KeyValuePair<TKey, TValue>>[size];
        this.comparer = comparer ?? EqualityComparer<TKey>.Default;
        count = 0;
    }

    private static int NextPowerOfTwo(int n)
    {
        int p = 1;
        while (p < n) p <<= 1;
        return p;
    }

    private static int ToPositive(int hash) => hash & 0x7fffffff;

    private int GetBucketIndex(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return ToPositive(key.GetHashCode()) % size;
    }

    private void EnsureCapacity()
    {
        if ((double)count / size <= LoadFactor) return;
        Resize(size * 2);
    }

    private void Resize(int newSize)
    {
        var oldBuckets = buckets;
        size = newSize;
        buckets = new LinkedList<KeyValuePair<TKey, TValue>>[size];
        count = 0;

        foreach (var list in oldBuckets)
        {
            if (list == null) continue;
            foreach (var kv in list)
            {
                Add(kv.Key, kv.Value); // Rehash
            }
        }
    }

    // IDictionary

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out var v)) return v;
            throw new KeyNotFoundException();
        }
        set
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            EnsureCapacity();
            int idx = GetBucketIndex(key);
            buckets[idx] ??= new LinkedList<KeyValuePair<TKey, TValue>>();

            var node = buckets[idx].First;
            while (node != null)
            {
                if (comparer.Equals(node.Value.Key, key))
                {
                    node.Value = new KeyValuePair<TKey, TValue>(key, value);
                    return;
                }
                node = node.Next;
            }

            buckets[idx].AddLast(new KeyValuePair<TKey, TValue>(key, value));
            count++;
        }
    }

    public ICollection<TKey> Keys =>
        buckets.Where(b => b != null)
               .SelectMany(b => b)
               .Select(kv => kv.Key)
               .ToList();

    public ICollection<TValue> Values =>
        buckets.Where(b => b != null)
               .SelectMany(b => b)
               .Select(kv => kv.Value)
               .ToList();

    public int Count => count;
    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        EnsureCapacity();
        int idx = GetBucketIndex(key);
        buckets[idx] ??= new LinkedList<KeyValuePair<TKey, TValue>>();

        foreach (var kv in buckets[idx])
        {
            if (comparer.Equals(kv.Key, key))
                throw new ArgumentException("키 중복");
        }

        buckets[idx].AddLast(new KeyValuePair<TKey, TValue>(key, value));
        count++;
    }

    public bool ContainsKey(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        int idx = GetBucketIndex(key);
        var list = buckets[idx];
        if (list == null) return false;
        foreach (var kv in list)
            if (comparer.Equals(kv.Key, key)) return true;
        return false;
    }

    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        int idx = GetBucketIndex(key);
        var list = buckets[idx];
        if (list == null) return false;

        var node = list.First;
        while (node != null)
        {
            if (comparer.Equals(node.Value.Key, key))
            {
                list.Remove(node);
                count--;
                if (list.Count == 0) buckets[idx] = null;
                return true;
            }
            node = node.Next;
        }
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        int idx = GetBucketIndex(key);
        var list = buckets[idx];
        if (list != null)
        {
            foreach (var kv in list)
            {
                if (comparer.Equals(kv.Key, key))
                {
                    value = kv.Value;
                    return true;
                }
            }
        }
        value = default;
        return false;
    }

    // ICollection

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        Array.Clear(buckets, 0, size);
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!TryGetValue(item.Key, out var v)) return false;
        return EqualityComparer<TValue>.Default.Equals(v, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        int i = arrayIndex;
        foreach (var kv in this)
        {
            if (i >= array.Length) throw new ArgumentException("대상 배열의 공간이 부족합니다.");
            array[i++] = kv;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!TryGetValue(item.Key, out var v)) return false;
        if (!EqualityComparer<TValue>.Default.Equals(v, item.Value)) return false;
        return Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var list in buckets)
        {
            if (list == null) continue;
            foreach (var kv in list) yield return kv;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<IReadOnlyList<KeyValuePair<TKey, TValue>>> DebugSnapshot()
    {
        foreach (var list in buckets)
            yield return list == null ? Array.Empty<KeyValuePair<TKey, TValue>>() : list.ToList();
    }

    public int DebugSize => size;
    // 디버그용: 각 버킷의 연결리스트를 그대로 반환
    public IEnumerable<LinkedList<KeyValuePair<TKey, TValue>>> DebugEnumerateBuckets()
    {
        foreach (var list in buckets)
            yield return list;
    }


    public int DebugBucketOf(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        // 실제 해시 인덱스 계산과 동일한 공식 사용
        int hash = key.GetHashCode();
        int positive = hash & 0x7fffffff;   // ToPositive
        return positive % size;             // 현재 테이블 크기에 대해 버킷 인덱스 반환
    }

}
