using System;
using System.Diagnostics;

class MachineStarter {
    public static PMachine CreateMainMachine() {
        return new MachineClient();
    }
}