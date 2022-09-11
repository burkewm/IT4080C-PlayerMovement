using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//  Adapted from https://docs-multiplayer.unity3d.com/netcode/current/tutorials/helloworld
// 
// This adds the DefaultStart enum and adds a public setting that can be set in the editor.
// This setting allows you to define how it should behave when you build and run from unity.
// Saves you one whole click.  You're welcome.
public class NetworkCommandLine : MonoBehaviour {
    private NetworkManager netManager;
    public enum StartModes {
        CLIENT,
        SERVER,
        HOST,
        CHOOSE
    }
    public StartModes defaultStart = StartModes.CHOOSE;
    

    void Start() {
        if (Application.isEditor) return;
        var args = GetCommandlineArgs();
        var startVal = defaultStart;

        if (args.TryGetValue("-mlapi", out string mlapiValue)) {
            switch (mlapiValue) {
                case "server":
                    startVal = StartModes.SERVER;
                    break;
                case "host":
                    startVal = StartModes.HOST;
                    break;
                case "client":
                    startVal = StartModes.CLIENT;
                    break;
            }
        }

        StartAs(startVal);
    }


    public void StartAs(StartModes startVal) {
        netManager = GetComponentInParent<NetworkManager>();

        if (!(netManager.IsClient || netManager.IsHost || netManager.IsServer)) {
            if (startVal == StartModes.SERVER) {
                netManager.StartServer();
            } else if (startVal == StartModes.CLIENT) {
                netManager.StartClient();
            } else if (startVal == StartModes.HOST) {
                netManager.StartHost();
            }
        }
    }


    private Dictionary<string, string> GetCommandlineArgs() {
        Dictionary<string, string> argDictionary = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; ++i) {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-")) {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argDictionary.Add(arg, value);
            }
        }
        return argDictionary;
    }
}