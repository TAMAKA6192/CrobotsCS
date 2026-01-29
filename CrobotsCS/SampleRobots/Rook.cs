// Rook Robot
// Strategy: Move in straight lines, turn at walls

public class Rook
{
    public void Execute(RobotApi api)
    {
        // Move forward
        api.Drive(api.Heading, 3);
        
        // Turn if near wall
        if (api.X < 100 || api.X > 900 || api.Y < 100 || api.Y > 900)
        {
            api.Drive(api.Heading + 90, 3);
        }
        
        // Fire in random directions
        api.Cannon(api.Heading + api.Random(360), 500);
    }
}
