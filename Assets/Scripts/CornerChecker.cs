using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CornerChecker : MonoBehaviour
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
            generator = StartCoroutine(Generate());
            update = false;
        }
        if(test)
        {
            HasCorner(Vector2.zero, true);
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
        foreach (GameObject corner in Corners)
        {
            DestroyImmediate(corner);
        }
        for (int x = 0; x < CheckWidth; x++)
        {
            for (int y = 0; y < CheckWidth; y++)
            {
                Vector2 originPos = new Vector2(OriginPoint.position.x + x * Steps, OriginPoint.position.y + y * Steps);
                if (HasCorner(originPos, false))
                {
                    GameObject cornerIndicator = Instantiate(CornerObject, originPos, Quaternion.identity);
                    Corners.Add(cornerIndicator);
                }
                //yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
        Debug.Log("UpdatedCorners");
    }

    public bool HasCorner(Vector2 originPos, bool test)
    {
        Vector2 rayOrigin;
        float lenght = ExtraLenght;
        float basicLenght = Mathf.Sqrt(1f);

        foreach(RayClass rayClass in rayClasses)
        {
            bool long1 = CheckRaycast(originPos + rayClass.origin, rayClass.direction1, rayClass.color1, basicLenght, test);
            bool check1 = CheckRaycast(originPos + rayClass.origin + rayClass.direction1, rayClass.direction1, rayClass.color2, lenght, test);


            bool long2 = CheckRaycast(originPos + rayClass.origin, rayClass.direction2, rayClass.color1, basicLenght, test);
            bool check2 = CheckRaycast(originPos + rayClass.origin + rayClass.direction2, rayClass.direction2, rayClass.color2, lenght, test);

            if (check1 == true && check2 == true)
            {
                rayOrigin = originPos + rayClass.origin;
                Debug.DrawRay(rayOrigin, rayClass.direction1 * basicLenght, rayClass.color1, 2);

                rayOrigin = originPos + rayClass.origin + rayClass.direction1;
                Debug.DrawRay(rayOrigin, rayClass.direction1 * lenght, rayClass.color2, 2);

                rayOrigin = originPos + rayClass.origin;
                Debug.DrawRay(rayOrigin, rayClass.direction2 * basicLenght, rayClass.color1, 2);

                rayOrigin = originPos + rayClass.origin + rayClass.direction2;
                Debug.DrawRay(rayOrigin, rayClass.direction2 * lenght, rayClass.color2, 2);
                return true;
            }
            if (long1 && long2)
            {
                Debug.LogError("LONG TRUE");
                rayOrigin = originPos + rayClass.origin;
                Debug.DrawRay(rayOrigin, rayClass.direction1 * basicLenght, rayClass.color1, 2);
                rayOrigin = originPos + rayClass.origin;
                Debug.DrawRay(rayOrigin, rayClass.direction2 * basicLenght, rayClass.color1, 2);
            }
        }
        #region Old
        //bool long6 = CheckRaycast(originPos + new Vector2(-1, 1), new Vector2(2, -1), Color.cyan, basicLenght, test);
        //bool check7 = CheckRaycast(originPos + new Vector2(-1, 1) + new Vector2(2, -1), new Vector2(2, -1), Color.blue, lenght, test);


        //bool long8 = CheckRaycast(originPos + new Vector2(-1, 1), new Vector2(1, -2), Color.cyan, basicLenght, test);
        //bool short9 = CheckRaycast(originPos + new Vector2(-1, 1) + new Vector2(1, -2), new Vector2(1, -2), Color.blue, lenght, test);

        //if (long6 == false && check7 == true && long8 == false && short9 == true)
        //{
        //    rayOrigin = originPos + new Vector2(-1, 1) + new Vector2(2, -1);
        //    Debug.DrawRay(rayOrigin, new Vector2(2, -1) * lenght, Color.blue, 2);
        //    rayOrigin = originPos + new Vector2(-1, 1) + new Vector2(1, -2);
        //    Debug.DrawRay(rayOrigin, new Vector2(1, -2) * lenght, Color.blue, 2);
        //    return true;
        //}


        //rayOrigin = originPos + new Vector2(1, 1) + new Vector2(-2, -1);
        //RaycastHit2D hit3 = Physics2D.Raycast(rayOrigin, new Vector2(-2, -1), lenght, CollisionMask);
        //if (test)
        //{
        //    Debug.DrawRay(rayOrigin, new Vector2(-2, -1) * lenght, Color.green, 0.1f);
        //}

        //rayOrigin = originPos + new Vector2(1, 1) + new Vector2(-1, -2);
        //RaycastHit2D hit4 = Physics2D.Raycast(rayOrigin, new Vector2(-1, -2), lenght, CollisionMask);
        //if (test)
        //{
        //    Debug.DrawRay(rayOrigin, new Vector2(-1, -2) * lenght, Color.green, 0.1f);
        //}
        //if (hit3 && hit4)
        //{
        //    rayOrigin = originPos + new Vector2(1, 1) + new Vector2(-2, -1);
        //    Debug.DrawRay(rayOrigin, new Vector2(-2, -1) * lenght, Color.green, 2);
        //    rayOrigin = originPos + new Vector2(1, 1) + new Vector2(-1, -2);
        //    Debug.DrawRay(rayOrigin, new Vector2(-1, -2) * lenght, Color.green, 2);
        //    return true;
        //}


        //rayOrigin = originPos + new Vector2(-1, -1) + new Vector2(1, 2);
        //RaycastHit2D hit5 = Physics2D.Raycast(rayOrigin, new Vector2(1, 2), lenght, CollisionMask);
        //if (test)
        //{
        //    Debug.DrawRay(rayOrigin, new Vector2(1, 2) * lenght, Color.red, 0.1f);
        //}

        //rayOrigin = originPos + new Vector2(-1, -1) + new Vector2(2, 1);
        //RaycastHit2D hit6 = Physics2D.Raycast(rayOrigin, new Vector2(2, 1), lenght, CollisionMask);
        //if (test)
        //{
        //    Debug.DrawRay(rayOrigin, new Vector2(2, 1) * lenght, Color.red, 0.1f);
        //}
        //if (hit5 && hit6)
        //{
        //    rayOrigin = originPos + new Vector2(-1, -1) + new Vector2(1, 2);
        //    Debug.DrawRay(rayOrigin, new Vector2(1, 2) * lenght, Color.red, 2);
        //    rayOrigin = originPos + new Vector2(-1, -1) + new Vector2(2, 1);
        //    Debug.DrawRay(rayOrigin, new Vector2(2, 1) * lenght, Color.red, 2);
        //    return true;
        //}


        //rayOrigin = originPos + new Vector2(1, -1) + new Vector2(-1, 2);
        //RaycastHit2D hit7 = Physics2D.Raycast(rayOrigin, new Vector2(-1, 2), lenght, CollisionMask);
        //if (test)
        //{
        //    Debug.DrawRay(rayOrigin, new Vector2(-1, 2) * lenght, Color.magenta, 0.1f);
        //}

        //rayOrigin = originPos + new Vector2(1, -1) + new Vector2(-2, 1);
        //RaycastHit2D hit8 = Physics2D.Raycast(rayOrigin, new Vector2(-2, 1), lenght, CollisionMask);
        //if (test)
        //{
        //    Debug.DrawRay(rayOrigin, new Vector2(-2, 1) * lenght, Color.magenta, 0.1f);
        //}
        //if (hit7 && hit8)
        //{
        //    rayOrigin = originPos + new Vector2(1, -1) + new Vector2(-1, 2);
        //    Debug.DrawRay(rayOrigin, new Vector2(-1, 2) * lenght, Color.magenta, 2);
        //    rayOrigin = originPos + new Vector2(1, -1) + new Vector2(-2, 1);
        //    Debug.DrawRay(rayOrigin, new Vector2(-2, 1) * lenght, Color.magenta, 2);
        //    return true;
        //}
        #endregion
        return false;
    }

    public bool CheckRaycast(Vector2 origin, Vector2 direction, Color color, float lenght, bool test)
    {
        RaycastHit2D ray = Physics2D.Raycast(origin, direction, lenght, CollisionMask);
        if (test)
        {
            Debug.DrawRay(origin, direction * lenght, color, 0.1f);
        }
        if(ray.collider != null)
        {
            return true;
        } else
        {
            return false;
        }
    }
}
