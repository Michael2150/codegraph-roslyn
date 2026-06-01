using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace VbFixture.Tests
{
    // Invokes the codegraph-roslyn binary against a fixture file and returns the
    // parsed ExtractionResult.  Set the CODEGRAPH_ROSLYN_BIN environment variable
    // to the absolute path of the binary when it is not on PATH.
    internal static class FixtureRunner
    {
        private static readonly string BinPath =
            Environment.GetEnvironmentVariable("CODEGRAPH_ROSLYN_BIN") ?? "codegraph-roslyn";

        // Fixtures root: codegraph-roslyn/fixtures/ relative to this project
        private static readonly string FixturesRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures"));

        public static bool IsBinaryAvailable()
        {
            try
            {
                var psi = new ProcessStartInfo(BinPath, "--version")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(5_000);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // relativeFixturePath is relative to codegraph-roslyn/fixtures/,
        // e.g. "VbFixture/Animals.vb"
        public static ExtractionResult RunOnFile(string relativeFixturePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(FixturesRoot, relativeFixturePath));
            Assert.True(File.Exists(fullPath), $"Fixture not found: {fullPath}");

            var psi = new ProcessStartInfo
            {
                FileName               = BinPath,
                Arguments              = $"--file \"{fullPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException($"Could not start {BinPath}");

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Assert.True(
                process.ExitCode == 0,
                $"codegraph-roslyn exited {process.ExitCode} for {relativeFixturePath}.\nSTDERR: {stderr}");

            return JsonSerializer.Deserialize<ExtractionResult>(stdout, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }) ?? throw new InvalidOperationException("Null JSON response");
        }
    }

    // Extension helpers used in test assertions
    internal static class ResultExtensions
    {
        public static ExtractionNode? Node(this ExtractionResult r, string kind, string name) =>
            r.Nodes.FirstOrDefault(n => n.Kind == kind && n.Name == name);

        public static bool HasNode(this ExtractionResult r, string kind, string name) =>
            r.Nodes.Any(n => n.Kind == kind && n.Name == name);

        // Returns true when a resolved edge exists whose from-node is named fromName
        // and whose to-node (or unresolved toQualifiedName) ends with toName.
        public static bool HasEdge(this ExtractionResult r, string kind, string fromName, string toName)
        {
            var fromIds = r.Nodes.Where(n => n.Name == fromName).Select(n => n.Id).ToHashSet();
            var toIds   = r.Nodes.Where(n => n.Name == toName).Select(n => n.Id).ToHashSet();

            if (r.Edges.Any(e =>
                e.Kind == kind &&
                fromIds.Contains(e.FromId) &&
                toIds.Contains(e.ToId)))
                return true;

            // Also check unresolved cross-file references
            return r.UnresolvedReferences.Any(u =>
                u.Kind == kind &&
                fromIds.Contains(u.FromId) &&
                (u.ToQualifiedName.EndsWith("." + toName) || u.ToQualifiedName == toName));
        }

        public static ExtractionNode? ChildNode(
            this ExtractionResult r, string parentName, string childKind, string childName)
        {
            var parent = r.Nodes.FirstOrDefault(n => n.Name == parentName);
            if (parent is null) return null;
            return r.Nodes.FirstOrDefault(n =>
                n.Kind == childKind && n.Name == childName && n.ParentId == parent.Id);
        }
    }
}
