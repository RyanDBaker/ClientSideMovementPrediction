using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Prediction1 : NetworkPacketController
{

    [SerializeField]
    float moveSpeed;

    static public bool isOn;

    [SerializeField]
    [Range(0.1f, 1)]
    float networkSendRate = 0.5f;

    [SerializeField]
    bool isPredictionEnabled;

    [SerializeField]
    float correctionThreshold;

    public CharacterController controller;

    List<RecievePackage> predictedPackages;
    Vector3 lastPosition;

    // Description: Start function is called at the beginning of the script
    //              used to initialise the variables
    void Start()
    {
        if (!isOn)
        {
            enabled = false;
        }

        controller = GetComponent<CharacterController>();
        PacketManager.sendSpeed = networkSendRate;
        ServerPacketManager.sendSpeed = networkSendRate;
        predictedPackages = new List<RecievePackage>();
    }

    // Description: Update is called once per frame and used as the main
    //              game loop to call the functions used for movement and
    //              prediction
    void Update()
    {
        LocalClientUpdate();
        ServerUpdate();
        RemoteClientUpdate();
    }

    // Description: uses the horizontal and vertical values to move the player
    //              to the desired location using the move function which is
    //              built into the player controller
    private void Move(float horizontal, float vertical)
    {
        controller.Move(new Vector3(horizontal, vertical, 0));
    }

    // Description: Called in the update function every frame, checks if the player
    //              has made any inputs. if yes, a packet containing those inputs is
    //              sent to the server and the inputs are also added to the prediction
    //              list
    void LocalClientUpdate()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            float timeStep = Time.time;
            PacketManager.AddPackage(new Package
            {
                horizontal = Input.GetAxis("Horizontal"),
                vertical = Input.GetAxis("Vertical"),
                timeStamp = timeStep
            });

            if (isPredictionEnabled)
            {
                Move(Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed, Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed);
                predictedPackages.Add(new RecievePackage
                {
                    x = transform.position.x,
                    y = transform.position.y,
                    z = transform.position.z,
                    timeStamp = timeStep
                });
            }
        }
    }

    // Description: Called in the update function every frame. Passes the latest packet to the server if it contains
    //              new data. If there is no new packets to send then the function returns since there is no data
    //              to send
    void ServerUpdate()
    {
        if (!isServer || isLocalPlayer)
            return;

        Package packageData = PacketManager.GetNextDataRecieved();

        if (packageData == null)
            return;

        Move(packageData.horizontal * Time.deltaTime * moveSpeed, packageData.vertical * Time.deltaTime * moveSpeed);

        if (transform.position == lastPosition)
            return;

        lastPosition = transform.position;

        ServerPacketManager.AddPackage(new RecievePackage
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            timeStamp = packageData.timeStamp
        });
    }

    // Description: Called in the update function every frame. Updates all player locations using the prediction
    //              list. Also performs server reconciliation is the distance between the server position and
    //              client position is greater than the threshold
    public void RemoteClientUpdate()
    {
        if (isServer)
            return;

        var data = ServerPacketManager.GetNextDataRecieved();

        if (data == null)
            return;

        if (isLocalPlayer && isPredictionEnabled)
        {
            var transmittedPackage = predictedPackages.Where(x => x.timeStamp == data.timeStamp).FirstOrDefault();
            if (transmittedPackage == null)
                return;

            if (Vector3.Distance(new Vector3(transmittedPackage.x, transmittedPackage.y, transmittedPackage.z), new Vector3(data.x, data.y, data.z)) > correctionThreshold)
            {
                transform.position = new Vector3(data.x, data.y, data.z);
            }

            predictedPackages.RemoveAll(x => x.timeStamp <= data.timeStamp);
        }
        else
        {
            transform.position = new Vector3(data.x, data.y, data.z);
        }
    }

    // Description: Overrides the OnStartLocalPlayer fucntion which is locating in unity
    //              network behaviour. Changes the colour of your player black leaving all
    //              other players white
    public override void OnStartLocalPlayer()
    {
        GetComponent<SpriteRenderer>().material.SetColor("_EmissionColor", Color.black);
    }
}

