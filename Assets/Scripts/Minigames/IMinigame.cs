using System;
using System.Threading.Tasks;

// Every minigame has their own implementation, but sends back finishResult
// Should be a range from 0 to 2. 0- Failure, 1.0- default/normal (used for enemies), 2.0- perfect success
public interface IMinigame
{
    Task<float> PlayAsync();
}
