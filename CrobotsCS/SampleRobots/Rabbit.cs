// Rabbit Robot
// Strategy: Fast and erratic movement

public class Rabbit {
	public void Execute(RobotApi api) {
		// Random direction changes
		api.Drive(api.Random(360), 4);

		// Fire occasionally
		if (api.Random(10) < 2) {
			api.Cannon(api.Heading, 700);
		}
	}
}
