using System.Collections.Generic;
using UnityEngine;

namespace Bloomquartz.Juice
{
    /// Object pool for FloatingText instances.
    /// Must live in its own file so Unity can resolve the script GUID by filename.
    public class FloatingTextPool : MonoBehaviour
    {
        public static FloatingTextPool Instance { get; private set; }

        private readonly Queue<FloatingText> _pool = new Queue<FloatingText>();

        private void Awake() => Instance = this;

        public void Spawn(Vector3 pos, string text, Color color)
        {
            FloatingText ft;
            if (_pool.Count > 0)
            {
                ft = _pool.Dequeue();
            }
            else
            {
                var go = new GameObject("FloatingText");
                go.transform.SetParent(transform);
                ft = go.AddComponent<FloatingText>();
            }
            ft.Spawn(pos, text, color);
        }

        public void Return(FloatingText ft) => _pool.Enqueue(ft);
    }
}
