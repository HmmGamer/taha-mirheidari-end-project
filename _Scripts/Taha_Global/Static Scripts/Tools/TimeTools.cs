public static class TimeTools
{
    public static int[] ArrayTotalTime(float iTime)
    {
        int Hour = 0;
        int minutes = 0;
        int Seconds = 0;
        if (iTime / 3600 > 1)
        {
            Hour = (int)(iTime / 3600);
            iTime -= Hour * 3600;
        }
        if (iTime / 60 > 1)
        {
            minutes = (int)(iTime / 60);
            iTime -= minutes * 60;
        }
        Seconds = (int)iTime;
        int[] _Result = { Hour, minutes, Seconds };
        return _Result;
    }
    public static int TotalHour(float iTime)
    {
        int Hour = 0;
        if (iTime / 3600 > 1)
        {
            Hour = (int)(iTime / 3600);
        }
        return Hour;
    }
    public static int TotalMinutes(float iTime)
    {
        int Hour = 0;
        int minutes = 0;
        if (iTime / 3600 > 1)
        {
            Hour = (int)(iTime / 3600);
            iTime -= Hour * 3600;
        }
        if (iTime / 60 > 1)
        {
            minutes = (int)(iTime / 60);
        }
        return minutes;
    }
    public static int TotalSeconds(float iTime)
    {
        int Hour = 0;
        int minutes = 0;
        int Seconds = 0;
        if (iTime / 3600 > 1)
        {
            Hour = (int)(iTime / 3600);
            iTime -= Hour * 3600;
        }
        if (iTime / 60 > 1)
        {
            minutes = (int)(iTime / 60);
            iTime -= minutes * 60;
        }
        Seconds = (int)iTime;
        return Seconds;
    }
    public static string TotalStringTime(float iTime)
    {
        //int Hour = 0;
        int minutes = 0;
        int Seconds = 0;
        //if (iTime / 3600 > 1)
        //{
        //    Hour = (int)(iTime / 3600);
        //    iTime -= Hour * 3600;
        //}
        if (iTime / 60 > 1)
        {
            minutes = (int)(iTime / 60);
            iTime -= minutes * 60;
        }
        Seconds = (int)iTime;

        string _Result;
        if (Seconds > 9)
            _Result = string.Format("{0}:{1}", minutes, Seconds);
        else
            _Result = minutes.ToString() + ":0" + Seconds.ToString();

        return _Result;
    }
    public static string ReverseTimerString(float iTime, float iTimerTime)
    {
        iTimerTime -= iTime;
        int Hour = 0;
        int minutes = 0;
        int Seconds = 0;
        if (iTimerTime / 3600 > 1)
        {
            Hour = (int)(iTimerTime / 3600);
            iTimerTime -= Hour * 3600;
        }
        if (iTimerTime / 60 > 1)
        {
            minutes = (int)(iTimerTime / 60);
            iTimerTime -= minutes * 60;
        }
        if (iTimerTime <= 0)
        {
            iTimerTime = 0;
        }
        Seconds = (int)iTimerTime;
        string _Result = System.String.Format("{0} : {1} : {2}", Hour, minutes, Seconds);
        return _Result;
    }
    public static int ReverseTimerMin(float iTime, float iTimerTime)
    {
        iTimerTime -= iTime;
        int Hour = 0;
        int minutes = 0;
        if (iTimerTime / 3600 > 1)
        {
            Hour = (int)(iTimerTime / 3600);
            iTimerTime -= Hour * 3600;
        }
        if (iTimerTime / 60 > 1)
        {
            minutes = (int)(iTimerTime / 60);
            iTimerTime -= minutes * 60;
            return minutes;
        }
        return 0;

    }
    public static int ReverseTimerSec(float iTime, float iTimerTime)
    {
        iTimerTime -= iTime;
        int Hour = 0;
        int minutes = 0;
        int Seconds = 0;
        if (iTimerTime / 3600 > 1)
        {
            Hour = (int)(iTimerTime / 3600);
            iTimerTime -= Hour * 3600;
        }
        if (iTimerTime / 60 > 1)
        {
            minutes = (int)(iTimerTime / 60);
            iTimerTime -= minutes * 60;
        }
        if (iTimerTime <= 0)
        {
            iTimerTime = 0;
        }
        Seconds = (int)iTimerTime;
        return Seconds;
    }
}
