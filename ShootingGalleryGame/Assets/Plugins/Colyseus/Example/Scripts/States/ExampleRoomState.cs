using Colyseus.Schema;
using Colyseus;

public class ExampleRoomState : ColyseusRoomState
{
	[CSAType(0, "map", typeof(ColyseusMapSchema<ExampleNetworkedEntity>))]
	public ColyseusMapSchema<ExampleNetworkedEntity> networkedEntities = new ColyseusMapSchema<ExampleNetworkedEntity>();
	[CSAType(1, "map", typeof(ColyseusMapSchema<ExampleNetworkedUser>))]
	public ColyseusMapSchema<ExampleNetworkedUser> networkedUsers = new ColyseusMapSchema<ExampleNetworkedUser>();
	[CSAType(2, "map", typeof(ColyseusMapSchema<string>), "string")]
	public ColyseusMapSchema<string> attributes = new ColyseusMapSchema<string>();

}

