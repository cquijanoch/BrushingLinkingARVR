using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrushingAndLinking
{
    public class PathManager : MonoBehaviour
    {
        public static PathManager Instance { get; private set; }

        private static UnbundleFD unbundleFD;
        private static List<KeyValuePair<GameObject, GameObject>> frustumDisplayed = new();
       // private static QueueManager UpdatingQueue;
        public int count = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;

            //UpdatingQueue =  gameObject.AddComponent<QueueManager>();
        }

        private void Start()
        {
            if (unbundleFD == null)
                unbundleFD = FindAnyObjectByType<UnbundleFD>();

            if (!unbundleFD.enabled)
                unbundleFD.enabled = true;
        }

        private void LateUpdate()
        {
            if (!OcclusionManager.Instance.DoINeedNewPath())
                return;
            StartCoroutine(ResetFrustumLink());
        //            UpdatingQueue.AddToQueue(ResetFrustumLink());
        }

        public void PathAddingTarget(GameObject target)
        {
            //if (OcclusionManager.Instance.DoINeedNewPath())
            //    OcclusionManager.Instance.RecalculatePath(Camera.main.transform.position);

            count++;
            StartCoroutine(AddingSteps(target));
            //UpdatingQueue.AddToQueue(AddingSteps(target));
        }

        private IEnumerator AddingSteps(GameObject target)
        {
            var frustum = OcclusionManager.Instance.GetSteps(target, Camera.main.transform.position, true);
            frustumDisplayed = new(frustum);
            unbundleFD.ResetBundling();
            if (frustumDisplayed.Count > 0)
                unbundleFD.InitUnbundling(frustumDisplayed);

            
            yield return null;
        }

        public void PathRemovingTarget(GameObject target)
        {
            //if (OcclusionManager.Instance.DoINeedNewPath())
            //    OcclusionManager.Instance.RecalculatePath(Camera.main.transform.position);
            count--;
            StartCoroutine(RemovingSteps(target));
            //UpdatingQueue.AddToQueue(RemovingSteps(target));
        }

        private IEnumerator RemovingSteps(GameObject target)
        {
            var newFrustum = OcclusionManager.Instance.GetSteps(target, Camera.main.transform.position, false).ToList();

            bool isRemovedFrustum = !frustumDisplayed.SequenceEqual(newFrustum);
            if (isRemovedFrustum)
                frustumDisplayed = newFrustum;

            unbundleFD.ResetBundling();

            if (frustumDisplayed.Count > 0)
                unbundleFD.InitUnbundling(frustumDisplayed);

            yield return null;
        }

        private IEnumerator ResetFrustumLink()
        {
            frustumDisplayed = new (OcclusionManager.Instance.RecalculatePath(Camera.main.transform.position));
            unbundleFD.ResetBundling();
            if (frustumDisplayed.Count > 0)
                unbundleFD.InitUnbundling(frustumDisplayed);

            yield return null;
        }

        private void OnDisable()
        {
            count = 0;
            frustumDisplayed?.Clear();
            unbundleFD?.ResetBundling();
        }

        private void OnDestroy()
        {
            count = 0;
            frustumDisplayed?.Clear();
            unbundleFD?.ResetBundling();
        }
    }
}

