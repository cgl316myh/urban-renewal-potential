using System;
using System.Collections;
using System.Collections.Generic;

namespace UrbanRenewal.GIS
{
    /// <summary>包装消息列表：每次 Add 时同步触发回调，便于分析过程实时写主窗体日志。</summary>
    public sealed class LiveMessageList : IList<string>
    {
        private readonly IList<string> _inner;
        private readonly Action<string> _onAdd;

        public LiveMessageList(IList<string> inner, Action<string> onAdd)
        {
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            _inner = inner;
            _onAdd = onAdd;
        }

        public int Count
        {
            get { return _inner.Count; }
        }

        public bool IsReadOnly
        {
            get { return _inner.IsReadOnly; }
        }

        public string this[int index]
        {
            get { return _inner[index]; }
            set { _inner[index] = value; }
        }

        public void Add(string item)
        {
            _inner.Add(item);
            if (_onAdd != null && !string.IsNullOrEmpty(item))
            {
                _onAdd(item);
            }
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public bool Contains(string item)
        {
            return _inner.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _inner.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        public int IndexOf(string item)
        {
            return _inner.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            _inner.Insert(index, item);
            if (_onAdd != null && !string.IsNullOrEmpty(item))
            {
                _onAdd(item);
            }
        }

        public bool Remove(string item)
        {
            return _inner.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _inner.RemoveAt(index);
        }
    }
}
