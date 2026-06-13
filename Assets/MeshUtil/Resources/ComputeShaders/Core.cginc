struct Vertex
{
    float3 pt;
    uint tag;
    uint id;
};

struct Triangle
{
    Vertex v0;
    Vertex v1;
    Vertex v2;
    float3 n;
    uint tag;
};

#define ACCURACY 0.000001f

bool AlmostEqual(float3 p0, float3 p1)
{
    return abs(p0.x-p1.x)<ACCURACY && abs(p0.y-p1.y)<ACCURACY && abs(p0.z-p1.z)<ACCURACY;
}

void Set(RWStructuredBuffer<Triangle> buffer, int target, float3 p0, float3 p1, float3 p2, float3 tn, int ttag, int t0, int t1, int t2)
{
    Triangle t;
    t.v0.id = -1;
    t.v0.pt = p0;
    t.v0.tag = t0;
    t.v1.id = -1;
    t.v1.pt = p1;
    t.v1.tag = t1;
    t.v2.id = -1;
    t.v2.pt = p2;
    t.v2.tag = t2;
    t.tag = ttag;
    t.n = tn;
    buffer[target] = t;
}

void Add(RWStructuredBuffer<Triangle> triangles,RWStructuredBuffer<int> counters,int cntidx,float3 p0, float3 p1, float3 p2, float3 tn, int ttag, int t0,int t1, int t2)
{
    if(AlmostEqual(p1,p0) || AlmostEqual(p2,p0) || AlmostEqual(p2,p1))
        return;
    int index;
    InterlockedAdd(counters[cntidx], 1, index);
    Set(triangles,index,p0, p1, p2, tn, ttag, t0,t1,t2);
}

