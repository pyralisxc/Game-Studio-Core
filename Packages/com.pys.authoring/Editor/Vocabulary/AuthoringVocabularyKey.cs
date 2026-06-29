namespace Pys.Authoring.Editor.Vocabulary
{
    public static class AuthoringVocabularyKey
    {
        public const string NodeAssembly = "node:Assembly";
        public const string NodeNamespace = "node:Namespace";
        public const string NodeType = "node:Type";
        public const string NodeScript = "node:Script";
        public const string NodeComponent = "node:Component";
        public const string NodeScriptableObject = "node:ScriptableObject";
        public const string NodeContract = "node:Contract";
        public const string NodeValidator = "node:Validator";
        public const string NodeSceneObject = "node:SceneObject";
        public const string NodePrefab = "node:Prefab";
        public const string NodeAsset = "node:Asset";
        public const string NodeIssue = "node:Issue";

        public const string EdgeAssemblyReference = "edge:AssemblyReference";
        public const string EdgeNamespaceUsing = "edge:NamespaceUsing";
        public const string EdgeInherits = "edge:Inherits";
        public const string EdgeImplements = "edge:Implements";
        public const string EdgeSerializedField = "edge:SerializedField";
        public const string EdgeRequiredComponent = "edge:RequiredComponent";
        public const string EdgeContractDeclares = "edge:ContractDeclares";
        public const string EdgeValidatorReports = "edge:ValidatorReports";
        public const string EdgeSceneContains = "edge:SceneContains";
        public const string EdgePrefabContains = "edge:PrefabContains";
        public const string EdgeObserves = "edge:Observes";
        public const string EdgeOwns = "edge:Owns";

        public const string ActionInspectObject = "action:InspectObject";
        public const string ActionAddComponent = "action:AddComponent";
        public const string ActionAssignField = "action:AssignField";
        public const string ActionCreateAsset = "action:CreateAsset";
        public const string ActionOpenAsset = "action:OpenAsset";
        public const string ActionOpenWindow = "action:OpenWindow";
        public const string ActionCreateGameObject = "action:CreateGameObject";
        public const string ActionCreateComponent = "action:CreateComponent";
        public const string ActionCreatePrefab = "action:CreatePrefab";
        public const string ActionCreateClip = "action:CreateClip";
        public const string ActionCreateController = "action:CreateController";
        public const string ActionBindReference = "action:BindReference";
        public const string ActionAssignTrack = "action:AssignTrack";
        public const string ActionPreviewAnimation = "action:PreviewAnimation";
        public const string ActionFrameSelected = "action:FrameSelected";
        public const string ActionPingAsset = "action:PingAsset";
        public const string ActionSelectInHierarchy = "action:SelectInHierarchy";
        public const string ActionOpenGraphAsset = "action:OpenGraphAsset";
        public const string ActionReviewCode = "action:ReviewCode";
        public const string ActionRunPlayModeCheck = "action:RunPlayModeCheck";
        public const string ActionResolveMissingScript = "action:ResolveMissingScript";

        public const string ProjectionSettings = "projection:Settings";
        public const string ProjectionIntent = "projection:Intent";
        public const string ProjectionOverview = "projection:Overview";
        public const string ProjectionGuide = "projection:Guide";
        public const string ProjectionMap = "projection:Map";
        public const string ProjectionHygiene = "projection:Hygiene";
        public const string ProjectionFacts = "projection:Facts";

        public static string Node(System.Enum kind)
        {
            return "node:" + kind;
        }

        public static string Edge(System.Enum kind)
        {
            return "edge:" + kind;
        }
    }
}
