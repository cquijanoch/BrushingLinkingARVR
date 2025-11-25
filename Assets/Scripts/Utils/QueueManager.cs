using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrushingAndLinking
{
    [DisallowMultipleComponent]
    public class QueueManager: MonoBehaviour
    {
        private readonly List<IEnumerator> _coroutines = new();

        public void AddToQueue(IEnumerator coroutine)
        {
            _coroutines.Add(coroutine);
            if (_coroutines.Count == 1) //no previous items in queue
                StartCoroutine(CoroutineCoordinator());
        }

        private IEnumerator CoroutineCoordinator()
        {
            while (true)
            {
                while (_coroutines.Count > 0)
                {
                    yield return StartCoroutine(TimingCoroutines());
                }

                if (_coroutines.Count == 0)
                    yield break;
            }
        }

        private IEnumerator TimingCoroutines()
        {
            var last = _coroutines.Last();
            yield return last;
            if (_coroutines.Count > 1)
                _coroutines.RemoveAll(c => !_coroutines.Contains(last));

            _coroutines.RemoveAt(0);

            if (_coroutines.Count == 0)
                yield break;
        }
    }
}
