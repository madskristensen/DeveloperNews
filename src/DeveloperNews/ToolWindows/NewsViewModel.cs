using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.Syndication;
using DevNews.Resources;

namespace DevNews.ToolWindows
{
    /// <summary>
    /// View model for the news window
    /// </summary>
    public class NewsViewModel : INotifyPropertyChanged
    {
        private string _totalCount;
        private ObservableCollection<object> _items;

        public NewsViewModel()
        {
            Items = new ObservableCollection<object>();
        }

        public ObservableCollection<object> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public string TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
            }
        }

        public void UpdateFeed(SyndicationFeed feed)
        {
            Items.Clear();

            if (feed == null)
            {
                TotalCount = "";
                return;
            }

            var feedItems = feed.Items.ToList();
            TotalCount = string.Format(Text.Totalcount, feedItems.Count);

            if (!feedItems.Any()) return;

            var currentTime = GetTimestamp(feedItems.First());
            if (!string.IsNullOrEmpty(currentTime))
            {
                Items.Add(new TimelineHeaderViewModel(currentTime));

                foreach (var item in feedItems)
                {
                    var newTime = GetTimestamp(item);

                    if (newTime != currentTime)
                    {
                        Items.Add(new TimelineHeaderViewModel(newTime));
                        currentTime = newTime;
                    }

                    Items.Add(new PostViewModel(item));
                }
            }
        }

        private static string GetTimestamp(SyndicationItem item)
        {
            if (item == null) return null;

            var publishDate = item.PublishDate.Date;
            var now = DateTime.Now;

            if (publishDate >= now.AddDays(-1))
                return Text.Timeline_today;
            if (publishDate >= now.AddDays(-2))
                return Text.Timeline_today;
            if (publishDate >= now.AddDays(-7))
                return Text.Timeline_thisweek;

            return Text.Timeline_older;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}