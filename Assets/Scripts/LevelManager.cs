using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public bool SHOW_COLLIDER = true; //$$
    public static LevelManager Instance { set; get; }

    //Level Spawning
    private const float DISTANCE_BEFORE_SPAWN = 100.0f;
    private const int INITIAL_SEGMENTS = 10;
    private const int INITIAL_TRANSITION_SEGMENTS = 2;
    private const int MAX_SEGMENTS_ON_SCREEN = 15;
    private Transform cameraContainer;
    private int amountofActiveSegments;
    private int continuousSegments;
    private int currentSpawnz;
    private int currentLevel;
    private int y1, y2, y3;
    [HideInInspector]
    public bool standOffGenerated = false;

    //List of pieces
    public List<Piece> ramps = new List<Piece>();
    public List<Piece> longblocks = new List<Piece>();
    public List<Piece> jumps = new List<Piece>();
    public List<Piece> slides = new List<Piece>();
    public List<Piece> enemy = new List<Piece>();
    [HideInInspector]
    public List<Piece> pieces = new List<Piece>();  //All the pieces in the pool

    //List of Segments
    public List<Segment> availableSegments = new List<Segment>();
    public List<Segment> availableTransitions = new List<Segment>();
    public List<Segment> availablestandOffSegments = new List<Segment>();
    [HideInInspector]
    public List<Segment> segments = new List<Segment>();

    //Gameplay
    private bool isMoving = false;

    private void Awake()
    {
        Instance = this;
        cameraContainer = Camera.main.transform;
        currentSpawnz = 0;
        currentLevel = 0;
    }

    private void Start()
    {
        for (int i = 0; i < INITIAL_SEGMENTS; i++)
        {
            if (i < INITIAL_TRANSITION_SEGMENTS)
                SpawnTransition();
            else
                GenerateSegment();
        }
    }

    private void Update()
    {
        if (currentSpawnz - cameraContainer.position.z < DISTANCE_BEFORE_SPAWN)
            GenerateSegment();

        if(amountofActiveSegments>=MAX_SEGMENTS_ON_SCREEN)
        {
            segments[amountofActiveSegments - 1].DeSpawn();
            amountofActiveSegments--;
        }
    }

    private void GenerateSegment()
    {

        if (GameManager.Instance.standOff && !standOffGenerated)
        {
            standOffGenerated = true;
            SpawnStandOff();
        }
        else
        {
            SpawnSegment();
        }

        if ((UnityEngine.Random.Range(0f, 1f) < (continuousSegments * 0.25f)) && !GameManager.Instance.standOff)
        {
            //Spawn transition Segment
            continuousSegments = 0;
            SpawnTransition();
        }
        else
        { 
            continuousSegments++;
        }
    }

    private void SpawnStandOff()
    {
        List<Segment> possibleStandOffSegment = availablestandOffSegments.FindAll(x => x.beginY1 == y1 || x.beginY2 == y2 || x.beginY3 == y3 || x.beginY1 != y1 || x.beginY2 != y2 || x.beginY3 != y3);
        int id = UnityEngine.Random.Range(0, possibleStandOffSegment.Count);

        Segment s = GetStandOffSegment(id, false);

        y1 = s.endY1;
        y2 = s.endY2;
        y3 = s.endY3;

        s.transform.SetParent(transform);
        s.transform.localPosition = Vector3.forward * currentSpawnz;

        currentSpawnz += s.length;
        amountofActiveSegments++;
        s.Spawn();
        GameManager.Instance.standOff = false;
    }
    private void SpawnTransition()
    {
        List<Segment> possibleTransition = availableTransitions.FindAll(x => x.beginY1 == y1 || x.beginY2 == y2 || x.beginY3 == y3);
        int id = UnityEngine.Random.Range(0, possibleTransition.Count);

        Segment s = GetSegment(id, true);

        y1 = s.endY1;
        y2 = s.endY2;
        y3 = s.endY3;

        s.transform.SetParent(transform);
        s.transform.localPosition = Vector3.forward * currentSpawnz;

        currentSpawnz += s.length;
        amountofActiveSegments++;
        s.Spawn();
    }

    private void SpawnSegment()
    {
        List<Segment> possibleSeg = availableSegments.FindAll(x => x.beginY1 == y1 || x.beginY2 == y2 || x.beginY3 == y3);
        int id = UnityEngine.Random.Range(0, possibleSeg.Count);

        Segment s = GetSegment(id, false);

        y1 = s.endY1;
        y2 = s.endY2;
        y3 = s.endY3;

        s.transform.SetParent(transform);
        s.transform.localPosition = Vector3.forward * currentSpawnz;

        currentSpawnz += s.length;
        amountofActiveSegments++;
        s.Spawn();
    }

    public Segment GetSegment(int id,bool transition)
    {
        Segment s = null;
        s = segments.Find(x => x.SegId == id && x.transition == transition && !x.gameObject.activeSelf);

        if(s==null)
        {
            GameObject go = Instantiate((transition) ? availableTransitions[id].gameObject : availableSegments[id].gameObject) as GameObject;
            s = go.GetComponent<Segment>();

            s.SegId = id;
            s.transition = transition;

            segments.Insert(0, s);
        }
        else
        {
            segments.Remove(s);
            segments.Insert(0, s);
        }

        return s;
    }

    public Segment GetStandOffSegment(int id,bool transition)
    {
        Segment s = null;
        s = segments.Find(x => x.SegId == id && x.transition == transition && !x.gameObject.activeSelf);

        if (s == null)
        {
            GameObject go = Instantiate(availablestandOffSegments[id].gameObject) as GameObject;
            s = go.GetComponent<Segment>();

            s.SegId = id;
            s.transition = transition;

            segments.Insert(0, s);
        }
        else
        {
            segments.Remove(s);
            segments.Insert(0, s);
        }

        return s;
    }

    public Piece GetPiece(PieceType pt,int visualIndex)
    {
        Piece p = pieces.Find(x => x.type == pt && x.visualIndex == visualIndex && !x.gameObject.activeSelf);

        if(p==null)
        {
            GameObject go = null;
            if (pt == PieceType.ramp)
                go = ramps[visualIndex].gameObject;
            else if(pt==PieceType.longblock)
                go = longblocks[visualIndex].gameObject;
            else if (pt == PieceType.jump)
                go = jumps[visualIndex].gameObject;
            else if (pt == PieceType.slide)
                go = slides[visualIndex].gameObject;
            else if (pt == PieceType.enemy)
                go = enemy[visualIndex].gameObject;

            go = Instantiate(go);
            p = go.GetComponent<Piece>();
            pieces.Add(p);
        }

        return p;
    }
}
