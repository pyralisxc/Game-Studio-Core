using System;
using System.Collections.Generic;
using System.IO;
using Pys.Authoring.Editor.Hygiene;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Exports
{
    public static class HygieneJsonExporter
    {
        public static string Export(HygieneProjection projection, string scriptsRoot)
        {
            string folder = Path.Combine(Path.GetFullPath("."), AuthoringGraphJsonExporter.DefaultExportFolder);
            Directory.CreateDirectory(folder);

            string safeRoot = string.IsNullOrWhiteSpace(scriptsRoot)
                ? "Project"
                : scriptsRoot.Replace('/', '_').Replace('\\', '_').Replace(':', '_');

            string path = Path.Combine(folder, safeRoot + "_hygiene.json");
            File.WriteAllText(path, ToJson(projection, scriptsRoot));
            EditorUtility.RevealInFinder(path);
            return path;
        }

        public static string ToJson(HygieneProjection projection, string scriptsRoot)
        {
            HygieneExportDto dto = HygieneExportDto.FromProjection(projection, scriptsRoot);
            return JsonUtility.ToJson(dto, true);
        }

        [Serializable]
        private sealed class HygieneExportDto
        {
            public string scriptsRoot;
            public int rowCount;
            public int reviewCount;
            public int warningCount;
            public int errorCount;
            public List<HygieneLensDto> lenses = new List<HygieneLensDto>();
            public List<HygieneRowDto> rows = new List<HygieneRowDto>();

            public static HygieneExportDto FromProjection(HygieneProjection projection, string scriptsRoot)
            {
                HygieneExportDto dto = new HygieneExportDto
                {
                    scriptsRoot = scriptsRoot ?? string.Empty,
                    rowCount = projection != null ? projection.Rows.Count : 0,
                    reviewCount = projection != null ? projection.ReviewCount : 0,
                    warningCount = projection != null ? projection.WarningCount : 0,
                    errorCount = projection != null ? projection.ErrorCount : 0
                };

                if (projection == null)
                    return dto;

                for (int i = 0; i < projection.Lenses.Count; i++)
                    dto.lenses.Add(HygieneLensDto.FromLens(projection.Lenses[i]));

                for (int i = 0; i < projection.Rows.Count; i++)
                    dto.rows.Add(HygieneRowDto.FromRow(projection.Rows[i]));

                return dto;
            }
        }

        [Serializable]
        private sealed class HygieneLensDto
        {
            public string kind;
            public string title;
            public string question;
            public int rowCount;
            public int reviewCount;
            public int warningCount;
            public int errorCount;
            public List<HygieneRowDto> rows = new List<HygieneRowDto>();

            public static HygieneLensDto FromLens(HygieneLensProjection lens)
            {
                HygieneLensDto dto = new HygieneLensDto
                {
                    kind = lens != null ? lens.Kind.ToString() : string.Empty,
                    title = lens != null ? lens.Title : string.Empty,
                    question = lens != null ? lens.Question : string.Empty,
                    rowCount = lens != null ? lens.Rows.Count : 0,
                    reviewCount = lens != null ? lens.ReviewCount : 0,
                    warningCount = lens != null ? lens.WarningCount : 0,
                    errorCount = lens != null ? lens.ErrorCount : 0
                };

                if (lens == null)
                    return dto;

                for (int i = 0; i < lens.Rows.Count; i++)
                    dto.rows.Add(HygieneRowDto.FromRow(lens.Rows[i]));

                return dto;
            }
        }

        [Serializable]
        private sealed class HygieneRowDto
        {
            public string lens;
            public string issueCode;
            public string title;
            public string severity;
            public string ownerId;
            public string detail;
            public string sourceKind;
            public string sourcePath;
            public string evidenceIds;
            public string claim;
            public string evidence;
            public string recommendation;
            public string confidence;
            public bool canNavigate;

            public static HygieneRowDto FromRow(HygieneRow row)
            {
                return new HygieneRowDto
                {
                    lens = row != null ? row.Lens.ToString() : string.Empty,
                    issueCode = row != null ? row.IssueCode : string.Empty,
                    title = row != null ? row.Title : string.Empty,
                    severity = row != null ? row.Severity.ToString() : string.Empty,
                    ownerId = row != null ? row.OwnerId : string.Empty,
                    detail = row != null ? row.Detail : string.Empty,
                    sourceKind = row != null ? row.SourceKind : string.Empty,
                    sourcePath = row != null ? row.SourcePath : string.Empty,
                    evidenceIds = row != null ? row.EvidenceIds : string.Empty,
                    claim = row != null ? row.Claim : string.Empty,
                    evidence = row != null ? row.Evidence : string.Empty,
                    recommendation = row != null ? row.Recommendation : string.Empty,
                    confidence = row != null ? row.Confidence : string.Empty,
                    canNavigate = row != null && row.CanNavigate
                };
            }
        }
    }
}
