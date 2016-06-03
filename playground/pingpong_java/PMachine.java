import java.util.*;

class PMachine {
    public int state;
    public List<Integer> events = new ArrayList<>();
    public List<Object> payloads = new ArrayList<>();

    public void sendMsg(PMachine other, int e, Object payload) {
        System.out.println(this.toString() + " send event " + Integer.toString(e) + " to " + other.toString());
        other.events.add(e);
        other.payloads.add(payload);
    }
}