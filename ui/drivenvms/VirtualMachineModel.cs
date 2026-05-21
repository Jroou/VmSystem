namespace drivenvms
{
    public class VirtualMachineModel
    {
        public string Name { get; set; }
        public string Uuid { get; set; }
        public string State { get; set; } // Наприклад: running, poweroff, saved
    }
}