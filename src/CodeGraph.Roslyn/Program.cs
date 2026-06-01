using System.Text.Json;
using CodeGraph.Roslyn;

if (args.Contains("--version"))
{
    Console.WriteLine("1.0.0");
    return 0;
}

var fileIdx = Array.IndexOf(args, "--file");
if (fileIdx < 0 || fileIdx + 1 >= args.Length)
{
    Console.Error.WriteLine("Usage: codegraph-roslyn --file <path> [--stdin]");
    return 1;
}

var filePath = args[fileIdx + 1];
var useStdin = args.Contains("--stdin");

try
{
    // When --stdin is passed, read source from stdin so the caller does not
    // need the file to be accessible on disk at the binary's working directory.
    // The file path is still used for node IDs and qualified names.
    var source = useStdin
        ? await Console.In.ReadToEndAsync()
        : await File.ReadAllTextAsync(filePath);

    if (!useStdin && !File.Exists(filePath))
    {
        Console.Error.WriteLine($"File not found: {filePath}");
        return 1;
    }

    var ext = Path.GetExtension(filePath).ToLowerInvariant();

    ExtractionResult result = ext switch
    {
        ".cs" => new CSharpExtractor(filePath, source).Extract(),
        ".vb" => new VBNetExtractor(filePath, source).Extract(),
        _ => new ExtractionResult
        {
            Errors = [new ExtractionError { Message = $"Unsupported file type: {ext}" }]
        }
    };

    var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    Console.WriteLine(json);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
