using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// to save the hand permenantly
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[CreateAssetMenu(fileName = "HandToSave", menuName = "HandToSave", order = 1)]
public class HandToSave : ScriptableObject
{
    public GameObject m_HandRoot;
    //tuple of strings and transforms
    public string[] m_JointNames;
    public Transform[] m_JointXforms;
    public bool m_HandIsLeft;
    public string m_name;

    public HandToSave(string name, GameObject handRoot, string[] jointNames, Transform[] jointXforms, bool handIsLeft){
        m_name = name;
        m_HandRoot = handRoot;
        m_JointNames = jointNames;
        m_JointXforms = jointXforms;
        m_HandIsLeft = handIsLeft;
    }

    static public void Save(HandToSave handToSave){
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + handToSave.m_name + "_gesture.dat";
        FileStream stream = File.Create(path);

        HandToSave data = new HandToSave(handToSave.m_name, handToSave.m_HandRoot, handToSave.m_JointNames, handToSave.m_JointXforms, handToSave.m_HandIsLeft);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    static public HandToSave Load(string name){
        string path = Application.persistentDataPath + "/" + name + "_gesture.dat";
        if(File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            HandToSave data = formatter.Deserialize(stream) as HandToSave;
            stream.Close();

            if(data == null){
                Debug.LogError("Save file is corrupt");
                return null;
            }

            return data;
        } else {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
