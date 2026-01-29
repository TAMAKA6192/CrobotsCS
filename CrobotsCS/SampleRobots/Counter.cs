// Counter Robot
// Strategy: Circle and shoot behind

public class Counter
{
    public void Execute(RobotApi api)
    {
        // Move in a circle
        api.Drive(api.Heading + 5, 2);
        
        // Scan behind
        api.Scan(api.Heading + 180, 5);
        
        // Shoot behind
        api.Cannon(api.Heading + 180, 500);
    }
}
