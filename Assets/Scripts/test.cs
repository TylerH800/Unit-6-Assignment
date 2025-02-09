using UnityEngine;

public class test : MonoBehaviour
{
    public Transform camera, sp;
    public float fov;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float angle= Vector3.Angle(camera.transform.forward, sp.position - transform.position);
        if (Mathf.Abs(angle) < fov)
        {
            print("Object2 if front Obj1");
        }

        Debug.DrawRay(camera.transform.position + new Vector3(0, 1, 0), camera.transform.forward + new Vector3( 0, 0, fov), Color.red, 1f);
        Debug.DrawRay(camera.transform.position + new Vector3(0, 1, 0), camera.transform.forward + new Vector3( 0, 0, -fov), Color.red, 1f);
    }
}
