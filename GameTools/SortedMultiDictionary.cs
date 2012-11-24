using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTools
{
    public class SortedMultiDictionary<Key, Value>
    {
        private SortedDictionary<Key, Stack<Value>> dictionary = new SortedDictionary<Key, Stack<Value>>();

        public void Add(Key key, Value value)
        {
            if(!dictionary.ContainsKey(key))
            {
                Stack<Value> newStack = new Stack<Value>();
                dictionary.Add(key, newStack);
            }

            dictionary[key].Push(value);

        }
        public void Remove(Key key)
        {
            dictionary.Remove(key);
        }
        public void Remove(Key key, int numToRemove)
        {
            Stack<Value> stack = dictionary[key];

            int removed = 0;
            while(stack.Count > 0 && removed < numToRemove)
            {
                stack.Pop();
                removed++;
            }

            if(stack.Count == 0)
                dictionary.Remove(key);
        }
        public Value Pop()
        {
            KeyValuePair<Key, Stack<Value>> topEntry = dictionary.ElementAt(0);
            Value value = topEntry.Value.Pop();

            if(topEntry.Value.Count == 0)
                dictionary.Remove(topEntry.Key);

            return value;
        }
        public Value this[Key key]
        {
            get { return dictionary[key].Peek(); }
            set
            {
                this.Add(key, value);
            }
        }

        public int Count { get { return dictionary.Count; } }
    }
}
