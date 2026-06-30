using NeonBlack.Gameplay.Glue.Bootstrap;
using NeonBlack.Gameplay.Glue.Wiring.Reporting;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Tools.Validation
{
    public static class GameplayWiringReportMenu
    {
        [MenuItem("Tools/NeonBlack/Gameplay/Wiring/Copy Selected Root Report")]
        private static void CopySelectedRootReport()
        {
            GameObject root = Selection.activeGameObject;
            if (root == null)
                return;

            GameplaySessionBootstrap bootstrap = root.GetComponent<GameplaySessionBootstrap>();
            GameplayWiringReport report = bootstrap != null
                ? GameplayWiringReportBuilder.BuildFrom(bootstrap)
                : GameplayWiringReportBuilder.Build(root);

            string formattedReport = GameplayWiringReportTextFormatter.Format(report);
            EditorGUIUtility.systemCopyBuffer = formattedReport;
            Debug.Log($"[GameplayWiringReport] Copied {report.Count} wiring rows for `{root.name}` to the clipboard.", root);
        }

        [MenuItem("Tools/NeonBlack/Gameplay/Wiring/Copy Selected Root Report", true)]
        private static bool CanCopySelectedRootReport()
        {
            return Selection.activeGameObject != null;
        }
    }
}
