using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPacketController : NetworkBehaviour {

    [System.Serializable]
    public class Package
    {
        public float horizontal;
        public float vertical;
        public float timeStamp;
    }

    [System.Serializable]
    public class RecievePackage
    {
        public float x;
        public float y;
        public float z;
        public float timeStamp;
    }

    private NetworkPacketManager<Package> m_PacketManager;
    public NetworkPacketManager<Package> PacketManager
    {
        get
        {
            if (m_PacketManager == null)
            {
                m_PacketManager = new NetworkPacketManager<Package>();
                if (isLocalPlayer)
                    m_PacketManager.OnRequirePackageTransmit += TransmitPackageToServer; 
            }
            return m_PacketManager;
        }
    }

    private NetworkPacketManager<RecievePackage> m_ServerPacketManager;
    public NetworkPacketManager<RecievePackage> ServerPacketManager
    {
        get
        {
            if (m_ServerPacketManager == null)
            {
                m_ServerPacketManager = new NetworkPacketManager<RecievePackage>();
                if (isServer)
                    m_ServerPacketManager.OnRequirePackageTransmit += TransmitToClients;
            }
            return m_ServerPacketManager;
        }
    }

    private void TransmitPackageToServer(byte[] bytes)
    {
        CmdTransmitPackages(bytes);
    }

    private void TransmitToClients(byte[] bytes)
    {
        RpcRecieveDataOnClient(bytes);
    }

    [Command]
    void CmdTransmitPackages(byte[] data)
    {
        PacketManager.RecieveData(data);
    }

    [ClientRpc]
    void RpcRecieveDataOnClient(byte[] data)
    {
        ServerPacketManager.RecieveData(data);
    }

    public virtual void FixedUpdate()
    {
        PacketManager.Tick();
        ServerPacketManager.Tick();
    }
}
