using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace WebDownloader
{
    public class WebDownloader : WebClient
    {
        private readonly IDictionary<AsyncCompletedEventHandler, AsyncCompletedEventHandler> _extendedEventHandlerDictionary;

        private System.Timers.Timer _timer;

        public int Timeout { get; set; } = 10000;

        public new event AsyncCompletedEventHandler DownloadFileCompleted
        {
            add
            {
                void DownloadFileCompletedEventWrapper(object sender, AsyncCompletedEventArgs e)
                {
                    value.Invoke(sender, e);
                    var semaphore = (CountdownEvent)e.UserState;
                    semaphore.Signal();
                }

                _extendedEventHandlerDictionary.Add(value, DownloadFileCompletedEventWrapper);
                base.DownloadFileCompleted += DownloadFileCompletedEventWrapper;
            }
            remove
            {
                if (_extendedEventHandlerDictionary.ContainsKey(value))
                {
                    var eventHandler = _extendedEventHandlerDictionary[value];
                    _extendedEventHandlerDictionary.Remove(eventHandler);
                    base.DownloadFileCompleted -= eventHandler;
                }
            }
        }

        public WebDownloader()
        {
            _extendedEventHandlerDictionary = new Dictionary<AsyncCompletedEventHandler, AsyncCompletedEventHandler>();
            DownloadProgressChanged += WebClientDownloadProgressChanged;
            DownloadFileCompleted += WebClientDownloadFileCompleted;
        }

        public new void DownloadFile(Uri address, string fileName)
        {
            Task.Run(() => DownloadFileAsyncLocked(address, fileName)).Wait();
        }

        public new void DownloadFileAsync(Uri address, string fileName)
        {
            DisposeTimer();
            InitTimer();
            _timer.Start();
            base.DownloadFileAsync(address, fileName);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = Timeout;
            }
            return request;
        }

        private void DownloadFileAsyncLocked(Uri address, string fileName)
        {
            DisposeTimer();
            InitTimer();
            _timer.Start();

            var semaphoreObject = new CountdownEvent(_extendedEventHandlerDictionary.Count);
            DownloadFileAsync(address, fileName, semaphoreObject);
            semaphoreObject.Wait();
            semaphoreObject.Dispose();
        }

        private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _timer?.Stop();
            _timer?.Start();
        }

        private void WebClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DisposeTimer();
        }

        private void InitTimer()
        {
            _timer = new System.Timers.Timer
            {
                Interval = Timeout
            };

            _timer.Elapsed += TimerElapsed;
        }

        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= TimerElapsed;
                _timer.Close();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            CancelAsync();
            _timer.Stop();
        }
    }
}
