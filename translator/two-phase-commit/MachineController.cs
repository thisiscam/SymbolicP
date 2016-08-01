class MachineController
{
    static MonitorPMachine m = new MachineM();
    public static ValueSummary<PMachine> CreateMainMachine()
    {
        return new ValueSummary<PMachine>(new MachineTwoPhaseCommit());
    }

    /* Observers */
    public static void AnnounceEvent(ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        switch (e)
        {
            case Constants.MONITOR_READ_SUCCESS:
            {
                m.InvokeMethod<PInteger, IPType>((_, a0, a1) => _.ServeEvent(a0, a1), (PInteger)Constants.MONITOR_READ_SUCCESS, payload);
                break;
            }

            case Constants.MONITOR_READ_UNAVAILABLE:
            {
                m.InvokeMethod<PInteger, IPType>((_, a0, a1) => _.ServeEvent(a0, a1), (PInteger)Constants.MONITOR_READ_UNAVAILABLE, payload);
                break;
            }

            case Constants.MONITOR_WRITE:
            {
                m.InvokeMethod<PInteger, IPType>((_, a0, a1) => _.ServeEvent(a0, a1), (PInteger)Constants.MONITOR_WRITE, payload);
                break;
            }
        }
    }
}