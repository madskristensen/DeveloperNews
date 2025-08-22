using System.Windows;
using System.Windows.Controls;

namespace DevNews.ToolWindows
{
    /// <summary>
    /// Template selector for news items to distinguish between timeline headers and posts
    /// </summary>
    public class NewsItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TimelineHeaderTemplate { get; set; }
        public DataTemplate PostTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TimelineHeaderViewModel)
                return TimelineHeaderTemplate;
            
            if (item is PostViewModel)
                return PostTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}