using System;
using System.Text;

namespace NeonBlack.Gameplay.Glue.Wiring.Reporting
{
    public static class GameplayWiringReportTextFormatter
    {
        public static string Format(GameplayWiringReport report)
        {
            if (report == null)
                return "Gameplay Wiring Report\nRows: 0\n";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Gameplay Wiring Report");
            builder.Append("Rows: ");
            builder.AppendLine(report.Count.ToString());
            AppendSummary(builder, report);
            builder.AppendLine();

            for (int i = 0; i < report.Rows.Count; i++)
            {
                GameplayWiringRow row = report.Rows[i];
                builder.Append(i + 1);
                builder.Append(". ");
                builder.Append(row.Kind);
                builder.Append(" | ");
                builder.Append(row.Contract);
                builder.Append(" | Scope=");
                builder.Append(row.Scope);
                builder.Append(" | Timing=");
                builder.Append(row.Timing);
                builder.Append(" | Requiredness=");
                builder.Append(row.Requiredness);
                builder.Append(" | Severity=");
                builder.AppendLine(row.Severity.ToString());

                AppendField(builder, "Provider", row.Provider);
                AppendField(builder, "Receiver", row.Receiver);
                AppendField(builder, "Package", row.Package);
                AppendField(builder, "Owner", row.Owner);
                AppendField(builder, "Evidence", row.Evidence);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static void AppendSummary(StringBuilder builder, GameplayWiringReport report)
        {
            foreach (GameplayWiringRowKind kind in Enum.GetValues(typeof(GameplayWiringRowKind)))
            {
                int count = CountKind(report, kind);
                if (count == 0)
                    continue;

                builder.Append(kind);
                builder.Append(": ");
                builder.AppendLine(count.ToString());
            }
        }

        private static int CountKind(GameplayWiringReport report, GameplayWiringRowKind kind)
        {
            int count = 0;
            for (int i = 0; i < report.Rows.Count; i++)
            {
                if (report.Rows[i].Kind == kind)
                    count++;
            }

            return count;
        }

        private static void AppendField(StringBuilder builder, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            builder.Append("   ");
            builder.Append(label);
            builder.Append(": ");
            builder.AppendLine(value);
        }
    }
}
