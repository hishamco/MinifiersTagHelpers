using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;

namespace MinifiersTagHelpers.TagHelpers
{
    [HtmlTargetElement("script")]
    public class JavaScriptMinifierTagHelper : TagHelper
    {
        private const string MinifyAttributeName = "minify";
        private readonly IFileProvider _wwwroot;
        private readonly string _wwwrootFolder;

        public JavaScriptMinifierTagHelper(IHostingEnvironment env)
        {
            _wwwroot = env.WebRootFileProvider;
            _wwwrootFolder = env.WebRootPath;
        }

        [HtmlAttributeName("src")]
        public string Src { get; set; }

        [HtmlAttributeName(MinifyAttributeName)]
        public bool? Minify { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (output.TagName == "script")
            {
                if (Src == null)
                {
                    var content = output.GetChildContentAsync().Result.GetContent();
                    var result = await JavaScriptMinifier.MinifyJs(content);

                    output.Content.SetContent(result);
                }
                else
                {
                    if (!Minify.HasValue)
                    {
                        if (Src != null)
                        {
                            output.Attributes.SetAttribute("src", Src);
                        }

                        return;
                    }

                    var fileInfo = _wwwroot.GetFileInfo(Src);
                    var jsDirectory = Src.Substring(0, Src.IndexOf(fileInfo.Name) - 1);
                    var minFileName = fileInfo.Name.Insert(fileInfo.Name.Length - 3, ".min");
                    var minFilePath = Path.Combine(_wwwrootFolder, jsDirectory, minFileName);

                    if (File.Exists(minFilePath))
                    {
                        if (Src != null)
                        {
                            output.Attributes.SetAttribute("src", Src.Replace(".js", ".min.js"));
                        }

                        return;
                    }

                    using (var readStream = fileInfo.CreateReadStream())
                    using (var reader = new StreamReader(readStream, Encoding.UTF8))
                    {
                        var content = await JavaScriptMinifier.MinifyJs(await reader.ReadToEndAsync());

                        using (var writer = new StreamWriter(File.Create(minFilePath), Encoding.UTF8))
                        {
                            await writer.WriteAsync(content);
                        }
                    }

                    if (Src != null)
                    {
                        output.Attributes.SetAttribute("src", Src.Replace(".js", ".min.js"));
                    }
                }
            }
        }
    }
}