using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Tabletop;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopTurnStatusPresenter))]
    public sealed class TabletopTurnStatusPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Tabletop Turn Status Presenter",
                "Binds a tabletop board presenter to a small TextMeshPro status label so local board proofs can show the active seat.",
                whenToUse: new[]
                {
                    "A local tabletop, board, card, or tactical proof needs clear active-turn feedback.",
                    "The scene already has a TabletopBoardGridPresenter with a TurnOrderDefinition assigned.",
                    "You want a disposable proof HUD before designing project-specific turn UI."
                },
                createBefore: new[]
                {
                    "TabletopBoardGridPresenter configured with BoardDefinition and TurnOrderDefinition.",
                    "A Canvas with a TextMeshProUGUI label for the current turn."
                },
                assignFirst: new[]
                {
                    "Assign Board Presenter.",
                    "Assign the TextMeshPro label that should display the active seat.",
                    "Rename Seat Zero and Seat One if the game does not use White and Black."
                },
                safeToCustomize: new[]
                {
                    "Replace this component with project-owned HUD, animation, audio, or accessibility feedback once the proof loop is established.",
                    "Keep the actual turn state owned by TabletopBoardGridPresenter and TurnOrderDefinition."
                },
                validation: new[]
                {
                    "Enter Play Mode and confirm the label starts on the first seat.",
                    "Resolve a legal board move and confirm the label advances to the next seat.",
                    "If the label says board turn order is not ready, assign a TurnOrderDefinition on the board presenter."
                },
                manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Board_Card_Tabletop_Setup.md")));

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
