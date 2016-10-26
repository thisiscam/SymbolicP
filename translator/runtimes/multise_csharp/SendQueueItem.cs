class SendQueueItem
{
    /* I felt a bit overengineered if we use private fields and create getters for each */
    public ValueSummary<PMachine> target = new ValueSummary<PMachine>(default (PMachine));
    public ValueSummary<PInteger> e = new ValueSummary<PInteger>(default (PInteger));
    public ValueSummary<IPType> payload = new ValueSummary<IPType>(default (IPType));
    public SendQueueItem(ValueSummary<PMachine> target, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        this.target.Assign<PMachine>(target);
        this.e.Assign<PInteger>(e);
        this.payload.Assign<IPType>(payload);
    }
}