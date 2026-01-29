// Sniper Robot
// Strategy: Move slowly and scan/shoot in the heading direction

public class Sniper
{
    public void Execute(RobotApi api)
    {
        // Random small heading adjustments
        api.Drive(api.Heading + api.Random(20) - 10, 2);
        
        // Scan ahead
        api.Scan(api.Heading, 10);
        
        // Fire occasionally
        if (api.Random(10) < 3)
        {
            api.Cannon(api.Heading, 500);
        }
    }
}
