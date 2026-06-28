using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Pys.Authoring.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pys.Authoring.Editor.Scanning
{
    internal static class UnityObjectScanner
    {
        public static IReadOnlyList<UnityObjectObservation> ScanActiveSceneObjects(UnityCodebaseScanRequest request)
        {
            List<UnityObjectObservation> observations = new List<UnityObjectObservation>();
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                    AddSceneObjectTree(scene.path, roots[rootIndex], request, observations);
            }

            return observations;
        }

        public static IReadOnlyList<UnityObjectObservation> ScanPrefabs(UnityCodebaseScanRequest request)
        {
            List<UnityObjectObservation> observations = new List<UnityObjectObservation>();
            string[] prefabGuids = UnityScanPathUtility.FindAssetsInRoot("t:Prefab", request);
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                UnityObjectObservation observation = new UnityObjectObservation("prefab:" + path, prefab.name, path, "Prefab");
                AddComponentEvidence(prefab, observation, request.RuntimeValidationMethodNames);
                observations.Add(observation);
            }

            return observations;
        }

        private static void AddSceneObjectTree(string scenePath, GameObject gameObject, UnityCodebaseScanRequest request, List<UnityObjectObservation> observations)
        {
            if (gameObject == null)
                return;

            UnityObjectObservation observation = new UnityObjectObservation(
                "scene:" + scenePath + ":" + GetHierarchyPath(gameObject.transform),
                gameObject.name,
                scenePath,
                "GameObject");

            AddComponentEvidence(gameObject, observation, request != null ? request.RuntimeValidationMethodNames : null);
            observations.Add(observation);

            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
                AddSceneObjectTree(scenePath, transform.GetChild(i).gameObject, request, observations);
        }

        private static void AddComponentEvidence(GameObject gameObject, UnityObjectObservation observation, IEnumerable<string> validationMethodNames)
        {
            Component[] components = gameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    observation.Issues.Add(new AuthoringIssue(
                        "Unity.MissingScript",
                        "Missing script component.",
                        AuthoringIssueSeverity.Required,
                        targetLabel: gameObject.name,
                        actionKind: AuthoringActionKind.ResolveMissingScript));
                    continue;
                }

                observation.Components.Add(component.GetType().FullName);

                ReflectiveRuntimeValidationObserver.AddValidationIssues(component, validationMethodNames, observation.Issues);
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            List<string> parts = new List<string>();
            Transform current = transform;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }
    }

    public static class ReflectiveRuntimeValidationObserver
    {
        private const string DefaultIssueCode = "RuntimeValidation.Issue";

        public static IReadOnlyList<MethodInfo> FindValidationMethods(Type type, IEnumerable<string> methodNames)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            if (type == null)
                return methods;

            HashSet<string> names = BuildMethodNameSet(methodNames);
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;
            MethodInfo[] candidates = type.GetMethods(Flags);
            for (int i = 0; i < candidates.Length; i++)
            {
                MethodInfo method = candidates[i];
                if (!names.Contains(method.Name))
                    continue;

                if (method.IsGenericMethod || method.GetParameters().Length != 0)
                    continue;

                if (method.ReturnType == typeof(void) || method.ReturnType == typeof(string))
                    continue;

                if (!typeof(IEnumerable).IsAssignableFrom(method.ReturnType))
                    continue;

                methods.Add(method);
            }

            return methods;
        }

        public static void AddValidationIssues(object provider, IEnumerable<string> methodNames, List<AuthoringIssue> issues)
        {
            if (provider == null || issues == null)
                return;

            IReadOnlyList<MethodInfo> methods = FindValidationMethods(provider.GetType(), methodNames);
            for (int i = 0; i < methods.Count; i++)
                AddValidationIssues(provider, methods[i], issues);
        }

        private static void AddValidationIssues(object provider, MethodInfo method, List<AuthoringIssue> issues)
        {
            object result;
            try
            {
                result = method.Invoke(provider, null);
            }
            catch (TargetInvocationException exception)
            {
                issues.Add(new AuthoringIssue(
                    "RuntimeValidation.InvocationFailed",
                    exception.InnerException != null ? exception.InnerException.Message : exception.Message,
                    AuthoringIssueSeverity.Required,
                    nativeAction: "Inspect the validation method that failed while PYS observed runtime validation evidence.",
                    successCheck: "The validation method returns issue evidence without throwing.",
                    actionKind: AuthoringActionKind.ReviewCode));
                return;
            }

            IEnumerable enumerable = result as IEnumerable;
            if (enumerable == null)
                return;

            foreach (object issueObject in enumerable)
            {
                AuthoringIssue issue = NormalizeIssue(issueObject);
                if (issue != null)
                    issues.Add(issue);
            }
        }

        private static AuthoringIssue NormalizeIssue(object issueObject)
        {
            if (issueObject == null)
                return null;

            AuthoringIssue issue = issueObject as AuthoringIssue;
            if (issue != null)
                return issue;

            string issueCode = FirstNonEmpty(ReadString(issueObject, "IssueCode"), ReadString(issueObject, "Code"), DefaultIssueCode);
            string message = FirstNonEmpty(ReadString(issueObject, "Message"), ReadString(issueObject, "Summary"), issueObject.GetType().Name);
            AuthoringIssueSeverity severity = ReadSeverity(issueObject, "Severity", AuthoringIssueSeverity.Required);
            AuthoringActionKind actionKind = ReadActionKind(issueObject, "ActionKind", AuthoringActionKind.None);

            return new AuthoringIssue(
                issueCode,
                message,
                severity,
                fieldPath: ReadString(issueObject, "FieldPath"),
                targetLabel: ReadString(issueObject, "TargetLabel"),
                nativeAction: ReadString(issueObject, "NativeAction"),
                successCheck: ReadString(issueObject, "SuccessCheck"),
                actionKind: actionKind,
                ownerStableId: ReadString(issueObject, "OwnerStableId"),
                relatedStableIds: ReadStringArray(issueObject, "RelatedStableIds"));
        }

        private static HashSet<string> BuildMethodNameSet(IEnumerable<string> methodNames)
        {
            HashSet<string> names = new HashSet<string>();
            if (methodNames != null)
            {
                foreach (string methodName in methodNames)
                {
                    if (!string.IsNullOrWhiteSpace(methodName))
                        names.Add(methodName.Trim());
                }
            }

            if (names.Count == 0)
                names.Add("GetRuntimeValidationIssues");

            return names;
        }

        private static string ReadString(object target, string propertyName)
        {
            object value = ReadProperty(target, propertyName);
            return value != null ? value.ToString() : string.Empty;
        }

        private static AuthoringIssueSeverity ReadSeverity(object target, string propertyName, AuthoringIssueSeverity fallback)
        {
            object value = ReadProperty(target, propertyName);
            if (value == null)
                return fallback;

            if (Enum.TryParse(value.ToString(), true, out AuthoringIssueSeverity parsed))
                return parsed;

            if (value is int intValue && Enum.IsDefined(typeof(AuthoringIssueSeverity), intValue))
                return (AuthoringIssueSeverity)intValue;

            return fallback;
        }

        private static AuthoringActionKind ReadActionKind(object target, string propertyName, AuthoringActionKind fallback)
        {
            object value = ReadProperty(target, propertyName);
            if (value == null)
                return fallback;

            if (Enum.TryParse(value.ToString(), true, out AuthoringActionKind parsed))
                return parsed;

            if (value is int intValue && Enum.IsDefined(typeof(AuthoringActionKind), intValue))
                return (AuthoringActionKind)intValue;

            return fallback;
        }

        private static string[] ReadStringArray(object target, string propertyName)
        {
            object value = ReadProperty(target, propertyName);
            if (value == null)
                return new string[0];

            string stringValue = value as string;
            if (stringValue != null)
                return SplitStringList(stringValue);

            IEnumerable enumerable = value as IEnumerable;
            if (enumerable == null)
                return new string[0];

            List<string> values = new List<string>();
            foreach (object item in enumerable)
            {
                if (item == null)
                    continue;

                string itemValue = item.ToString();
                if (!string.IsNullOrWhiteSpace(itemValue))
                    values.Add(itemValue.Trim());
            }

            return values.ToArray();
        }

        private static string[] SplitStringList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];

            string[] raw = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < raw.Length; i++)
                raw[i] = raw[i].Trim();

            return raw;
        }

        private static object ReadProperty(object target, string propertyName)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null || property.GetIndexParameters().Length != 0)
                return null;

            return property.GetValue(target, null);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    return values[i];
            }

            return string.Empty;
        }
    }
}
