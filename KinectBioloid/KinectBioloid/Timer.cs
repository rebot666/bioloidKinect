using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public class Timer
{
    public void tick()
    {
        int i = 1;
        while (i <= 5)
        {
            Thread.Sleep(1000);
            i++;
        }
    }

    public void corre() {
        Thread thread = new Thread(new ThreadStart(tick));
    }
};
namespace KinectBioloid
{
    
}
