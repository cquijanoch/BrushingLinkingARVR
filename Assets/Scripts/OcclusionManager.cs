using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OVRPlugin;

namespace BrushingAndLinking
{
    public class OcclusionManager : MonoBehaviour
    {
        [Header("Steps points for aisle 4")]
        public GameObject StepPoint41;
        public GameObject StepPoint42;
        public GameObject StepPoint10;
        public GameObject StepPoint11;
        public GameObject StepPoint12;

        [Header("Steps points for aisle 5")]
        public GameObject StepPoint51;
        public GameObject StepPoint52;
        public GameObject StepPoint53;

        [Header("Steps points for aisle 1")]
        public GameObject StepPoint18;
        public GameObject StepPoint17;
        public GameObject StepPoint16;

        [Header("Steps points for aisle 2")]
        public GameObject StepPoint2;
        public GameObject StepPoint3;
        public GameObject StepPoint4;

        [Header("Steps points for aisle 3")]
        public GameObject StepPoint1;
        public GameObject StepPoint13;
        public GameObject StepPoint14;

        private Dictionary<GameObject, int> VertexDic = new Dictionary<GameObject, int>();

        [Header("Shelves")]
        public GameObject Shelf_181716; //Aisle #1
        public GameObject Shelf_234;// Aisle #2
        public GameObject Shelf_765;// Aisle #2
        public GameObject Shelf_14131; // Aisle #3
        public GameObject Shelf_101112;// Aisle #4

        //private enum ShelfGroup { G1, G2, G3, G4 }
        public enum AisleArea { A1, A2, A3, A4, A5, None } //A14, A24, A34

        private Dictionary<AisleArea, List<GameObject>> ShelvesGroup;
        //private Dictionary<AisleArea, List<ShelfGroup>> AisleShelvesConnections;

        private AisleArea CurrentUserAisle;
        private Graph graphSteps;

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
            InitializeGraphSteps();

            CurrentUserAisle = AisleArea.None;
        }

        private void LateUpdate()
        {
            var previousAisle = CurrentUserAisle;
            CurrentUserAisle = GetPositionAisle(Camera.main.transform.position);
            if (CurrentUserAisle != AisleArea.None)
            {
                if (previousAisle != CurrentUserAisle)
                    HighlightManager.Instance.ReHighlightProducts(); 
            }
        }

        private AisleArea GetReferentAisle(Transform referent)
        {
            var aisle = ShelvesGroup.FirstOrDefault(shelves => shelves.Value.Contains(referent.parent.gameObject));
            return aisle.Key;
        }

        public List<GameObject> GetSteps(GameObject referent, Vector3 userPos)
        {
            return GetSteps(new List<GameObject>() { referent }, userPos);
        }

        public List<GameObject> GetSteps(List<GameObject> referents, Vector3 userPos)
        {
            CurrentUserAisle = GetPositionAisle(userPos);

            List<AisleArea> aislesReferents = new List<AisleArea>();

            foreach (var refObj in referents)
                aislesReferents.Add(GetReferentAisle(refObj.transform));
            
            if (CurrentUserAisle == AisleArea.None || aislesReferents.Count == 0 || aislesReferents.TrueForAll(a => a == AisleArea.None))
                return new List<GameObject>();

            if (aislesReferents.Distinct().Count() == 1)
            {
                foreach (var refObj in referents)
                {
                    var stepUser = GetClosestStep(userPos);

                    DijkstraAlgorithm.Dijkstra(graphSteps, VertexDic[stepUser]);

                }
                    //GetStepsFromAisle(aislesReferents.First())
            }
            //var ReferentPoint = GetClosestStep(referent.transform.position, PivotCalibration.position);
            //var UserPoint = GetClosestStep(userPos, PivotCalibration.position);

            //graphSteps

            return new List<GameObject>();
        }

        private void InitializeShelvesGroup()
        {
            ShelvesGroup = new Dictionary<AisleArea, List<GameObject>>();
            ShelvesGroup.Add(AisleArea.A1, new List<GameObject>() { Shelf_181716 });
            ShelvesGroup.Add(AisleArea.A2, new List<GameObject>() { Shelf_234, Shelf_765 });
            ShelvesGroup.Add(AisleArea.A3, new List<GameObject>() { Shelf_14131 });
            ShelvesGroup.Add(AisleArea.A4, new List<GameObject>() { Shelf_101112 });
            ShelvesGroup.Add(AisleArea.A5, new List<GameObject>());
        }

        //private void InitializeShelvesConnections()
        //{
        //    AisleShelvesConnections = new Dictionary<AisleArea, List<ShelfGroup>>();
        //    AisleShelvesConnections.Add(AisleArea.A1, new List<ShelfGroup>() { ShelfGroup.G1 });
        //    AisleShelvesConnections.Add(AisleArea.A2, new List<ShelfGroup>() { ShelfGroup.G2 });
        //    AisleShelvesConnections.Add(AisleArea.A3, new List<ShelfGroup>() { ShelfGroup.G3 });
        //    AisleShelvesConnections.Add(AisleArea.A4, new List<ShelfGroup>() { ShelfGroup.G4 });
        //    ///AisleShelvesConnections.Add(AisleArea.A14, new List<ShelfGroup>() { ShelfGroup.G1 , ShelfGroup.G4 });
        //    //AisleShelvesConnections.Add(AisleArea.A24, new List<ShelfGroup>() { ShelfGroup.G2, ShelfGroup.G4 });
        //    //AisleShelvesConnections.Add(AisleArea.A34, new List<ShelfGroup>() { ShelfGroup.G3, ShelfGroup.G4 });
        //}

        private void InitializeGraphSteps()
        {
            VertexDic.Add(StepPoint41, 0);
            VertexDic.Add(StepPoint10, 1);
            VertexDic.Add(StepPoint11, 2);
            VertexDic.Add(StepPoint12, 3);
            VertexDic.Add(StepPoint42, 4);

            VertexDic.Add(StepPoint51, 5);
            VertexDic.Add(StepPoint52, 6);
            VertexDic.Add(StepPoint53, 7);

            VertexDic.Add(StepPoint18, 8);
            VertexDic.Add(StepPoint17, 9);
            VertexDic.Add(StepPoint16, 10);

            VertexDic.Add(StepPoint2, 11);
            VertexDic.Add(StepPoint3, 12);
            VertexDic.Add(StepPoint4, 13);

            VertexDic.Add(StepPoint1, 14);
            VertexDic.Add(StepPoint13, 15);
            VertexDic.Add(StepPoint14, 16);

            graphSteps = new Graph(VertexDic.Count);
            graphSteps.AddEdge(VertexDic[StepPoint41], VertexDic[StepPoint10], 1);
            graphSteps.AddEdge(VertexDic[StepPoint10], VertexDic[StepPoint41], 1);
            graphSteps.AddEdge(VertexDic[StepPoint10], VertexDic[StepPoint11], 1);
            graphSteps.AddEdge(VertexDic[StepPoint11], VertexDic[StepPoint10], 1);
            graphSteps.AddEdge(VertexDic[StepPoint11], VertexDic[StepPoint12], 1);
            graphSteps.AddEdge(VertexDic[StepPoint12], VertexDic[StepPoint11], 1);
            graphSteps.AddEdge(VertexDic[StepPoint12], VertexDic[StepPoint42], 1);
            graphSteps.AddEdge(VertexDic[StepPoint42], VertexDic[StepPoint12], 1);

            graphSteps.AddEdge(VertexDic[StepPoint41], VertexDic[StepPoint18], 1);
            graphSteps.AddEdge(VertexDic[StepPoint18], VertexDic[StepPoint41], 1);

            graphSteps.AddEdge(VertexDic[StepPoint18], VertexDic[StepPoint17], 1);
            graphSteps.AddEdge(VertexDic[StepPoint17], VertexDic[StepPoint18], 1);
            graphSteps.AddEdge(VertexDic[StepPoint17], VertexDic[StepPoint16], 1);
            graphSteps.AddEdge(VertexDic[StepPoint16], VertexDic[StepPoint17], 1);

            graphSteps.AddEdge(VertexDic[StepPoint11], VertexDic[StepPoint2], 1);
            graphSteps.AddEdge(VertexDic[StepPoint2], VertexDic[StepPoint11], 1);

            graphSteps.AddEdge(VertexDic[StepPoint2], VertexDic[StepPoint3], 1);
            graphSteps.AddEdge(VertexDic[StepPoint3], VertexDic[StepPoint2], 1);
            graphSteps.AddEdge(VertexDic[StepPoint3], VertexDic[StepPoint4], 1);
            graphSteps.AddEdge(VertexDic[StepPoint4], VertexDic[StepPoint3], 1);

            graphSteps.AddEdge(VertexDic[StepPoint42], VertexDic[StepPoint1], 1);
            graphSteps.AddEdge(VertexDic[StepPoint1], VertexDic[StepPoint42], 1);

            graphSteps.AddEdge(VertexDic[StepPoint1], VertexDic[StepPoint13], 1);
            graphSteps.AddEdge(VertexDic[StepPoint13], VertexDic[StepPoint1], 1);
            graphSteps.AddEdge(VertexDic[StepPoint13], VertexDic[StepPoint14], 1);
            graphSteps.AddEdge(VertexDic[StepPoint14], VertexDic[StepPoint13], 1);

            graphSteps.AddEdge(VertexDic[StepPoint51], VertexDic[StepPoint16], 1);
            graphSteps.AddEdge(VertexDic[StepPoint16], VertexDic[StepPoint51], 1);
            graphSteps.AddEdge(VertexDic[StepPoint4], VertexDic[StepPoint52], 1);
            graphSteps.AddEdge(VertexDic[StepPoint52], VertexDic[StepPoint4], 1);
            graphSteps.AddEdge(VertexDic[StepPoint14], VertexDic[StepPoint53], 1);
            graphSteps.AddEdge(VertexDic[StepPoint53], VertexDic[StepPoint14], 1);

            graphSteps.AddEdge(VertexDic[StepPoint51], VertexDic[StepPoint52], 2);
            graphSteps.AddEdge(VertexDic[StepPoint52], VertexDic[StepPoint51], 2);
            graphSteps.AddEdge(VertexDic[StepPoint52], VertexDic[StepPoint53], 2);
            graphSteps.AddEdge(VertexDic[StepPoint53], VertexDic[StepPoint52], 2);
        }

        private List<GameObject> GetStepsFromAisle(AisleArea aisle)
        {
            switch (aisle)
            {
                case AisleArea.A1: 
                    return new List<GameObject>() { StepPoint18, StepPoint17, StepPoint16 };
                case AisleArea.A2:
                    return new List<GameObject>() { StepPoint2, StepPoint3, StepPoint4 };
                case AisleArea.A3:
                    return new List<GameObject>() { StepPoint1, StepPoint13, StepPoint14 };
                case AisleArea.A4:
                    return new List<GameObject>() { StepPoint41, StepPoint10, StepPoint11, StepPoint12, StepPoint42 };
                case AisleArea.A5:
                    return new List<GameObject>() { StepPoint51, StepPoint52, StepPoint53 };
            }
            
            return null;
        }

        private GameObject GetClosestStep(Vector3 Position)
        {
            var currentAisle = GetPositionAisle(Position);

            var StepByAisle = GetStepsFromAisle(currentAisle);

            if (StepByAisle == null)
                return null;

            var sorted = StepByAisle.OrderBy(pos => (Position - pos.transform.position).sqrMagnitude);
            return sorted.FirstOrDefault();
        }

        public AisleArea GetPositionAisle(Vector3 Position)
        {
            var CenterPivot = PivotCalibration.position;

            //Aisle 1   
            var Pos0 = CenterPivot + new Vector3(1.8f, 0, -1.5f);
            var Pos1 = CenterPivot + new Vector3(1.05f, 0, 1.5f);

            //Aisle 2
            var Pos2 = CenterPivot + new Vector3(0.6f, 0, -1.5f);
            var Pos3 = CenterPivot + new Vector3(-0.75f, 0, 1.5f);

            //Aisle 3
            var Pos4 = CenterPivot + new Vector3(-1.2f, 0, -1.5f);
            var Pos5 = CenterPivot + new Vector3(-2f, 0, 1.5f);

            //Aisle 4
            var Pos6 = CenterPivot + new Vector3(1.8f, 0, -2.7f);
            var Pos7 = CenterPivot + new Vector3(-2f, 0, -1.5f);

            //Aisle 24
            //var Pos8 = CenterPivot + new Vector3(0.6f, 0, -2.7f);
            //var Pos9 = CenterPivot + new Vector3(-0.75f, 0, -1.5f);

            //Aisle 34
            //var Pos10 = CenterPivot + new Vector3(-1.2f, 0, -2.7f);
            //var Pos11 = CenterPivot + new Vector3(-2f, 0, -1.5f);


            if (Position.x < Pos0.x && Position.x > Pos1.x)
            {
                if (Position.z > Pos0.z && Position.z < Pos1.z)
                    return AisleArea.A1;
            }

            if (Position.x < Pos2.x && Position.x > Pos3.x)
            {
                if (Position.z > Pos2.z && Position.z < Pos3.z)
                    return AisleArea.A2;
            }

            if (Position.x < Pos4.x && Position.x > Pos5.x)
            {
                if (Position.z > Pos4.z && Position.z < Pos5.z)
                    return AisleArea.A3;
            }

            if (Position.x > Pos7.x && Position.x < Pos6.x &&
                Position.z < Pos7.z && Position.z > Pos6.z)
                return AisleArea.A4;

            return AisleArea.None;
        }
    }
}
    
