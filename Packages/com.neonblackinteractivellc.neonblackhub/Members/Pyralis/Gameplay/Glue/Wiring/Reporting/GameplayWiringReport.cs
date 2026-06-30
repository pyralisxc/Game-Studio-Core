using System.Collections.Generic;
using System.Text;

namespace NeonBlack.Gameplay.Glue.Wiring.Reporting
{
    public enum GameplayWiringRowKind
    {
        DataIntake,
        Provider,
        Receiver,
        Delivery,
        MissingProvider,
        AmbiguousProvider,
        TimingIssue,
        Inventory,
        CutCandidate,
        ValidationIssue,
        ServiceActivation
    }

    public enum GameplayWiringScope
    {
        Unknown,
        Scene,
        Session,
        Participant,
        Pawn,
        Feature,
        Presentation,
        Network,
        Editor
    }

    public enum GameplayWiringTiming
    {
        Unknown,
        Authoring,
        Startup,
        Spawn,
        Join,
        Play,
        Respawn,
        Teardown,
        EditorOnly
    }

    public enum GameplayWiringRequiredness
    {
        Unknown,
        Required,
        Optional,
        FallbackFree,
        AutoDerived,
        DisplayOnly
    }

    public enum GameplayWiringSeverity
    {
        Info,
        Warning,
        Error,
        Cleanup
    }

    public readonly struct GameplayWiringRow
    {
        public GameplayWiringRow(
            GameplayWiringRowKind kind,
            string contract,
            string provider,
            string receiver,
            string package,
            string owner,
            GameplayWiringScope scope,
            GameplayWiringTiming timing,
            GameplayWiringRequiredness requiredness,
            GameplayWiringSeverity severity,
            string evidence = null)
        {
            Kind = kind;
            Contract = contract ?? string.Empty;
            Provider = provider ?? string.Empty;
            Receiver = receiver ?? string.Empty;
            Package = package ?? string.Empty;
            Owner = owner ?? string.Empty;
            Scope = scope;
            Timing = timing;
            Requiredness = requiredness;
            Severity = severity;
            Evidence = evidence ?? string.Empty;
        }

        public GameplayWiringRowKind Kind { get; }
        public string Contract { get; }
        public string Provider { get; }
        public string Receiver { get; }
        public string Package { get; }
        public string Owner { get; }
        public GameplayWiringScope Scope { get; }
        public GameplayWiringTiming Timing { get; }
        public GameplayWiringRequiredness Requiredness { get; }
        public GameplayWiringSeverity Severity { get; }
        public string Evidence { get; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Kind);
            builder.Append(": ");
            builder.Append(Contract);

            if (!string.IsNullOrWhiteSpace(Provider))
            {
                builder.Append(" provider=");
                builder.Append(Provider);
            }

            if (!string.IsNullOrWhiteSpace(Receiver))
            {
                builder.Append(" receiver=");
                builder.Append(Receiver);
            }

            return builder.ToString();
        }
    }

    public sealed class GameplayWiringReport
    {
        private readonly List<GameplayWiringRow> _rows = new List<GameplayWiringRow>();

        public IReadOnlyList<GameplayWiringRow> Rows => _rows;
        public int Count => _rows.Count;

        public void Add(GameplayWiringRow row)
        {
            _rows.Add(row);
        }

        public IEnumerable<GameplayWiringRow> RowsOfKind(GameplayWiringRowKind kind)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].Kind == kind)
                    yield return _rows[i];
            }
        }
    }
}
