using System.Globalization;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    public enum CameraProfileMappingStatus
    {
        Valid,
        MissingComponent,
        MissingProperty,
        Unsupported,
        Stale
    }

    public enum CameraProfileValueKind
    {
        Float,
        Int,
        Bool,
        Enum,
        Vector2,
        Vector3,
        String
    }

    [System.Serializable]
    public sealed class CameraProfileEntry
    {
        [SerializeField] private string componentTypeName;
        [SerializeField] private string componentDisplayName;
        [SerializeField] private string propertyPath;
        [SerializeField] private string displayLabel;
        [SerializeField] private string serializedValue;
        [SerializeField] private CameraProfileValueKind valueKind;
        [SerializeField] private string lastReflectedSource;
        [SerializeField] private CameraProfileMappingStatus status;
        [SerializeField] private string tooltip;
        [SerializeField] private string category;

        public string ComponentTypeName => componentTypeName;
        public string ComponentDisplayName => componentDisplayName;
        public string PropertyPath => propertyPath;
        public string DisplayLabel => displayLabel;
        public string SerializedValue => serializedValue;
        public CameraProfileValueKind ValueKind => valueKind;
        public string LastReflectedSource => lastReflectedSource;
        public CameraProfileMappingStatus Status => status;
        public string Tooltip => tooltip;
        public string Category => category;

        public CameraProfileEntry(
            string componentTypeName,
            string componentDisplayName,
            string propertyPath,
            string displayLabel,
            string serializedValue,
            CameraProfileValueKind valueKind,
            string lastReflectedSource,
            CameraProfileMappingStatus status,
            string tooltip,
            string category)
        {
            this.componentTypeName = componentTypeName;
            this.componentDisplayName = componentDisplayName;
            this.propertyPath = propertyPath;
            this.displayLabel = displayLabel;
            this.serializedValue = serializedValue;
            this.valueKind = valueKind;
            this.lastReflectedSource = lastReflectedSource;
            this.status = status;
            this.tooltip = tooltip;
            this.category = category;
            Sanitize();
        }

        public CameraProfileEntry WithStatus(CameraProfileMappingStatus nextStatus)
        {
            return new CameraProfileEntry(
                componentTypeName,
                componentDisplayName,
                propertyPath,
                displayLabel,
                serializedValue,
                valueKind,
                lastReflectedSource,
                nextStatus,
                tooltip,
                category);
        }

        public bool TryReadFloat(out float value)
        {
            return float.TryParse(serializedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public bool TryReadInt(out int value)
        {
            return int.TryParse(serializedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public bool TryReadBool(out bool value)
        {
            return bool.TryParse(serializedValue, out value);
        }

        public bool TryReadVector2(out Vector2 value)
        {
            value = default;
            string[] parts = serializedValue?.Split('|');
            if (parts == null || parts.Length != 2)
                return false;

            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                return false;
            }

            value = new Vector2(x, y);
            return true;
        }

        public bool TryReadVector3(out Vector3 value)
        {
            value = default;
            string[] parts = serializedValue?.Split('|');
            if (parts == null || parts.Length != 3)
                return false;

            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)
                || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            {
                return false;
            }

            value = new Vector3(x, y, z);
            return true;
        }

        public void Sanitize()
        {
            componentTypeName = componentTypeName?.Trim() ?? string.Empty;
            componentDisplayName = componentDisplayName?.Trim() ?? string.Empty;
            propertyPath = propertyPath?.Trim() ?? string.Empty;
            displayLabel = displayLabel?.Trim() ?? propertyPath;
            serializedValue = serializedValue?.Trim() ?? string.Empty;
            lastReflectedSource = lastReflectedSource?.Trim() ?? string.Empty;
            tooltip = tooltip?.Trim() ?? string.Empty;
            category = category?.Trim() ?? string.Empty;
        }

        public static string SerializeFloat(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        public static string SerializeInt(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string SerializeBool(bool value)
        {
            return value.ToString();
        }

        public static string SerializeVector2(Vector2 value)
        {
            return string.Join(
                "|",
                SerializeFloat(value.x),
                SerializeFloat(value.y));
        }

        public static string SerializeVector3(Vector3 value)
        {
            return string.Join(
                "|",
                SerializeFloat(value.x),
                SerializeFloat(value.y),
                SerializeFloat(value.z));
        }
    }
}
