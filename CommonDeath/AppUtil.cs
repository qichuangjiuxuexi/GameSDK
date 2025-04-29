using UnityEngine;

namespace AppBase.CommonDeath
{
    public static class AppUtil
    {
        /// <summary>
        /// 设备Id
        /// IOS会存储在KeyChain
        /// </summary>
        public static string DeviceId
        {
            get
            {
                if (_deviceId != null) return _deviceId;
                _deviceId = SystemInfo.deviceUniqueIdentifier;
                return _deviceId;
            }
        }
        private static string _deviceId;
    }
}