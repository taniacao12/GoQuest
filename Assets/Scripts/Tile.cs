using UnityEngine;

public class Tile : MonoBehaviour {
    [HideInInspector] public Shader inactiveTexture, activeTexture, finalTexture;
    [HideInInspector] public Shader[] colorTextures;
    [SerializeField] private int x = 0;
    [SerializeField] private int y = 0;
    [SerializeField] private int z = 0;
    public int type;
    public bool gridOn = false;
    public bool active = false;
    public bool colorOn = false;
    public int section, color = -1;

    // initial tile setup
    public void setup (int x1, int y1, int z1, int size) {
        x = x1; y = y1; z = z1;
        section = Mathf.FloorToInt((y + 1) * 3 / size);
        if (section > 2) section = 2;
        name = x + "," + y + "," + z;

        /* Tile Types
         * 1 - regular tile: separated in two types (active and inactive)
         * 2 - trap tile: disappears/breaks at touch
         * 3 - floater: activates random floating tile
         * 4 - extruder: activates x number of tiles in front of it
         * 5 - scaler: scales itself up and down
        */

        // generate and set tile functionality
        if (x == size / 2 && y == 0 && z == size / 2) type = 1;
        else if (x == 0 || x == size - 1 || y == 0 || y == size - 1 || z == 0 || z == size - 1) {            
            type = Random.Range(1, 5); // maybe make interior extruders be pushers?
        } else type = Random.Range(1, 3);
        if (type == 2) tag = "Trap";
        else {
            tag = "Regular";
            this.gameObject.layer = LayerMask.NameToLayer("Ground");
        }

        // set grid texture on floor and inactivate inner tiles
        if (y == 0) addGrid();
    }

    public int[] getCoor() {
        return new int[] {x, y, z};
    }

    // add grid texture to tile
    public void addGrid() {
        if (!gridOn) { 
            GetComponent<Renderer>().material.shader = inactiveTexture;
            gridOn = true;
        }
    }

    // add active texture to tile
    public void activate() {
        if (!active) {
            GetComponent<Renderer>().material.shader = activeTexture;
            active = true;
        }
    }

    // add color texture to tile
    public void addColor (int c) {
        if (!colorOn) {
            colorOn = true;
            color = c;
            GetComponent<Renderer>().material.shader = colorTextures[color];
        }
    }

    // add final texture to tile
    public void finalColor() {
        GetComponent<Renderer>().material.shader = finalTexture;
    }
}