# ColyseusTechDemo-ShootingGallery

## Getting Started
* Install and set up [Docker](https://www.docker.com/get-started)

### Deploying a local server - Windows
* Open a terminal window within *ServerCode\colyseus* 
* Run the command `docker build -t colyseus .` (including the ".")
* To launch a local container, run the command `docker-compose up`

### Connecting in game
* Launch the Unity Project located within *ShootingGalleryGame*
* Locate the *ColyseusSettings* ScriptableObject located at *ShootingGalleryGame\Assets\Plugins\Colyseus\Example\Settings*
 * Confirm that the **ColyseusServerAddress** is *127.0.0.1* (this will connect to your local container)
* Launch the game from *Lobby* scene
