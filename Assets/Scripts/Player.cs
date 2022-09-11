using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.Netcode;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameManager _gameManager;

    public NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();
    public float moveSpeed = 0.33f;

    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);

    public override void OnNetworkSpawn()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _gameManager. RequestNewPlayerColorServerRpc();
    }


    public void Start()
    {
        ApplyPlayerColor();
        PlayerColor.OnValueChanged += OnPlayerColorChanged;
    }

    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyPlayerColor();
    }
    public void ApplyPlayerColor()
    {
        GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
    }

    //Get Axis Inputs For Movement
    Vector3 CalculateMovement()
    {
        Vector3 movementVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        movementVector *= moveSpeed;
        return movementVector;
    }
    //Request Server To Update Position Based on Player Movement, Bounds set by size of Plane
    [ServerRpc]
    void RequestPositionForMovementServerRpc(Vector3 movement)
    {
        position.Value += movement;

        float planeSize = 5f;
        Vector3 newPosition = position.Value + movement;
        newPosition.x = Mathf.Clamp(newPosition.x, planeSize * -1, planeSize);
        newPosition.z = Mathf.Clamp(newPosition.z, planeSize * -1, planeSize);

        position.Value = newPosition;
    }

   

    //If Movement Value is about 0, Need to Request Server to Update Positon
    private void Update()
    {

        if (IsOwner)
        {
            Vector3 move = CalculateMovement();
            if (move.magnitude > 0)
            {
                RequestPositionForMovementServerRpc(move);
            }
            else
            {
                transform.position = position.Value;
            }
            
        }
    }
}
