using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Unity.Mathematics;
using UnityEngine;

public class MRUKSnapToWall : MonoBehaviour
{
    [SerializeField] private MRUK mruk;
    [SerializeField] private OVRInput.Controller controller;
    [SerializeField] private GameObject objectToSnap;

    private bool sceneHasBeenLoaded;
    private MRUKRoom currentRoom;
    private List<GameObject> objectsCreated = new List<GameObject>();
    
    private void OnEnable()
    {
        mruk.RoomCreatedEvent.AddListener(BindRoomInfo);
    }

    private void OnDisable()
    {
        mruk.RoomCreatedEvent.RemoveListener(BindRoomInfo);
    }

    public void EnableSnapToWall()
    {
        sceneHasBeenLoaded = true;
    }

    private void BindRoomInfo(MRUKRoom room)
    {
        currentRoom = room;
        
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller) && sceneHasBeenLoaded)
        {
            foreach (var wall in currentRoom.WallAnchors)
            {
                GameObject go = Instantiate(objectToSnap, Vector3.zero, quaternion.identity, wall.transform);
                go.transform.SetLocalPositionAndRotation(Vector3.zero, quaternion.identity);
            }
        }
        
    }
}

