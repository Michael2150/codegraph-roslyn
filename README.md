# codegraph-roslyn

Roslyn-based extractor for [@colbymchenry/codegraph](https://github.com/colbymchenry/codegraph). Parses VB.NET and C# source files using the actual .NET compiler API and emits a JSON symbol graph — classes, modules, methods, properties, fields, enums, imports, call edges, inheritance edges — for import into codegraph's SQLite index.

Tree-sitter has no VB.NET grammar and its C# grammar lacks semantic information. This tool fills that gap by running as a subprocess invoked by the `RoslynExtractor` bridge in codegraph.

---

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows, macOS, or Linux

---

## Build

```bash
# Clone the repo
git clone https://github.com/your-fork/codegraph-roslyn
cd codegraph-roslyn

# Debug build (used during development and by the test suite)
dotnet build

# The binary lands at:
# src/CodeGraph.Roslyn/bin/Debug/net8.0/codegraph-roslyn.exe   (Windows)
# src/CodeGraph.Roslyn/bin/Debug/net8.0/codegraph-roslyn       (macOS / Linux)
```

### Self-contained release build (for bundling with the npm package)

```bash
# Windows
dotnet publish src/CodeGraph.Roslyn -r win-x64 -c Release \
  -p:PublishSingleFile=true --self-contained true \
  -o publish/win-x64

# macOS (Intel)
dotnet publish src/CodeGraph.Roslyn -r osx-x64 -c Release \
  -p:PublishSingleFile=true --self-contained true \
  -o publish/osx-x64

# Linux
dotnet publish src/CodeGraph.Roslyn -r linux-x64 -c Release \
  -p:PublishSingleFile=true --self-contained true \
  -o publish/linux-x64
```

Copy the resulting binary into the codegraph fork's `bin/` directory:

```
bin/
  codegraph-roslyn-win-x64.exe
  codegraph-roslyn-osx-x64
  codegraph-roslyn-linux-x64
```

---

## CLI usage

```
codegraph-roslyn --file <path-to-source-file>
```

Writes JSON to stdout. Writes errors to stderr. Exit code 0 on success.

```bash
# Example
codegraph-roslyn --file fixtures/VbFixture/GeometryTypes.vb
```

Output:

```json
{
  "nodes": [
    {
      "id": "C:/repo/GeometryTypes.vb::Fixtures::GeometryUtils",
      "kind": "module",
      "name": "GeometryUtils",
      "qualifiedName": "Fixtures.GeometryUtils",
      "filePath": "C:/repo/GeometryTypes.vb",
      "startLine": 3,
      "endLine": 20,
      "visibility": "public",
      "isStatic": false,
      "isAsync": false,
      "parentId": "C:/repo/GeometryTypes.vb::Fixtures"
    }
  ],
  "edges": [
    {
      "kind": "contains",
      "fromId": "C:/repo/GeometryTypes.vb::Fixtures",
      "toId": "C:/repo/GeometryTypes.vb::Fixtures::GeometryUtils",
      "toQualifiedName": "Fixtures.GeometryUtils"
    }
  ],
  "unresolvedReferences": [],
  "errors": []
}
```

---

## Running against the test fixtures

The `fixtures/` directory contains small VB.NET and C# files that cover the node and edge types the extractor must handle. You can run the tool against any of them directly:

```bash
# After a debug build, point to the binary
export CODEGRAPH_ROSLYN_BIN=./src/CodeGraph.Roslyn/bin/Debug/net8.0/codegraph-roslyn
# Windows (PowerShell)
$env:CODEGRAPH_ROSLYN_BIN = ".\src\CodeGraph.Roslyn\bin\Debug\net8.0\codegraph-roslyn.exe"

# Run against a VB.NET fixture
$env:CODEGRAPH_ROSLYN_BIN --file fixtures/VbFixture/GeometryTypes.vb | python -m json.tool

# Run against a C# fixture
$env:CODEGRAPH_ROSLYN_BIN --file fixtures/CsFixture/GeometryTypes.cs | python -m json.tool
```

Available fixtures:

| File | What it tests |
|---|---|
| `VbFixture/GeometryTypes.vb` | Module, classes, methods, fields, `Overrides` |
| `VbFixture/EventSystem.vb` | `WithEvents`, `Handles`, `RaiseEvent`, event handler wiring |
| `VbFixture/PartialClass.Part1.vb` / `Part2.vb` | Partial class across two files |
| `VbFixture/ImportAliases.vb` | `Imports` with aliases, nested namespaces |
| `VbFixture/AsyncPatterns.vb` | `Async Sub` (void async), `Async Function`, `Await` |
| `VbFixture/Animals.vb` | Inheritance (`Inherits`), `Implements`, qualified names |
| `CsFixture/GeometryTypes.cs` | Classes, structs, interfaces, generics |
| `CsFixture/EventSystem.cs` | Events, delegates, `+=` wiring |
| `CsFixture/AsyncPatterns.cs` | `async`/`await`, `Task<T>` return types |
| `CsFixture/GenericTypes.cs` | Generic classes, constrained type parameters |
| `CsFixture/Animals.cs` | Inheritance chain, interface implementation, `override` |
| `CsFixture/ImportAliases.cs` | `using` aliases, nested namespaces |

---

## Running the tests

The test suite is xUnit-based. Tests require the debug binary to exist and are skipped (not failed) when it is unavailable.

```bash
# 1. Build the extractor first
dotnet build

# 2. Set the binary path
# macOS / Linux
export CODEGRAPH_ROSLYN_BIN=./src/CodeGraph.Roslyn/bin/Debug/net8.0/codegraph-roslyn

# Windows (PowerShell)
$env:CODEGRAPH_ROSLYN_BIN = ".\src\CodeGraph.Roslyn\bin\Debug\net8.0\codegraph-roslyn.exe"

# 3. Run the full suite
dotnet test

# Run only VB.NET tests
dotnet test tests/VbFixture.Tests

# Run only C# tests
dotnet test tests/CsFixture.Tests

# Run a specific test by name
dotnet test --filter "Module_IsExtractedWithKindModule"
```

Expected output: **131 tests pass, 0 fail, 0 skip** when `CODEGRAPH_ROSLYN_BIN` is set. All tests skip cleanly when the env var is absent (useful in CI before a binary build step).

---

## Project structure

```
codegraph-roslyn/
├── src/
│   └── CodeGraph.Roslyn/
│       ├── CodeGraph.Roslyn.csproj
│       ├── Program.cs          # CLI entry point
│       ├── Models.cs           # JSON output types
│       ├── CSharpExtractor.cs  # C# syntax walker
│       └── VBNetExtractor.cs   # VB.NET syntax walker
├── tests/
│   ├── CsFixture.Tests/        # xUnit tests for C# extraction
│   └── VbFixture.Tests/        # xUnit tests for VB.NET extraction
├── fixtures/
│   ├── CsFixture/              # C# source fixtures
│   └── VbFixture/              # VB.NET source fixtures
└── codegraph-roslyn.slnx       # Solution file
```

---

## How it integrates with codegraph

codegraph's `RoslynExtractor` class (at `src/extraction/roslyn-extractor.ts` in the codegraph fork) invokes this binary as a subprocess via `execFileSync`, passing `--file <path>`. The JSON output is parsed and mapped to codegraph's internal `Node`, `Edge`, and `UnresolvedReference` types before being written to SQLite.

The binary is selected at runtime based on platform:

| Platform | Binary |
|---|---|
| Windows | `bin/codegraph-roslyn-win-x64.exe` |
| macOS | `bin/codegraph-roslyn-osx-x64` |
| Linux | `bin/codegraph-roslyn-linux-x64` |

During development, set `CODEGRAPH_ROSLYN_BIN` to override the path to the binary.
