using UnityEngine;

namespace Colyseus
{
    /// <summary>
    /// Base manager class 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ColyseusManager<T> : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Colyseus settings object.
        /// </summary>
        [SerializeField]
        protected ColyseusSettings _colyseusSettings;

        private ColyseusRequest _requests;
        // Getters
        //==========================
        /// <summary>
        /// The singleton instance of the Colyseus Manager.
        /// </summary>
        public static T Instance { get; private set; }

        /// <summary>
        /// Returns the Colyseus server address as defined
        /// in the Colyseus settings object.
        /// </summary>
        public string ColyseusServerAddress
        {
            get { return _colyseusSettings.colyseusServerAddress; }
        }

        /// <summary>
        /// Returns the Colyseus server port as defined
        /// in the Colyseus settings object. 
        /// </summary>
        public string ColyseusServerPort
        {
            get { return _colyseusSettings.colyseusServerPort; }
        }
        //==========================

        /// <summary>
        /// The Client that is created when connecting to the Colyseus server.
        /// </summary>
        protected ColyseusClient client;

        /// <summary>
        /// <see cref="MonoBehaviour"/> callback when the manager object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/> callback when the script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            InitializeInstance();
        }

        /// <summary>
        /// Initializes the Colyseus manager singleton.
        /// </summary>
        private void InitializeInstance()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = GetComponent<T>();

            // Initialize the requests object with settings
            _requests = new ColyseusRequest(_colyseusSettings);
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/> callback when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        protected virtual void Start()
        {
            
        }

        /// <summary>
        /// Frame-rate independent message for physics calculations.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            
        }

        /// <summary>
        /// Creates a new <see cref="ColyseusClient"/> object, with the given endpoint, and returns it
        /// </summary>
        /// <param name="endpoint">URL to the Colyseus server</param>
        /// <returns></returns>
        private ColyseusClient CreateClient(string endpoint)
        {
            return new ColyseusClient(endpoint);
        }

        /// <summary>
        /// Connect to the Colyseus server.
        /// </summary>
        protected virtual void ConnectToServer()
        {
            client = CreateClient(_colyseusSettings.WebSocketEndpoint);
        }

        /// <summary>
        /// <see cref="MonoBehaviour"/> callback that gets called just before app exit.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {

        }
    }
}