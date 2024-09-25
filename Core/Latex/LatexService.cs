using Aspose.TeX.Features;
using Utils;

namespace Core.Latex;

public class LatexService
{
    private MathRenderer _mathRenderer;
    private MathRendererOptions _options;
    private string _path;
    private Dictionary<string, string> _rendered = new();
    
    private static LatexService? _instance;

    public static LatexService Instance
    {
        get
        {
            _instance ??= new LatexService();
            return _instance;
        }
    }

    private LatexService()
    {
        _mathRenderer = new PngMathRenderer();
        _options = new PngMathRendererOptions(){Resolution = 150};

        // _options.Preamble = @"\usepackage{amsmath}
        //             \usepackage{amsfonts}
        //             \usepackage{amssymb}
        //             \usepackage{color}";

        _options.Scale = 1000;

        _options.TextColor = System.Drawing.Color.Black;

        _options.BackgroundColor = System.Drawing.Color.Transparent;

        _path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SergeiKrivko",
            Config.AppName, "Latex");
        Directory.CreateDirectory(_path);
    }

    private string GeneratePath()
    {
        return Path.Join(_path, Guid.NewGuid() + ".png");
    }

    public string Render(string formula)
    {
        if (_rendered.TryGetValue(formula, out var path) && !string.IsNullOrEmpty(path))
            return path;

        path = GeneratePath();
        var stream = File.Open(path, FileMode.Create);
        _mathRenderer.Render(formula, stream, _options);
        stream.Close();
        _rendered[formula] = path;
        return path;
    }

    public void Clear()
    {
        _rendered.Clear();
        foreach (var file in Directory.GetFiles(_path))
        {
            File.Delete(file);
        }
    }

    public string ParseMarkdown(string markdown)
    {
        int startIndex, endIndex = 0;
        while (true)
        {
            startIndex = markdown.IndexOf("\\(", endIndex, StringComparison.Ordinal);
            if (startIndex == -1)
                break;
            endIndex = markdown.IndexOf("\\)", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;
            var formula = markdown.Substring(startIndex, endIndex - startIndex + 2);
            markdown = markdown.Replace(formula, $"![image]({Render(formula.Substring(2, formula.Length - 4))})");
            // markdown = markdown.Replace(formula, 
                // "\n\n![image](http://placehold.it/200x100)\n\n");
        }

        return markdown;
    }
}