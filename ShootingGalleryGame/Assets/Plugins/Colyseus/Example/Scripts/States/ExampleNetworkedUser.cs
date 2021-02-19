using Colyseus.Schema;
using Colyseus;

[System.Serializable]
public class ExampleNetworkedUser : ColyseusNetworkedUser
{
	public string updateHash;

	[CSAType(0, "string")]
	public string id = default(string);

	[CSAType(1, "string")]
	public string sessionId = default(string);

	[CSAType(2, "boolean")]
	public bool connected = default(bool);

	[CSAType(3, "number")]
	public double timestamp = default(double);

	[CSAType(4, "map", typeof(ColyseusMapSchema<string>), "string")]
	public ColyseusMapSchema<string> attributes = new ColyseusMapSchema<string>();

}

