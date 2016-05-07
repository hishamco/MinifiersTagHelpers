using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;

namespace MinifiersTagHelpers.TagHelpers
{
    [HtmlTargetElement("style")]
    [HtmlTargetElement("link", Attributes = MinifyAttributeName)]
    public class CssMinifierTagHelper : TagHelper
    {
        private const string MinifyAttributeName = "minify";
        private readonly IFileProvider _wwwroot;
        private readonly string _wwwrootFolder;

        public CssMinifierTagHelper(IHostingEnvironment env)
        {
            _wwwroot = env.WebRootFileProvider;
            _wwwrootFolder = env.WebRootPath;
        }

        [HtmlAttributeName("rel")]
        public string Rel { get; set; }

        [HtmlAttributeName("href")]
        public string Href { get; set; }

        [HtmlAttributeName(MinifyAttributeName)]
        public bool? Minify { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (output.TagName == "style")
            {
                var content = output.GetChildContentAsync().Result.GetContent();
                var result = await CssMinifier.MinifyCss(content);

                output.Content.SetContent(result);
            }

            if (Rel == null || Href == null)
            {
                return;
            }

            if (output.TagName == "link" && Rel == "stylesheet")
            {
                if (!string.IsNullOrEmpty(Href))
                {
                    if (Minify.HasValue && !Minify.Value)
                    {
                        return;
                    }

                    var fileInfo = _wwwroot.GetFileInfo(Href);
                    var cssDirectory = Href.Substring(0, Href.IndexOf(fileInfo.Name) - 1);
                    var minFileName = fileInfo.Name.Replace(".css", ".min.css");
                    var minFilePath = Path.Combine(_wwwrootFolder, cssDirectory, minFileName);

                    if (Rel != null)
                    {
                        output.Attributes.SetAttribute("rel", "stylesheet");
                    }

                    if (File.Exists(minFilePath))
                    {
                        if (Href != null)
                        {
                            output.Attributes.SetAttribute("href", Href.Replace(".css", ".min.css"));
                        }

                        return;
                    }

                    using (var readStream = fileInfo.CreateReadStream())
                    using (var reader = new StreamReader(readStream, Encoding.UTF8))
                    {
                        var content = await CssMinifier.MinifyCss(await reader.ReadToEndAsync());

                        using (var writer = new StreamWriter(File.Create(minFilePath), Encoding.UTF8))
                        {
                            await writer.WriteAsync(content);
                        }
                    }

                    if (Href != null)
                    {
                        output.Attributes.SetAttribute("href", Href.Replace(".css", ".min.css"));
                    }
                }
            }
        }
    }
}