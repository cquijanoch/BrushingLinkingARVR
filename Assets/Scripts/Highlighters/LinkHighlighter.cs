using System.Collections.Generic;
using DxR;
using UnityEngine;

namespace BrushingAndLinking
{
    public class LinkHighlighter : Highlighter
    {
        public override HighlightTechnique Mode { get { return HighlightTechnique.Link; } }

        private static Dictionary<string, Mark> marksDict;
        private static UnbundleFD unbundleFD;
        private static List<KeyValuePair<GameObject, GameObject>> linkPairList = new ();

        private GameObject markGameObject;

        //private bool isHighlighted = false;

        private void Start()
        {
            if (unbundleFD == null)
                unbundleFD = FindObjectOfType<UnbundleFD>();

            if (!unbundleFD.enabled)
                unbundleFD.enabled = true;

            unbundleFD.SetManageDisplayer(AlwaysVisualLink);

            if (marksDict == null)
                CreateMarksDictionary();

            markGameObject = marksDict[gameObject.name].gameObject;
        }

        private void CreateMarksDictionary()
        {
            marksDict = new Dictionary<string, Mark>();
            var marks = FindObjectsOfType<Mark>();
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
                linkPairList.Add(new KeyValuePair<GameObject, GameObject>(gameObject, markGameObject));
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
                    if (linkPairList[i].Key == gameObject)
                    {
                        linkPairList.RemoveAt(i);
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

        public static void VisMarksChanged()
        {
            marksDict = null;

            linkPairList?.Clear();
            unbundleFD?.ResetBundling();
        }
    }
}