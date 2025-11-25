using DxR;
using System.Collections.Generic;
using UnityEngine;

namespace BrushingAndLinking
{
    [DisallowMultipleComponent]
    public class LinkHighlighter : Highlighter
    {
        public override HighlightTechnique Mode { get { return HighlightTechnique.Link; } }

        private static Dictionary<string, Mark> marksDict;
        //private static UnbundleFD unbundleFD;
        //private static UnbundleFD unbundleFDExt;
        private static List<KeyValuePair<GameObject, GameObject>> linkPairList = new ();

        private GameObject markGameObject;
        private GameObject targetReferent;

        //private bool isHighlighted = false;

        private void Start()
        {
            //if (unbundleFD == null)
            //    unbundleFD = FindAnyObjectByType<UnbundleFD>();

            //if (!unbundleFD.enabled)
            //    unbundleFD.enabled = true;

            //if (unbundleFDExt == null)
            //    unbundleFDExt = FindAnyObjectByType<UnbundleFD>();

            //if (!unbundleFDExt.enabled)
            //    unbundleFDExt.enabled = true;

            AlwaysVisualLink = true;
            //UpdateVisualLink();

            if (marksDict == null)
                CreateMarksDictionary();

            markGameObject = marksDict[gameObject.name].gameObject;
            targetReferent = GetComponent<Product>().LinkToChildForwarding ? transform.GetChild(0).gameObject : gameObject;
        }

        private void CreateMarksDictionary()
        {
            marksDict = new Dictionary<string, Mark>();
            var marks = FindObjectsByType<Mark>(FindObjectsSortMode.None);
            foreach (var mark in marks)
                marksDict.Add(mark.GameObjectName, mark);
        }

        public override void Highlight()
        {
            if (marksDict == null)
                CreateMarksDictionary();

            if (markGameObject == null)
                markGameObject = marksDict[gameObject.name].gameObject;

            if (!isHighlighted)
            {
                PathManager.Instance.PathAddingTarget(gameObject);
                //if (OcclusionManager.Instance.DoINeedNewPath())
                //    OcclusionManager.Instance.RecalculatePath(Camera.main.transform.position);

                //var frustum = OcclusionManager.Instance.GetSteps(gameObject, Camera.main.transform.position, true);
                //frustumDisplayed = new(frustum);
                //unbundleFD.ResetBundling();

                //if (frustum.Count == 0)
                //    linkPairList.Add(new KeyValuePair<GameObject, GameObject>(targetReferent, markGameObject));
                //else
                //{

                //var referentStep = OcclusionManager.Instance.GetStepReferent(gameObject.transform);
                //linkPairList.Add(new KeyValuePair<GameObject, GameObject>(targetReferent, referentStep));
                //linkPairList.Add(new KeyValuePair<GameObject, GameObject>(frustum.First().Key, markGameObject));
                //}

                //frustumDisplayed.AddRange(linkPairList);

                //unbundleFD.InitUnbundling(frustumDisplayed);

                isHighlighted = true;
            }
        }

        //public void ResetFrustumLink()
        //{
        //    if (!OcclusionManager.Instance.DoINeedNewPath())
        //        return;

        //    frustumDisplayed = new(OcclusionManager.Instance.RecalculatePath(Camera.main.transform.position));
        //    unbundleFD.ResetBundling();
        //    unbundleFD.InitUnbundling(frustumDisplayed);
        //}

        public override void Unhighlight()
        {
            if (isHighlighted)
            {
                PathManager.Instance.PathRemovingTarget(gameObject);

                //var newFrustum = OcclusionManager.Instance.GetSteps(gameObject, Camera.main.transform.position, false).ToList();

                //bool isRemovedFrustum = !frustumDisplayed.SequenceEqual(newFrustum);
                //if (isRemovedFrustum)
                //    frustumDisplayed = newFrustum;

                //var referentStep = OcclusionManager.Instance.GetStepReferent(gameObject.transform);

                //for (int i = 0; i < linkPairList.Count; i++)
                //{
                //    if (linkPairList[i].Key == targetReferent && linkPairList[i].Value == referentStep)
                //    {
                //        if (!isRemovedFrustum)
                //            frustumDisplayed.Remove(linkPairList[i]);
                //        else
                //            frustumDisplayed.Add(linkPairList[i]);
                //            linkPairList.RemoveAt(i);
                //        break;
                //    }

                //}
                //    else
                //    {
                //        if (linkPairList[i].Key == targetReferent && linkPairList[i].Value == frustumDisplayed.Last().Value)
                //            idxs.Add(i);
                //        else if (linkPairList[i].Key == frustumDisplayed.First().Key && linkPairList[i].Value == markGameObject)
                //            idxs.Add(i);


                //    }

                //    foreach (var idx in idxs.OrderByDescending(c => c).ToArray())
                //    {
                //        linkPairList.RemoveAt(idx);
                //        isRemovedReferent = true;
                //    }
                //}

                //unbundleFD.ResetBundling();            

                //if (frustumDisplayed.Count > 0)
                //    unbundleFD.InitUnbundling(frustumDisplayed);

                isHighlighted = false;
            }
        }

        public void OnDisable()
        {
            Unhighlight();
        }

        public void OnDestroy()
        {
            Unhighlight();
        }

        public void UpdateVisualLink()
        {
            //unbundleFD.SetManageDisplayer(AlwaysVisualLink);
        }

        public static void VisMarksChanged()
        {
            marksDict = null;

            linkPairList?.Clear();
            //frustumDisplayed?.Clear();

            //unbundleFD?.ResetBundling();
        }
    }
}