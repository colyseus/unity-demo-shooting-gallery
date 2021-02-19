using System;

namespace Colyseus
{
    /// <summary>
    ///     Data representing the current device
    /// </summary>
    [Serializable]
    public class CSADeviceData
    {
        /// <summary>
        ///     The device's unique ID
        /// </summary>
        public string id = null;

        /// <summary>
        ///     The device's platform
        /// </summary>
        public string platform = null;
    }
}