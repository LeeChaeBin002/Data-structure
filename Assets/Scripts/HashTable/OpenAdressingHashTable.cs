using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum ProbingStrategy
{
    Linear,
    Quadracic,
    DoubleHash,
}


public class OpenAddressingHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    private const double LoadFactor = 0.6;

    private KeyValuePair<TKey, TValue>[] table;
    private bool[] occupied;
    private bool[] deleted;
    private int size;
    private int count;

    private readonly IEqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;
    private ProbingStrategy probingStrategy;


    public OpenAddressingHashTable(ProbingStrategy strategy = ProbingStrategy.Linear)
    {
        table = new KeyValuePair<TKey, TValue>[DefaultCapacity];
        occupied = new bool[DefaultCapacity];
        deleted = new bool[DefaultCapacity];
        size = DefaultCapacity;
        count = 0;

        probingStrategy = strategy;
    }
    private static int ToPositive(int hash) => hash & 0x7fffffff;
    public int GetPrimaryHash(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        return ToPositive(key.GetHashCode()) % size;
        
    }

    public int GetSecondaryHash(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        // size가 2 이상이라고 가정 (최소 16에서 시작)
        int h = ToPositive(key.GetHashCode());
        int step = 1 + (h % (size - 1)); // 1..size-1
        step |= 1; // 홀수 보장
        return step;
    }

    public int GetProbeIndex(TKey key, int attempt)
    {
        int primaryHash = GetPrimaryHash(key);

        switch (probingStrategy)
        {
            case ProbingStrategy.Linear:
                return (primaryHash + attempt) % size;
            case ProbingStrategy.Quadracic:
                return (primaryHash + attempt * attempt) % size;
            case ProbingStrategy.DoubleHash:
                
                int step = GetSecondaryHash(key);
                // 오버플로 방지 위해 long으로 계산
                return (int)((primaryHash + (long)attempt * step) % size);
        }

        throw new ArgumentException(nameof(probingStrategy));
    }


    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            throw new KeyNotFoundException();
        }
        set
        {
            if (key == null)
                throw new ArgumentNullException();

            if ((double)count / size > LoadFactor)
            {
                Resize();
            }

            int attempt = 0;
            do
            {
                int index = GetProbeIndex(key, attempt);
                if (!occupied[index] || deleted[index])
                {
                    table[index] = new KeyValuePair<TKey, TValue>(key, value);
                    occupied[index] = true;
                    deleted[index] = false;
                    ++count;
                    return;
                }

                if (table[index].Key.Equals(key))
                {
                    table[index] = new KeyValuePair<TKey, TValue>(key, value);
                    return;
                }

                ++attempt;

                if (attempt > size)
                {
                    Resize();
                    attempt = 0;
                }
            }
            while (true);
        }

    }

    public ICollection<TKey> Keys => Enumerable.Range(0, size)
                                        .Where(i => occupied[i] && !deleted[i])
                                        .Select(i => table[i].Key)
                                        .ToList();

    public ICollection<TValue> Values => Enumerable.Range(0, size)
                                        .Where(i => occupied[i] && !deleted[i])
                                        .Select(i => table[i].Value)
                                        .ToList();

    public int Count => count;

    public bool IsReadOnly => false;

    private void Resize()
    {
        var oldTable = table;
        var oldOccupied = occupied;
        var oldDeleted = deleted;
        var oldSize = size;

        size *= 2;
        table = new KeyValuePair<TKey, TValue>[size];
        occupied = new bool[size];
        deleted = new bool[size];
        count = 0;

        for (int i = 0; i < oldSize; ++i)
        {
            if (oldOccupied[i] && !oldDeleted[i])
            {
                Add(oldTable[i].Key, oldTable[i].Value);
            }
        }
    }

    private int FindIndex(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException();

        int attempt = 0;
        do
        {
            int index = GetProbeIndex(key, attempt);
            if (!occupied[index] && !deleted[index])
            {
                return -1;
            }
            if (occupied[index] && !deleted[index] && table[index].Key.Equals(key))
            {
                return index;
            }
            ++attempt;
        } while (attempt < size);
        return -1;
    }

    public void Add(TKey key, TValue value)
    {
        if (key == null)
            throw new ArgumentNullException();

        if ((double)count / size > LoadFactor)
        {
            Resize();
        }

        int attempt = 0;
        do
        {
            int index = GetProbeIndex(key, attempt);
            if (!occupied[index] || deleted[index])
            {
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
                occupied[index] = true;
                deleted[index] = false;
                ++count;
                return;
            }

            if (table[index].Key.Equals(key))
            {
                throw new ArgumentException("키 중복");
            }

            ++attempt;

            if (attempt > size)
            {
                Resize();
                attempt = 0;
            }
        }
        while (true);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Array.Clear(table, 0, size);
        Array.Clear(occupied, 0, size);
        Array.Clear(deleted, 0, size);
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        int index = FindIndex(item.Key);
        if (index != -1)
        {
            return table[index].Value.Equals(item.Value);
        }

        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return FindIndex(key) != -1;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        int index = arrayIndex;
        foreach (var kvp in this)
        {
            array[index++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < size; ++i)
        {
            if (occupied[i] && !deleted[i])
            {
                yield return table[i];
            }
        }
    }

    public bool Remove(TKey key)
    {
        int index = FindIndex(key);
        if (index != -1)
        {
            deleted[index] = true;
            --count;
            return true;
        }
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        int index = FindIndex(item.Key);
        if (index != -1 && table[index].Value.Equals(item.Value))
        {
            deleted[index] = true;
            --count;
            return true;
        }
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = FindIndex(key);

        if (index != -1)
        {
            value = table[index].Value;
            return true;
        }

        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
