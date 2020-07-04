using System.ComponentModel;

namespace DeveloperNews
{
    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [Category("General")]
        [DisplayName("Open in default browser")]
        [Description("Specifies if the blog posts should open in the default browser or inside Visual Studio.")]
        [DefaultValue(false)]
        public bool OpenInDefaultBrowser { get; set; }
    }
}
