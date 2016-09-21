class MachineController
{
    public static ValueSummary<PMachine> CreateMainMachine()
    {
        return new ValueSummary<PMachine>(new MachinePING());
    }
}