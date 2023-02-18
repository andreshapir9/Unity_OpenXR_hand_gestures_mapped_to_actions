using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Management;
//we need to use Namespace UnityEngine.XR.Hands
using UnityEngine.XR.Hands;
//we need TMP for the text
using TMPro;
//we need the UI for the button
using UnityEngine.UI;


public class Hand_as_Object : MonoBehaviour
{

    [SerializeField]
    private  XROrigin m_Origin;
    [SerializeField]
    public GameObject m_HandRoot;
    [SerializeField]
    public Transform[] m_JointXforms;
    [SerializeField]
    public string[] m_JointNames;
    [SerializeField]
    public Transform m_WristRootXform;
    [SerializeField]
    public bool m_HandIsLeft;
    [SerializeField]
    public TextMeshProUGUI m_Text;
    [SerializeField]
    public Canvas m_Canvas;

    private bool m_IsHandInitialized = false;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("________________HERE________ Start");
        //declare and assign the joint names
        m_JointNames = new string[XRHandJointID.EndMarker.ToIndex()];
        for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
            m_JointNames[jointIndex] = XRHandJointIDUtility.FromIndex(jointIndex).ToString();

        //declare and assign the joint transforms
        m_JointXforms = new Transform[XRHandJointID.EndMarker.ToIndex()];       
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: find solution for this
        /*
            the hand takes a while to initiliaze, so the best solution for now
            is to wait until the hand is initialized before we try to find the hand root
            so we just kill resources by looping forever until the hand is initialized
        */
        if(!m_IsHandInitialized){
             //find the right hand object that is a child of the xrOrigin
            m_HandRoot = FindHandRoot(m_Origin.gameObject, m_HandIsLeft);
            Debug.Log("________________HAND ROOT________ " + m_HandRoot.name);
            if(m_HandRoot != null){
                m_IsHandInitialized = true;

                //find the wrist joint
                m_WristRootXform = FindWrist(m_HandRoot, XRHandJointID.Wrist, m_JointNames);
                Debug.Log("________________WRISt ROOT________ " + m_WristRootXform.name);
                AssignJoint(m_HandRoot, XRHandJointID.Wrist, m_JointNames);


                //using the wrist joint, assign the rest of the joints
                AssignJoints(m_WristRootXform, m_JointXforms, m_JointNames);
            }
        }else{
            string text = "";
           //lets debug the joint positions
            for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
            {
                if (m_JointXforms[jointIndex] != null)
                {
                    // Debug.Log("________________"+ m_JointNames[jointIndex] + "________ " + m_JointXforms[jointIndex].position);
                    text += m_JointNames[jointIndex] + ": " + m_JointXforms[jointIndex].position + "\n";
                }
            }
            m_Text.text = text;
            
                
        }

        
        m_Canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 200;
        m_Canvas.transform.rotation = Camera.main.transform.rotation;

    }
    
    //using the hand and joint Id and joint names, find the appropriate joint transform
    private void AssignJoint(GameObject handRoot, XRHandJointID jointId, string[] jointNames){
        int jointIndex = jointId.ToIndex();
        m_JointXforms[jointIndex] = handRoot.transform;
    }

    //finds the wrist joint transform
    private Transform FindWrist(GameObject handRoot, XRHandJointID jointId, string[] jointNames){
        Transform wristRootXform = null;
        //traverse all the children of the hand root
        for (int childIndex = 0; childIndex < m_HandRoot.transform.childCount; ++childIndex)
        {
            //if the child name ends with the wrist joint name, assign it to the wristRootXform
            var child = m_HandRoot.transform.GetChild(childIndex);
            if (child.gameObject.name.EndsWith(jointNames[XRHandJointID.Wrist.ToIndex()]))
            {
                wristRootXform = child;
                break;
            }
            //otherwise, traverse the grandchildren of the hand root at the current child index
            for (int grandchildIndex = 0; grandchildIndex < child.childCount; ++grandchildIndex)
            {
                var grandchild = child.GetChild(grandchildIndex);
                if (grandchild.gameObject.name.EndsWith(jointNames[XRHandJointID.Wrist.ToIndex()]))
                {
                    wristRootXform = grandchild;
                    break;
                }
            }
            if (wristRootXform != null)
                break;
        }
        return wristRootXform;
    }
    // finds the hand root object
    private GameObject FindHandRoot(GameObject origin, bool isLeft){

        //traverse the children of the xrOrigin
        GameObject handRoot = null;
        
        //while the handroot is null we loop forever
        while (handRoot == null)
        {
             //lets print all of the gameobjects in the scene and their parents
            foreach (GameObject go in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                //Debug.Log("________________HERE________ GameObject: " + go.name + " Parent: " + go.transform.parent);

                //if we find the right hand or left hand, assign it to the handRoot they will be called RightHand(Clone) or LeftHand(Clone)
                if (go.name == (isLeft ? "LeftHand(Clone)" : "RightHand(Clone)"))
                {
                    handRoot = go;
                    return handRoot;
                }
            }
        }

        return handRoot;
    }

    private void AssignJoints(Transform wristRootXform, Transform[] jointXforms, string[] jointNames){
       
        //traverse the children of the wrist root
        for (int childIndex = 0; childIndex < wristRootXform.childCount; ++childIndex)
        {
            var child = wristRootXform.GetChild(childIndex);
            //if the child is a palm joint, assign it to the palm joint and continue
            if (child.name.EndsWith(jointNames[XRHandJointID.Palm.ToIndex()]))
            {
                AssignJoint(child.gameObject, XRHandJointID.Palm, jointNames);
                Debug.Log("________________ ASSIGNING PALM______________: " + child.name);
                continue;
            }

            //otherwise we traverse across the fingerindex joints
            for (int fingerindex =(int)XRHandFingerID.Thumb; fingerindex <= (int)XRHandFingerID.Little; ++fingerindex)
            {
                var fingerId = (XRHandFingerID)fingerindex;

                var jointIdFront = fingerId.GetFrontJointID();
                //as long as the child name does not end with the front joint name, continue
                if (!child.name.EndsWith(jointNames[jointIdFront.ToIndex()]))
                    continue;

                AssignJoint(child.gameObject, jointIdFront, jointNames);
                Debug.Log("________________ ASSIGNING FRONT______________: " + child.name);

                var lastChild = child;
                int jointIndexBack = fingerId.GetBackJointID().ToIndex();

                //traverse the children of the fingerindex joints
                for( int joointIndex = jointIdFront.ToIndex() + 1; joointIndex <= jointIndexBack; ++joointIndex)
                {
                    Transform nextChild = null;
                    
                    //traverse the children of the last child
                    for (int nextChildIndex = 0; nextChildIndex < lastChild.childCount; ++nextChildIndex)
                    {
                        nextChild = lastChild.GetChild(nextChildIndex);
                        
                        //if the child name ends with the joint name, lastChild is assigned to the nextChild and break
                        if (nextChild.name.EndsWith(jointNames[joointIndex]))
                        {
                            lastChild = nextChild;
                            break;
                        }
                    }

                    //if the last child does not end with the joint name, we have a problem
                    if (!lastChild.name.EndsWith(jointNames[joointIndex]))
                    {
                        Debug.LogError("________________ERROR________ Could not find joint " + jointNames[joointIndex]);
                        continue;
                    }

                    //assign the last child to the jointXforms
                    var jointId = XRHandJointIDUtility.FromIndex(joointIndex);
                    AssignJoint(lastChild.gameObject, jointId, jointNames);
                    Debug.Log("_______________ASSIGNING JOINT_____________: " + lastChild.name);
                }

            }
        }
    }



       
}
