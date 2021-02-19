using Colyseus.Schema;
using Colyseus;
using System.Collections.Generic;

[System.Serializable]
public class ExampleNetworkedEntity : ColyseusNetworkedEntity
{
	//public string updateHash;

	[CSAType(0, "string")]
	public string id = default(string);

	[CSAType(1, "string")]
	public string ownerId = default(string);

	[CSAType(2, "string")]
	public string creationId = default(string);

	[CSAType(3, "number")]
	public double xPos = default(double);

	[CSAType(4, "number")]
	public double yPos = default(double);

	[CSAType(5, "number")]
	public double zPos = default(double);

	[CSAType(6, "number")]
	public float xRot = default(float);

	[CSAType(7, "number")]
	public float yRot = default(float);

	[CSAType(8, "number")]
	public float zRot = default(float);

	[CSAType(9, "number")]
	public float wRot = default(float);

	[CSAType(10, "number")]
	public float xScale = default(float);

	[CSAType(11, "number")]
	public float yScale = default(float);

	[CSAType(12, "number")]
	public float zScale = default(float);

    [CSAType(13, "number")]
	public double xVel = default(double);

	[CSAType(14, "number")]
	public double yVel = default(double);

	[CSAType(15, "number")]
	public double zVel = default(double);

    [CSAType(16, "number")]
    public double timestamp = default(double);

	[CSAType(17, "map", typeof(ColyseusMapSchema<string>),"string")]
	public ColyseusMapSchema<string> attributes = new ColyseusMapSchema<string>();

	// Make sure to update Clone fi you add any attributes
	public ExampleNetworkedEntity Clone()
	{
		return new ExampleNetworkedEntity() {id = id, ownerId = ownerId, creationId = creationId, xPos = xPos, yPos = yPos, zPos = zPos, xRot = xRot, yRot = yRot, zRot = zRot, wRot = wRot, xScale = xScale, yScale = yScale, zScale = zScale, timestamp = timestamp, attributes = attributes };
	}

}

[System.Serializable]
class EntityCreationMessage
{
	public string creationId;
	public Dictionary<string, object> attributes;
}

