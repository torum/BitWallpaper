using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;

namespace BitWallpaper.Helpers
{
    internal class NotificationManager
    {
        private bool m_isRegistered;

        //private Dictionary<string, Action<AppNotificationActivatedEventArgs>> c_map;

        public NotificationManager()
        {
            m_isRegistered = false;

            // When adding new a scenario, be sure to add its notification handler here.
            //c_map = new Dictionary<int, Action<AppNotificationActivatedEventArgs>>();
            //c_map.Add(ToastWithAvatar.ScenarioId, ToastWithAvatar.NotificationReceived);
            //c_map.Add(ToastWithTextBox.ScenarioId, ToastWithTextBox.NotificationReceived);
        }

        ~NotificationManager()
        {
            Unregister();
        }

        public void Init()
        {
            // To ensure all Notification handling happens in this process instance, register for
            // NotificationInvoked before calling Register(). Without this a new process will
            // be launched to handle the notification.
            AppNotificationManager notificationManager = AppNotificationManager.Default;

            //notificationManager.NotificationInvoked += OnNotificationInvoked;

            notificationManager.Register();
            m_isRegistered = true;
        }

        public void Unregister()
        {
            if (m_isRegistered)
            {
                AppNotificationManager.Default.Unregister();
                m_isRegistered = false;
            }
        }

        public void ProcessLaunchActivationArgs(AppNotificationActivatedEventArgs notificationActivatedEventArgs)
        {
            // Complete in Step 5
        }

    }
    /*
    // ToastWithAvatar.cs
    class ToastWithAvatar
    {
        public const int ScenarioId = 1;
        public const string ScenarioName = "Local Toast with Avatar Image";

        public static bool SendToast()
        {
            var appNotification = new AppNotificationBuilder()
                .AddArgument("action", "ToastClick")
                .AddArgument(Common.scenarioTag, ScenarioId.ToString())
                .SetAppLogoOverride(new System.Uri("file://" + App.GetFullPathToAsset("Square150x150Logo.png")), AppNotificationImageCrop.Circle)
                .AddText(ScenarioName)
                .AddText("This is an example message using XML")
                .AddButton(new AppNotificationButton("Open App")
                    .AddArgument("action", "OpenApp")
                    .AddArgument(Common.scenarioTag, ScenarioId.ToString()))
                .BuildNotification();

            AppNotificationManager.Default.Show(appNotification);

            return appNotification.Id != 0; // return true (indicating success) if the toast was sent (if it has an Id)
        }

        public static void NotificationReceived(AppNotificationActivatedEventArgs notificationActivatedEventArgs)
        {
            // Complete in Step 5   
        }
    }
    */
    // Call SendToast() to send a notification.

    /*
var builder = new AppNotificationBuilder()
    .AddArgument("conversationId", 9813)
    .AddText("Adaptive Tiles Meeting", new AppNotificationTextProperties().SetMaxLines(1))
    .AddText("Conf Room 2001 / Building 135")
    .AddText("10:00 AM - 10:30 AM");

        .SetTimeStamp(new DateTime(2017, 04, 15, 19, 45, 00, DateTimeKind.Utc));

    .SetScenario(AppNotificationScenario.Alarm)
        AppNotificationManager.Default.Show(appNotification);
    */



}
