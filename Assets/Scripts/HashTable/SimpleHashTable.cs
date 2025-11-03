using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SimpleHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    private const double LoadFactor = 0.75;

    private KeyValuePair<TKey, TValue>[] table;
    private bool[] occuiped;

    private int size;
    private int count;

    
    public SimpleHashTable(int capacity = DefaultCapacity)
    {
        size = DefaultCapacity;

        table = new KeyValuePair<TKey, TValue>[size];
        occuiped = new bool[size];
        count = 0;
    }


    private int GetIndex(TKey key, int size)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int hash = key.GetHashCode();
        return Math.Abs(hash) % size;
    }

    private int GetIndex(TKey key)
    {
        return GetIndex(key, this.size);
    }


    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            throw new KeyNotFoundException("키 없음!");
        }
        set
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int index = GetIndex(key);

            if (occuiped[index] && table[index].Key.Equals(key))
            {
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
            }
            else if (!occuiped[index])
            {
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
                occuiped[index] = true;
                ++count;
            }
            else
            {
                throw new InvalidOperationException("해시 충돌!");
            }
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var list = new List<TKey>(count);
            for (int i = 0; i < size; ++i)
                if (occuiped[i]) list.Add(table[i].Key);
            return list;
        }
    }

    //public ICollection<TValue> Values
    //{
    //    get
    //    {
    //        var list = new List<TValue>(count);
    //        for (int i = 0; i < size; ++i)
    //            if (occuiped[i]) list.Add(table[i].Value);
    //        return list;
    //    }
    //}
    public ICollection<TKey> keys => Enumerable.Range(0, size).Where(i => occuiped[i]).Select(i => table[i].Key).ToList();
    public ICollection<TValue> Values => Enumerable.Range(0, size).Where(i => occuiped[i]).Select(i => table[i].Value).ToList();
    public int Count => count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        if ((double)count / size >= LoadFactor)//왜 count+1이지 ?
        {
            Resize();
        }

        int index = GetIndex(key);
        if (!occuiped[index])
        {
            table[index] = new KeyValuePair<TKey, TValue>(key, value);
            occuiped[index] = true;
            ++count;
        }
        else if (table[index].Key.Equals(key))
        {
            throw new ArgumentException("키 중복");
        }
        else
        {
            throw new InvalidOperationException("해시 충돌");
        }
    }

    public void Resize()
    {
        int newSize = size * 2;
        var newTable = new KeyValuePair<TKey, TValue>[newSize];
        var newOccupied = new bool[newSize];

        for (int i = 0; i < size; ++i)
        {
            if (!occuiped[i]) continue;

            int newIndex = GetIndex(table[i].Key, newSize);

            if (newOccupied[newIndex])//true
            {
                throw new InvalidOperationException("해시 충돌");
            }

            newTable[newIndex] = table[i];
            newOccupied[newIndex] = true;//점유
        }

        size = newSize;
        table = newTable;
        occuiped = newOccupied;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Array.Clear(table, 0, size);
        Array.Clear(occuiped, 0, size);
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if(TryGetValue(item.Key,out var v))
            return EqualityComparer<TValue>.Default.Equals(v, item.Value);
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int index = GetIndex(key);
        return occuiped[index] && table[index].Key.Equals(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < count)
            throw new ArgumentException("대상 배열 공간이 부족합니다.");

        int written = 0;
        for (int i = 0; i < size; ++i)
        {
            if (!occuiped[i]) continue;
            array[arrayIndex + written] = table[i];
            written++;
            if (written == count) break;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < size; ++i)
        {
            if (occuiped[i])
            {
                yield return table[i];
            }
        }
    }
  

    public bool Remove(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int index = GetIndex(key);
        if (occuiped[index] && table[index].Key.Equals(key))
        {
            occuiped[index] = false;
            table[index] = default;
            --count;
            return true;
        }

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!Contains(item)) return false;
        return Remove(item.Key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int index = GetIndex(key);
        if (occuiped[index] && table[index].Key.Equals(key))
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
