// Stream A. The items of one shift, in data order: which have arrived, which are done.
using System.Collections.Generic;
using ByteWorld.Data;

namespace ByteWorld
{
    public class ItemQueue
    {
        readonly List<ItemData> items;
        readonly HashSet<string> done = new();
        int next;

        public ItemQueue(IEnumerable<ItemData> source) => items = new List<ItemData>(source ?? new ItemData[0]);

        public int Total => items.Count;
        public int DoneCount => done.Count;
        public bool HasPending => next < items.Count;
        public bool AllDone => done.Count >= items.Count;

        /// <summary>The next item to arrive, or null when all have arrived.</summary>
        public ItemData Next() => HasPending ? items[next++] : null;

        public void MarkDone(ItemData item) => done.Add(item.id);

        public bool IsDone(ItemData item) => done.Contains(item.id);

        /// <summary>An attack that hasn't been handled yet (arrived or not): what a timer running out lets through.</summary>
        public ItemData FirstUnhandledAttack()
        {
            foreach (var item in items)
                if (item.IsAttack && !done.Contains(item.id)) return item;
            return null;
        }
    }
}
