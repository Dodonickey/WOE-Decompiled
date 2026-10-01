#if !UNITY_IPHONE && !UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace UnityEngine
{
    [Flags]
    public enum PushNotificationType
    {
        None = 0,
        Badge = 1,
        Sound = 2,
        Alert = 4
    }
    public enum LocalNotificationType
    {
        None = 0,
        Badge = 1,
        Sound = 2,
        Alert = 4
    }

    public enum RemoteNotificationType
    {
        None = 0,
        Badge = 1,
        Sound = 2,
        Alert = 4
    }

    public sealed class LocalNotification
    {
        public DateTime fireDate { get; set; }
        public string alertBody { get; set; }
        public string alertAction { get; set; }
        public bool hasAction { get; set; }
        public int applicationIconBadgeNumber { get; set; }
        public string soundName { get; set; }
        public IDictionary<string, string> userInfo { get; set; }
    }

    public sealed class RemoteNotification
    {
        public string alertBody { get; private set; }
        public bool hasAction { get; private set; }
        public int applicationIconBadgeNumber { get; private set; }
        public string soundName { get; private set; }
        public IDictionary<string, string> userInfo { get; private set; }
    }

    public sealed class NotificationServices
    {
        private static List<LocalNotification> _scheduledNotifications = new List<LocalNotification>();
        private static byte[] _mockDeviceToken = null;
        private static RemoteNotificationType _enabledTypes = RemoteNotificationType.None;

        public static int localNotificationCount
        {
            get { return _scheduledNotifications.Count; }
        }

        public static LocalNotification[] localNotifications
        {
            get { return _scheduledNotifications.ToArray(); }
        }

        public static LocalNotification[] scheduledLocalNotifications
        {
            get { return _scheduledNotifications.ToArray(); }
        }

        public static int remoteNotificationCount
        {
            get { return 0; }
        }

        public static RemoteNotification[] remoteNotifications
        {
            get { return new RemoteNotification[0]; }
        }

        public static RemoteNotificationType enabledRemoteNotificationTypes
        {
            get { return _enabledTypes; }
        }

        /// <summary>
        /// Mimics iOS APNs token (32 bytes). Returns null until Register is called, 
        /// then returns a valid dummy byte array so ByteArrayToString() won't crash.
        /// </summary>
        public static byte[] deviceToken
        {
            get { return _mockDeviceToken; }
        }

        public static string registrationError
        {
            get { return null; }
        }

        public static LocalNotification GetLocalNotification(int index)
        {
            if (index >= 0 && index < _scheduledNotifications.Count)
                return _scheduledNotifications[index];
            return null;
        }

        public static void ScheduleLocalNotification(LocalNotification notification)
        {
            if (notification != null)
                _scheduledNotifications.Add(notification);
        }

        public static void PresentLocalNotificationNow(LocalNotification notification) { }

        public static void CancelLocalNotification(LocalNotification notification)
        {
            if (notification != null)
                _scheduledNotifications.Remove(notification);
        }

        public static void CancelAllLocalNotifications()
        {
            _scheduledNotifications.Clear();
        }

        public static RemoteNotification GetRemoteNotification(int index)
        {
            return null;
        }

        public static void ClearLocalNotifications()
        {
            _scheduledNotifications.Clear();
        }

        public static void ClearRemoteNotifications() { }

        public static void RegisterForLocalNotificationTypes(LocalNotificationType notificationTypes) { }

        /// <summary>
        /// When the game registers for push notifications, we generate a valid 32-byte dummy token.
        /// </summary>
        public static void RegisterForRemoteNotificationTypes(RemoteNotificationType notificationTypes)
        {
            _enabledTypes = notificationTypes;

            // Generate a 32-byte mock token (Standard APNs token length)
            // Example: [0xDE, 0xAD, 0xBE, 0xEF, ...]
            _mockDeviceToken = new byte[32];
            for (int i = 0; i < _mockDeviceToken.Length; i++)
            {
                _mockDeviceToken[i] = (byte)(i + 1);
            }
        }

        public static void RegisterForNotifications(PushNotificationType notificationTypes)
        {
            RegisterForRemoteNotificationTypes((RemoteNotificationType)(int)notificationTypes);
        }

        public static void UnregisterForRemoteNotifications()
        {
            _mockDeviceToken = null;
            _enabledTypes = RemoteNotificationType.None;
        }
    }
}
#endif