using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Collections.Generic;


namespace BrainGames.Services
{
    public class AppCenterAnalyticsService : IAnalyticsService
    {
        public void TrackEvent(string eventKey)
        {
            Analytics.TrackEvent(eventKey);
        }

        public void TrackEvent(string eventKey, IDictionary<string, string> data)
        {
            Analytics.TrackEvent(eventKey, data);
        }

        public void TrackError(Exception exception)
        {
            Crashes.TrackError(exception);
        }

        public void TrackError(Exception exception, IDictionary<string, string> data)
        {
            Crashes.TrackError(exception, data);
        }
    }
}
