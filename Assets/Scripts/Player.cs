using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {
    private int size;
    private Vector3 startPos = Vector3.zero;
    [HideInInspector] public GameObject world;
    [HideInInspector] public AudioClip activeSound, trapSound, extruderSound, floaterSound;
    public bool gridOn = false;
    public List<GameObject> hiddenTiles = new List<GameObject>();
    public List<GameObject> inactiveTiles = new List<GameObject>();
    public List<GameObject> activeTiles = new List<GameObject>();
    public List<GameObject> removedTiles = new List<GameObject>();

    // initialize player variables
    void Start() {
        size = world.GetComponent<Environment>().size;
        transform.parent.position = startPos;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) || inactiveTiles.Count == 0) {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        } else if (inactiveTiles.Count == 1) {
            inactiveTiles[0].GetComponent<Tile>().finalColor();
        }
    }

    void LateUpdate() {
        // respawn if player falls out of bounds
        //print(("LateUpdate", transform.position, transform.parent.position));
        if (transform.position.y < -115) {
            transform.parent.position = startPos;
        }
    }

    void OnTriggerEnter (Collider other) {
        Vector3 pos = other.gameObject.transform.localPosition;
        // if grid is not on yet, turn on grid when the player touches a wall
        if (!gridOn && (pos.x == -size / 2 || pos.x == size / 2 || pos.z == -size / 2 || pos.z == size / 2)) {
            for (int i = 0; i < other.gameObject.transform.parent.childCount; i++) {
                other.gameObject.transform.parent.GetChild(i).gameObject.GetComponent<Tile>().addGrid();
            } gridOn = true;
        } // if tile is a trap, disable or break it
        if (other.gameObject.tag == "Trap") {
            //print("trap");
            AudioSource.PlayClipAtPoint(trapSound, other.gameObject.transform.localPosition);
            other.gameObject.SetActive(false);
            removedTiles.Add(other.gameObject);
            inactiveTiles.Remove(other.gameObject);
        } // if tile is a regular tile, activate it
        else if (other.gameObject.tag == "Regular") {
            other.gameObject.GetComponent<Tile>().activate();
            activeTiles.Add(other.gameObject);
            inactiveTiles.Remove(other.gameObject);
            //// if the tile is a scaler, animate scaling
            //if (other.gameObject.GetComponent<Tile>().type == 5) {
            //    //print("scaler");
            //    //AudioSource.PlayClipAtPoint(extruderSound, other.gameObject.transform.localPosition);
            //    StartCoroutine(scale(other.gameObject, Random.Range(0.1f, 1.0f)));
            //} // if the tile is an extruder, animate extrusion
            if (other.gameObject.GetComponent<Tile>().type == 4) {
                //print("extruder");
                AudioSource.PlayClipAtPoint(extruderSound, other.gameObject.transform.localPosition);
                StartCoroutine(extrude(other.gameObject, other.gameObject.GetComponent<Tile>().getCoor(), 10.0f));
            } // if the tile is a floater, active 1-6 random tiles
            else if (other.gameObject.GetComponent<Tile>().type == 3) {
                //print("floater");
                AudioSource.PlayClipAtPoint(floaterSound, other.gameObject.transform.localPosition);
                for (int count = Random.Range(1, 5); count >= 0; count--) {
                    int index = Random.Range(0, hiddenTiles.Count);
                    hiddenTiles[index].SetActive(true);
                    inactiveTiles.Add(hiddenTiles[index]);
                    hiddenTiles.RemoveAt(index);
                }
            } else AudioSource.PlayClipAtPoint(activeSound, other.gameObject.transform.localPosition); // regular tile
        }
    }

    //public IEnumerator scale (GameObject obj, float duration) {
    //    while (true) {
    //        float scale = Random.Range(0.1f, 2.0f);
    //        Vector3 startScale = new Vector3(1, 1, 1);
    //        Vector3 endScale = new Vector3(scale, scale, scale);
    //        for (float time = 0.0f; time < duration; time += Time.deltaTime) {
    //            obj.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
    //            yield return null;
    //        } obj.transform.localScale = endScale;
    //        yield return null;
    //        for (float time = 0.0f; time < duration; time += Time.deltaTime) {
    //            obj.transform.localScale = Vector3.Lerp(endScale, startScale, time / duration);
    //            yield return null;
    //        } obj.transform.localScale = startScale;
    //        yield return new WaitForSeconds(5.0f);
    //    }
    //}

    public IEnumerator extrude (GameObject obj, int[] coor, float duration) {
        float x = obj.transform.localPosition.x;
        float y = obj.transform.localPosition.y;
        float z = obj.transform.localPosition.z;
        int numTiles = Random.Range(1, size / 2);
        for (int i = 0; i < numTiles; i++) {
            GameObject nextObj = null;
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = new Vector3(1, 1, 1);
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            if (coor[1] == 0) { // extrude up
                startScale = new Vector3(1, 0, 1);
                nextObj = obj.transform.parent.Find(coor[0] + "," + (coor[1] + i + 1) + "," + coor[2]).gameObject;
                startPos = new Vector3(x, y + i + 1 - 0.5f, z);
                endPos = new Vector3(x, y + i + 1, z);
            } else if (coor[1] == size - 1) { // extrude down
                startScale = new Vector3(1, 0, 1);
                nextObj = obj.transform.parent.Find(coor[0] + "," + (coor[1] - i - 1) + "," + coor[2]).gameObject;
                startPos = new Vector3(x, y - i - 1 + 0.5f, z);
                endPos = new Vector3(x, y - i - 1, z);
            } else if (coor[0] == 0) { // extrude right
                startScale = new Vector3(0, 1, 1);
                nextObj = obj.transform.parent.Find((coor[0] + i + 1) + "," + coor[1] + "," + coor[2]).gameObject;
                startPos = new Vector3(x + i + 1 - 0.5f, y, z);
                endPos = new Vector3(x + i + 1, y, z);
            } else if (coor[0] == size - 1) { // extrude left
                startScale = new Vector3(0, 1, 1);
                nextObj = obj.transform.parent.Find((coor[0] - i - 1) + "," + coor[1] + "," + coor[2]).gameObject;
                startPos = new Vector3(x - i - 1 + 0.5f, y, z);
                endPos = new Vector3(x - i - 1, y, z);
            } else if (coor[2] == 0) { // extrude front
                startScale = new Vector3(1, 1, 0);
                nextObj = obj.transform.parent.Find(coor[0] + "," + coor[1] + "," + (coor[2] + i + 1)).gameObject;
                startPos = new Vector3(x, y, z + i + 1 - 0.5f);
                endPos = new Vector3(x, y, z + i + 1);
            } else if (coor[2] == size - 1) { // extrude back
                startScale = new Vector3(1, 1, 0);
                nextObj = obj.transform.parent.Find(coor[0] + "," + coor[1] + "," + (coor[2] - i - 1)).gameObject;
                startPos = new Vector3(x, y, z - i - 1 + 0.5f);
                endPos = new Vector3(x, y, z - i - 1);
            } nextObj.transform.localPosition = startPos;
            nextObj.SetActive(true);
            inactiveTiles.Add(nextObj);
            hiddenTiles.Remove(nextObj);
            for (float time = 0.0f; time < duration; time += Time.deltaTime) {
                nextObj.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
                nextObj.transform.localPosition = Vector3.Lerp(startPos, endPos, time / duration);
                yield return null; // wait for the next frame
            } nextObj.transform.localScale = endScale;
            nextObj.transform.localPosition = endPos;
        }
    }
}