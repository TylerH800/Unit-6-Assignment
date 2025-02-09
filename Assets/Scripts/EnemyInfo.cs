using TMPro;
using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    private GameObject player;
    private GameObject cam;
    public GameObject parent; //used for turning on and off based on distance
    public float viewDistance;


    public string enemyName;
    [HideInInspector] public int level;
    public TextMeshProUGUI enemyNameText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        cam = GameObject.Find("Camera");

        enemyNameText.text = "Lvl." + level + " " + enemyName;
    }


    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.transform.forward);        
    }

    private void Update()
    {
        float distance = (transform.position - player.transform.position).magnitude;
        if (distance <= viewDistance)
        {
            parent.SetActive(true);     
        }
        else
        {
            parent.SetActive(false);
        }
    }
}
