using System;

namespace Colyseus
{
    /// <summary>
    ///     Serializable class centralizing all of a user's information
    /// </summary>
    [Serializable]
    public class CSAUserData
    {
        /// <summary>
        ///     Unique user ID
        /// </summary>
        public string _id = null;

        /// <summary>
        ///     URL location of the user's avatar
        /// </summary>
        public string avatarUrl;

        /// <summary>
        ///     Array of IDs that the user has blocked
        /// </summary>
        public string[] blockedUserIds = null;

        /// <summary>
        ///     Date of user creation
        /// </summary>
        public DateTime createdAt = DateTime.MinValue;

        /// <summary>
        ///     Devices the user has signed in from
        /// </summary>
        public CSADeviceData[] devices = null;

        /// <summary>
        ///     Name other users will see
        /// </summary>
        public string displayName;

        /// <summary>
        ///     E-mail account associated with this user
        /// </summary>
        public string email = null;

        /// <summary>
        ///     Facebook ID associated with this user
        /// </summary>
        public string facebookId = null;

        /// <summary>
        ///     Array of IDs that are friends of the user
        /// </summary>
        public string[] friendIds = null;

        /// <summary>
        ///     User's GameCenter ID
        /// </summary>
        public string gameCenterId = null;

        /// <summary>
        ///     User's Google ID
        /// </summary>
        public string googleId = null;

        /// <summary>
        ///     If this user wishes to be anonymous
        /// </summary>
        public bool isAnonymous = true;

        /// <summary>
        ///     User's language of choice
        /// </summary>
        public string lang;

        /// <summary>
        ///     User's location
        /// </summary>
        public string location;

        /// <summary>
        ///     User specific metadata
        /// </summary>
        public object metadata = null;

        /// <summary>
        ///     User's Steam ID
        /// </summary>
        public string steamId = null;

        /// <summary>
        ///     User's timezone (EST, PST etc)
        /// </summary>
        public string timezone;

        /// <summary>
        ///     Saved token for the user
        /// </summary>
        public string token = null;

        /// <summary>
        ///     User's Twitter ID
        /// </summary>
        public string twitterId = null;

        /// <summary>
        ///     Date of the last update to this CSAUserData
        /// </summary>
        public DateTime updatedAt = DateTime.MinValue;

        /// <summary>
        ///     Username the user logs in with
        /// </summary>
        public string username;
    }
}