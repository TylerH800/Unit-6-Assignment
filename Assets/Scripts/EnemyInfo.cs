using TMPro;
using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    private GameObject cam;

    public string enemyName;
    public TextMeshProUGUI enemyNameText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GameObject.Find("Camera");
        enemyNameText.text = enemyName;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.transform.forward);
    }
}
