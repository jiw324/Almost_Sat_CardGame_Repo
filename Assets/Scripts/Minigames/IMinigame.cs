using System;
using System.Threading.Tasks;

// Every minigame has their own implementation, but sends back finishResult
// Should be a range from 0 to 1. 0- Failure, 0.5- decent, 1- success
public interface IMinigame
{
    Task<float> PlayAsync();
}
