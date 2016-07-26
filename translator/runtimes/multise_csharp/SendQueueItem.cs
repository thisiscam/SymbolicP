class SendQueueItem
{
    /* I felt a bit overengineered if we use private fields and create getters for each */
    public ValueSummary<PMachine> target;
    public ValueSummary<PInteger> e;
    public ValueSummary<IPType> payload;
    public SendQueueItem(ValueSummary<PMachine> target, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        this.target = target;
        this.e = e;
        this.payload = payload;
    }
}