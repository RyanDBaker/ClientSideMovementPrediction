using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class NetworkPacketManager<T> where T : class {

    public event System.Action<byte[]> OnRequirePackageTransmit;

    private float m_SendSpeed = 0.2f;
    public float sendSpeed
    {
        get
        {
            if (m_SendSpeed < 0.1f)
                return m_SendSpeed = 0.1f;
            return m_SendSpeed;
        }

        set
        {
            m_SendSpeed = value;
        }
    }

    float nextTick;

    private List<T> m_Packages;
    public List<T> Packages
    {
        get
        {
            if (m_Packages == null)
                m_Packages = new List<T>();
            return m_Packages;
        }
    }

    // Add new packages that will be transmitted soon to the List T
    public Queue<T> recievedPackages;
    public void AddPackage(T package)
    {
        Packages.Add(package);
    }

    // Process any data recieved
    public void RecieveData(byte[] bytes)
    {
        if (recievedPackages == null)
            recievedPackages = new Queue<T>();

        T[] packages = ReadBytes(bytes).ToArray();

        for (int i = 0; i < packages.Length; i++)
            recievedPackages.Enqueue(packages[i]);
    }

    public void Tick()
    {
        nextTick += 1 / this.sendSpeed * Time.fixedDeltaTime;
        if(nextTick > 1 && Packages.Count > 0)
        {
            nextTick = 0;

            if(OnRequirePackageTransmit != null)
            {
                byte[] bytes = CreateBytes();
                Packages.Clear();
                OnRequirePackageTransmit(bytes);
            }
        }
    }

    public T GetNextDataRecieved()
    {
        if (recievedPackages == null || recievedPackages.Count == 0)
            return default(T);

        return recievedPackages.Dequeue();
    }

    byte[] CreateBytes()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            formatter.Serialize(ms, this.Packages);
            return ms.ToArray();
        }
    }
     
    List<T> ReadBytes(byte[] bytes)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(bytes, 0, bytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return (List<T>)formatter.Deserialize(ms);
        }
    }

}
