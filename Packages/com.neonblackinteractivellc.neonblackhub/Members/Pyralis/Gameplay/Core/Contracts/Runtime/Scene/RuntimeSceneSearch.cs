using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public static class RuntimeSceneSearch
    {
        public static T Find<T>() where T : Component
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    if (roots[rootIndex] == null)
                        continue;

                    T component = roots[rootIndex].GetComponentInChildren<T>(true);
                    if (component != null)
                        return component;
                }
            }

            return null;
        }

        public static bool ContainsComponent<T>() where T : Component
        {
            return Find<T>() != null;
        }

        public static bool ContainsComponentInNamespace(string namespacePrefix)
        {
            if (string.IsNullOrWhiteSpace(namespacePrefix))
                return false;

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    GameObject root = roots[rootIndex];
                    if (root == null)
                        continue;

                    MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        Type type = behaviours[i] != null ? behaviours[i].GetType() : null;
                        if (type != null
                            && type.Namespace != null
                            && type.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
