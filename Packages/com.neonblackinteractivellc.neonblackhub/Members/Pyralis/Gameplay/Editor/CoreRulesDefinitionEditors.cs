using NeonBlack.Gameplay.Data.Definitions.Rules;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(BoardDefinition))]
    public class BoardDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BoardDefinition definition = (BoardDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Board Definition",
                "A board definition creates the logical play space and starting pieces for tabletop, tactical, card, token, and grid-driven modes.",
                whenToUse: new[]
                {
                    "Use this when players control spaces, pieces, seats, cursors, or factions instead of only pawns.",
                    "Use one board per game mode unless the mode intentionally swaps boards at runtime."
                },
                createBefore: new[]
                {
                    "BoardPieceDefinition assets for every piece type that starts on the board.",
                    "TurnOrderDefinition when the board game advances by seats or phases.",
                    "GameModeDefinition so validation can report this board's setup issues."
                },
                assignFirst: new[]
                {
                    "Set Board Id and Display Name.",
                    "Set Width and Height.",
                    "Add Starting Pieces with unique instance ids and valid coordinates."
                },
                validation: new[]
                {
                    "Every starting piece has a piece definition.",
                    "No two starting pieces share the same instance id or coordinate.",
                    "Every starting coordinate is inside the board."
                }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "Board definition is ready for game-mode assignment.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(BoardPieceDefinition))]
    public class BoardPieceDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BoardPieceDefinition definition = (BoardPieceDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Board Piece Definition",
                "A board piece definition names a logical piece type. Movement, attacks, and scoring rules should reference this identity instead of hardcoded scene objects.",
                whenToUse: new[]
                {
                    "Use this for pawns, kings, units, tokens, cards-on-board, tiles, and faction markers.",
                    "Create one definition per rules identity, not one per piece instance."
                },
                assignFirst: new[]
                {
                    "Set Piece Id.",
                    "Set Display Name.",
                    "Set Piece Family for filtering and editor grouping.",
                    "Assign Visual Prefab when the presenter should instantiate creator-owned board art for this piece type."
                },
                validation: new[]
                {
                    "Piece Id is stable and unique in the project.",
                    "Display Name is readable for designers.",
                    "Piece Family is not empty."
                }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "Board piece definition is ready for board setup.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(BoardMovePolicyDefinition))]
    public class BoardMovePolicyDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BoardMovePolicyDefinition definition = (BoardMovePolicyDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Board Move Policy",
                "A board move policy describes a reusable legal-move pattern that board pieces and tabletop actions can evaluate without custom code.",
                whenToUse: new[]
                {
                    "Use this for regular grid movement such as orthogonal steps, diagonal movement, adjacent movement, straight-line rays, or fixed jumps.",
                    "Create separate policies when different piece families move differently."
                },
                createBefore: new[]
                {
                    "BoardDefinition and BoardPieceDefinition assets for the game pieces that will use this policy.",
                    "TurnOrderDefinition when moves should be gated by active seat."
                },
                assignFirst: new[]
                {
                    "Set Policy Id and Display Name.",
                    "Choose the movement Shape.",
                    "Set Max Distance.",
                    "For Offset shape, add Allowed Offsets such as 1,2 and 2,1 for knight-style jumps.",
                    "Enable Allow Capture only when an opposing destination piece can be captured."
                },
                validation: new[]
                {
                    "Policy Id is stable and unique in the project.",
                    "Max Distance is greater than zero.",
                    "Offset policies define at least one allowed offset.",
                    "Capture behavior matches the game rule this policy represents."
                }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "Board move policy is ready for board action setup.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(BoardTerminalConditionDefinition))]
    public class BoardTerminalConditionDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BoardTerminalConditionDefinition definition = (BoardTerminalConditionDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Board Terminal Condition",
                "A board terminal condition tells a tabletop game when a board state ends the round or game.",
                whenToUse: new[]
                {
                    "Use Side Eliminated for tactics, token capture, elimination, and last-side-standing games.",
                    "Use Objective Occupied for race, king-of-the-hill, extraction, and reach-the-space games."
                },
                createBefore: new[]
                {
                    "BoardDefinition with starting pieces.",
                    "GameModeDefinition so validation can report missing or invalid terminal conditions."
                },
                assignFirst: new[]
                {
                    "Set Condition Id and Display Name.",
                    "Choose the condition Kind.",
                    "For Side Eliminated, set Observed Seat and Winning Seat.",
                    "For Objective Occupied, set Objective Coordinate and optionally constrain Observed Seat or override Winning Seat."
                },
                validation: new[]
                {
                    "Condition Id is stable and unique in the game mode.",
                    "Side-eliminated conditions have valid observed and winning seats.",
                    "Objective coordinates match a valid board space in the intended board definition."
                }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "Board terminal condition is ready for game-mode assignment.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(PhaseDefinition))]
    public class PhaseDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PhaseDefinition definition = (PhaseDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Phase Definition",
                "A phase definition names one step inside a turn, such as draw, move, attack, resolve, buy, or cleanup.",
                whenToUse: new[]
                {
                    "Use this when a turn has repeatable steps.",
                    "Keep phases broad enough to be reusable across rule packs."
                },
                assignFirst: new[]
                {
                    "Set Phase Id.",
                    "Set Display Name.",
                    "Choose whether this phase allows action selection.",
                    "Choose whether completing this phase ends the turn."
                },
                validation: new[]
                {
                    "Phase Id is stable and unique in the turn order.",
                    "Display Name is readable in future UI."
                }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "Phase definition is ready for turn-order setup.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(TurnOrderDefinition))]
    public class TurnOrderDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TurnOrderDefinition definition = (TurnOrderDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Turn Order Definition",
                "A turn order definition controls which participant seat acts next and which phases a turn can contain.",
                whenToUse: new[]
                {
                    "Use this for board games, card games, tactical games, menu combat, and local turn variants.",
                    "Use seat numbers to stay independent from specific player objects or pawns."
                },
                createBefore: new[]
                {
                    "PhaseDefinition assets if the game has named turn steps.",
                    "GameModeDefinition so validation can report turn-order setup issues."
                },
                assignFirst: new[]
                {
                    "Set Turn Order Id and Display Name.",
                    "Set Participant Seats in play order.",
                    "Assign phases when the game needs named turn steps."
                },
                validation: new[]
                {
                    "At least one participant seat exists.",
                    "Participant seats are unique and non-negative.",
                    "Assigned phases are unique and valid."
                }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "Turn order definition is ready for game-mode assignment.");
            DrawRuntimeSummary(definition);

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawRuntimeSummary(TurnOrderDefinition definition)
        {
            int seatCount = definition.participantSeats == null ? 0 : definition.participantSeats.Length;
            int phaseCount = definition.phases == null ? 0 : definition.phases.Length;

            UnityEditor.EditorGUILayout.Space(4f);
            UnityEditor.EditorGUILayout.LabelField("Runtime Summary", UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUILayout.LabelField("Seats", seatCount.ToString());
            UnityEditor.EditorGUILayout.LabelField("Phases", phaseCount.ToString());
        }
    }
}
