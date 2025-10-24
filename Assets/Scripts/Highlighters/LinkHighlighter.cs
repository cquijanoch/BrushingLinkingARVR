using DxR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrushingAndLinking
{
    [DisallowMultipleComponent]
    public class LinkHighlighter : Highlighter
    {
        public override HighlightTechnique Mode { get { return HighlightTechnique.Link; } }

        private static Dictionary<string, Mark> marksDict;
        private static UnbundleFD unbundleFD;
        private static List<KeyValuePair<GameObject, GameObject>> linkPairList = new ();

        private GameObject markGameObject;
        private List<GameObject> stepsPoints;
        private GameObject targetReferent;

        //private bool isHighlighted = false;

        private void Start()
        {
            if (unbundleFD == null)
                unbundleFD = FindAnyObjectByType<UnbundleFD>();

            if (!unbundleFD.enabled)
                unbundleFD.enabled = true;

            AlwaysVisualLink = true;
            UpdateVisualLink();

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
                stepsPoints = OcclusionManager.Instance.GetSteps(gameObject, Camera.main.transform.position);
                GameObject pNode = targetReferent;
              
                for (int i = 0; i < stepsPoints.Count; i++)
                {
                    linkPairList.Add(new KeyValuePair<GameObject, GameObject>(pNode, stepsPoints[i]));
                    pNode = stepsPoints[i];
                }   
               
                linkPairList.Add(new KeyValuePair<GameObject, GameObject>(pNode, markGameObject));
                unbundleFD.ResetBundling();
                unbundleFD.InitUnbundling(linkPairList);

                isHighlighted = true;
            }
        }

        public override void Unhighlight()
        {
            if (isHighlighted)
            {
                bool isRemoved = false;
                for (int i = 0; i < linkPairList.Count; i++)
                {
                    var idxs = new List<int>();
                    if (linkPairList[i].Key == targetReferent)
                    {
                        if (stepsPoints.Count == 2)
                        {
                            var idx1 = linkPairList.FindIndex(o => o.Key == stepsPoints[0] && o.Value == stepsPoints[1]);
                            var idx2 = linkPairList.FindIndex(o => o.Key == stepsPoints[1] && o.Value == markGameObject);
                            idxs.Add(idx1);
                            idxs.Add(idx2);
                        }

                        if (stepsPoints.Count == 1)
                        {
                            var idx = linkPairList.FindIndex(o => o.Key == stepsPoints[0] && o.Value == markGameObject);
                            idxs.Add(idx);
                        }

                        idxs.Add(i);

                        foreach (var idx in idxs.OrderByDescending(c => c).ToArray())
                            linkPairList.RemoveAt(idx);

                        isRemoved = true;
                        break;
                    }
                }

                if (isRemoved)
                {
                    unbundleFD.ResetBundling();
                    if (linkPairList.Count > 0)
                        unbundleFD.InitUnbundling(linkPairList);
                }

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
            unbundleFD.SetManageDisplayer(AlwaysVisualLink);
        }

        public static void VisMarksChanged()
        {
            marksDict = null;

            linkPairList?.Clear();
            unbundleFD?.ResetBundling();
        }
    }
}