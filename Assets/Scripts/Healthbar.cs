using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Slider[] healthSlider;
    public Slider[] easeHealthSlider;
    public float maxHealth;
    public float health;
    public float lerpSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
    }

    void Update()
    {
        foreach (Slider slider in healthSlider) //foreach used for double healthbars like the player hb
        {
            if (slider.value >= health)
            {
                slider.value = health;
            }
        }

        foreach (Slider slider in easeHealthSlider)
        {
            if (slider.value != health)
            {
                //slowly lerps the health on the bar down
                slider.value = Mathf.Lerp(slider.value, health, lerpSpeed * Time.deltaTime);

                //stops the lerp taking ages at the end
                if (Mathf.Abs(slider.value - health) < 1)
                {
                    print("Snap");
                    slider.value = health;
                } 
            }

            
        }
    }
}
