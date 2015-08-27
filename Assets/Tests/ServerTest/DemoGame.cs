// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Exit Games GmbH">
//   Exit Games GmbH, 2015
// </copyright>
// <summary>
// Extending the LoadBalancingClient, this class implements game-related logic for the demo.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using SPShared.Operations;

public class DemoGame : LoadBalancingClient
{
    public string ErrorMessageToShow { get; set; }

    public Vector3 lastMoveEv;

    public int evCount = 0;

    // overriding the CreatePlayer "factory" provides us with custom DemoPlayers (that also know their position)
    protected internal override Player CreatePlayer(string actorName, int actorNumber, bool isLocal, Hashtable actorProperties)
    {
        return new DemoPlayer(actorName, actorNumber, isLocal, actorProperties);
    }

    public void SendMove()
    {
        Hashtable evData = new Hashtable();
        evData[(byte)1] = Random.onUnitSphere;
        this.loadBalancingPeer.OpRaiseEvent(1, evData, true, null);
    }

	public void OpUpdateFlightControls()
	{
		Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
		opParameters[(byte)SPShared.Operations.SPParameterCode.MoveRight] = false;
		opParameters [(byte)SPShared.Operations.SPParameterCode.MoveLeft] = true; // MoveLeft
		opParameters[(byte)SPShared.Operations.SPParameterCode.MoveForward] = true; // MoveForward
		opParameters [(byte)SPShared.Operations.SPParameterCode.MoveBackward] = false; // MoveBackward
		this.loadBalancingPeer.OpCustom((byte)SPShared.Operations.SPOperationCode.UpdateFlightControls, opParameters, true);
	}

    public override void OnOperationResponse(OperationResponse operationResponse)
    {
        base.OnOperationResponse(operationResponse);
        switch (operationResponse.OperationCode)
        {
            case (byte)OperationCode.Authenticate:
                if (operationResponse.ReturnCode == ErrorCode.InvalidAuthentication)
                {
                    this.ErrorMessageToShow = string.Format("Authentication failed. Your AppId: {0}.\nMake sure to set the AppId in DemoGUI.cs by replacing \"<insert your app id here>\".\nResponse: {1}", this.AppId, operationResponse.ToStringFull());
                    this.DebugReturn(DebugLevel.ERROR, this.ErrorMessageToShow);
                }
                if (operationResponse.ReturnCode == ErrorCode.InvalidOperation || operationResponse.ReturnCode == ErrorCode.InternalServerError)
                {
                    this.ErrorMessageToShow = string.Format("Authentication failed. You successfully connected but the server ({0}) but it doesn't know the 'authenticate'. Check if it runs the Loadblancing server-logic.\nResponse: {1}", this.MasterServerAddress, operationResponse.ToStringFull());
                    this.DebugReturn(DebugLevel.ERROR, this.ErrorMessageToShow);
                }
                break;

            case (byte)OperationCode.CreateGame:
                string gsAddress = (string)operationResponse[ParameterCode.Address];
                if (!string.IsNullOrEmpty(gsAddress) && gsAddress.StartsWith("127.0.0.1"))
                {
                    this.ErrorMessageToShow = string.Format("The master forwarded you to a gameserver with address: {0}.\nThat address points to 'this computer' anywhere. This might be a configuration error in the game server.", gsAddress);
                    this.DebugReturn(DebugLevel.ERROR, this.ErrorMessageToShow);
                }
                break;

            case (byte)OperationCode.JoinRandomGame:
                string gsAddressJoin = (string)operationResponse[ParameterCode.Address];
                if (!string.IsNullOrEmpty(gsAddressJoin) && gsAddressJoin.StartsWith("127.0.0.1"))
                {
                    this.ErrorMessageToShow = string.Format("The master forwarded you to a gameserver with address: {0}.\nThat address points to 'this computer' anywhere. This might be a configuration error in the game server.", gsAddressJoin);
                    this.DebugReturn(DebugLevel.ERROR, this.ErrorMessageToShow);
                }

                if (operationResponse.ReturnCode != 0)
                {
                    this.OpCreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, null);
                }
                break;
			case (byte) SPShared.Operations.SPOperationCode.UpdateFlightControls:
				Debug.Log ("Got Update response!");
			break;

        }
    }

    public override void OnEvent(EventData photonEvent)
    {
        base.OnEvent(photonEvent);
		DebugReturn (DebugLevel.INFO, photonEvent.ToStringFull ());
        switch (photonEvent.Code)
        {
            case EventCode.PropertiesChanged:
				Debug.Log ("propertiesChanged!");
                var data = photonEvent.Parameters[ParameterCode.Properties] as Hashtable;
                DebugReturn(DebugLevel.ALL, "Got EV PropertiesChanged: " + (data["data"] as string));
                break;
			case (byte)SPShared.Operations.SPOperationCode.Join:
				var actorNum = photonEvent.Parameters[(byte)SPShared.Operations.SPParameterCode.ActorNr];
				DebugReturn(DebugLevel.ALL, "Actor: " + actorNum + " joined the game");
				break;
        }
    }

    public override void DebugReturn(DebugLevel level, string message)
    {
        base.DebugReturn(level, message);
        Debug.Log(message);
    }

    public override void OnStatusChanged(StatusCode statusCode)
    {
        base.OnStatusChanged(statusCode);

        switch (statusCode)
        {
			case StatusCode.Disconnect:
			case StatusCode.EncryptionEstablished:
				break;
			case StatusCode.Connect:
				Debug.Log("Connected");
				break;
            case StatusCode.TimeoutDisconnect:
                Debug.LogError("Timeout by client.");
                break;
            case StatusCode.DisconnectByServer:
                Debug.LogError("Timeout by server received.");
                break;
            case StatusCode.Exception:
            case StatusCode.ExceptionOnConnect:
                Debug.LogWarning("Exception on connection level. Is the server running? Is the address (" + this.MasterServerAddress+ ") reachable?");
                break;
        }
    }
}
