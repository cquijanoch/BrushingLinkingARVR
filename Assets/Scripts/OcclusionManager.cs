using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrushingAndLinking
{
    public class OcclusionManager : MonoBehaviour
    {
        [Header("Steps points for aisle area")]
        public GameObject StepAisle1;
        public GameObject StepAisle11;
        public GameObject StepAisle2;
        public GameObject StepAisle21;
        public GameObject StepAisle3;
        public GameObject StepAisle31;

        private Dictionary<GameObject, int> VertexDic = new Dictionary<GameObject, int>();

        [Header("Shelves")]
        public GameObject Shelf_181716; //Aisle #1
        public GameObject Shelf_234;// Aisle #2
        public GameObject Shelf_765;// Aisle #2
        public GameObject Shelf_14131; // Aisle #3
        public GameObject Shelf_101112;// Aisle #4

        private enum ShelfGroup { G1, G2, G3, G4 }
        public enum AisleArea { A1, A2, A3, A4, A14, A24, A34, None }

        private Dictionary<ShelfGroup, List<GameObject>> ShelvesGroup;
        private Dictionary<AisleArea, List<ShelfGroup>> AisleShelvesConnections;

        private AisleArea CurrentUserArea;

        public Transform PivotCalibration;

        public static OcclusionManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            InitializeShelvesGroup();
            InitializeShelvesConnections();
            InitializeGraphSteps();

            CurrentUserArea = AisleArea.None;
        }

        private void LateUpdate()
        {
            var previousAisle = CurrentUserArea;
            CurrentUserArea = GetCurrentAisle(Camera.main.transform.position, PivotCalibration.position);
            if (CurrentUserArea != AisleArea.None)
            {
                if (previousAisle != CurrentUserArea)
                    HighlightManager.Instance.ReHighlightProducts(); 
            }
        }

        public List<GameObject> GetSteps(GameObject referent, Vector3 userPos)
        {
            CurrentUserArea = GetCurrentAisle(userPos, PivotCalibration.position);

            if (CurrentUserArea == AisleArea.None)
                return new List<GameObject>();

            var shelfGroup = ShelvesGroup.FirstOrDefault(s => s.Value.Contains(referent.transform.parent.gameObject)).Key;

            if (AisleShelvesConnections[CurrentUserArea].Contains(shelfGroup))
                return new List<GameObject>();

            if (CurrentUserArea == AisleArea.A4 || CurrentUserArea == AisleArea.A14 || CurrentUserArea == AisleArea.A24 || CurrentUserArea == AisleArea.A34)
            {
                var shelfAisl = AisleShelvesConnections.FirstOrDefault(s => s.Value.Contains(shelfGroup)).Key;
                var stepAisle1 = GetStepsFromAisle(shelfAisl).Item1;
                var stepAisle2 = GetStepsFromAisle(shelfAisl).Item2;
                var distReferentToAisleStep1 = Vector3.Distance(referent.transform.position, stepAisle1.transform.position);
                var distReferentToAisleStep2 = Vector3.Distance(referent.transform.position, stepAisle2.transform.position);
                var distUserStep1 = distReferentToAisleStep1 + Vector3.Distance(userPos, stepAisle1.transform.position);
                var distUserStep2 = distReferentToAisleStep2 + Vector3.Distance(userPos, stepAisle2.transform.position);

                var closerStep = distUserStep1 < distUserStep2 ? stepAisle1 : stepAisle2;

                return new List<GameObject>() { closerStep };
            }

            var distRefToUserStep1 = Vector3.Distance(referent.transform.position, GetStepsFromAisle(CurrentUserArea).Item1.transform.position);
            var distRefToUserStep2 = Vector3.Distance(referent.transform.position, GetStepsFromAisle(CurrentUserArea).Item2.transform.position);

            var closestUserStep = distRefToUserStep1 < distRefToUserStep2 ? GetStepsFromAisle(CurrentUserArea).Item1 : GetStepsFromAisle(CurrentUserArea).Item2;
            if (shelfGroup == ShelfGroup.G4)
                return new List<GameObject>() { closestUserStep };

            var shelfAisle = AisleShelvesConnections.FirstOrDefault(s => s.Value.Contains(shelfGroup)).Key;
            var distUserStepToAisleStep1 = Vector3.Distance(closestUserStep.transform.position, GetStepsFromAisle(shelfAisle).Item1.transform.position);
            var distUserStepToAisleStep2 = Vector3.Distance(closestUserStep.transform.position, GetStepsFromAisle(shelfAisle).Item2.transform.position);

            var closestStep = distUserStepToAisleStep1 < distUserStepToAisleStep2 ? GetStepsFromAisle(shelfAisle).Item1 : GetStepsFromAisle(shelfAisle).Item2;

            return new List<GameObject>() { closestStep , closestUserStep };
        }

        private void InitializeShelvesGroup()
        {
            ShelvesGroup = new Dictionary<ShelfGroup, List<GameObject>>();
            ShelvesGroup.Add(ShelfGroup.G1, new List<GameObject>() { Shelf_181716 });
            ShelvesGroup.Add(ShelfGroup.G2, new List<GameObject>() { Shelf_234, Shelf_765 });
            ShelvesGroup.Add(ShelfGroup.G3, new List<GameObject>() { Shelf_14131 });
            ShelvesGroup.Add(ShelfGroup.G4, new List<GameObject>() { Shelf_101112 });
        }

        private void InitializeShelvesConnections()
        {
            AisleShelvesConnections = new Dictionary<AisleArea, List<ShelfGroup>>();
            AisleShelvesConnections.Add(AisleArea.A1, new List<ShelfGroup>() { ShelfGroup.G1 });
            AisleShelvesConnections.Add(AisleArea.A2, new List<ShelfGroup>() { ShelfGroup.G2 });
            AisleShelvesConnections.Add(AisleArea.A3, new List<ShelfGroup>() { ShelfGroup.G3 });
            AisleShelvesConnections.Add(AisleArea.A4, new List<ShelfGroup>() { ShelfGroup.G4 });
            AisleShelvesConnections.Add(AisleArea.A14, new List<ShelfGroup>() { ShelfGroup.G1 , ShelfGroup.G4 });
            AisleShelvesConnections.Add(AisleArea.A24, new List<ShelfGroup>() { ShelfGroup.G2, ShelfGroup.G4 });
            AisleShelvesConnections.Add(AisleArea.A34, new List<ShelfGroup>() { ShelfGroup.G3, ShelfGroup.G4 });
        }

        private void InitializeGraphSteps()
        {
            VertexDic.Add(StepAisle1, 0);
            VertexDic.Add(StepAisle11, 1);
            VertexDic.Add(StepAisle2, 2);
            VertexDic.Add(StepAisle21, 3);
            VertexDic.Add(StepAisle3, 4);
            VertexDic.Add(StepAisle31, 5);

            Graph graph = new Graph(VertexDic.Count);
            graph.AddEdge(VertexDic[StepAisle1], VertexDic[StepAisle11], (int)Math.Round(Vector3.Distance(StepAisle1.transform.position, StepAisle11.transform.position)));
            graph.AddEdge(VertexDic[StepAisle11], VertexDic[StepAisle1], (int)Math.Round(Vector3.Distance(StepAisle1.transform.position, StepAisle11.transform.position)));
            graph.AddEdge(VertexDic[StepAisle2], VertexDic[StepAisle21], (int)Math.Round(Vector3.Distance(StepAisle2.transform.position, StepAisle21.transform.position)));
            graph.AddEdge(VertexDic[StepAisle21], VertexDic[StepAisle2], (int)Math.Round(Vector3.Distance(StepAisle2.transform.position, StepAisle21.transform.position)));
            graph.AddEdge(VertexDic[StepAisle3], VertexDic[StepAisle31], (int)Math.Round(Vector3.Distance(StepAisle3.transform.position, StepAisle31.transform.position)));
            graph.AddEdge(VertexDic[StepAisle31], VertexDic[StepAisle3], (int)Math.Round(Vector3.Distance(StepAisle3.transform.position, StepAisle31.transform.position)));
            graph.AddEdge(VertexDic[StepAisle1], VertexDic[StepAisle2], (int)Math.Round(Vector3.Distance(StepAisle1.transform.position, StepAisle2.transform.position)));
            graph.AddEdge(VertexDic[StepAisle11], VertexDic[StepAisle21], (int)Math.Round(Vector3.Distance(StepAisle11.transform.position, StepAisle21.transform.position)));
            graph.AddEdge(VertexDic[StepAisle2], VertexDic[StepAisle3], (int)Math.Round(Vector3.Distance(StepAisle2.transform.position, StepAisle3.transform.position)));
            graph.AddEdge(VertexDic[StepAisle21], VertexDic[StepAisle31], (int)Math.Round(Vector3.Distance(StepAisle21.transform.position, StepAisle31.transform.position)));
        }

        private Tuple<GameObject, GameObject> GetStepsFromAisle(AisleArea aisle)
        {
            if (aisle == AisleArea.A1)
                return Tuple.Create(StepAisle1, StepAisle11);
            if (aisle == AisleArea.A2)
                return Tuple.Create(StepAisle2, StepAisle21);
            if (aisle == AisleArea.A3)
                return Tuple.Create(StepAisle3, StepAisle31);
            //if (aisle == AisleArea.A14)
            //    return Tuple.Create(StepAisle1, StepAisle2);
            //if (aisle == AisleArea.A24)
            //    return Tuple.Create(StepAisle2, StepAisle31);
            //if (aisle == AisleArea.A34)
            //    return Tuple.Create(StepAisle3, StepAisle31);

            return null;
        }

        public AisleArea GetCurrentAisle(Vector3 userPos, Vector3 CenterPivot)
        {
            //Aisle 1
            var Pos0 = CenterPivot + new Vector3(1.8f, 0, -1.5f);
            var Pos1 = CenterPivot + new Vector3(1.05f, 0, 1.5f);

            //Aisle 2
            var Pos2 = CenterPivot + new Vector3(0.6f, 0, -1.5f);
            var Pos3 = CenterPivot + new Vector3(-0.75f, 0, 1.5f);

            //Aisle 3
            var Pos4 = CenterPivot + new Vector3(-1.2f, 0, -1.5f);
            var Pos5 = CenterPivot + new Vector3(-2f, 0, 1.5f);

            //Aisle 14
            var Pos6 = CenterPivot + new Vector3(1.8f, 0, -2.7f);
            var Pos7 = CenterPivot + new Vector3(1.05f, 0, -1.5f);

            //Aisle 24
            var Pos8 = CenterPivot + new Vector3(0.6f, 0, -2.7f);
            var Pos9 = CenterPivot + new Vector3(-0.75f, 0, -1.5f);

            //Aisle 34
            var Pos10 = CenterPivot + new Vector3(-1.2f, 0, -2.7f);
            var Pos11 = CenterPivot + new Vector3(-2f, 0, -1.5f);


            if (userPos.x < Pos0.x && userPos.x > Pos1.x)
            {
                if (userPos.z > Pos0.z && userPos.z < Pos1.z)
                    return AisleArea.A1;
                else if (userPos.z > Pos6.z && userPos.z < Pos7.z)
                    return AisleArea.A14;
            }

            if (userPos.x < Pos2.x && userPos.x > Pos3.x)
            {
                if (userPos.z > Pos2.z && userPos.z < Pos3.z)
                    return AisleArea.A2;
                else if (userPos.z > Pos8.z && userPos.z < Pos9.z)
                    return AisleArea.A24;
            }

            if (userPos.x < Pos4.x && userPos.x > Pos5.x)
            {
                if (userPos.z > Pos4.z && userPos.z < Pos5.z)
                    return AisleArea.A3;
                else if (userPos.z > Pos10.z && userPos.z < Pos11.z)
                    return AisleArea.A34;
            }

            if (userPos.x < Pos7.x && userPos.x > Pos10.x &&
                userPos.z < Pos7.z && userPos.z > Pos10.z)
                return AisleArea.A4;

            return AisleArea.None;
        }
    }
}
    
