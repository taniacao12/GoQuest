using UnityEngine;
using System.Collections;

public class Environment : MonoBehaviour {
    private float startTime;
    [HideInInspector] public GameObject capsule;
    [HideInInspector] public GameObject box;
    [Range(3, 51)] public int size = 31;
    //[Space]
    //private bool colorOn = false;
    [SerializeField] private int colored = 0;
    [SerializeField] private int pink = 0;
    [SerializeField] private int purple = 0;
    [SerializeField] private int blue = 0;
    //[Space]
    //private bool rotateOn = false;

    // initialize world variables and set up world environment
    void Start() {
        startTime = Time.time;
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                for (int z = 0; z < size; z++) {
                    makeBox(box, x, y, z);
                }
            }
        } 
    }

    public void makeBox (GameObject box, int x, int y, int z) {
        GameObject obj = Instantiate(box, new Vector3(x - (size / 2), y - (size / 2), z - (size / 2)), Quaternion.identity);
        obj.GetComponent<Tile>().setup(x, y, z, size);
        obj.transform.parent = transform;
        if (x == 0 || x == size - 1 || y == 0 || y == size - 1 || z == 0 || z == size - 1) {
            capsule.GetComponent<Player>().inactiveTiles.Add(obj);
        } else {
            obj.SetActive(false);
            capsule.GetComponent<Player>().hiddenTiles.Add(obj);
        }
    }

    void Update() {
        if (capsule.GetComponent<Player>().gridOn) {
            // color 5 random tiles every 15 seconds after 1 minutes of play
            if (Time.time - startTime >= 60.0f) StartCoroutine(color(5.0f));
            // rotate world environment by a random degree and direction every 15 seconds after 2 minutes of play
            if (Time.time - startTime >= 120.0f) StartCoroutine(rotate(10.0f));
        }
    }

    IEnumerator color (float duration) {
        while (true) {
            GameObject obj = capsule.GetComponent<Player>().activeTiles[Random.Range(0, capsule.GetComponent<Player>().activeTiles.Count)];
            int section = obj.GetComponent<Tile>().section;
            int shade = -1;
            if (!obj.GetComponent<Tile>().colorOn) {
                colored++;
                if (section == 0) pink++;
                else if (section == 1) purple++;
                else if (section == 2) blue++;
            } if (section == 0) shade = Mathf.FloorToInt((pink - 1) * 3 / colored);
            else if (section == 1) shade = Mathf.FloorToInt((purple - 1) * 3 / colored);
            else if (section == 2) shade = Mathf.FloorToInt((blue - 1) * 3 / colored);
            //print(("colorize", 3 * section + shade));
            obj.GetComponent<Tile>().addColor(3 * section + shade);
            yield return new WaitForSeconds(duration);
        }
    }

    IEnumerator rotate (float duration) {
        while (true) {
            Quaternion startRotation = transform.rotation;
            int x = (int) startRotation.eulerAngles.x + Random.Range(-90, 91);
            int y = (int) startRotation.eulerAngles.y + Random.Range(-90, 91);
            int z = (int) startRotation.eulerAngles.z + Random.Range(-90, 91);
            Quaternion endRotation = Quaternion.Euler(x, y, z);
            for (float time = 0.0f; time < duration; time += Time.deltaTime) {
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, time / duration);
                yield return null;
            } transform.rotation = endRotation;
        }
    }
}