using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public enum AisleArea { A1, A2, A3, A4, A5, None } //A14, A24, A34

        private Dictionary<AisleArea, List<GameObject>> ShelvesGroup;

        //private AisleArea LastUserAisle;
        private GameObject LastUserStep;

        private Graph graphSteps;
        public Transform PivotCalibration;

        //private Dictionary<AisleArea, bool> AislesVisited = new Dictionary<AisleArea, bool>();
        private List<KeyValuePair<GameObject, GameObject>> StepsFrustum = new List<KeyValuePair<GameObject, GameObject>>();
        private Dictionary<string, int> FrustumCount = new Dictionary<string, int>();
        private Dictionary<GameObject, int> EndSteps = new ();

        public List<GameObject> EndStepList = new();
        private List<int> Distances = new ();
        public int count = 0;

        public static OcclusionManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            RestartFrustum();
            InitializeShelvesGroup();
            InitializeGraphSteps();

            //LastUserAisle = GetPositionAisle(Camera.main.transform.position);
        }

        private AisleArea GetReferentAisle(Transform referent)
        {
            var aisle = ShelvesGroup.FirstOrDefault(shelves => shelves.Value.Contains(referent.parent.gameObject));
            return aisle.Key;
        }

        public List<KeyValuePair<GameObject, GameObject>> GetSteps(GameObject referent, Vector3 userPos, bool addOrRemove)
        {
            if (LastUserStep == null)
                return StepsFrustum;

            var referentAisle = GetReferentAisle(referent.transform);
            var stepRef = GetClosestStep(referent.transform.position, referentAisle);

            if (addOrRemove)
            {
                int idxClosest = VertexDic[stepRef];
                var stepsToClosest = DijkstraAlgorithm.ReturnPath(Distances, idxClosest);
                AddPathRouting(stepsToClosest, stepRef);
                count++;
            }  
            else
            {
                if (EndSteps.ContainsKey(stepRef) && EndSteps[stepRef] > 0)
                {
                    EndSteps[stepRef]--;
                    
                }

                if (EndSteps.ContainsKey(stepRef) && EndSteps[stepRef] == 0)
                {
                    EndSteps.Remove(stepRef);
                    EndStepList.Remove(stepRef);
                }    

                RecalculatePath(userPos);
                count--;
                //RemovePathRouting(stepsToClosest, stepRef);
            }
                

            return StepsFrustum;
        }

        private List<GameObject> AddPathRouting(Stack<int> path, GameObject stepRef, bool recalculating = false)
        {
            if (!recalculating)
            {
                if (!EndSteps.ContainsKey(stepRef))
                    EndSteps.Add(stepRef, 1);
                else
                    EndSteps[stepRef]++;

                if (!EndStepList.Contains(stepRef))
                    EndStepList.Add(stepRef);
            }
            
            var steps = new List<GameObject>();
            GameObject previous = null;

            while (path.Count > 0)
            {
                var idx = path.Pop();
                var step = VertexDic.FirstOrDefault(kv => kv.Value == idx);

                if (steps.Count > 0)
                {
                    var edge = previous.name + "_" + step.Key.name;
                    if (StepsFrustum.Count(p => p.Key == previous && p.Value == step.Key)  == 0)
                    {
                        StepsFrustum.Add(new KeyValuePair<GameObject, GameObject>(previous, step.Key));
                        FrustumCount.Add(edge, 1);
                    }
                    else if (StepsFrustum.Count(p => p.Key == previous && p.Value == step.Key) > 0)
                        FrustumCount[edge]++;
                }

                previous = step.Key;
                steps.Add(previous);
            }

            return steps;
        }

        public List<KeyValuePair<GameObject, GameObject>> RecalculatePath(Vector3 userPos)
        {
            LastUserStep = GetClosestStep(userPos);
            if (LastUserStep == null)
                return StepsFrustum;

            var distancePaths = DijkstraAlgorithm.Dijkstra(graphSteps, VertexDic[LastUserStep]);
            Distances = distancePaths.Item2.ToList();

            RestartFrustum();

            for (int i = 0; i < EndSteps.Count;i++)
            {
                var gO = EndSteps.ElementAt(i);
                if (gO.Value > 0)
                {
                    var stepsToClosest = DijkstraAlgorithm.ReturnPath(Distances, VertexDic[gO.Key]);
                    for (int j = 0; j < EndSteps[gO.Key]; j++)
                    {
                        var copy = stepsToClosest.Clone();
                        AddPathRouting(copy, gO.Key, true);
                    }
                }
            }
            
            return StepsFrustum;
        }

        //private List<string> RemovePathRouting(GameObject stepRef)
        //{
        //    if (EndSteps.ContainsKey(stepRef) && EndSteps[stepRef] > 0)
        //    {
        //        EndSteps[stepRef]--;
        //        EndStepList.Remove(stepRef);
        //    }

        //    var stepsToRemove = new List<string>();
        //    GameObject previous = null;

        //    while (path.Count > 0)
        //    {
        //        var idx = path.Pop();
        //        var step = VertexDic.FirstOrDefault(kv => kv.Value == idx);

        //        if (previous != null)
        //        {
        //            var edge = previous.name + "_" + step.Key.name;
        //            if (!FrustumCount.ContainsKey(edge))
        //                Debug.LogError("[OcclusionManager]::edge::No exist::" + edge);
        //            else if (FrustumCount[edge] == 0)
        //                Debug.LogError("[OcclusionManager]::edge::ZERO::" + edge);
        //            else
        //            {
        //                FrustumCount[edge]--;

        //                if (FrustumCount[edge] == 0)
        //                {
        //                    var index = StepsFrustum.IndexOf(new KeyValuePair<GameObject, GameObject>(previous, step.Key));
        //                    StepsFrustum.RemoveAt(index);
        //                    stepsToRemove.Add(edge);
        //                }
        //            }
        //        }

        //        previous = step.Key;
        //    }

        //    foreach (var k in stepsToRemove)
        //        FrustumCount.Remove(k);

        //    return stepsToRemove;
        //}

        private void InitializeShelvesGroup()
        {
            ShelvesGroup = new Dictionary<AisleArea, List<GameObject>>()
            {
                { AisleArea.A1, new List<GameObject>() { Shelf_181716 } },
                { AisleArea.A2, new List<GameObject>() { Shelf_234, Shelf_765 } },
                { AisleArea.A3, new List<GameObject>() { Shelf_14131 } },
                { AisleArea.A4, new List<GameObject>() { Shelf_101112 } },
                { AisleArea.A5, new List<GameObject>() }
            };
        }

        public void RestartFrustum()
        {
            StepsFrustum = new ();
            FrustumCount = new ();
            //EndStepList = new();
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

        public bool DoINeedNewPath()
        {
            var userPos = Camera.main.transform.position;
            var newUserStep = GetClosestStep(userPos);

            if (newUserStep == null)
                return false;

            if (LastUserStep != newUserStep)
            {
                LastUserStep = newUserStep;
                return true;
            }

            return false;
        }

        public GameObject GetStepReferent(Transform referent)
        {
            var Position = referent.position;
            var currentAisle = GetReferentAisle(referent);
            var StepsByAisle = GetStepsFromAisle(currentAisle);

            if (StepsByAisle == null)
                return null;

            var sorted = StepsByAisle.OrderBy(pos => (Position - pos.transform.position).sqrMagnitude);
            return sorted.FirstOrDefault();
        }

        private GameObject GetClosestStep(Vector3 Position, AisleArea Area = AisleArea.None)
        {
            var currentAisle = Area == AisleArea.None? GetPositionAisle(Position) : Area;

            var StepsByAisle = GetStepsFromAisle(currentAisle);

            if (StepsByAisle == null)
                return null;

            var sorted = StepsByAisle.OrderBy(pos => (Position - pos.transform.position).sqrMagnitude);
            return sorted.FirstOrDefault();
        }

        public AisleArea GetPositionAisle(Vector3 Position)
        {
            var CenterPivot = PivotCalibration.position;

            //Aisle 1   
            var Pos0 = CenterPivot + new Vector3(2.5f, 0, -1.5f);
            var Pos1 = CenterPivot + new Vector3(1.05f, 0, 1.5f);

            //Aisle 2
            var Pos2 = CenterPivot + new Vector3(0.6f, 0, -1.5f);
            var Pos3 = CenterPivot + new Vector3(-0.75f, 0, 1.5f);

            //Aisle 3
            var Pos4 = CenterPivot + new Vector3(-1.2f, 0, -1.5f);
            var Pos5 = CenterPivot + new Vector3(-2.5f, 0, 1.5f);

            //Aisle 4
            var Pos6 = CenterPivot + new Vector3(2.5f, 0, -2.7f);
            var Pos7 = CenterPivot + new Vector3(-2.5f, 0, -1.5f);

            //Aisle 5
            var Pos8 = CenterPivot + new Vector3(2.5f, 0, 1.5f);
            var Pos9 = CenterPivot + new Vector3(-2.5f, 0, 3f);

            if (Position.x < Pos0.x && Position.x > Pos1.x &&
                Position.z > Pos0.z && Position.z < Pos1.z)
                    return AisleArea.A1;

            if (Position.x < Pos2.x && Position.x > Pos3.x && 
                Position.z > Pos2.z && Position.z < Pos3.z)
                    return AisleArea.A2;

            if (Position.x < Pos4.x && Position.x > Pos5.x &&
                Position.z > Pos4.z && Position.z < Pos5.z)
                    return AisleArea.A3;

            if (Position.x > Pos7.x && Position.x < Pos6.x &&
                Position.z < Pos7.z && Position.z > Pos6.z)
                return AisleArea.A4;

            if (Position.x < Pos8.x && Position.x > Pos9.x &&
                Position.z > Pos8.z && Position.z < Pos9.z)
                return AisleArea.A5;

            return AisleArea.None;
        }

        private void OnDestroy()
        {
            count = 0;
            FrustumCount.Clear();
            StepsFrustum.Clear();
            EndSteps.Clear();
            EndStepList.Clear();
        }

        private void OnDisable()
        {
            count = 0;
            FrustumCount.Clear();
            StepsFrustum.Clear();
            EndSteps.Clear();
            EndStepList.Clear();
        }
    }
}
    
