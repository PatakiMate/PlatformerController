using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CornerChecker : Singleton<CornerChecker>
{
    public bool update;
    public bool clear;
    public bool stop;
    public bool test;
    public List<GameObject> Corners;
    public GameObject CornerObject;
    public Transform OriginPoint;
    public float CheckWidth;
    public float CheckHeight;
    public float Steps;
    public List<Vector2> CheckOrigins = new List<Vector2>();
    public LayerMask CollisionMask;
    public float ExtraLenght;
    private Coroutine generator;
    public List<RayClass> rayClasses = new List<RayClass>();
    public List<RayClass> realRayClasses = new List<RayClass>();

    private List<Vector2> hitpoints = new List<Vector2>();
    public bool testRay;
    public float divider;

    [System.Serializable]
    public class RayClass
    {
        public Vector2 origin;
        public Vector2 direction1;
        public Vector2 direction2;
        public Color color1;
        public Color color2;
    }
    void Update()
    {
        if(update)
        {
            realRayClasses.Clear();
            foreach(RayClass rayClass in rayClasses)
            {
                RayClass ray = new RayClass();
                ray.origin = new Vector2(rayClass.origin.x / divider, rayClass.origin.y / divider);
                ray.direction1 = new Vector2(rayClass.direction1.x / divider, rayClass.direction1.y / divider);
                ray.direction2 = new Vector2(rayClass.direction2.x / divider, rayClass.direction2.y / divider);
                ray.color1 = rayClass.color1;
                ray.color2 = rayClass.color2;
                realRayClasses.Add(ray);
            }
            generator = StartCoroutine(Generate());
            update = false;
        }
        if(test)
        {
            Vector2 cornerpos;
            HasCorner(Vector2.zero, true, out cornerpos);
            test = false;
        }
        if(clear)
        {
            foreach(GameObject corner in Corners)
            {
                DestroyImmediate(corner.gameObject);
            }
            Corners.Clear();
            Debug.Log("ClearedCorners");
            clear = false;
        }
        if(stop)
        {
            StopCoroutine(generator);
            stop = false;
        }
    }
    public IEnumerator Generate()
    {
        hitpoints.Clear();
        foreach (GameObject corner in Corners)
        {
            DestroyImmediate(corner);
        }
        for (int x = 0; x < CheckWidth; x++)
        {
            for (int y = 0; y < CheckWidth; y++)
            {
                Vector2 originPos = new Vector2(OriginPoint.position.x + x * Steps, OriginPoint.position.y + y * Steps);
                Vector2 cornerPos = new Vector2();
                if (HasCorner(originPos, true, out cornerPos))
                {
                    GameObject cornerIndicator = Instantiate(CornerObject, cornerPos, Quaternion.identity);
                    cornerIndicator.name = "CornerIndicator" + "(" + Corners.Count + ")";
                    Corners.Add(cornerIndicator);
                }
                //yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
        Debug.Log("UpdatedCorners");
    }

    public bool HasCorner(Vector2 originPos, bool test, out Vector2 cornerPos)
    {
        Vector2 rayOrigin;
       

        foreach(RayClass rayClass in realRayClasses)
        {
            float lenght = ExtraLenght;
            float basicLenght = Mathf.Sqrt(Mathf.Abs(rayClass.direction1.x - rayClass.direction1.y) - ExtraLenght);


            bool long1 = CheckRaycast(originPos + rayClass.origin, rayClass.direction1, rayClass.color1, basicLenght, test, false);
            bool long2 = CheckRaycast(originPos + rayClass.origin, rayClass.direction2, rayClass.color1, basicLenght, test, false);

            bool check1 = CheckRaycast(originPos + rayClass.origin + rayClass.direction1, rayClass.direction1, rayClass.color2, lenght, test, true);
            bool check2 = CheckRaycast(originPos + rayClass.origin + rayClass.direction2, rayClass.direction2, rayClass.color2, lenght, test, true);

            if (check1 == true && check2 == true && long1 == false && long2 == false)
            {
                rayOrigin = originPos + rayClass.origin;
                Debug.DrawRay(rayOrigin, rayClass.direction1 * basicLenght, rayClass.color1, 2);

                rayOrigin = originPos + rayClass.origin + rayClass.direction1;
                Debug.DrawRay(rayOrigin, rayClass.direction1 * lenght, rayClass.color2, 2);

                rayOrigin = originPos + rayClass.origin;
                Debug.DrawRay(rayOrigin, rayClass.direction2 * basicLenght, rayClass.color1, 2);

                rayOrigin = originPos + rayClass.origin + rayClass.direction2;
                Debug.DrawRay(rayOrigin, rayClass.direction2 * lenght, rayClass.color2, 2);

                RaycastHit2D ray = Physics2D.Raycast(originPos + rayClass.origin, originPos - (originPos + rayClass.origin), 5, CollisionMask);
                Debug.DrawRay(originPos + rayClass.origin, (originPos - (originPos + rayClass.origin)) * 10, Color.white, 2);
                if (ray)
                {
                    cornerPos = ray.point;
                } else
                {
                    cornerPos = Vector2.zero;
                }
                return true;
            }
        }
        cornerPos = Vector2.zero;
        return false;
    }

    public bool CheckRaycast(Vector2 origin, Vector2 direction, Color color, float lenght, bool test, bool check)
    {

        RaycastHit2D ray = new RaycastHit2D();
        if (check)
        {
            ray = Physics2D.Raycast(origin, direction, lenght, CollisionMask);
        }
        else
        {
            ray = Physics2D.Raycast(origin, direction, direction.magnitude - ExtraLenght, CollisionMask);
        }
      
        if (test)
        {
            Debug.DrawRay(origin, direction * lenght, color, 2);
        }
        if(ray)
        {
            hitpoints.Add(ray.point);
            return true;
        } else
        {
            return false;
        }
    }
}
