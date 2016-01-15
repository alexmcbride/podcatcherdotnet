using System;

namespace PodcatcherDotNet.ViewModels {
    public class RemoveDateViewModel {
        public DateTime Date { get; private set; }
        public string Title { get; private set; }

        public RemoveDateViewModel(DateTime date, string title) {
            Date = date;
            Title = title;
        }
    }
}
