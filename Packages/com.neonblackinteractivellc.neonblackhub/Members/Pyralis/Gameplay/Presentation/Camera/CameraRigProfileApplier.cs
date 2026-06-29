using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using Unity.Cinemachine;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Camera
{
    public static class CameraRigProfileApplier
    {
        private static readonly CameraProfileSettingBinding[] Bindings =
        {
            CameraProfileSettingBinding.Float<CinemachineCamera>(
                "Lens.FieldOfView",
                "Field Of View",
                "Lens",
                "Cinemachine vertical field of view.",
                camera => camera.Lens.FieldOfView,
                (camera, value) =>
                {
                    LensSettings lens = camera.Lens;
                    lens.FieldOfView = value;
                    camera.Lens = lens;
                }),
            CameraProfileSettingBinding.Float<CinemachineCamera>(
                "Lens.OrthographicSize",
                "Orthographic Size",
                "Lens",
                "Cinemachine orthographic half-height.",
                camera => camera.Lens.OrthographicSize,
                (camera, value) =>
                {
                    LensSettings lens = camera.Lens;
                    lens.OrthographicSize = value;
                    camera.Lens = lens;
                }),
            CameraProfileSettingBinding.Float<CinemachineCamera>(
                "Lens.NearClipPlane",
                "Near Clip Plane",
                "Lens",
                "Near clip plane for the virtual camera lens.",
                camera => camera.Lens.NearClipPlane,
                (camera, value) =>
                {
                    LensSettings lens = camera.Lens;
                    lens.NearClipPlane = value;
                    camera.Lens = lens;
                }),
            CameraProfileSettingBinding.Float<CinemachineCamera>(
                "Lens.FarClipPlane",
                "Far Clip Plane",
                "Lens",
                "Far clip plane for the virtual camera lens.",
                camera => camera.Lens.FarClipPlane,
                (camera, value) =>
                {
                    LensSettings lens = camera.Lens;
                    lens.FarClipPlane = value;
                    camera.Lens = lens;
                }),
            CameraProfileSettingBinding.Float<CinemachineCamera>(
                "Lens.Dutch",
                "Dutch",
                "Lens",
                "Camera roll, in degrees.",
                camera => camera.Lens.Dutch,
                (camera, value) =>
                {
                    LensSettings lens = camera.Lens;
                    lens.Dutch = value;
                    camera.Lens = lens;
                }),
            CameraProfileSettingBinding.Enum<CinemachineCamera>(
                "Lens.ModeOverride",
                "Projection Mode",
                "Lens",
                "Cinemachine projection mode override.",
                camera => (int)camera.Lens.ModeOverride,
                (camera, value) =>
                {
                    LensSettings lens = camera.Lens;
                    lens.ModeOverride = (LensSettings.OverrideModes)value;
                    camera.Lens = lens;
                }),
            CameraProfileSettingBinding.Int<CinemachineCamera>(
                "Priority",
                "Priority",
                "Activation",
                "Cinemachine camera activation priority.",
                camera => camera.Priority.Value,
                (camera, value) => camera.Priority = value),
            CameraProfileSettingBinding.Vector3<CinemachinePositionComposer>(
                "TargetOffset",
                "Target Offset",
                "Body",
                "Position Composer target offset.",
                composer => composer.TargetOffset,
                (composer, value) => composer.TargetOffset = value),
            CameraProfileSettingBinding.Vector3<CinemachinePositionComposer>(
                "Damping",
                "Position Damping",
                "Body",
                "Position Composer follow damping.",
                composer => composer.Damping,
                (composer, value) => composer.Damping = value),
            CameraProfileSettingBinding.Float<CinemachinePositionComposer>(
                "CameraDistance",
                "Camera Distance",
                "Body",
                "Position Composer camera distance.",
                composer => composer.CameraDistance,
                (composer, value) => composer.CameraDistance = value),
            CameraProfileSettingBinding.Float<CinemachinePositionComposer>(
                "DeadZoneDepth",
                "Dead Zone Depth",
                "Body",
                "Position Composer dead zone depth.",
                composer => composer.DeadZoneDepth,
                (composer, value) => composer.DeadZoneDepth = value),
            CameraProfileSettingBinding.Vector3<CinemachineRotationComposer>(
                "TargetOffset",
                "Look At Offset",
                "Aim",
                "Rotation Composer target offset.",
                composer => composer.TargetOffset,
                (composer, value) => composer.TargetOffset = value),
            CameraProfileSettingBinding.Vector2<CinemachineRotationComposer>(
                "Damping",
                "Aim Damping",
                "Aim",
                "Rotation Composer damping.",
                composer => composer.Damping,
                (composer, value) => composer.Damping = value),
        };

        public static List<CameraProfileEntry> SyncFromSceneCamera(Component sceneCameraOrRig)
        {
            List<CameraProfileEntry> entries = new List<CameraProfileEntry>();
            if (sceneCameraOrRig == null)
                return entries;

            Transform root = sceneCameraOrRig.transform;
            for (int i = 0; i < Bindings.Length; i++)
            {
                CameraProfileSettingBinding binding = Bindings[i];
                Component component = FindComponent(root, binding.ComponentType);
                if (component == null)
                    continue;

                entries.Add(binding.CreateEntry(component, GetHierarchyPath(component.transform), CameraProfileMappingStatus.Valid));
            }

            return entries;
        }

        public static List<CameraProfileEntry> ValidateAgainstSceneCamera(CameraRigProfile profile, Component sceneCameraOrRig)
        {
            List<CameraProfileEntry> entries = new List<CameraProfileEntry>();
            if (profile == null)
                return entries;

            Transform root = sceneCameraOrRig != null ? sceneCameraOrRig.transform : null;
            IReadOnlyList<CameraProfileEntry> reflectedSettings = profile.ReflectedSettings;
            for (int i = 0; i < reflectedSettings.Count; i++)
            {
                CameraProfileEntry entry = reflectedSettings[i];
                if (entry == null)
                    continue;

                CameraProfileSettingBinding binding = FindBinding(entry);
                if (binding == null)
                {
                    entries.Add(entry.WithStatus(CameraProfileMappingStatus.Unsupported));
                    continue;
                }

                if (root == null || FindComponent(root, binding.ComponentType) == null)
                {
                    entries.Add(entry.WithStatus(CameraProfileMappingStatus.MissingComponent));
                    continue;
                }

                entries.Add(entry.WithStatus(CameraProfileMappingStatus.Valid));
            }

            return entries;
        }

        public static CameraProfileApplyReport ApplyToSceneCamera(CameraRigProfile profile, Component sceneCameraOrRig)
        {
            CameraProfileApplyReport report = new CameraProfileApplyReport();
            if (profile == null || sceneCameraOrRig == null)
                return report;

            Transform root = sceneCameraOrRig.transform;
            IReadOnlyList<CameraProfileEntry> reflectedSettings = profile.ReflectedSettings;
            for (int i = 0; i < reflectedSettings.Count; i++)
            {
                CameraProfileEntry entry = reflectedSettings[i];
                if (entry == null)
                    continue;

                CameraProfileSettingBinding binding = FindBinding(entry);
                if (binding == null)
                {
                    report.Unsupported++;
                    continue;
                }

                Component component = FindComponent(root, binding.ComponentType);
                if (component == null)
                {
                    report.MissingComponent++;
                    continue;
                }

                if (entry.Status != CameraProfileMappingStatus.Valid)
                {
                    report.Skipped++;
                    continue;
                }

                if (binding.TryApply(component, entry))
                    report.Applied++;
                else
                    report.Unsupported++;
            }

            return report;
        }

        public static bool HasReflectedProjectionOverride(CameraRigProfile profile, out bool orthographic, out float orthographicSize)
        {
            orthographic = false;
            orthographicSize = 0f;
            if (profile == null)
                return false;

            bool hasMode = false;
            bool hasSize = false;
            IReadOnlyList<CameraProfileEntry> reflectedSettings = profile.ReflectedSettings;
            for (int i = 0; i < reflectedSettings.Count; i++)
            {
                CameraProfileEntry entry = reflectedSettings[i];
                if (entry == null || entry.Status != CameraProfileMappingStatus.Valid)
                    continue;

                if (entry.ComponentTypeName == typeof(CinemachineCamera).FullName
                    && entry.PropertyPath == "Lens.ModeOverride"
                    && entry.TryReadInt(out int modeValue))
                {
                    orthographic = modeValue == (int)LensSettings.OverrideModes.Orthographic;
                    hasMode = true;
                }
                else if (entry.ComponentTypeName == typeof(CinemachineCamera).FullName
                    && entry.PropertyPath == "Lens.OrthographicSize"
                    && entry.TryReadFloat(out float size))
                {
                    orthographicSize = size;
                    hasSize = true;
                }
            }

            return hasMode && hasSize;
        }

        private static Component FindComponent(Transform root, Type componentType)
        {
            if (root == null || componentType == null)
                return null;

            return root.GetComponentInChildren(componentType, true);
        }

        private static CameraProfileSettingBinding FindBinding(CameraProfileEntry entry)
        {
            if (entry == null)
                return null;

            for (int i = 0; i < Bindings.Length; i++)
            {
                CameraProfileSettingBinding binding = Bindings[i];
                if (entry.ComponentTypeName == binding.ComponentType.FullName
                    && entry.PropertyPath == binding.PropertyPath)
                {
                    return binding;
                }
            }

            return null;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        private sealed class CameraProfileSettingBinding
        {
            private readonly Func<Component, string> _readValue;
            private readonly Func<Component, CameraProfileEntry, bool> _applyValue;

            private CameraProfileSettingBinding(
                Type componentType,
                string propertyPath,
                string displayLabel,
                string category,
                string tooltip,
                CameraProfileValueKind valueKind,
                Func<Component, string> readValue,
                Func<Component, CameraProfileEntry, bool> applyValue)
            {
                ComponentType = componentType;
                PropertyPath = propertyPath;
                DisplayLabel = displayLabel;
                Category = category;
                Tooltip = tooltip;
                ValueKind = valueKind;
                _readValue = readValue;
                _applyValue = applyValue;
            }

            public Type ComponentType { get; }
            public string PropertyPath { get; }
            private string DisplayLabel { get; }
            private string Category { get; }
            private string Tooltip { get; }
            private CameraProfileValueKind ValueKind { get; }

            public CameraProfileEntry CreateEntry(Component component, string sourcePath, CameraProfileMappingStatus status)
            {
                return new CameraProfileEntry(
                    ComponentType.FullName,
                    NicifyTypeName(ComponentType.Name),
                    PropertyPath,
                    DisplayLabel,
                    _readValue(component),
                    ValueKind,
                    sourcePath,
                    status,
                    Tooltip,
                    Category);
            }

            public bool TryApply(Component component, CameraProfileEntry entry)
            {
                return component != null && entry != null && _applyValue(component, entry);
            }

            public static CameraProfileSettingBinding Float<TComponent>(
                string propertyPath,
                string displayLabel,
                string category,
                string tooltip,
                Func<TComponent, float> getValue,
                Action<TComponent, float> setValue)
                where TComponent : Component
            {
                return new CameraProfileSettingBinding(
                    typeof(TComponent),
                    propertyPath,
                    displayLabel,
                    category,
                    tooltip,
                    CameraProfileValueKind.Float,
                    component => CameraProfileEntry.SerializeFloat(getValue((TComponent)component)),
                    (component, entry) =>
                    {
                        if (!entry.TryReadFloat(out float value))
                            return false;

                        setValue((TComponent)component, value);
                        return true;
                    });
            }

            public static CameraProfileSettingBinding Int<TComponent>(
                string propertyPath,
                string displayLabel,
                string category,
                string tooltip,
                Func<TComponent, int> getValue,
                Action<TComponent, int> setValue)
                where TComponent : Component
            {
                return new CameraProfileSettingBinding(
                    typeof(TComponent),
                    propertyPath,
                    displayLabel,
                    category,
                    tooltip,
                    CameraProfileValueKind.Int,
                    component => CameraProfileEntry.SerializeInt(getValue((TComponent)component)),
                    (component, entry) =>
                    {
                        if (!entry.TryReadInt(out int value))
                            return false;

                        setValue((TComponent)component, value);
                        return true;
                    });
            }

            public static CameraProfileSettingBinding Enum<TComponent>(
                string propertyPath,
                string displayLabel,
                string category,
                string tooltip,
                Func<TComponent, int> getValue,
                Action<TComponent, int> setValue)
                where TComponent : Component
            {
                return new CameraProfileSettingBinding(
                    typeof(TComponent),
                    propertyPath,
                    displayLabel,
                    category,
                    tooltip,
                    CameraProfileValueKind.Enum,
                    component => CameraProfileEntry.SerializeInt(getValue((TComponent)component)),
                    (component, entry) =>
                    {
                        if (!entry.TryReadInt(out int value))
                            return false;

                        setValue((TComponent)component, value);
                        return true;
                    });
            }

            public static CameraProfileSettingBinding Vector2<TComponent>(
                string propertyPath,
                string displayLabel,
                string category,
                string tooltip,
                Func<TComponent, Vector2> getValue,
                Action<TComponent, Vector2> setValue)
                where TComponent : Component
            {
                return new CameraProfileSettingBinding(
                    typeof(TComponent),
                    propertyPath,
                    displayLabel,
                    category,
                    tooltip,
                    CameraProfileValueKind.Vector2,
                    component => CameraProfileEntry.SerializeVector2(getValue((TComponent)component)),
                    (component, entry) =>
                    {
                        if (!entry.TryReadVector2(out Vector2 value))
                            return false;

                        setValue((TComponent)component, value);
                        return true;
                    });
            }

            public static CameraProfileSettingBinding Vector3<TComponent>(
                string propertyPath,
                string displayLabel,
                string category,
                string tooltip,
                Func<TComponent, Vector3> getValue,
                Action<TComponent, Vector3> setValue)
                where TComponent : Component
            {
                return new CameraProfileSettingBinding(
                    typeof(TComponent),
                    propertyPath,
                    displayLabel,
                    category,
                    tooltip,
                    CameraProfileValueKind.Vector3,
                    component => CameraProfileEntry.SerializeVector3(getValue((TComponent)component)),
                    (component, entry) =>
                    {
                        if (!entry.TryReadVector3(out Vector3 value))
                            return false;

                        setValue((TComponent)component, value);
                        return true;
                    });
            }

            private static string NicifyTypeName(string typeName)
            {
                if (string.IsNullOrEmpty(typeName))
                    return string.Empty;

                List<char> chars = new List<char>(typeName.Length + 8);
                for (int i = 0; i < typeName.Length; i++)
                {
                    char current = typeName[i];
                    if (i > 0 && char.IsUpper(current) && !char.IsUpper(typeName[i - 1]))
                        chars.Add(' ');

                    chars.Add(current);
                }

                return new string(chars.ToArray());
            }
        }
    }

    public struct CameraProfileApplyReport
    {
        public int Applied;
        public int MissingComponent;
        public int Unsupported;
        public int Skipped;

        public override string ToString()
        {
            return $"Applied: {Applied}, Missing Components: {MissingComponent}, Unsupported: {Unsupported}, Skipped: {Skipped}";
        }
    }
}
