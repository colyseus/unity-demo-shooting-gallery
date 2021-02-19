using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable InconsistentNaming - Unsure what re-naming fields could do with legacy apps using Colyseus

#if NET_LEGACY
using System.Web;
#endif

namespace Colyseus
{
    /// <summary>
    ///     Serializable class containing a collection of CSAUserData
    /// </summary>
    [Serializable]
    public class CSAUserDataCollection
    {
        /// <summary>
        ///     Array of users
        /// </summary>
        public CSAUserData[] users;
    }

    /// <summary>
    ///     Wrapper class for user authorization. Public fields are all set via incoming CSAUserData
    /// </summary>
    public class ColyseusAuth
    {
        /// <summary>
        ///     Unique user ID
        /// </summary>
        public string _id;

        /// <summary>
        ///     URL location of the user's avatar
        /// </summary>
        public string AvatarUrl;

        /// <summary>
        ///     Array of IDs that the user has blocked
        /// </summary>
        public string[] BlockedUserIds;

        /// <summary>
        ///     Devices the user has signed in from
        /// </summary>
        public CSADeviceData[] Devices;

        /// <summary>
        ///     Name other users will see
        /// </summary>
        public string DisplayName;

        /// <summary>
        ///     E-mail account associated with this user
        /// </summary>
        public string Email;

        /// <summary>
        ///     Cached reference to the WebSocketEndpoint that the user requests from
        /// </summary>
        protected Uri Endpoint;

        /// <summary>
        ///     Facebook ID associated with this user
        /// </summary>
        public string FacebookId;

        /// <summary>
        ///     Array of IDs that are friends of the user
        /// </summary>
        public string[] FriendIds;

        /// <summary>
        ///     User's GameCenter ID
        /// </summary>
        public string GameCenterId;

        /// <summary>
        ///     User's Google ID
        /// </summary>
        public string GoogleId;

        /// <summary>
        ///     If this user wishes to be anonymous
        /// </summary>
        public bool IsAnonymous;

        /// <summary>
        ///     User's language of choice
        /// </summary>
        public string Lang;

        /// <summary>
        ///     User's location
        /// </summary>
        public string Location;

        /// <summary>
        ///     User specific metadata
        /// </summary>
        public object Metadata;

        /// <summary>
        ///     User's Steam ID
        /// </summary>
        public string SteamId;

        /// <summary>
        ///     User's timezone (EST, PST etc)
        /// </summary>
        public string Timezone;

        /// <summary>
        ///     Saved auth token for the user
        /// </summary>
        public string Token;

        /// <summary>
        ///     User's Twitter ID
        /// </summary>
        public string TwitterId;

        /// <summary>
        ///     Username the user logs in with
        /// </summary>
        public string Username;

        public ColyseusAuth(Uri endpoint)
        {
            Endpoint = endpoint;
            Token = PlayerPrefs.GetString("Token", string.Empty);
        }

        /// <summary>
        ///     Getter function to determine if Token has been set
        /// </summary>
        public bool HasToken
        {
            get { return !string.IsNullOrEmpty(Token); }
        }

        /// <summary>
        ///     Login Anonymously or as a Guest
        /// </summary>
        /// <returns><see cref="ColyseusAuth" /> via async task</returns>
        public async Task<ColyseusAuth> Login( /* anonymous */)
        {
            return await Login(ColyseusHttpUtility.ParseQueryString(string.Empty));
        }

        /// <summary>
        ///     Login with a Facebook Access Token
        /// </summary>
        /// <param name="facebookAccessToken">User's Facebook access token</param>
        /// <returns><see cref="ColyseusAuth" /> via async task</returns>
        public async Task<ColyseusAuth> Login(string facebookAccessToken)
        {
            ColyseusHttpQSCollection query = ColyseusHttpUtility.ParseQueryString(string.Empty);
            query["accessToken"] = facebookAccessToken;
            return await Login(query);
        }

        /// <summary>
        ///     Login with the e-mail and password of an existing user
        /// </summary>
        /// <param name="email">User's e-mail address</param>
        /// <param name="password">User's password</param>
        /// <returns><see cref="ColyseusAuth" /> via async task</returns>
        public async Task<ColyseusAuth> Login(string email, string password)
        {
            ColyseusHttpQSCollection query = ColyseusHttpUtility.ParseQueryString(string.Empty);
            query["email"] = email;
            query["password"] = password;
            return await Login(query);
        }

        /// <summary>
        ///     Login given a set of query parameters
        /// </summary>
        /// <remarks>
        ///     This is the final Login task that the other Login functions call (after building the query parameters)
        ///     <para>The function will await a <see cref="Request{T}" /> task where we POST the query parameters</para>
        /// </remarks>
        /// <param name="queryParams">The parameters with which the user is trying to login</param>
        /// <returns><see cref="ColyseusAuth" /> via async task</returns>
        public async Task<ColyseusAuth> Login(NameValueCollection queryParams)
        {
            queryParams["deviceId"] = GetDeviceId();
            queryParams["platform"] = GetPlatform();

            CSAUserData csaUserData = await Request<CSAUserData>("POST", "/auth", queryParams);

            _id = csaUserData._id;
            Username = csaUserData.username;
            DisplayName = csaUserData.displayName;
            AvatarUrl = csaUserData.avatarUrl;

            IsAnonymous = csaUserData.isAnonymous;
            Email = csaUserData.email;

            Lang = csaUserData.lang;
            Location = csaUserData.location;
            Timezone = csaUserData.timezone;
            Metadata = csaUserData.metadata;

            Devices = csaUserData.devices;

            FacebookId = csaUserData.facebookId;
            TwitterId = csaUserData.twitterId;
            GoogleId = csaUserData.googleId;
            GameCenterId = csaUserData.gameCenterId;
            SteamId = csaUserData.steamId;

            FriendIds = csaUserData.friendIds;
            BlockedUserIds = csaUserData.blockedUserIds;

            Token = csaUserData.token;
            PlayerPrefs.SetString("Token", Token);

            return this;
        }

        /// <summary>
        ///     Saves the <see cref="ColyseusAuth" /> information as <see cref="CSAUserData" />
        /// </summary>
        /// <remarks>The function will await a <see cref="Request{T}" /> task</remarks>
        /// <returns><see cref="ColyseusAuth" /> via async task</returns>
        public async Task<ColyseusAuth> Save()
        {
            CSAUserData uploadData = new CSAUserData();
            if (!string.IsNullOrEmpty(Username))
            {
                uploadData.username = Username;
            }

            if (!string.IsNullOrEmpty(DisplayName))
            {
                uploadData.displayName = DisplayName;
            }

            if (!string.IsNullOrEmpty(AvatarUrl))
            {
                uploadData.avatarUrl = AvatarUrl;
            }

            if (!string.IsNullOrEmpty(Lang))
            {
                uploadData.lang = Lang;
            }

            if (!string.IsNullOrEmpty(Location))
            {
                uploadData.location = Location;
            }

            if (!string.IsNullOrEmpty(Timezone))
            {
                uploadData.timezone = Timezone;
            }

            string bodyString = JsonUtility.ToJson(uploadData);
            await Request<CSAUserData>("PUT", "/auth", null, new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyString)));

            return this;
        }

        /// <summary>
        ///     Gets all of this user's friends in a <see cref="CSAUserDataCollection" />
        /// </summary>
        /// <returns><see cref="CSAUserDataCollection" /> via async <see cref="Request{T}" /></returns>
        public async Task<CSAUserDataCollection> GetFriends()
        {
            return await Request<CSAUserDataCollection>("GET", "/friends/all");
        }

        /// <summary>
        ///     Get's all of this user's online friends in a <see cref="CSAUserDataCollection" />
        /// </summary>
        /// <returns><see cref="CSAUserDataCollection" /> via async <see cref="Request{T}" /></returns>
        public async Task<CSAUserDataCollection> GetOnlineFriends()
        {
            return await Request<CSAUserDataCollection>("GET", "/friends/online");
        }

        /// <summary>
        ///     Get's all of this user's pending friend requests in a <see cref="CSAUserDataCollection" />
        /// </summary>
        /// <returns><see cref="CSAUserDataCollection" /> via async <see cref="Request{T}" /></returns>
        public async Task<CSAUserDataCollection> GetFriendRequests()
        {
            return await Request<CSAUserDataCollection>("GET", "/friends/requests");
        }

        /// <summary>
        ///     Send a friend request
        /// </summary>
        /// <param name="friendId">The ID of the user being requested</param>
        /// <returns><see cref="CSAStatusData" /> via async <see cref="Request{T}" /></returns>
        public async Task<CSAStatusData> SendFriendRequest(string friendId)
        {
            ColyseusHttpQSCollection query = ColyseusHttpUtility.ParseQueryString(string.Empty);
            query["userId"] = friendId;
            return await Request<CSAStatusData>("POST", "/friends/requests", query);
        }

        /// <summary>
        ///     Accept a friend request
        /// </summary>
        /// <param name="friendId">The ID of the user being accepted</param>
        /// <returns><see cref="CSAStatusData" /> via async <see cref="Request{T}" /></returns>
        public async Task<CSAStatusData> AcceptFriendRequest(string friendId)
        {
            ColyseusHttpQSCollection query = ColyseusHttpUtility.ParseQueryString(string.Empty);
            query["userId"] = friendId;
            return await Request<CSAStatusData>("PUT", "/friends/requests", query);
        }

        /// <summary>
        ///     Decline a friend request
        /// </summary>
        /// <param name="friendId">The ID of the user being declined</param>
        /// <returns><see cref="CSAStatusData" /> via async <see cref="Request{T}" /></returns>
        public async Task<CSAStatusData> DeclineFriendRequest(string friendId)
        {
            ColyseusHttpQSCollection query = ColyseusHttpUtility.ParseQueryString(string.Empty);
            query["userId"] = friendId;
            return await Request<CSAStatusData>("DELETE", "/friends/requests", query);
        }

        /// <summary>
        ///     Accept a friend request
        /// </summary>
        /// <param name="friendId">The ID of the user being requested</param>
        /// <returns><see cref="CSAStatusData" /> via async <see cref="Request{T}" /></returns>
        public async Task<CSAStatusData> BlockUser(string friendId)
        {
            ColyseusHttpQSCollection query = ColyseusHttpUtility.ParseQueryString(string.Empty);
            query["userId"] = friendId;
            return await Request<CSAStatusData>("POST", "/friends/block", query);
        }

        /// <summary>
        ///     Remove a user from the blocked list
        /// </summary>
        /// <param name="friendId">The ID of the user being un-blocked</param>
        /// <returns><see cref="CSAStatusData" /> via async <see cref="Request{T}" /></returns>
        public async Task<CSAStatusData> UnblockUser(string friendId)
        {
            ColyseusHttpQSCollection query = ColyseusHttpUtility.ParseQueryString(string.Empty);
            query["userId"] = friendId;
            return await Request<CSAStatusData>("PUT", "/friends/block", query);
        }

        /// <summary>
        ///     Clear out the cached <see cref="Token" />
        /// </summary>
        public void Logout()
        {
            Token = string.Empty;
            PlayerPrefs.SetString("Token", Token);
        }

        /// <summary>
        ///     Make a request to the server
        /// </summary>
        /// <param name="method">The method of the request (POST,GET, etc)</param>
        /// <param name="segments">
        ///     Starting with a "/", additional info that will be used for the <see cref="UriBuilder.Path" />
        /// </param>
        /// <param name="query">Optional query that will be sent to the backend</param>
        /// <param name="data">Optional data to be uploaded </param>
        /// <typeparam name="T">
        ///     The type of object we will deserialize from the <see cref="UnityWebRequest" />
        ///     <para>Object will be deserialized with <see cref="JsonUtility.FromJson{T}" /></para>
        /// </typeparam>
        /// <returns>
        ///     Response from the <see cref="UnityWebRequest" />, deserialized with <see cref="JsonUtility.FromJson{T}" />
        /// </returns>
        /// <exception cref="Exception">
        ///     Thrown if there is an error in the response of the <see cref="UnityWebRequest" />
        /// </exception>
        protected async Task<T> Request<T>(string method, string segments, NameValueCollection query = null,
            UploadHandlerRaw data = null)
        {
            if (query == null)
            {
                query = ColyseusHttpUtility.ParseQueryString(string.Empty);
            }

            // Append auth token, if it exists
            if (HasToken)
            {
                query["token"] = Token;
            }

            try
            {
                string json = await ColyseusRequest.Request(method, segments, query.ToString(), Token, data);

                // Workaround for decoding a CSAUserDataCollection
                if (json.StartsWith("[", StringComparison.CurrentCulture))
                {
                    json = "{\"users\": " + json + "}";
                }
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        ///     Get the device's ID
        /// </summary>
        /// <returns>
        ///     <see cref="SystemInfo.deviceUniqueIdentifier" />
        /// </returns>
        protected string GetDeviceId()
        {
            // TODO: Create a random id and assign it to PlayerPrefs for WebGL
            // #if UNITY_WEBGL
            return SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        ///     Get the current platform
        /// </summary>
        /// <returns>
        ///     Platform name, depending on
        ///     <see href="https://docs.unity3d.com/Manual/PlatformDependentCompilation.html">Unity platform #define directives</see>
        /// </returns>
        protected string GetPlatform()
        {
#if UNITY_EDITOR
            return "unity_editor";
#elif UNITY_STANDALONE_OSX
		return "osx";
#elif UNITY_STANDALONE_WIN
		return "windows";
#elif UNITY_STANDALONE_LINUX
		return "linux";
#elif UNITY_WII
		return "wii";
#elif UNITY_IOS
		return "ios";
#elif UNITY_ANDROID
		return "android";
#elif UNITY_PS4
		return "ps2";
#elif UNITY_XBOXONE
		return "xboxone";
#elif UNITY_TIZEN
		return "tizen";
#elif UNITY_TVOS
		return "tvos";
#elif UNITY_WSA || UNITY_WSA_10_0
		return "wsa";
#elif UNITY_WINRT || UNITY_WINRT_10_0
		return "winrt";
#elif UNITY_WEBGL
		return "html5";
#elif UNITY_FACEBOOK
		return "facebook";
#elif UNITY_ADS
		return "unity_ads";
#elif UNITY_ANALYTICS
		return "unity_analytics";
#elif UNITY_ASSERTIONS
		return "unity_assertions";
#endif
        }
    }
}