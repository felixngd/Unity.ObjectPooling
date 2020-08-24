﻿using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class ComponentPool<T> : IPool<T> where T : Component
    {
        protected static readonly Type ComponentType = typeof(T);

        public ReadList<T> ActiveObjects => this.activeObjects;

        private readonly List<T> activeObjects = new List<T>();
        private readonly Queue<T> pool = new Queue<T>();
        private readonly IInstantiator<T> instantiator;

        public ComponentPool(IInstantiator<T> instantiator)
        {
            this.instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        public void Prepool(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var item = this.instantiator.Instantiate();

                if (!item)
                    continue;

                item.gameObject.SetActive(false);
                this.pool.Enqueue(item);
            }
        }

        public void Return(T item)
        {
            if (!item)
                return;

            if (this.activeObjects.Contains(item))
                this.activeObjects.Remove(item);

            if (item.gameObject.activeSelf)
                item.gameObject.SetActive(false);

            if (!this.pool.Contains(item))
                this.pool.Enqueue(item);
        }

        public void Return(params T[] items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void Return(IEnumerable<T> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void ReturnAll()
        {
            for (var i = this.activeObjects.Count - 1; i >= 0; i--)
            {
                var item = this.activeObjects[i];
                this.activeObjects.RemoveAt(i);

                if (item.gameObject.activeSelf)
                    item.gameObject.SetActive(false);

                if (!this.pool.Contains(item))
                    this.pool.Enqueue(item);
            }
        }

        public T Get()
        {
            T item;

            if (this.pool.Count > 0)
            {
                item = this.pool.Dequeue();
                item.transform.position = Vector3.zero;
                item.gameObject.SetActive(true);
            }
            else
            {
                item = this.instantiator.Instantiate();
            }

            if (item)
                this.activeObjects.Add(item);

            return item;
        }
    }
}
