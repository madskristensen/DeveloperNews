using System;
using System.ComponentModel;

namespace DevNews
{
    /// <summary>
    /// The options used by this extension.
    /// </summary>
    internal class Options : BaseOptionModel<Options>
    {
        [DefaultValue("")]
        [Browsable(false)]
        public string FeedSelection { get; set; } = "";

        [Browsable(false)]
        public DateTime LastRead { get; set; }

        [Browsable(false)]
        public int UnreadPosts { get; set; }
    }
}
