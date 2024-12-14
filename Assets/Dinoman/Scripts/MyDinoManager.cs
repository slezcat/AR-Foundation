using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
public class MyDinoManager : MonoBehaviour
{
    public Creature creature;
    public AudioSource[] source;
    public AudioClip[] hitSounds;
    public AudioClip killSound;
    public AudioClip Hit_jaw;
    public float maxHealth = 100;
    public float health;
    public Slider healthSlider;
    public Slider easeHealthSlider;

    public float lerpSpeed = 0.005f;
    protected int killCount = 0;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI killCountText;
    public GameObject bloodPrefab1;
    public GameObject bloodPrefab2;
    public Transform swordTransform;
    public float bloodSpawnOffset = 1f;
    public float humanDamage = 10;


    private RexControls rexControls;
    public GameObject restartScreen;
    public GameObject otherObj;

    public GameObject dino; // Reference to the dino GameObject
    public GameObject human; // Reference to the human GameObject
     private ARSession arSession;


    void OnEnable()
    {
        rexControls.Rex.Enable();
    }

    void OnDisable()
    {
        rexControls.Rex.Disable();
    }
    void Awake()
    {
        rexControls = new();
        creature = GetComponent<Creature>();
    }



    void Start()
    {
        arSession = FindObjectOfType<ARSession>();
        restartScreen.gameObject.SetActive(false);

        healthText.text = "Health: " + maxHealth.ToString();
        killCountText.text = "Kill: " + killCount.ToString();

        health = maxHealth;

    }

    void Update()
    {
        if (healthSlider.value != health)
        {
            healthSlider.value = health;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, health, lerpSpeed);
        }

        if (health <= 0)
        {
            if (dino != null) dino.SetActive(false);
            if (human != null) human.SetActive(false);

            otherObj.gameObject.SetActive(false);
            restartScreen.gameObject.SetActive(true);
        }

        // if (rexControls.Rex.Growl.triggered)
        // {
        //     health += 10;
        // }
    }

    public void RestartScene()
    {
        // Reload the current active scene
        arSession.Reset();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnCollisionEnter(Collision other)
    {
        OnKill(other);
    }

    public void UpdateHealthUI()
    {


    }
    public void UpdateKillUI()
    {
        killCountText.text = "Kill: " + killCount.ToString();
    }

    public void SpawnBlood(GameObject bloodPrefab, Vector3 position)
    {
        if (bloodPrefab != null)
        {
            GameObject bloodEffect = Instantiate(bloodPrefab, position, Quaternion.identity);

            Destroy(bloodEffect, 3f);
        }
    }

    public void OnHit()
    {
        health -= humanDamage;
        Vector3 currentPosition = transform.position;
        SpawnBlood(bloodPrefab2, currentPosition);
        UpdateHealthUI();

        if (hitSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, hitSounds.Length);
            source[1].PlayOneShot(hitSounds[randomIndex], 1f); // Set volume to max for testing

        }
        else
        {
            Debug.LogWarning("No hit sounds assigned!");
        }
    }

    public void OnKill(Collision other)
    {
        if (creature.onAttack)
        {
            if (other.transform.root.CompareTag("Human"))
            {
                Debug.Log("You kill a  human");

                SpawnBlood(bloodPrefab1, other.transform.root.position);
                source[0].PlayOneShot(Hit_jaw, Random.Range(0.1f, 0.4f));
                source[1].PlayOneShot(killSound, 0.2f);

                Destroy(other.transform.root.gameObject);
                killCount += 1;
                health += 5;

                UpdateKillUI();
            }
        }

    }
}
