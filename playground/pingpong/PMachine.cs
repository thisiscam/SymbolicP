using System;
using System.Collections.Generic;

class PMachine {
    public int state;
    public List<int> events = new List<int>();
    public List<object[]> payloads = new List<object[]>();

    public void sendMsg(PMachine other, int e, object[] payload) {
        Console.WriteLine(this.ToString() + " send event " + e.ToString() + " to " + other.ToString());
        other.events.Add(e);
        other.payloads.Add(payload);
    }
}