using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public enum PyralisSetupFlowStepStatus
    {
        Ready,
        Missing,
        Blocked,
        Recommended,
        Optional
    }

    public enum PyralisSetupFlowActionKind
    {
        None,
        SelectObject,
        PingObject,
        AddLifetimeScope,
        RestoreFirstSceneDefaults,
        CreateProfile
    }

    public enum PyralisSetupFlowStepId
    {
        Unknown,
        SelectGameplaySessionBootstrap,
        GameplayRoot,
        VisibleLifetimeScope,
        FirstSceneDefaults,
        RuntimeServiceOwnership,
        AssignSessionDefinition,
        AssignDefaultGameMode,
        AssignSetupProfile,
        AddRuntimePatterns,
        AssignDefaultParticipants,
        AssignParticipantPawn,
        AssignInputProfile,
        AssignSpawnPoints,
        AssignCameraRig,
        AssignPlayerInputManager,
        TuneCameraFraming,
        TunePawnVisualsAndCollision,
        TuneMovementAndInputFeel,
        AssignPlayfieldProfile,
        EnableScoringRoute,
        AssignGameplayStateService,
        AssignCameraBoundsService,
        AssignScoreService,
        AddHudOrMenuSurface,
        AddProjectileLauncher,
        TabletopRuntimeContract,
        TabletopSelectionSurface,
        AssignSettingsManager,
        SceneAndPrefabReadiness
    }

    public enum PyralisSetupFlowWorkIntent
    {
        Foundation,
        RequiredSetup,
        ProofEnhancer,
        FeatureCard
    }

    public sealed class PyralisSetupFlowStep
    {
        public PyralisSetupFlowStep(
            string label,
            PyralisSetupFlowStepStatus status,
            string message,
            Object referencedObject = null,
            PyralisSetupFlowActionKind actionKind = PyralisSetupFlowActionKind.None,
            PyralisSetupFlowStepId stepId = PyralisSetupFlowStepId.Unknown,
            PyralisSetupFlowWorkIntent workIntent = PyralisSetupFlowWorkIntent.RequiredSetup,
            PyralisAuthoringNativeAction? nativeAction = null,
            Type referencedType = null)
        {
            Label = label;
            Status = status;
            Message = message;
            ReferencedObject = referencedObject;
            ActionKind = actionKind;
            StepId = stepId;
            WorkIntent = workIntent == PyralisSetupFlowWorkIntent.RequiredSetup && stepId != PyralisSetupFlowStepId.Unknown
                ? PyralisSetupFlowGuidance.GetDefaultWorkIntent(stepId)
                : workIntent;
            NativeAction = nativeAction ?? PyralisSetupFlowGuidance.GetNativeAction(stepId, message);
            ReferencedType = referencedType;
        }

        public PyralisSetupFlowStepId StepId { get; }
        public string Label { get; }
        public PyralisSetupFlowStepStatus Status { get; }
        public string Message { get; }
        public Object ReferencedObject { get; }
        public PyralisSetupFlowActionKind ActionKind { get; }
        public PyralisSetupFlowWorkIntent WorkIntent { get; }
        public PyralisAuthoringNativeAction? NativeAction { get; }
        public Type ReferencedType { get; }

        public bool IsRequiredIssue => Status == PyralisSetupFlowStepStatus.Missing || Status == PyralisSetupFlowStepStatus.Blocked;
    }

    public sealed class PyralisSetupFlowReport
    {
        private readonly List<PyralisSetupFlowStep> _steps;
        private readonly List<PyralisSetupFlowStep> _guidedDisplaySteps;

        public PyralisSetupFlowReport(IEnumerable<PyralisSetupFlowStep> steps)
        {
            _steps = new List<PyralisSetupFlowStep>(steps ?? System.Array.Empty<PyralisSetupFlowStep>());
            _guidedDisplaySteps = BuildGuidedDisplaySteps(_steps);
        }

        public IReadOnlyList<PyralisSetupFlowStep> Steps => _steps;
        public IReadOnlyList<PyralisSetupFlowStep> GuidedDisplaySteps => _guidedDisplaySteps;

        public PyralisSetupFlowStep FirstBlockingStep
        {
            get
            {
                for (int i = 0; i < _steps.Count; i++)
                {
                    if (_steps[i].Status == PyralisSetupFlowStepStatus.Missing || _steps[i].Status == PyralisSetupFlowStepStatus.Blocked)
                        return _steps[i];
                }

                return null;
            }
        }

        public int RequiredIssueCount => Count(PyralisSetupFlowStepStatus.Missing) + Count(PyralisSetupFlowStepStatus.Blocked);
        public int MissingCount => Count(PyralisSetupFlowStepStatus.Missing);
        public int BlockedCount => Count(PyralisSetupFlowStepStatus.Blocked);
        public int RecommendedIssueCount => Count(PyralisSetupFlowStepStatus.Recommended);
        public int OptionalCount => Count(PyralisSetupFlowStepStatus.Optional);
        public int ReadyCount => Count(PyralisSetupFlowStepStatus.Ready);

        public PyralisSetupFlowStep GetStep(string label)
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                if (string.Equals(_steps[i].Label, label, System.StringComparison.Ordinal))
                    return _steps[i];
            }

            return null;
        }

        public PyralisSetupFlowStep GetStep(PyralisSetupFlowStepId stepId)
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].StepId == stepId)
                    return _steps[i];
            }

            return null;
        }

        public string BuildChecklistText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Pyralis Setup Flow");

            for (int i = 0; i < _guidedDisplaySteps.Count; i++)
            {
                PyralisSetupFlowStep step = _guidedDisplaySteps[i];
                builder.Append("- [");
                builder.Append(step.Status == PyralisSetupFlowStepStatus.Ready ? "x" : " ");
                builder.Append("] ");
                builder.Append(step.Label);
                builder.Append(" - ");
                builder.Append(step.Status);
                if (!string.IsNullOrWhiteSpace(step.Message))
                {
                    builder.Append(": ");
                    builder.Append(step.Message);
                }

                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private static List<PyralisSetupFlowStep> BuildGuidedDisplaySteps(IReadOnlyList<PyralisSetupFlowStep> steps)
        {
            List<PyralisSetupFlowStep> ordered = new List<PyralisSetupFlowStep>();
            PyralisSetupFlowStep firstBlocking = null;

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].IsRequiredIssue)
                {
                    firstBlocking = steps[i];
                    ordered.Add(steps[i]);
                    break;
                }
            }

            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Missing, firstBlocking);
            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Recommended, firstBlocking);
            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Ready, firstBlocking);
            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Optional, firstBlocking);
            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Blocked, firstBlocking);

            return ordered;
        }

        private static void AddByStatus(List<PyralisSetupFlowStep> ordered, IReadOnlyList<PyralisSetupFlowStep> steps, PyralisSetupFlowStepStatus status, PyralisSetupFlowStep skip)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                PyralisSetupFlowStep step = steps[i];
                if (step == skip || step.Status != status)
                    continue;

                ordered.Add(step);
            }
        }

        private int Count(PyralisSetupFlowStepStatus status)
        {
            int count = 0;
            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].Status == status)
                    count++;
            }

            return count;
        }
    }
}
