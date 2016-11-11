using Microsoft.AspNetCore.SignalR;

public class TestHub : Hub
{
    public TestHub()
    {

    }

    public void Test(string message)
    {
        Clients.All.tested(message);
    }
}