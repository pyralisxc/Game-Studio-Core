using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Networking.Editor
{
    public static class NetworkingStarterPackFactory
    {
        [MenuItem("NeonBlack/Infrastructure/Networking/Setup NGO (UnityTransport)", false, 100)]
        public static void CreateNgoStarterPack()
        {
            var existingManager = Object.FindAnyObjectByType<NetworkManager>();
            if (existingManager != null)
            {
                Selection.activeGameObject = existingManager.gameObject;
                Debug.Log("NetworkManager already exists in scene.");
                return;
            }

            GameObject managerGo = new GameObject("NetworkManager");
            NetworkManager manager = managerGo.AddComponent<NetworkManager>();
            
            // Add UnityTransport
            UnityTransport transport = managerGo.AddComponent<UnityTransport>();
            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport
            };

            // Register default prefabs if any (optional)
            
            Undo.RegisterCreatedObjectUndo(managerGo, "Create NetworkManager");
            Selection.activeGameObject = managerGo;
            Debug.Log("NGO Starter Pack created with UnityTransport.");
        }
    }
}