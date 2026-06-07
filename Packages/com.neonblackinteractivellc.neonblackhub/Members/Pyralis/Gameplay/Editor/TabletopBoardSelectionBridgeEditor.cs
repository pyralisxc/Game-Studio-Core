using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopBoardSelectionBridge))]
    public sealed class TabletopBoardSelectionBridgeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Tabletop Board Selection Bridge",
                "Connect this component to a project-owned board presenter, cursor, card hand, or menu surface so Unity selections become queued board actions.",
                whenToUse: new[]
                {
                    "A tabletop route has BoardDefinition rules and needs a Unity selection surface.",
                    "A project-owned board presenter can translate clicks, touches, cards, or menu choices into BoardCoordinate values.",
                    "You want move selection to go through ActionQueueService instead of moving board state directly."
                },
                createBefore: new[]
                {
                    "BoardDefinition and starting pieces.",
                    "BoardRuntimeState created from the board definition.",
                    "ActionQueueService with BoardMoveActionResolver registered."
                },
                assignFirst: new[]
                {
                    "At runtime, call Initialize with BoardRuntimeState and ActionQueueService.",
                    "Forward piece clicks through TrySelectPieceAt or TrySelectPiece.",
                    "Forward destination clicks through TrySelectDestination."
                },
                safeToCustomize: new[]
                {
                    "Keep your project-owned board presenter, visual highlighting, and card-hand UI outside this bridge.",
                    "Toggle Resolve Queued Move Immediately for prototypes that do not have a separate queue runner yet."
                },
                validation: new[]
                {
                    "Selecting a piece sets SelectedPieceId.",
                    "Selecting a destination enqueues a BoardMoveActionPayload.",
                    "Invalid selections report LastIssue instead of mutating board state."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Board_Card_Tabletop_Setup.md")));

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
