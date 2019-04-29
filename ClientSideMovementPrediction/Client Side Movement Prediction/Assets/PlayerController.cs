using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



public class PlayerController : NetworkBehaviour {

    public static bool isOn;

    // Description: Update is called once per frame and used as the main
    //              game loop to call the functions used for movement
    void Update () {
        if (!isOn)
        {
            enabled = false;
        }
        if (isLocalPlayer)
        {
            MovePlayer();
        }
    }

    // Description: Called in the update function every frame. Collects horizontal
    //              and vertical inputs. Translates the player using the collected
    //              inputs
    public virtual void MovePlayer()
    {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
        var y = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Translate(x, y, 0);
    }

    // Description: Overrides the OnStartLocalPlayer fucntion which is locating in unity
    //              network behaviour. Changes the colour of your player black leaving all
    //              other players white
    public override void OnStartLocalPlayer()
    {
        GetComponent<SpriteRenderer>().material.SetColor("_EmissionColor", Color.black);
    }
}
